using AccountingProject.Data;
using AccountingProject.Domain;
using AccountingProject.Models;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace AccountingProject.Services
{
    public interface IReportExportService
    {
        Task<byte[]> KindergartenAnnualAsync(int employerId, string academicYear);
        Task<byte[]> SchoolAnnualAsync(int employerId, string academicYear);
        Task<byte[]> MonthlyComparisonAsync(int employerId, string academicYear, int month, Stream uploadedFile);
        Task<byte[]> AnnualComparisonAsync(int employerId, string academicYear, Stream uploadedFile);
        Task<byte[]> AnnualComparisonFromSavedDataAsync(int employerId, string academicYear);
        Task<byte[]> InstitutionHoursAsync(int employerId, string academicYear, string institutionSymbol);
        Task<byte[]> EmployeesPersonalAsync(int employerId);
        Task<byte[]> EmployeesEmploymentDataAsync(int employerId, string academicYear);
    }

    public sealed class ReportExportService : IReportExportService
    {
        /// <summary>Sentinel for institution-hours report: all symbols for the employer.</summary>
        public const string InstitutionHoursAllSymbols = "*";

        private readonly PayrollDbContext _db;
        private readonly IAnnualComparisonSavedReportService _annualSavedReport;

        public ReportExportService(
            PayrollDbContext db,
            IAnnualComparisonSavedReportService? annualSavedReport = null)
        {
            _db = db;
            _annualSavedReport = annualSavedReport ?? new AnnualComparisonSavedReportService(db);
        }

        // ─────────────────────────────────────────────────────────────────
        //  Parse Hebrew academic year string (e.g. תשפ"ו) to September Gregorian year (e.g. 2025).
        //  Strips punctuation, sums letter values from thousands-less representation (mod 1000),
        //  adds 5000 for the current millennium, then subtracts 3761 for September Gregorian year.
        // ─────────────────────────────────────────────────────────────────
        private static readonly Dictionary<char, int> HebrewLetterValues = new()
        {
            ['ת'] = 400, ['ש'] = 300, ['ר'] = 200, ['ק'] = 100,
            ['צ'] = 90,  ['פ'] = 80,  ['ע'] = 70,  ['ס'] = 60,
            ['נ'] = 50,  ['מ'] = 40,  ['ל'] = 30,  ['כ'] = 20,
            ['י'] = 10,  ['ט'] = 9,   ['ח'] = 8,   ['ז'] = 7,
            ['ו'] = 6,   ['ה'] = 5,   ['ד'] = 4,   ['ג'] = 3,
            ['ב'] = 2,   ['א'] = 1
        };

        private static int ParseSeptemberGregorianYear(string hebrewYear)
        {
            var sum = hebrewYear
                .Where(c => HebrewLetterValues.ContainsKey(c))
                .Sum(c => HebrewLetterValues[c]);
            var hebrewYearFull = 5000 + sum;
            return hebrewYearFull - 3761;
        }

        private static readonly string[] AnnualRosterHeaders =
        [
            "סמל מוסד", "שם", "ת.ז.", "טלפון", "תפקיד", "דרגה", "ותק",
            "שעות גיל", "השתל'", "כפל תואר", "בסיס משרה", "שעות שבועיות", "חינוך", "סה\"כ"
        ];

        private sealed record AnnualRosterRow(string Symbol, EmploymentData Ed, EmploymentDataSlot Slot);

        private static string FormatDate(DateOnly? d) =>
            d.HasValue ? d.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : "";

        private static DateOnly?[] GetChildBirthDates(Employee emp) =>
        [
            emp.ChildBirthDate1, emp.ChildBirthDate2, emp.ChildBirthDate3, emp.ChildBirthDate4,
            emp.ChildBirthDate5, emp.ChildBirthDate6, emp.ChildBirthDate7, emp.ChildBirthDate8,
            emp.ChildBirthDate9, emp.ChildBirthDate10
        ];

        private static byte[] ToBytes(XLWorkbook wb)
        {
            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }

        private static void SetHeaders(IXLWorksheet ws, string[] headers)
        {
            for (var i = 0; i < headers.Length; i++)
                ws.Cell(1, i + 1).Value = headers[i];
            ws.Row(1).Style.Font.Bold = true;
        }

        private static void BoldAndFit(IXLWorksheet ws, int colCount, int rowCount)
        {
            ws.Columns(1, colCount).AdjustToContents(1, Math.Max(rowCount, 2));
        }

        private static void SetDec(IXLWorksheet ws, int row, int col, decimal? value)
        {
            if (value.HasValue) ws.Cell(row, col).Value = value.Value;
        }

        // ─────────────────────────────────────────────────────────────────
        //  Report 1 — מצבת גנים שנתי (לפי סוג מוסד = גן)
        // ─────────────────────────────────────────────────────────────────
        public Task<byte[]> KindergartenAnnualAsync(int employerId, string academicYear) =>
            BuildAnnualRosterByInstitutionTypeAsync(employerId, RequireCanonicalAcademicYear(academicYear), InstitutionTypes.Kindergarten, "מצבת גנים");

        // ─────────────────────────────────────────────────────────────────
        //  Report 2 — מצבת בית ספר שנתי (לפי סוג מוסד = בית ספר)
        // ─────────────────────────────────────────────────────────────────
        public Task<byte[]> SchoolAnnualAsync(int employerId, string academicYear) =>
            BuildAnnualRosterByInstitutionTypeAsync(employerId, RequireCanonicalAcademicYear(academicYear), InstitutionTypes.School, "מצבת בית ספר");

        // ─────────────────────────────────────────────────────────────────
        //  Report 3 — דוח השוואה לפי חודש מסוים (V/X מול קובץ עוקץ)
        // ─────────────────────────────────────────────────────────────────
        public async Task<byte[]> MonthlyComparisonAsync(int employerId, string academicYear, int month, Stream uploadedFile)
        {
            var canonicalYear = RequireCanonicalAcademicYear(academicYear);
            var records = await GetEmploymentDataWithSlots(employerId, canonicalYear);
            return MonthlyComparisonReportBuilder.Build(
                records, canonicalYear, month, uploadedFile, ParseSeptemberGregorianYear);
        }

        // ─────────────────────────────────────────────────────────────────
        //  Report 4 — דוח השוואה שנתי (V / פירוט פערים לכל חודש)
        // ─────────────────────────────────────────────────────────────────
        public async Task<byte[]> AnnualComparisonAsync(int employerId, string academicYear, Stream uploadedFile)
        {
            var canonicalYear = RequireCanonicalAcademicYear(academicYear);
            var records = await GetEmploymentDataWithSlots(employerId, canonicalYear);
            return AnnualComparisonReportBuilder.Build(
                records, canonicalYear, uploadedFile, ParseSeptemberGregorianYear);
        }

        public async Task<byte[]> AnnualComparisonFromSavedDataAsync(int employerId, string academicYear)
        {
            var canonicalYear = RequireCanonicalAcademicYear(academicYear);
            if (!await _db.Employers.AnyAsync(e => e.Id == employerId))
                throw new InvalidOperationException("המעסיק לא נמצא במערכת.");

            var records = await GetEmploymentDataWithSlots(employerId, canonicalYear);
            if (!HebrewAcademicYear.TryParseSeptemberGregorianYear(canonicalYear, out var sepYear))
                throw new InvalidOperationException(HebrewAcademicYear.InvalidMessage);

            var monthSequence = SchoolYearGregorian.GetSchoolYearMonthSequence(sepYear);
            var (activeBatches, comparisonRows) = await LoadSavedAnnualComparisonInputAsync(employerId, canonicalYear);
            var overrides = await _annualSavedReport.LoadOverridesBySlotAsync(employerId, canonicalYear);

            return AnnualComparisonReportBuilder.BuildFromSavedData(
                records,
                monthSequence,
                activeBatches,
                comparisonRows,
                overrides);
        }

        // ─────────────────────────────────────────────────────────────────
        //  Report 5 — בדיקת שעות לסמל (השוואה: תקן / מצבת / הפרש)
        // ─────────────────────────────────────────────────────────────────
        public async Task<byte[]> InstitutionHoursAsync(int employerId, string academicYear, string institutionSymbol)
        {
            const decimal RequiredTeacherHours = 34.5m;
            const decimal RequiredAssistantHours = 34.5m;
            const decimal RequiredSecondAssistantHours = 40m;
            const string SheetName = "בדיקת שעות לסמל";
            string[] headers =
            [
                "סמל מוסד",
                "מס' שעות גננת סה\"כ",
                "מס' שעות סייעת סה\"כ",
                "סייעת שניה",
            ];

            var canonicalYear = RequireCanonicalAcademicYear(academicYear);
            var records = await GetEmploymentDataWithSlots(employerId, canonicalYear);
            var symbols = IsInstitutionHoursAllSymbols(institutionSymbol)
                ? await ResolveInstitutionHoursSymbolsAsync(employerId, records)
                : [institutionSymbol.Trim()];

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(SheetName);
            SetHeaders(ws, headers);

            var row = 2;
            foreach (var sym in symbols)
            {
                var (actualGannet, actualAssistant, actualSecondAssistant) =
                    InstitutionHoursComputeRoster(records, sym);
                row = InstitutionHoursWriteSymbolBlock(
                    ws,
                    row,
                    sym,
                    actualGannet,
                    actualAssistant,
                    actualSecondAssistant,
                    RequiredTeacherHours,
                    RequiredAssistantHours,
                    RequiredSecondAssistantHours,
                    headers.Length);
            }

            BoldAndFit(ws, headers.Length, Math.Max(row - 1, 1));
            return ToBytes(wb);
        }

        public static bool IsInstitutionHoursAllSymbols(string? institutionSymbol) =>
            string.Equals(institutionSymbol?.Trim(), InstitutionHoursAllSymbols, StringComparison.Ordinal);

        private async Task<List<string>> ResolveInstitutionHoursSymbolsAsync(
            int employerId,
            IReadOnlyList<Models.EmploymentData> records)
        {
            var fromDb = await _db.EmployerInstitutionSymbols
                .Where(s => s.EmployerId == employerId)
                .Select(s => s.InstitutionSymbol)
                .ToListAsync();

            var fromSlots = records
                .SelectMany(ed => ed.Slots)
                .Select(s => s.InstitutionSymbol?.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s));

            return fromDb
                .Concat(fromSlots)
                .Select(s => s!.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(s => s, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static (decimal Gannet, decimal Assistant, decimal SecondAssistant) InstitutionHoursComputeRoster(
            IReadOnlyList<Models.EmploymentData> records,
            string institutionSymbol)
        {
            decimal actualGannet = 0;
            decimal actualAssistant = 0;
            decimal actualSecondAssistant = 0;

            foreach (var ed in records)
            {
                foreach (var slot in ed.Slots)
                {
                    if (!InstitutionHoursSlotMatches(slot, institutionSymbol))
                        continue;
                    if (InstitutionHoursSlotIsEmpty(slot))
                        continue;

                    var role = InstitutionHoursRoleForSlot(ed, slot);
                    if (role == null)
                        continue;

                    var hours = slot.WeeklyHours ?? 0;
                    InstitutionHoursAddByRole(role, hours, ref actualGannet, ref actualAssistant, ref actualSecondAssistant);
                }
            }

            return (actualGannet, actualAssistant, actualSecondAssistant);
        }

        private static int InstitutionHoursWriteSymbolBlock(
            IXLWorksheet ws,
            int row,
            string institutionSymbol,
            decimal actualGannet,
            decimal actualAssistant,
            decimal actualSecondAssistant,
            decimal requiredTeacherHours,
            decimal requiredAssistantHours,
            decimal requiredSecondAssistantHours,
            int columnCount)
        {
            ws.Cell(row, 1).Value = institutionSymbol;
            ws.Cell(row, 2).Value = requiredTeacherHours;
            ws.Cell(row, 3).Value = requiredAssistantHours;
            ws.Cell(row, 4).Value = requiredSecondAssistantHours;
            row++;

            ws.Cell(row, 1).Value = "מצבת";
            ws.Cell(row, 2).Value = actualGannet;
            ws.Cell(row, 3).Value = actualAssistant;
            ws.Cell(row, 4).Value = actualSecondAssistant;
            row++;

            ws.Cell(row, 1).Value = "הפרש";
            var diffGannet = requiredTeacherHours - actualGannet;
            var diffAssistant = requiredAssistantHours - actualAssistant;
            var diffSecond = requiredSecondAssistantHours - actualSecondAssistant;
            ws.Cell(row, 2).Value = diffGannet;
            ws.Cell(row, 3).Value = diffAssistant;
            ws.Cell(row, 4).Value = diffSecond;
            var allDifferencesZero = diffGannet == 0 && diffAssistant == 0 && diffSecond == 0;
            ws.Range(row, 1, row, columnCount).Style.Fill.BackgroundColor =
                allDifferencesZero ? XLColor.LightGreen : XLColor.LightPink;
            return row + 1;
        }

        private static bool InstitutionHoursSlotMatches(Models.EmploymentDataSlot slot, string institutionSymbol) =>
            string.Equals(slot.InstitutionSymbol?.Trim(), institutionSymbol.Trim(), StringComparison.OrdinalIgnoreCase);

        private static bool InstitutionHoursSlotIsEmpty(Models.EmploymentDataSlot slot) =>
            string.IsNullOrWhiteSpace(slot.InstitutionSymbol)
            && (!slot.WeeklyHours.HasValue || slot.WeeklyHours.Value == 0);

        private static string? InstitutionHoursRoleForSlot(Models.EmploymentData ed, Models.EmploymentDataSlot slot) =>
            slot.GradeBand switch
            {
                1 => ed.Grade1Role,
                2 => ed.Grade2Role,
                _ => null,
            };

        private static void InstitutionHoursAddByRole(
            string role,
            decimal hours,
            ref decimal gannet,
            ref decimal assistant,
            ref decimal secondAssistant)
        {
            if (InstitutionHoursIsSecondAssistantRole(role))
            {
                secondAssistant += hours;
                return;
            }

            if (role.Contains("גננת", StringComparison.OrdinalIgnoreCase))
            {
                gannet += hours;
                return;
            }

            if (role.Contains("סייעת", StringComparison.OrdinalIgnoreCase))
                assistant += hours;
        }

        private static bool InstitutionHoursIsSecondAssistantRole(string role) =>
            role.Contains("סייעת שניה", StringComparison.OrdinalIgnoreCase)
            || role.Contains("סייעת שנייה", StringComparison.OrdinalIgnoreCase);

        // ─────────────────────────────────────────────────────────────────
        //  Report 6 — עובדים אישיים
        // ─────────────────────────────────────────────────────────────────
        public async Task<byte[]> EmployeesPersonalAsync(int employerId)
        {
            var employer = await _db.Employers.FindAsync(employerId);
            var employees = await _db.Employees
                .Where(e => e.EmployerId == employerId && !e.IsDeleted)
                .OrderBy(e => e.LastName).ThenBy(e => e.FirstName)
                .ToListAsync();

            var employeeIdsWithEdList = await _db.EmploymentData
                .Where(e => e.EmployerId == employerId && !e.IsDeleted)
                .Select(e => e.EmployeeId)
                .Distinct()
                .ToListAsync();
            var employeeIdsWithEd = new HashSet<int>(employeeIdsWithEdList);

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("עובדים אישיים");
            var childHeaders = Enumerable.Range(1, 10).Select(n => $"ילד {n}").ToArray();
            string[] headers = ["שם משפחה", "שם פרטי", "ת\"ז", "תאריך לידה", "טלפון", "מין", "מעסיק", "סטטוס", .. childHeaders];
            SetHeaders(ws, headers);

            var r = 2;
            foreach (var emp in employees)
            {
                var active = emp.ManualActiveStatus ?? employeeIdsWithEd.Contains(emp.Id);
                ws.Cell(r, 1).Value = emp.LastName ?? "";
                ws.Cell(r, 2).Value = emp.FirstName ?? "";
                ws.Cell(r, 3).Value = emp.IdNumber;
                ws.Cell(r, 4).Value = FormatDate(emp.BirthDate);
                ws.Cell(r, 5).Value = emp.Phone ?? "";
                ws.Cell(r, 6).Value = emp.Gender ?? "";
                ws.Cell(r, 7).Value = employer?.Name ?? "";
                ws.Cell(r, 8).Value = active ? "פעיל" : "לא פעיל";
                var childDates = GetChildBirthDates(emp);
                for (var i = 0; i < childDates.Length; i++)
                    ws.Cell(r, 9 + i).Value = FormatDate(childDates[i]);
                r++;
            }
            BoldAndFit(ws, headers.Length, r - 1);
            return ToBytes(wb);
        }

        // ─────────────────────────────────────────────────────────────────
        //  Report 7 — עובדים נתוני העסקה (שורה לכל מקטע)
        // ─────────────────────────────────────────────────────────────────
        public async Task<byte[]> EmployeesEmploymentDataAsync(int employerId, string academicYear)
        {
            var canonicalYear = RequireCanonicalAcademicYear(academicYear);
            var records = await GetEmploymentDataWithSlots(employerId, canonicalYear);

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("עובדים נתוני העסקה");
            string[] headers = [
                "שם העובדת", "סמל מוסד", "תפקיד", "ש\"ש", "בסיס משרה",
                "אחוז משרה", "אחוז תוספת אם", "שעות גיל", "מס' גמולים", "כפל תואר",
                "הפרשה לקרן השתלמות", "הכפלה כללית"
            ];
            SetHeaders(ws, headers);

            var r = 2;
            foreach (var ed in records.OrderBy(e => e.Employee?.LastName).ThenBy(e => e.Employee?.FirstName))
            {
                foreach (var slot in ed.Slots
                    .Where(s => !string.IsNullOrWhiteSpace(s.InstitutionSymbol) || s.WeeklyHours is > 0)
                    .OrderBy(s => s.GradeBand).ThenBy(s => s.SlotIndex))
                {
                    var g1 = slot.GradeBand == 1;
                    ws.Cell(r, 1).Value = ed.Employee?.FullName ?? "";
                    ws.Cell(r, 2).Value = slot.InstitutionSymbol ?? "";
                    ws.Cell(r, 3).Value = (g1 ? ed.Grade1Role : ed.Grade2Role) ?? "";
                    SetDec(ws, r, 4, slot.WeeklyHours);
                    SetDec(ws, r, 5, slot.JobBase);
                    SetDec(ws, r, 6, g1 ? ed.Grade1JobPercent : ed.Grade2JobPercent);
                    SetDec(ws, r, 7, g1 ? ed.Grade1MotherBenefitPercent : ed.Grade2MotherBenefitPercent);
                    SetDec(ws, r, 8, g1 ? ed.Grade1AgeHours : ed.Grade2AgeHours);
                    SetDec(ws, r, 9, g1 ? ed.Grade1TrainingBenefits : ed.Grade2TrainingBenefits);
                    SetDec(ws, r, 10, g1 ? ed.Grade1DoubleDegree : ed.Grade2DoubleDegree);
                    SetDec(ws, r, 11, g1 ? ed.Grade1TrainingFundPercent : ed.Grade2TrainingFundPercent);
                    SetDec(ws, r, 12, 0m);
                    r++;
                }
            }
            BoldAndFit(ws, headers.Length, r - 1);
            return ToBytes(wb);
        }

        // ─────────────────────────────────────────────────────────────────
        //  Private — annual roster (גנים / בית ספר) לפי סוג מוסד
        // ─────────────────────────────────────────────────────────────────
        private async Task<byte[]> BuildAnnualRosterByInstitutionTypeAsync(
            int employerId, string academicYear, string institutionType, string sheetName)
        {
            var allowedSymbols = await GetInstitutionSymbolCodesByTypeAsync(employerId, institutionType);
            var records = await GetEmploymentDataWithSlots(employerId, academicYear);

            var rows = new List<AnnualRosterRow>();
            foreach (var ed in records)
            {
                foreach (var slot in ed.Slots
                    .Where(s => !string.IsNullOrWhiteSpace(s.InstitutionSymbol) || s.WeeklyHours is > 0))
                {
                    var sym = slot.InstitutionSymbol?.Trim() ?? "";
                    if (sym.Length == 0 || !allowedSymbols.Contains(sym))
                        continue;
                    rows.Add(new AnnualRosterRow(sym, ed, slot));
                }
            }

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(sheetName);
            SetHeaders(ws, AnnualRosterHeaders);

            var r = 2;
            var isFirstGroup = true;
            foreach (var group in rows
                .GroupBy(row => row.Symbol, StringComparer.OrdinalIgnoreCase)
                .OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase))
            {
                if (!isFirstGroup)
                    r++;
                else
                    isFirstGroup = false;

                foreach (var row in group
                    .OrderBy(x => x.Ed.Employee?.LastName)
                    .ThenBy(x => x.Ed.Employee?.FirstName)
                    .ThenBy(x => x.Ed.Employee?.IdNumber))
                {
                    WriteAnnualRosterRow(ws, r, row);
                    r++;
                }
            }

            BoldAndFit(ws, AnnualRosterHeaders.Length, Math.Max(r - 1, 1));
            return ToBytes(wb);
        }

        private async Task<HashSet<string>> GetInstitutionSymbolCodesByTypeAsync(int employerId, string institutionType)
        {
            var codes = await _db.EmployerInstitutionSymbols
                .AsNoTracking()
                .Where(s => s.EmployerId == employerId && s.InstitutionType == institutionType)
                .Select(s => s.InstitutionSymbol)
                .ToListAsync();
            return codes.ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        private static void WriteAnnualRosterRow(IXLWorksheet ws, int r, AnnualRosterRow row)
        {
            var ed = row.Ed;
            var slot = row.Slot;
            var g1 = slot.GradeBand == 1;
            var g2 = slot.GradeBand == 2;

            ws.Cell(r, 1).Value = row.Symbol;
            ws.Cell(r, 2).Value = ed.Employee?.FullName ?? "";
            ws.Cell(r, 3).Value = ed.Employee?.IdNumber ?? "";
            ws.Cell(r, 4).Value = ed.Employee?.Phone ?? "";

            if (g1 || g2)
            {
                ws.Cell(r, 5).Value = (g1 ? ed.Grade1Role : ed.Grade2Role) ?? "";
                ws.Cell(r, 6).Value = (g1 ? ed.Grade1Grade : ed.Grade2Grade) ?? "";
                ws.Cell(r, 7).Value = (g1 ? ed.Grade1Seniority : ed.Grade2Seniority) ?? "";
                SetDec(ws, r, 8, g1 ? ed.Grade1AgeHours : ed.Grade2AgeHours);
                SetDec(ws, r, 9, g1 ? ed.Grade1TrainingBenefits : ed.Grade2TrainingBenefits);
                SetDec(ws, r, 10, g1 ? ed.Grade1DoubleDegree : ed.Grade2DoubleDegree);
            }

            SetDec(ws, r, 11, slot.JobBase);
            SetDec(ws, r, 12, slot.WeeklyHours);
            // col 13 חינוך — always empty
            SetDec(ws, r, 14, slot.WeeklyHours);
        }

        // ─────────────────────────────────────────────────────────────────
        //  Private — shared DB query
        // ─────────────────────────────────────────────────────────────────
        private async Task<List<Models.EmploymentData>> GetEmploymentDataWithSlots(int employerId, string canonicalAcademicYear)
        {
            var rows = await _db.EmploymentData
                .Include(e => e.Employee)
                .Include(e => e.Employer)
                .Include(e => e.Slots)
                .Where(e => e.EmployerId == employerId && !e.IsDeleted)
                .ToListAsync();

            return rows
                .Where(e => HebrewAcademicYear.CanonicalForComparison(e.AcademicYear) == canonicalAcademicYear)
                .ToList();
        }

        private static string RequireCanonicalAcademicYear(string academicYear)
        {
            if (!HebrewAcademicYear.TryValidateAndCanonicalize(academicYear, out var canonical))
                throw new InvalidOperationException(HebrewAcademicYear.InvalidMessage);
            return canonical;
        }

        private async Task<(List<PayrollMonthlyInputBatch> ActiveBatches, List<PayrollComparisonInputRow> ComparisonRows)>
            LoadSavedAnnualComparisonInputAsync(int employerId, string canonYear)
        {
            var activeBatches = await _db.PayrollMonthlyInputBatches
                .AsNoTracking()
                .Where(b =>
                    b.EmployerId == employerId
                    && b.IsActive
                    && !b.IsDeleted)
                .ToListAsync();

            activeBatches = activeBatches
                .Where(b => PayrollComparisonUploadSupport.CanonAcademicYear(b.AcademicYear) == canonYear)
                .ToList();

            if (activeBatches.Count == 0)
                return ([], []);

            var batchIds = activeBatches.Select(b => b.Id).ToList();
            var entityRows = await _db.PayrollMonthlyInputRows
                .AsNoTracking()
                .Where(r =>
                    batchIds.Contains(r.BatchId)
                    && r.EmployerId == employerId
                    && !r.IsDeleted)
                .ToListAsync();

            var comparisonRows = entityRows
                .Select(AnnualComparisonSavedRowMapper.ToComparisonInput)
                .ToList();

            return (activeBatches, comparisonRows);
        }
    }
}
