using System.Globalization;
using AccountingProject.Contracts;
using AccountingProject.Data;
using AccountingProject.Domain;
using AccountingProject.Infrastructure;
using AccountingProject.Models;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AccountingProject.Services
{
    public class BulkImportService : IBulkImportService
    {
        private static readonly string[] RequiredEmployeeHeaders = ["שם_מעסיק", "תז", "שם_פרטי", "שם_משפחה", "מין", "תאריך_לידה", "שנת_לימודים"];
        private static readonly string[] RequiredEmployeeHeadersForEmployer = ["תז", "שם_פרטי", "שם_משפחה", "מין", "תאריך_לידה", "שנת_לימודים"];
        private static readonly string[] RequiredEmployerHeaders = ["חפ"];

        private readonly PayrollDbContext _db;
        private readonly IEmploymentCalculationService _calculations;
        private readonly ILogger<BulkImportService> _logger;

        public BulkImportService(
            PayrollDbContext db,
            IEmploymentCalculationService calculations,
            ILogger<BulkImportService> logger)
        {
            _db = db;
            _calculations = calculations;
            _logger = logger;
        }

        public async Task<ImportResult> ImportEmployeesAsync(IFormFile file, int? employerId = null)
        {
            var result = new ImportResult();
            using var workbook = OpenWorkbookOrThrow(file);
            var sheet = workbook.Worksheets.First();
            var headers = BuildHeaders(sheet);
            ValidateHeaders(headers, employerId.HasValue ? RequiredEmployeeHeadersForEmployer : RequiredEmployeeHeaders);

            Employer? fixedEmployer = null;
            if (employerId.HasValue)
            {
                fixedEmployer = await _db.Employers.FirstOrDefaultAsync(e => e.Id == employerId.Value);
                if (fixedEmployer == null)
                    throw new InvalidOperationException("המעסיק לא נמצא במערכת.");
            }

            await using var tx = await _db.Database.BeginTransactionAsync();
            var pendingSuccessfulRows = 0;
            // Key by (employer id, national id) — the same person may have multiple employee rows under different employers.
            var localEmployeesByEmployerAndTz = new Dictionary<(int EmployerId, string IdNumber), Employee>();
            var localEmploymentKeys = new HashSet<(string IdNumber, int EmployerId, string AcademicYear)>();

            int lastRow = sheet.LastRowUsed()?.RowNumber() ?? 1;
            for (int rowNumber = 2; rowNumber <= lastRow; rowNumber++)
            {
                var row = sheet.Row(rowNumber);
                if (IsEmptyRow(row, headers)) continue;

                var rowResult = new ImportRowResult { Row = rowNumber };
                try
                {
                    string Get(string key) => headers.TryGetValue(key, out var col) ? ExcelCellText.Get(row.Cell(col)).Trim() : string.Empty;
                    decimal? GetNum(string key) =>
                        decimal.TryParse(Get(key), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var value) ? value : null;
                    decimal? GetNumOrLegacy(string perBandKey, string legacyKey)
                    {
                        var v = GetNum(perBandKey);
                        return v ?? GetNum(legacyKey);
                    }

                    var plan = ParseEmployeeImportRow(
                        Get, GetNum, GetNumOrLegacy, headers, row, fixedEmployer?.Name, rowResult);
                    await ValidateEmployeeImportRowAsync(
                        plan,
                        fixedEmployer,
                        localEmploymentKeys,
                        localEmployeesByEmployerAndTz);

                    RecalculateEmploymentImportPlan(plan);
                    ApplyEmployeeImportRow(plan, localEmployeesByEmployerAndTz, localEmploymentKeys);
                    pendingSuccessfulRows++;
                        if (pendingSuccessfulRows >= 100)
                        {
                            await _db.SaveChangesAsync();
                            pendingSuccessfulRows = 0;
                        }

                        rowResult.Success = true;
                        rowResult.Message = "יובא בהצלחה.";
                        result.Imported++;
                }
                catch (InvalidOperationException ex)
                {
                    rowResult.Success = false;
                    rowResult.Message = ex.Message;
                    result.Errors++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Employee import row {Row} failed unexpectedly.", rowNumber);
                    rowResult.Success = false;
                    rowResult.Message = "שגיאה בשורה זו בעת העיבוד. בדקו את הנתונים ונסו שוב.";
                    result.Errors++;
                }

                result.Rows.Add(rowResult);
            }

            if (pendingSuccessfulRows > 0)
                await _db.SaveChangesAsync();

            await tx.CommitAsync();
            return result;
        }

        public async Task<ImportResult> ImportEmployersAsync(IFormFile file)
        {
            var result = new ImportResult();
            using var workbook = OpenWorkbookOrThrow(file);
            var sheet = workbook.Worksheets.First();
            var headers = BuildHeaders(sheet);
            ValidateHeaders(headers, RequiredEmployerHeaders);
            var pendingSuccessfulRows = 0;
            var localBusinessNumbers = new HashSet<string>(StringComparer.Ordinal);

            int lastRow = sheet.LastRowUsed()?.RowNumber() ?? 1;
            for (int rowNumber = 2; rowNumber <= lastRow; rowNumber++)
            {
                var row = sheet.Row(rowNumber);
                if (IsEmptyRow(row, headers)) continue;

                var rowResult = new ImportRowResult { Row = rowNumber };
                try
                {
                    string Get(string key) => headers.TryGetValue(key, out var col) ? ExcelCellText.Get(row.Cell(col)).Trim() : string.Empty;
                    var businessNumber = Normalize(Get("חפ"));
                    var name = Normalize(Get("שם_מעסיק"));
                    rowResult.BusinessNumber = businessNumber;
                    rowResult.EmployerName = name;

                    if (string.IsNullOrWhiteSpace(businessNumber))
                        throw new InvalidOperationException("שורה דולגה — ח.פ. ריק.");
                    if (localBusinessNumbers.Contains(businessNumber))
                        throw new InvalidOperationException($"מעסיק עם ח.פ. {businessNumber} כבר מופיע בקובץ — דולג.");

                    var exists = await _db.Employers.AnyAsync(e => e.BusinessNumber == businessNumber);
                    if (exists)
                        throw new InvalidOperationException($"מעסיק עם ח.פ. {businessNumber} כבר קיים — דולג.");

                    // Check for a previously soft-deleted employer with the same ח.פ. — restore it instead of blocking.
                    var deletedEmployer = await _db.Employers
                        .IgnoreQueryFilters()
                        .FirstOrDefaultAsync(e => e.BusinessNumber == businessNumber && e.IsDeleted);
                    if (deletedEmployer != null)
                    {
                        deletedEmployer.Name = name ?? string.Empty;
                        deletedEmployer.BeneficiarySymbol = Normalize(Get("סמל_מוטב"));
                        deletedEmployer.EketzNumber = Normalize(Get("מספר_עוקץ"));
                        deletedEmployer.IsDeleted = false;
                        deletedEmployer.DeletedAtUtc = null;
                        rowResult.Message = "שוחזר בהצלחה (מעסיק זה היה מחוק בעבר).";
                    }
                    else
                    {
                        _db.Employers.Add(new Employer
                        {
                            Name = name ?? string.Empty,
                            BusinessNumber = businessNumber,
                            BeneficiarySymbol = Normalize(Get("סמל_מוטב")),
                            EketzNumber = Normalize(Get("מספר_עוקץ"))
                        });
                        rowResult.Message = "יובא בהצלחה.";
                    }

                    localBusinessNumbers.Add(businessNumber);
                    pendingSuccessfulRows++;
                    if (pendingSuccessfulRows >= 100)
                    {
                        await _db.SaveChangesAsync();
                        pendingSuccessfulRows = 0;
                    }

                    rowResult.Success = true;
                    result.Imported++;
                }
                catch (InvalidOperationException ex)
                {
                    rowResult.Success = false;
                    rowResult.Message = ex.Message;
                    result.Errors++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Employer import row {Row} failed unexpectedly.", rowNumber);
                    rowResult.Success = false;
                    rowResult.Message = "שגיאה בשורה זו בעת העיבוד. בדקו את הנתונים ונסו שוב.";
                    result.Errors++;
                }

                result.Rows.Add(rowResult);
            }

            if (pendingSuccessfulRows > 0)
                await _db.SaveChangesAsync();

            return result;
        }

        public XLWorkbook BuildEmployeesTemplate(bool includeEmployerName = true, int? employerId = null)
        {
            var workbook = new XLWorkbook();
            var worksheet = workbook.AddWorksheet("עובדים");
            SetRtl(worksheet);
            var columns = GetEmployeeTemplateColumns(includeEmployerName).ToArray();
            WriteHeaders(worksheet, columns);
            var institutionSymbols = employerId.HasValue
                ? GetInstitutionSymbolValues(GetEmployer(employerId.Value))
                : [];
            AddGradeDataValidations(workbook, worksheet, columns, institutionSymbols);
            return workbook;
        }

        private static IReadOnlyList<string> GetEmployeeTemplateColumns(bool includeEmployerName)
        {
            var cols = new List<string>
            {
                "תז", "מספר_עובד_בעוקץ", "שם_משפחה", "שם_פרטי", "מין", "תאריך_לידה", "טל",
                "תאריך_לידה_ילד_1", "תאריך_לידה_ילד_2", "תאריך_לידה_ילד_3", "תאריך_לידה_ילד_4", "תאריך_לידה_ילד_5",
                "תאריך_לידה_ילד_6", "תאריך_לידה_ילד_7", "תאריך_לידה_ילד_8", "תאריך_לידה_ילד_9", "תאריך_לידה_ילד_10",
                "שנת_לימודים"
            };
            if (includeEmployerName)
            {
                cols.Insert(0, "שם_מעסיק");
                cols.Insert(1, "חפ");
            }
            for (var g = 1; g <= 2; g++)
            {
                cols.Add($"דרגה{g}_שם_הדירוג");
                cols.Add($"דרגה{g}_דרגה");
                cols.Add($"דרגה{g}_תפקיד");
                cols.Add($"דרגה{g}_ותק");
                var slotParts = new[] { "סמל_מוסד", "שעות_שבועיות", "בסיס_משרה" };
                for (var s = 1; s <= 6; s++)
                {
                    foreach (var p in slotParts)
                        cols.Add($"דרגה{g}_{s}_{p}");
                }
            }
            cols.AddRange(
            [
                "דרגה1_סהכ", "דרגה1_אחוז_משרה", "דרגה1_קרן_השתלמות_אחוז", "דרגה1_שעות_גיל", "דרגה1_אחוז_תוספת_אם",
                "דרגה2_סהכ", "דרגה2_אחוז_משרה", "דרגה2_קרן_השתלמות_אחוז", "דרגה2_שעות_גיל", "דרגה2_אחוז_תוספת_אם",
                "דרגה1_גמולי_השתלמות", "דרגה1_כפל_תואר", "דרגה2_גמולי_השתלמות", "דרגה2_כפל_תואר"
            ]);
            return cols;
        }

        public XLWorkbook BuildEmployersTemplate()
        {
            var workbook = new XLWorkbook();
            var worksheet = workbook.AddWorksheet("מעסיקים");
            SetRtl(worksheet);
            WriteHeaders(worksheet, ["חפ", "שם_מעסיק", "סמל_מוטב", "מספר_עוקץ"]);
            return workbook;
        }

        private static void SetRtl(IXLWorksheet worksheet)
        {
            var prop = worksheet.SheetView.GetType().GetProperty("RightToLeft");
            prop?.SetValue(worksheet.SheetView, true);
        }

        private static Dictionary<string, int> BuildHeaders(IXLWorksheet sheet)
        {
            var headers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            int last = sheet.LastColumnUsed()?.ColumnNumber() ?? 0;
            for (int column = 1; column <= last; column++)
            {
                var header = ExcelCellText.Get(sheet.Cell(1, column)).Trim();
                if (!string.IsNullOrEmpty(header))
                {
                    headers[header] = column;
                }
            }

            return headers;
        }

        private static void ValidateHeaders(Dictionary<string, int> headers, string[] requiredHeaders)
        {
            var missing = requiredHeaders.Where(h => !headers.ContainsKey(h)).ToArray();
            if (missing.Length > 0)
            {
                throw new InvalidOperationException($"חסרות כותרות חובה בקובץ: {string.Join(", ", missing)}");
            }
        }

        private static bool IsEmptyRow(IXLRow row, Dictionary<string, int> headers) =>
            headers.Values.All(column => string.IsNullOrWhiteSpace(ExcelCellText.Get(row.Cell(column))));

        private static void ValidateBandRank(int gradeBand, string? gradeName, string? grade, string? role, string? seniority)
        {
            var err = GradeOptions.GetGradeBandValidationError(gradeBand, gradeName, grade, role, seniority);
            if (err != null)
                throw new InvalidOperationException(err);
        }

        private static void ValidateSlotInstitution(
            int gradeBand,
            int slotIndex,
            string? institutionSymbol,
            IReadOnlySet<string> allowedInstitutionSymbols)
        {
            if (!string.IsNullOrWhiteSpace(institutionSymbol)
                && !allowedInstitutionSymbols.Contains(institutionSymbol.Trim()))
            {
                throw new InvalidOperationException($"סמל מוסד במקטע דרגה{gradeBand}_{slotIndex} אינו שייך למעסיק.");
            }
        }

        private static void WriteHeaders(IXLWorksheet worksheet, string[] columns)
        {
            for (int i = 0; i < columns.Length; i++)
            {
                var cell = worksheet.Cell(1, i + 1);
                cell.Value = columns[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.LightSteelBlue;
            }

            worksheet.Columns().AdjustToContents();
        }

        private void AddGradeDataValidations(
            XLWorkbook workbook,
            IXLWorksheet worksheet,
            string[] columns,
            IReadOnlyList<string> institutionSymbols)
        {
            const int firstDataRow = 2;
            const int lastDataRow = 1000;
            var lookup = workbook.AddWorksheet("GradeLookup");
            lookup.Visibility = XLWorksheetVisibility.VeryHidden;

            lookup.Cell(1, 1).Value = "שם_הדירוג";
            lookup.Cell(1, 2).Value = "טווח_דרגות";
            lookup.Cell(1, 6).Value = "שם_הדירוג";
            lookup.Cell(1, 7).Value = "טווח_תפקידים";

            var row = 2;
            var roleRow = 2;
            var optionIndex = 1;
            foreach (var option in GradeOptions.Options)
            {
                var rangeName = $"GradeOptions{optionIndex}";
                var roleRangeName = $"RoleOptions{optionIndex}";
                lookup.Cell(row, 1).Value = option.Key;
                lookup.Cell(row, 2).Value = rangeName;
                lookup.Cell(roleRow, 6).Value = option.Key;
                lookup.Cell(roleRow, 7).Value = roleRangeName;

                var gradesStartRow = row;
                for (var i = 0; i < option.Value.Length; i++)
                {
                    lookup.Cell(gradesStartRow + i, 4).Value = option.Value[i];
                }

                workbook.DefinedNames.Add(rangeName, lookup.Range(gradesStartRow, 4, gradesStartRow + option.Value.Length - 1, 4));

                var roles = GradeOptions.Roles[option.Key];
                var rolesStartRow = roleRow;
                for (var i = 0; i < roles.Length; i++)
                {
                    lookup.Cell(rolesStartRow + i, 9).Value = roles[i];
                }

                workbook.DefinedNames.Add(roleRangeName, lookup.Range(rolesStartRow, 9, rolesStartRow + roles.Length - 1, 9));
                row += Math.Max(option.Value.Length, 1);
                roleRow += Math.Max(roles.Length, 1);
                optionIndex++;
            }

            var gradeNameRange = lookup.Range(2, 1, 2 + GradeOptions.Options.Count - 1, 1);
            workbook.DefinedNames.Add("GradeNames", gradeNameRange);
            if (institutionSymbols.Count > 0)
            {
                for (var i = 0; i < institutionSymbols.Count; i++)
                {
                    lookup.Cell(2 + i, 11).Value = institutionSymbols[i];
                }
                workbook.DefinedNames.Add("InstitutionSymbols", lookup.Range(2, 11, 1 + institutionSymbols.Count, 11));
            }

            var headerMap = columns
                .Select((name, index) => new { name, column = index + 1 })
                .ToDictionary(x => x.name, x => x.column, StringComparer.Ordinal);

            for (var g = 1; g <= 2; g++)
            {
                var gradeNameHeader = $"דרגה{g}_שם_הדירוג";
                var gradeHeader = $"דרגה{g}_דרגה";
                var roleHeader = $"דרגה{g}_תפקיד";
                var seniorityHeader = $"דרגה{g}_ותק";
                if (headerMap.TryGetValue(gradeNameHeader, out var gradeNameColumn)
                    && headerMap.TryGetValue(gradeHeader, out var gradeColumn)
                    && headerMap.TryGetValue(roleHeader, out var roleColumn)
                    && headerMap.TryGetValue(seniorityHeader, out var seniorityColumn))
                {
                    worksheet.Range(firstDataRow, gradeNameColumn, lastDataRow, gradeNameColumn)
                        .CreateDataValidation()
                        .List("=GradeNames");

                    var gradeNameAddress = worksheet.Cell(firstDataRow, gradeNameColumn).Address.ToStringRelative();
                    worksheet.Range(firstDataRow, gradeColumn, lastDataRow, gradeColumn)
                        .CreateDataValidation()
                        .List($"=INDIRECT(VLOOKUP({gradeNameAddress},GradeLookup!$A$2:$B${row - 1},2,FALSE))");
                    worksheet.Range(firstDataRow, roleColumn, lastDataRow, roleColumn)
                        .CreateDataValidation()
                        .List($"=INDIRECT(VLOOKUP({gradeNameAddress},GradeLookup!$F$2:$G${roleRow - 1},2,FALSE))");

                    var seniorityAddress = worksheet.Cell(firstDataRow, seniorityColumn).Address.ToStringRelative();
                    worksheet.Range(firstDataRow, seniorityColumn, lastDataRow, seniorityColumn)
                        .CreateDataValidation()
                        .Custom($"=OR({seniorityAddress}=\"\",AND(ISNUMBER({seniorityAddress}),{seniorityAddress}>=0))");
                }

                for (var s = 1; s <= 6; s++)
                {
                    var institutionSymbolHeader = $"דרגה{g}_{s}_סמל_מוסד";
                    if (!headerMap.TryGetValue(institutionSymbolHeader, out var institutionSymbolColumn))
                        continue;
                    if (institutionSymbols.Count > 0)
                    {
                        worksheet.Range(firstDataRow, institutionSymbolColumn, lastDataRow, institutionSymbolColumn)
                            .CreateDataValidation()
                            .List("=InstitutionSymbols");
                    }
                }
            }
        }

        private Employer? GetEmployer(int employerId) =>
            _db.Employers.AsNoTracking().FirstOrDefault(e => e.Id == employerId);

        private List<string> GetInstitutionSymbolValues(Employer? employer)
        {
            if (employer == null) return [];
            return _db.EmployerInstitutionSymbols
                .AsNoTracking()
                .Where(s => s.EmployerId == employer.Id)
                .OrderBy(s => s.InstitutionSymbol)
                .Select(s => s.InstitutionSymbol)
                .ToList();
        }

        private async Task<HashSet<string>> GetInstitutionSymbolValuesAsync(Employer employer)
        {
            var values = await _db.EmployerInstitutionSymbols
                .AsNoTracking()
                .Where(s => s.EmployerId == employer.Id)
                .Select(s => s.InstitutionSymbol)
                .ToListAsync();
            return values.ToHashSet(StringComparer.Ordinal);
        }

        private static string? Normalize(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static bool IsFemaleGender(string? gender)
        {
            var g = gender?.Trim();
            return string.Equals(g, "נקבה", StringComparison.Ordinal)
                || string.Equals(g, "female", StringComparison.OrdinalIgnoreCase);
        }

        private static DateOnly? ParseOptionalDate(Dictionary<string, int> headers, IXLRow row, string key)
        {
            if (!headers.TryGetValue(key, out var col))
                return null;
            var cell = row.Cell(col);
            if (cell.IsEmpty() || string.IsNullOrWhiteSpace(ExcelCellText.Get(cell).Trim()))
                return null;
            return ExcelDateParser.TryParse(cell, out var date) ? date : null;
        }

        private static void ApplyEmployeePersonalFields(
            Employee employee,
            Func<string, string> get,
            Dictionary<string, int> headers,
            IXLRow row,
            string firstName,
            string lastName,
            string gender,
            DateOnly birthDate)
        {
            employee.FirstName = firstName;
            employee.LastName = lastName;
            employee.Gender = gender;
            employee.BirthDate = birthDate;
            employee.Phone = Normalize(get("טל"));
            employee.EmployeeNumber = ParseOptionalEmployeeNumber(get);
            employee.ChildBirthDate1 = ParseOptionalDate(headers, row, "תאריך_לידה_ילד_1");
            employee.ChildBirthDate2 = ParseOptionalDate(headers, row, "תאריך_לידה_ילד_2");
            employee.ChildBirthDate3 = ParseOptionalDate(headers, row, "תאריך_לידה_ילד_3");
            employee.ChildBirthDate4 = ParseOptionalDate(headers, row, "תאריך_לידה_ילד_4");
            employee.ChildBirthDate5 = ParseOptionalDate(headers, row, "תאריך_לידה_ילד_5");
            employee.ChildBirthDate6 = ParseOptionalDate(headers, row, "תאריך_לידה_ילד_6");
            employee.ChildBirthDate7 = ParseOptionalDate(headers, row, "תאריך_לידה_ילד_7");
            employee.ChildBirthDate8 = ParseOptionalDate(headers, row, "תאריך_לידה_ילד_8");
            employee.ChildBirthDate9 = ParseOptionalDate(headers, row, "תאריך_לידה_ילד_9");
            employee.ChildBirthDate10 = ParseOptionalDate(headers, row, "תאריך_לידה_ילד_10");
        }

        private static int? ParseOptionalEmployeeNumber(Func<string, string> get)
        {
            var raw = GetFirstPresent(get, "מספר_עובד_בעוקץ", "מספר_עובד");
            if (string.IsNullOrWhiteSpace(raw))
                return null;
            if (!int.TryParse(raw.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var n))
                throw new InvalidOperationException("מספר_עובד_בעוקץ אינו תקין.");
            if (n <= 0)
                throw new InvalidOperationException("מספר_עובד_בעוקץ חייב להיות מספר חיובי.");
            return n;
        }

        private static string GetFirstPresent(Func<string, string> get, params string[] keys)
        {
            foreach (var key in keys)
            {
                var v = get(key);
                if (!string.IsNullOrWhiteSpace(v))
                    return v;
            }

            return string.Empty;
        }

        private async Task<Employer> ResolveEmployerForImportRowAsync(string employerName, string? businessNumber)
        {
            var bn = Normalize(businessNumber);
            if (!string.IsNullOrWhiteSpace(bn))
            {
                var byBn = await _db.Employers
                    .Where(e => e.BusinessNumber == bn)
                    .ToListAsync();
                if (byBn.Count == 0)
                    throw new InvalidOperationException($"מעסיק עם ח.פ. {bn} לא נמצא במערכת.");
                if (byBn.Count > 1)
                    throw new InvalidOperationException($"ח.פ. {bn} משויך ליותר ממעסיק אחד — יש לפנות למנהל המערכת.");
                return byBn[0];
            }

            var name = Normalize(employerName);
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidOperationException("שורה דולגה — שם_מעסיק או ח.פ. נדרש.");

            var matches = await _db.Employers.Where(e => e.Name == name).ToListAsync();
            if (matches.Count == 0)
                throw new InvalidOperationException($"המעסיק \"{name}\" לא נמצא במערכת.");
            if (matches.Count > 1)
                throw new InvalidOperationException(
                    $"שם המעסיק \"{name}\" אינו ייחודי — יש להזין ח.פ. בעמודת \"חפ\" לזיהוי המעסיק.");

            return matches[0];
        }

        private sealed class EmployeeImportRowPlan
        {
            public required string IdNumber { get; init; }
            public required string EmployerName { get; init; }
            public string? EmployerBusinessNumber { get; init; }
            public required string FirstName { get; init; }
            public required string LastName { get; init; }
            public required string Gender { get; init; }
            public required DateOnly BirthDate { get; init; }
            public required string AcademicYear { get; init; }
            public int? EmployeeNumber { get; init; }
            public string? Phone { get; init; }
            public DateOnly? ChildBirthDate1 { get; init; }
            public DateOnly? ChildBirthDate2 { get; init; }
            public DateOnly? ChildBirthDate3 { get; init; }
            public DateOnly? ChildBirthDate4 { get; init; }
            public DateOnly? ChildBirthDate5 { get; init; }
            public DateOnly? ChildBirthDate6 { get; init; }
            public DateOnly? ChildBirthDate7 { get; init; }
            public DateOnly? ChildBirthDate8 { get; init; }
            public DateOnly? ChildBirthDate9 { get; init; }
            public DateOnly? ChildBirthDate10 { get; init; }
            public Employer Employer { get; set; } = null!;
            public Employee? ExistingEmployee { get; set; }
            public required ParsedEmploymentPlan Employment { get; init; }
        }

        private sealed class ParsedEmploymentPlan
        {
            public decimal? Grade1Total { get; set; }
            public decimal? Grade1JobPercent { get; set; }
            public decimal? Grade1TrainingFundPercent { get; set; }
            public decimal? Grade1AgeHours { get; set; }
            public decimal? Grade1MotherBenefitPercent { get; set; }
            public decimal? Grade1TrainingBenefits { get; init; }
            public decimal? Grade1DoubleDegree { get; init; }
            public decimal? Grade2Total { get; set; }
            public decimal? Grade2JobPercent { get; set; }
            public decimal? Grade2TrainingFundPercent { get; set; }
            public decimal? Grade2AgeHours { get; set; }
            public decimal? Grade2MotherBenefitPercent { get; set; }
            public decimal? Grade2TrainingBenefits { get; init; }
            public decimal? Grade2DoubleDegree { get; init; }
            public string? Grade1GradeName { get; init; }
            public string? Grade1Grade { get; init; }
            public string? Grade1Role { get; init; }
            public string? Grade1Seniority { get; init; }
            public string? Grade2GradeName { get; init; }
            public string? Grade2Grade { get; init; }
            public string? Grade2Role { get; init; }
            public string? Grade2Seniority { get; init; }
            public List<ParsedEmploymentSlotPlan> Slots { get; } = [];
        }

        private sealed class ParsedEmploymentSlotPlan
        {
            public byte GradeBand { get; init; }
            public byte SlotIndex { get; init; }
            public string? InstitutionSymbol { get; set; }
            public decimal? WeeklyHours { get; set; }
            public decimal? JobBase { get; set; }
            public byte? SupplementaryParentSlotIndex { get; set; }
        }

        private EmployeeImportRowPlan ParseEmployeeImportRow(
            Func<string, string> get,
            Func<string, decimal?> getNum,
            Func<string, string, decimal?> getNumOrLegacy,
            Dictionary<string, int> headers,
            IXLRow row,
            string? fixedEmployerName,
            ImportRowResult rowResult)
        {
            rowResult.IdNumber = get("תז");
            rowResult.EmployerName = fixedEmployerName ?? get("שם_מעסיק");
            var firstName = Normalize(get("שם_פרטי"));
            var lastName = Normalize(get("שם_משפחה"));
            var gender = Normalize(get("מין"));
            var birthCell = row.Cell(headers["תאריך_לידה"]);

            if (string.IsNullOrWhiteSpace(rowResult.IdNumber))
                throw new InvalidOperationException("שורה דולגה — תז ריק.");
            if (string.IsNullOrWhiteSpace(lastName))
                throw new InvalidOperationException("שורה דולגה — שם_משפחה ריק.");
            if (string.IsNullOrWhiteSpace(firstName))
                throw new InvalidOperationException("שורה דולגה — שם_פרטי ריק.");
            if (string.IsNullOrWhiteSpace(gender))
                throw new InvalidOperationException("שורה דולגה — מין ריק.");
            if (birthCell.IsEmpty() || string.IsNullOrWhiteSpace(ExcelCellText.Get(birthCell).Trim()))
                throw new InvalidOperationException("שורה דולגה — תאריך_לידה ריק.");
            if (!ExcelDateParser.TryParse(birthCell, out var birthDate))
                throw new InvalidOperationException("תאריך_לידה לא תקין.");

            var employerBusinessNumber = Normalize(get("חפ"));
            if (string.IsNullOrWhiteSpace(rowResult.EmployerName) && string.IsNullOrWhiteSpace(employerBusinessNumber))
                throw new InvalidOperationException("שורה דולגה — שם_מעסיק או ח.פ. נדרש.");

            if (!HebrewAcademicYear.TryValidateAndCanonicalize(get("שנת_לימודים"), out var academicYear))
                throw new InvalidOperationException("שנת_לימודים חסרה או לא תקינה. יש להזין שנה עברית, למשל תשפ\"ו.");

            var employeeNumber = ParseOptionalEmployeeNumber(get);

            string? BandField(int g, string suffix)
            {
                var direct = $"דרגה{g}_{suffix}";
                if (headers.ContainsKey(direct))
                    return Normalize(get(direct));
                for (var si = 1; si <= 6; si++)
                {
                    var legacy = $"דרגה{g}_{si}_{suffix}";
                    if (!headers.ContainsKey(legacy)) continue;
                    var v = Normalize(get(legacy));
                    if (!string.IsNullOrWhiteSpace(v)) return v;
                }

                return null;
            }

            var employment = new ParsedEmploymentPlan
            {
                Grade1AgeHours = getNum("דרגה1_שעות_גיל"),
                Grade1TrainingBenefits = getNumOrLegacy("דרגה1_גמולי_השתלמות", "גמולי_השתלמות"),
                Grade1DoubleDegree = getNumOrLegacy("דרגה1_כפל_תואר", "כפל_תואר"),
                Grade2AgeHours = getNum("דרגה2_שעות_גיל"),
                Grade2TrainingBenefits = getNum("דרגה2_גמולי_השתלמות"),
                Grade2DoubleDegree = getNum("דרגה2_כפל_תואר"),
                Grade1GradeName = GradeOptions.NormalizeGradeName(BandField(1, "שם_הדירוג")),
                Grade1Grade = BandField(1, "דרגה"),
                Grade1Role = BandField(1, "תפקיד"),
                Grade1Seniority = BandField(1, "ותק"),
                Grade2GradeName = GradeOptions.NormalizeGradeName(BandField(2, "שם_הדירוג")),
                Grade2Grade = BandField(2, "דרגה"),
                Grade2Role = BandField(2, "תפקיד"),
                Grade2Seniority = BandField(2, "ותק"),
            };

            ValidateBandRank(1, employment.Grade1GradeName, employment.Grade1Grade, employment.Grade1Role, employment.Grade1Seniority);
            ValidateBandRank(2, employment.Grade2GradeName, employment.Grade2Grade, employment.Grade2Role, employment.Grade2Seniority);

            for (var g = 1; g <= 2; g++)
            for (var s = 1; s <= 6; s++)
            {
                string K(string p) => $"דרגה{g}_{s}_{p}";
                employment.Slots.Add(new ParsedEmploymentSlotPlan
                {
                    GradeBand = (byte)g,
                    SlotIndex = (byte)s,
                    InstitutionSymbol = Normalize(get(K("סמל_מוסד"))),
                    WeeklyHours = getNum(K("שעות_שבועיות")),
                    JobBase = getNum(K("בסיס_משרה")),
                });
            }

            return new EmployeeImportRowPlan
            {
                IdNumber = rowResult.IdNumber,
                EmployerName = rowResult.EmployerName,
                EmployerBusinessNumber = employerBusinessNumber,
                FirstName = firstName!,
                LastName = lastName!,
                Gender = gender!,
                BirthDate = birthDate,
                AcademicYear = academicYear,
                EmployeeNumber = employeeNumber,
                Phone = Normalize(get("טל")),
                ChildBirthDate1 = ParseOptionalDate(headers, row, "תאריך_לידה_ילד_1"),
                ChildBirthDate2 = ParseOptionalDate(headers, row, "תאריך_לידה_ילד_2"),
                ChildBirthDate3 = ParseOptionalDate(headers, row, "תאריך_לידה_ילד_3"),
                ChildBirthDate4 = ParseOptionalDate(headers, row, "תאריך_לידה_ילד_4"),
                ChildBirthDate5 = ParseOptionalDate(headers, row, "תאריך_לידה_ילד_5"),
                ChildBirthDate6 = ParseOptionalDate(headers, row, "תאריך_לידה_ילד_6"),
                ChildBirthDate7 = ParseOptionalDate(headers, row, "תאריך_לידה_ילד_7"),
                ChildBirthDate8 = ParseOptionalDate(headers, row, "תאריך_לידה_ילד_8"),
                ChildBirthDate9 = ParseOptionalDate(headers, row, "תאריך_לידה_ילד_9"),
                ChildBirthDate10 = ParseOptionalDate(headers, row, "תאריך_לידה_ילד_10"),
                Employment = employment,
            };
        }

        private async Task ValidateEmployeeImportRowAsync(
            EmployeeImportRowPlan plan,
            Employer? fixedEmployer,
            HashSet<(string IdNumber, int EmployerId, string AcademicYear)> localEmploymentKeys,
            Dictionary<(int EmployerId, string IdNumber), Employee> localEmployeesByEmployerAndTz)
        {
            plan.Employer = fixedEmployer ?? await ResolveEmployerForImportRowAsync(plan.EmployerName, plan.EmployerBusinessNumber);

            var allowedInstitutionSymbols = await GetInstitutionSymbolValuesAsync(plan.Employer);
            foreach (var slot in plan.Employment.Slots)
            {
                ValidateSlotInstitution(
                    slot.GradeBand,
                    slot.SlotIndex,
                    slot.InstitutionSymbol,
                    allowedInstitutionSymbols);
            }

            var employmentKey = (plan.IdNumber, plan.Employer.Id, plan.AcademicYear);
            if (localEmploymentKeys.Contains(employmentKey))
                throw new InvalidOperationException($"כבר קיימת בקובץ רשומה לעובד זה, מעסיק זה ושנת הלימודים {plan.AcademicYear}.");

            var employeeKey = (plan.Employer.Id, plan.IdNumber);
            if (localEmployeesByEmployerAndTz.TryGetValue(employeeKey, out var cachedEmployee))
            {
                plan.ExistingEmployee = cachedEmployee;
            }
            else
            {
                plan.ExistingEmployee = await _db.Employees.FirstOrDefaultAsync(e =>
                    e.EmployerId == plan.Employer.Id && e.IdNumber == plan.IdNumber);
                if (plan.ExistingEmployee == null)
                {
                    plan.ExistingEmployee = await _db.Employees.IgnoreQueryFilters()
                        .Where(e => e.EmployerId == plan.Employer.Id && e.IdNumber == plan.IdNumber && e.IsDeleted)
                        .OrderBy(e => e.Id)
                        .FirstOrDefaultAsync();
                }
            }

            if (plan.ExistingEmployee != null)
            {
                if (plan.ExistingEmployee.EmployerId != plan.Employer.Id)
                    throw new InvalidOperationException("רשומת העובד אינה משויכת למעסיק של שורת הייבוא.");

                if (plan.ExistingEmployee.Id > 0
                    && await _db.EmploymentData.AnyAsync(ed =>
                        ed.EmployeeId == plan.ExistingEmployee.Id
                        && ed.EmployerId == plan.Employer.Id
                        && ed.AcademicYear == plan.AcademicYear))
                {
                    throw new InvalidOperationException(
                        $"כבר קיימת רשומה לעובד זה, מעסיק זה ושנת הלימודים {plan.AcademicYear}.");
                }
            }
        }

        private void ApplyEmployeeImportRow(
            EmployeeImportRowPlan plan,
            Dictionary<(int EmployerId, string IdNumber), Employee> localEmployeesByEmployerAndTz,
            HashSet<(string IdNumber, int EmployerId, string AcademicYear)> localEmploymentKeys)
        {
            var employeeKey = (plan.Employer.Id, plan.IdNumber);
            Employee employee;
            if (plan.ExistingEmployee != null)
            {
                employee = plan.ExistingEmployee;
                if (employee.IsDeleted)
                {
                    employee.IsDeleted = false;
                    employee.DeletedAtUtc = null;
                }
            }
            else if (localEmployeesByEmployerAndTz.TryGetValue(employeeKey, out var cachedEmployee))
            {
                employee = cachedEmployee;
            }
            else
            {
                employee = _db.Employees.Local.FirstOrDefault(e =>
                    e.EmployerId == plan.Employer.Id && e.IdNumber == plan.IdNumber && e.Id == 0);
                if (employee == null)
                {
                    employee = new Employee
                    {
                        EmployerId = plan.Employer.Id,
                        IdNumber = plan.IdNumber,
                    };
                    _db.Employees.Add(employee);
                }

                localEmployeesByEmployerAndTz[employeeKey] = employee;
            }

            employee.FirstName = plan.FirstName;
            employee.LastName = plan.LastName;
            employee.Gender = plan.Gender;
            employee.BirthDate = plan.BirthDate;
            employee.Phone = plan.Phone;
            employee.EmployeeNumber = plan.EmployeeNumber;
            employee.ChildBirthDate1 = plan.ChildBirthDate1;
            employee.ChildBirthDate2 = plan.ChildBirthDate2;
            employee.ChildBirthDate3 = plan.ChildBirthDate3;
            employee.ChildBirthDate4 = plan.ChildBirthDate4;
            employee.ChildBirthDate5 = plan.ChildBirthDate5;
            employee.ChildBirthDate6 = plan.ChildBirthDate6;
            employee.ChildBirthDate7 = plan.ChildBirthDate7;
            employee.ChildBirthDate8 = plan.ChildBirthDate8;
            employee.ChildBirthDate9 = plan.ChildBirthDate9;
            employee.ChildBirthDate10 = plan.ChildBirthDate10;
            localEmployeesByEmployerAndTz[employeeKey] = employee;

            var ed = new EmploymentData
            {
                Employee = employee,
                EmployerId = plan.Employer.Id,
                AcademicYear = plan.AcademicYear,
                Grade1Total = plan.Employment.Grade1Total,
                Grade1JobPercent = plan.Employment.Grade1JobPercent,
                Grade1TrainingFundPercent = plan.Employment.Grade1TrainingFundPercent,
                Grade1AgeHours = plan.Employment.Grade1AgeHours,
                Grade1MotherBenefitPercent = plan.Employment.Grade1MotherBenefitPercent,
                Grade1TrainingBenefits = plan.Employment.Grade1TrainingBenefits,
                Grade1DoubleDegree = plan.Employment.Grade1DoubleDegree,
                Grade2Total = plan.Employment.Grade2Total,
                Grade2JobPercent = plan.Employment.Grade2JobPercent,
                Grade2TrainingFundPercent = plan.Employment.Grade2TrainingFundPercent,
                Grade2AgeHours = plan.Employment.Grade2AgeHours,
                Grade2MotherBenefitPercent = plan.Employment.Grade2MotherBenefitPercent,
                Grade2TrainingBenefits = plan.Employment.Grade2TrainingBenefits,
                Grade2DoubleDegree = plan.Employment.Grade2DoubleDegree,
                Grade1GradeName = plan.Employment.Grade1GradeName,
                Grade1Grade = plan.Employment.Grade1Grade,
                Grade1Role = plan.Employment.Grade1Role,
                Grade1Seniority = plan.Employment.Grade1Seniority,
                Grade2GradeName = plan.Employment.Grade2GradeName,
                Grade2Grade = plan.Employment.Grade2Grade,
                Grade2Role = plan.Employment.Grade2Role,
                Grade2Seniority = plan.Employment.Grade2Seniority,
            };

            foreach (var slotPlan in plan.Employment.Slots)
            {
                var slot = new EmploymentDataSlot
                {
                    GradeBand = slotPlan.GradeBand,
                    SlotIndex = slotPlan.SlotIndex,
                    InstitutionSymbol = slotPlan.InstitutionSymbol,
                    WeeklyHours = slotPlan.WeeklyHours,
                    JobBase = slotPlan.JobBase,
                    SupplementaryParentSlotIndex = slotPlan.SupplementaryParentSlotIndex,
                };
                if (EmploymentSlotPersistence.ShouldPersistSlot(slot))
                    ed.Slots.Add(slot);
            }

            _db.EmploymentData.Add(ed);
            localEmploymentKeys.Add((plan.IdNumber, plan.Employer.Id, plan.AcademicYear));
        }

        private void RecalculateEmploymentImportPlan(EmployeeImportRowPlan plan)
        {
            var dto = ToEmploymentDataDto(plan);
            _calculations.PrepareForSave(
                dto,
                plan.BirthDate,
                IsFemaleGender(plan.Gender),
                GetChildBirthDates(plan));
            ApplyCalculatedValues(plan.Employment, dto);
        }

        private static EmploymentDataDto ToEmploymentDataDto(EmployeeImportRowPlan plan)
        {
            var employment = plan.Employment;
            return new EmploymentDataDto
            {
                AcademicYear = plan.AcademicYear,
                Grade1AgeHours = employment.Grade1AgeHours,
                Grade1TrainingBenefits = employment.Grade1TrainingBenefits,
                Grade1DoubleDegree = employment.Grade1DoubleDegree,
                Grade2AgeHours = employment.Grade2AgeHours,
                Grade2TrainingBenefits = employment.Grade2TrainingBenefits,
                Grade2DoubleDegree = employment.Grade2DoubleDegree,
                Grade1GradeName = employment.Grade1GradeName,
                Grade1Grade = employment.Grade1Grade,
                Grade1Role = employment.Grade1Role,
                Grade1Seniority = employment.Grade1Seniority,
                Grade2GradeName = employment.Grade2GradeName,
                Grade2Grade = employment.Grade2Grade,
                Grade2Role = employment.Grade2Role,
                Grade2Seniority = employment.Grade2Seniority,
                Slots = employment.Slots
                    .Select(s => new EmploymentDataSlotDto
                    {
                        GradeBand = s.GradeBand,
                        SlotIndex = s.SlotIndex,
                        InstitutionSymbol = s.InstitutionSymbol,
                        WeeklyHours = s.WeeklyHours,
                        JobBase = s.JobBase,
                        SupplementaryParentSlotIndex = s.SupplementaryParentSlotIndex,
                    })
                    .ToList(),
            };
        }

        private static void ApplyCalculatedValues(ParsedEmploymentPlan employment, EmploymentDataDto dto)
        {
            employment.Grade1Total = dto.Grade1Total;
            employment.Grade1JobPercent = dto.Grade1JobPercent;
            employment.Grade1TrainingFundPercent = dto.Grade1TrainingFundPercent;
            employment.Grade1AgeHours = dto.Grade1AgeHours;
            employment.Grade1MotherBenefitPercent = dto.Grade1MotherBenefitPercent;
            employment.Grade2Total = dto.Grade2Total;
            employment.Grade2JobPercent = dto.Grade2JobPercent;
            employment.Grade2TrainingFundPercent = dto.Grade2TrainingFundPercent;
            employment.Grade2AgeHours = dto.Grade2AgeHours;
            employment.Grade2MotherBenefitPercent = dto.Grade2MotherBenefitPercent;

            employment.Slots.Clear();
            foreach (var slotDto in dto.Slots ?? [])
            {
                employment.Slots.Add(new ParsedEmploymentSlotPlan
                {
                    GradeBand = (byte)slotDto.GradeBand,
                    SlotIndex = (byte)slotDto.SlotIndex,
                    InstitutionSymbol = slotDto.InstitutionSymbol,
                    WeeklyHours = slotDto.WeeklyHours,
                    JobBase = slotDto.JobBase,
                    SupplementaryParentSlotIndex = slotDto.SupplementaryParentSlotIndex is >= 1 and <= 5
                        ? (byte?)slotDto.SupplementaryParentSlotIndex
                        : null,
                });
            }
        }

        private static DateOnly?[] GetChildBirthDates(EmployeeImportRowPlan plan) =>
        [
            plan.ChildBirthDate1,
            plan.ChildBirthDate2,
            plan.ChildBirthDate3,
            plan.ChildBirthDate4,
            plan.ChildBirthDate5,
            plan.ChildBirthDate6,
            plan.ChildBirthDate7,
            plan.ChildBirthDate8,
            plan.ChildBirthDate9,
            plan.ChildBirthDate10,
        ];

        private static XLWorkbook OpenWorkbookOrThrow(IFormFile file)
        {
            try
            {
                return new XLWorkbook(file.OpenReadStream());
            }
            catch (Exception ex) when (
                ex is InvalidDataException ||
                ex is IOException ||
                ex is FormatException ||
                ex is ArgumentException)
            {
                throw new InvalidOperationException("קובץ ה-Excel פגום או לא תקין. יש להעלות קובץ .xlsx תקין.", ex);
            }
        }
    }
}
