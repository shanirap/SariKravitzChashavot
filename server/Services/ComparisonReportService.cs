using System.Globalization;
using System.Text;
using AccountingProject.Contracts;
using AccountingProject.Data;
using AccountingProject.Domain;
using AccountingProject.Infrastructure;
using AccountingProject.Models;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;

namespace AccountingProject.Services
{
    public class ComparisonReportService : IComparisonReportService
    {
        private const decimal NumericTolerance = 0.01m;
        private readonly PayrollDbContext _db;

        public ComparisonReportService(PayrollDbContext db)
        {
            _db = db;
        }

        public async Task<byte[]> GenerateMonthlyPayrollComparisonExcelAsync(int employerId, Stream excelStream, CancellationToken cancellationToken = default)
        {
            if (excelStream.CanSeek)
                excelStream.Position = 0;

            using var uploaded = new XLWorkbook(excelStream);
            var sheet = uploaded.Worksheets.FirstOrDefault()
                       ?? throw new InvalidOperationException("בחוברת Excel אין גיליונות נתונים.");

            var headerMap = BuildHeaderMap(sheet.Row(1));
            var uploadedRows = new List<UploadedComparisonRow>();
            var lastRow = sheet.LastRowUsed()?.RowNumber() ?? 1;

            for (var r = 2; r <= lastRow; r++)
            {
                var row = sheet.Row(r);
                if (IsEmptyRow(row, headerMap))
                    continue;
                var parsed = TryParseUploadedRow(r, row, headerMap);
                if (parsed != null)
                    uploadedRows.Add(parsed);
            }

            if (uploadedRows.Count == 0)
                throw new InvalidOperationException("לא נמצאו שורות נתונים בקובץ.");

            var distinctYears = uploadedRows.Select(u => CanonAcademicYear(u.ResolvedAcademicYearNormalized)).Distinct().ToList();
            if (distinctYears.Count != 1)
                throw new InvalidOperationException("כל השורות חייבות להשתייך לאותה שנת לימודים עברית.");

            var academicYearCanon = distinctYears[0];

            var employerExists = await _db.Employers.AsNoTracking().AnyAsync(e => !e.IsDeleted && e.Id == employerId, cancellationToken);
            if (!employerExists)
                throw new InvalidOperationException("המעסיק לא נמצא במערכת.");

            var employees = await _db.Employees
                .AsNoTracking()
                .Where(e => !e.IsDeleted && (e.EmployerId == employerId ||
                                             e.EmploymentData.Any(ed => !ed.IsDeleted && ed.EmployerId == employerId)))
                .ToListAsync(cancellationToken);

            var byTz = employees.Where(e => !string.IsNullOrWhiteSpace(e.IdNumber))
                .GroupBy(e => e.IdNumber.Trim())
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);

            var byEmpNum = employees.Where(e => e.EmployeeNumber.HasValue)
                .GroupBy(e => e.EmployeeNumber!.Value)
                .ToDictionary(g => g.Key, g => g.First());

            var empIds = employees.Select(e => e.Id).ToList();
            var employmentByEmployeeId = await _db.EmploymentData
                .AsNoTracking()
                .Include(ed => ed.Slots)
                .Where(ed => !ed.IsDeleted && ed.EmployerId == employerId && empIds.Contains(ed.EmployeeId))
                .Where(ed => CanonAcademicYear(ed.AcademicYear) == academicYearCanon)
                .ToDictionaryAsync(ed => ed.EmployeeId, cancellationToken);

            var septemberYear = SchoolYearGregorian.GetSeptemberGregorianYearForSchoolYearContaining(
                uploadedRows[0].GregorianMonth,
                uploadedRows[0].GregorianYear);
            var monthSequence = SchoolYearGregorian.GetSchoolYearMonthSequence(septemberYear);

            var groupedUpload = uploadedRows
                .GroupBy(u => ResolveEmployeeLookupKey(u))
                .ToDictionary(g => g.Key, g => g.ToDictionary(x => (x.GregorianMonth, x.GregorianYear)));

            var reportRows = new List<ComparisonReportRow>();

            foreach (var kv in groupedUpload.OrderBy(k => k.Key.SortKey))
            {
                Employee? emp = null;
                if (!string.IsNullOrEmpty(kv.Key.IdNumber) && byTz.TryGetValue(kv.Key.IdNumber.Trim(), out var e1))
                    emp = e1;
                else if (kv.Key.EmployeeNumber.HasValue && byEmpNum.TryGetValue(kv.Key.EmployeeNumber.Value, out var e2))
                    emp = e2;

                EmploymentData? ed = null;
                if (emp != null && employmentByEmployeeId.TryGetValue(emp.Id, out var edCand))
                {
                    if (CanonAcademicYear(edCand.AcademicYear) == academicYearCanon)
                        ed = edCand;
                }

                var monthMarks = new Dictionary<string, string>();
                var mismatchHighlightMonths = new HashSet<string>(StringComparer.Ordinal);
                var notesSb = new StringBuilder();
                var hoursValidityNote = BuildHoursValidityNote(kv.Value.Values.ToList());

                foreach (var (gm, gy) in monthSequence)
                {
                    var label = MonthColumnLabel(gm, gy);
                    if (!kv.Value.TryGetValue((gm, gy), out var upRow))
                    {
                        monthMarks[label] = string.Empty;
                        continue;
                    }

                    var mismatches = CompareMonth(ed, emp, upRow);
                    if (mismatches.Count == 0)
                        monthMarks[label] = "V";
                    else
                    {
                        monthMarks[label] = string.Empty;
                        mismatchHighlightMonths.Add(label);
                    }

                    if (mismatches.Count > 0)
                    {
                        notesSb.Append(label).Append(": ");
                        notesSb.AppendJoin("; ",
                            mismatches.Select(m =>
                                $"{m.FieldLabel} - Excel: {m.ExcelDisplay ?? "∅"}, DB: {m.DbDisplay ?? "∅"}"));
                        notesSb.Append(". ");
                    }
                }

                var institutionSymbol =
                    kv.Value.Values.Select(v => Cell(v, "סמל", "סמל_מוסד", "סמל מוסד")).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x))
                    ?? FirstDbInstitutionSymbol(ed);

                var displayName = emp?.FullName ?? Cell(kv.Value.Values.First(), "שם", "שם_מלא") ?? kv.Key.DisplayFallback;

                reportRows.Add(new ComparisonReportRow(
                    InstitutionSymbol: institutionSymbol,
                    EmployeeDisplayName: displayName,
                    RoleDisplay: ed?.Grade1Role?.Trim(),
                    HoursSummaryDisplay: ed?.Grade1Total?.ToString(CultureInfo.InvariantCulture),
                    EducationDisplay: ed?.Grade1GradeName?.Trim(),
                    HoursValidityNote: hoursValidityNote,
                    MonthMarksByColumnLabel: monthMarks,
                    MonthMismatchHighlightLabels: mismatchHighlightMonths,
                    NotesCombined: notesSb.ToString().Trim()));
            }

            return BuildOutputWorkbook(reportRows, monthSequence);
        }

        private sealed record LookupKey(string? IdNumber, int? EmployeeNumber, string SortKey, string DisplayFallback);

        private static LookupKey ResolveEmployeeLookupKey(UploadedComparisonRow row)
        {
            var tz = string.IsNullOrWhiteSpace(row.IdNumber) ? null : row.IdNumber.Trim();
            var num = row.EmployeeNumber;
            var sort = tz ?? $"#{num}";
            var disp = tz ?? num?.ToString(CultureInfo.InvariantCulture) ?? "?";
            return new LookupKey(tz, num, sort ?? "?", disp);
        }

        private static Dictionary<string, int> BuildHeaderMap(IXLRow headerRow)
        {
            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var cell in headerRow.CellsUsed())
            {
                var key = NormalizeHeaderKey(ExcelCellText.Get(cell));
                if (string.IsNullOrEmpty(key)) continue;
                if (!map.ContainsKey(key))
                    map[key] = cell.Address.ColumnNumber;
            }
            return map;
        }

        private static string NormalizeHeaderKey(string raw)
        {
            var parts = raw.Trim().Split([' ', '\t', '\u00A0'], StringSplitOptions.RemoveEmptyEntries);
            return string.Join("_", parts).Trim();
        }

        private static bool IsEmptyRow(IXLRow row, Dictionary<string, int> headers)
        {
            foreach (var col in headers.Values.Distinct())
            {
                var t = ExcelCellText.Get(row.Cell(col)).Trim();
                if (!string.IsNullOrEmpty(t)) return false;
            }
            return true;
        }

        private UploadedComparisonRow? TryParseUploadedRow(int excelRowNumber, IXLRow row, Dictionary<string, int> headers)
        {
            string Cell(params string[] aliases)
            {
                foreach (var a in aliases)
                {
                    var nk = NormalizeHeaderKey(a);
                    if (headers.TryGetValue(nk, out var col))
                        return ExcelCellText.Get(row.Cell(col)).Trim();
                }
                return string.Empty;
            }

            var tz = Cell("תז", "ת\"ז", "ת.ז.", "ת._ז.");
            var empNumRaw = Cell("מספר עובד", "מספר עובד בעוקץ", "מספר_עובד", "מספר_עובד_בעוקץ");
            int? empNum = int.TryParse(empNumRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var en) ? en : null;

            if (string.IsNullOrWhiteSpace(tz) && empNum == null)
                return null;

            int month;
            int year;
            var dateRaw = Cell("תאריך", "חודש_שנה");
            if (!string.IsNullOrWhiteSpace(dateRaw) &&
                TryParseFlexibleDate(dateRaw, out var dto))
            {
                month = dto.Month;
                year = dto.Year;
            }
            else
            {
                var monthRaw = Cell("חודש");
                var yearRaw = Cell("שנה");
                if (!int.TryParse(monthRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, out month))
                    return null;
                if (!int.TryParse(yearRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, out year))
                    return null;
            }

            var ayLabel = SchoolYearGregorian.GetSchoolYearFromGregorianMonth(month, year);
            var ayNorm = CanonAcademicYear(ayLabel);

            var cells = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            foreach (var (hk, col) in headers)
            {
                cells[hk] = ExcelCellText.Get(row.Cell(col)).Trim();
            }

            return new UploadedComparisonRow(
                excelRowNumber,
                string.IsNullOrWhiteSpace(tz) ? null : tz.Trim(),
                empNum,
                month,
                year,
                ayNorm,
                cells);
        }

        private static bool TryParseFlexibleDate(string raw, out DateOnly dto)
        {
            dto = default;
            var s = raw.Trim();
            if (DateOnly.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out dto))
                return true;
            if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            {
                dto = DateOnly.FromDateTime(dt);
                return true;
            }

            var parts = s.Split(['/', '.'], StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2 &&
                int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var a) &&
                int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var b))
            {
                if (a is >= 1 and <= 12 && b >= 1900 && b <= 2200)
                {
                    dto = new DateOnly(b, a, 1);
                    return true;
                }

                if (b is >= 1 and <= 12 && a >= 1900 && a <= 2200)
                {
                    dto = new DateOnly(a, b, 1);
                    return true;
                }
            }

            if (parts.Length == 3 &&
                int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var d) &&
                int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var m) &&
                int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var y))
            {
                if (y >= 1000 && m is >= 1 and <= 12)
                {
                    dto = new DateOnly(y, m, Math.Clamp(d, 1, DateTime.DaysInMonth(y, m)));
                    return true;
                }
            }

            return false;
        }

        private static string? Cell(UploadedComparisonRow row, params string[] aliases)
        {
            foreach (var a in aliases)
            {
                var nk = NormalizeHeaderKey(a);
                if (row.CellsByNormalizedHeader.TryGetValue(nk, out var v) && !string.IsNullOrWhiteSpace(v))
                    return v.Trim();
            }
            return null;
        }

        private static string? FirstDbInstitutionSymbol(EmploymentData? ed)
        {
            return ed?.Slots
                .Where(s => s.GradeBand == 1 && s.SlotIndex >= 1 && s.SlotIndex <= 6)
                .OrderBy(s => s.SlotIndex)
                .Select(s => s.InstitutionSymbol?.Trim())
                .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
        }

        private static string CanonAcademicYear(string? stored) =>
            HebrewAcademicYear.CanonicalForComparison(stored);

        private static string MonthColumnLabel(int month, int gregorianYear) => $"{month}.{gregorianYear}";

        private static List<FieldMismatch> CompareMonth(EmploymentData? ed, Employee? emp, UploadedComparisonRow up)
        {
            var list = new List<FieldMismatch>();

            if (emp == null)
            {
                list.Add(new FieldMismatch("עובד", up.IdNumber ?? up.EmployeeNumber?.ToString(CultureInfo.InvariantCulture), null));
                return list;
            }

            if (ed == null)
            {
                list.Add(new FieldMismatch("נתוני העסקה", "שורה בהעלאה", $"אין רשומה לשנת {up.ResolvedAcademicYearNormalized}"));
                return list;
            }

            void CmpStr(string label, string? dbVal, params string[] excelAliases)
            {
                var ex = PickFromRow(up, excelAliases);
                if (string.IsNullOrWhiteSpace(ex) && string.IsNullOrWhiteSpace(dbVal))
                    return;
                if (string.IsNullOrWhiteSpace(ex) != string.IsNullOrWhiteSpace(dbVal) ||
                    !string.Equals((ex ?? "").Trim(), (dbVal ?? "").Trim(), StringComparison.Ordinal))
                {
                    list.Add(new FieldMismatch(label, ex, dbVal));
                }
            }

            void CmpDec(string label, decimal? dbVal, params string[] excelAliases)
            {
                var exRaw = PickFromRow(up, excelAliases);
                var exDec = ParseDecimalNullable(exRaw);
                if (!dbVal.HasValue && exDec == null)
                    return;
                if (!DecimalsEqual(dbVal, exDec))
                    list.Add(new FieldMismatch(label, exRaw, dbVal?.ToString(CultureInfo.InvariantCulture)));
            }

            CmpStr("שם הדירוג (דרגה 1)", ed.Grade1GradeName, "שם הדירוג", "דירוג 1 שם הדירוג", "דרגה1_שם_הדירוג");
            CmpStr("דרגה (דרגה 1)", ed.Grade1Grade, "דרגה", "דרגה 1", "דרגה1_דרגה");
            CmpStr("ותק (דרגה 1)", ed.Grade1Seniority, "ותק", "ותק רגיל", "רמת מורכבות", "דרגה1_ותק");
            CmpDec("אחוז משרה מחושב (דרגה 1)", ed.Grade1JobPercent, "אחוז משרה מחושב", "אחוז משרה", "דרגה1_אחוז_משרה");
            CmpDec("אחוז תוספת אם (דרגה 1)", ed.Grade1MotherBenefitPercent, "אחוז תוספת אם", "דרגה1_אחוז_תוספת_אם");

            CmpStr("שם הדירוג (דרגה 2)", ed.Grade2GradeName, "דירוג 2 שם הדירוג", "דרגה2_שם_הדירוג");
            CmpStr("דרגה (דרגה 2)", ed.Grade2Grade, "דרגה 2", "דרגה2_דרגה");
            CmpStr("ותק (דרגה 2)", ed.Grade2Seniority, "דרגה 2 ותק", "דרגה2_ותק");
            CmpDec("אחוז משרה מחושב (דרגה 2)", ed.Grade2JobPercent, "דרגה 2 אחוז משרה", "דרגה2_אחוז_משרה");
            CmpDec("אחוז תוספת אם (דרגה 2)", ed.Grade2MotherBenefitPercent, "דרגה 2 אחוז תוספת אם", "דרגה2_אחוז_תוספת_אם");

            CmpDec("אחוז הפרשה לקרן השתלמות (דרגה 1)", ed.Grade1TrainingFundPercent,
                "אחוז הפרשה לקרן השתלמות", "קרן השתלמות", "דרגה1_קרן_השתלמות");
            CmpDec("אחוז הפרשה לקרן השתלמות (דרגה 2)", ed.Grade2TrainingFundPercent,
                "דרגה 2 קרן השתלמות", "דרגה2_קרן_השתלמות");

            CmpDec("שעות גיל מחושב (דרגה 1)", ed.Grade1AgeHours, "שעות גיל מחושב", "דרגה1_שעות_גיל");
            CmpDec("שעות גיל מחושב (דרגה 2)", ed.Grade2AgeHours, "דרגה 2 שעות גיל", "דרגה2_שעות_גיל");

            CmpDec("סה\"כ שעות שבועיות (דרגה 1)", ed.Grade1Total, "שש", "סהכ שעות", "דרגה1_סהכ");

            for (byte slot = 1; slot <= 6; slot++)
            {
                var dbH = SlotHours(ed, 1, slot);
                var dbB = SlotJobBase(ed, 1, slot);
                CmpDec($"משרה {slot}-שעות", dbH,
                    $"משרה {slot} שעות", $"משרה_{slot}_שעות", $"משרה{slot}שעות", $"דרגה1_משרה_{slot}_שעות");
                CmpDec($"משרה {slot}-מתוך שעות", dbB,
                    $"משרה {slot} מתוך שעות", $"משרה_{slot}_מתוך_שעות", $"משרה{slot}מתוך", $"דרגה1_משרה_{slot}_מתוך");
            }

            for (byte slot = 1; slot <= 6; slot++)
            {
                var dbH = SlotHours(ed, 2, slot);
                var dbB = SlotJobBase(ed, 2, slot);
                CmpDec($"משרה {slot}-שעות (דרגה 2)", dbH,
                    $"דרגה 2 משרה {slot} שעות", $"דרגה2_משרה_{slot}_שעות");
                CmpDec($"משרה {slot}-מתוך שעות (דרגה 2)", dbB,
                    $"דרגה 2 משרה {slot} מתוך שעות", $"דרגה2_משרה_{slot}_מתוך");
            }

            return list;
        }

        private static string? PickFromRow(UploadedComparisonRow row, params string[] headerAliases)
        {
            foreach (var a in headerAliases)
            {
                var nk = NormalizeHeaderKey(a);
                if (row.CellsByNormalizedHeader.TryGetValue(nk, out var v))
                {
                    var t = (v ?? "").Trim();
                    if (!string.IsNullOrEmpty(t))
                        return t;
                }
            }
            return null;
        }

        private static decimal? SlotHours(EmploymentData ed, byte band, byte slotIndex) =>
            ed.Slots.FirstOrDefault(s => s.GradeBand == band && s.SlotIndex == slotIndex)?.WeeklyHours;

        private static decimal? SlotJobBase(EmploymentData ed, byte band, byte slotIndex) =>
            ed.Slots.FirstOrDefault(s => s.GradeBand == band && s.SlotIndex == slotIndex)?.JobBase;

        private static bool DecimalsEqual(decimal? a, decimal? b)
        {
            if (!a.HasValue && !b.HasValue) return true;
            if (!a.HasValue || !b.HasValue) return false;
            return Math.Abs(a.Value - b.Value) <= NumericTolerance;
        }

        private static decimal? ParseDecimalNullable(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            return decimal.TryParse(raw.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : null;
        }

        private static string? BuildHoursValidityNote(IReadOnlyList<UploadedComparisonRow> monthRows)
        {
            foreach (var row in monthRows)
            {
                decimal? sumRow = null;
                for (byte slot = 1; slot <= 6; slot++)
                {
                    var raw = PickFromRow(row, $"משרה {slot} שעות", $"משרה_{slot}_שעות", $"דרגה1_משרה_{slot}_שעות");
                    var d = ParseDecimalNullable(raw);
                    if (d == null) continue;
                    sumRow = (sumRow ?? 0) + d.Value;
                }

                var shesh = ParseDecimalNullable(PickFromRow(row, "שש", "סהכ שעות", "דרגה1_סהכ"));
                if (sumRow != null && shesh != null && Math.Abs(sumRow.Value - shesh.Value) > NumericTolerance)
                    return $"סכום משרות 1–6 ({sumRow.Value.ToString(CultureInfo.InvariantCulture)}) ≠ שש ({shesh.Value.ToString(CultureInfo.InvariantCulture)})";
            }

            return null;
        }

        private static byte[] BuildOutputWorkbook(List<ComparisonReportRow> rows, IReadOnlyList<(int Month, int GregorianYear)> monthSequence)
        {
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Comparison");

            var col = 1;
            ws.Cell(1, col++).Value = "סמל";
            ws.Cell(1, col++).Value = "שם";
            ws.Cell(1, col++).Value = "תפקיד";
            ws.Cell(1, col++).Value = "שש";
            ws.Cell(1, col++).Value = "חינוך";
            ws.Cell(1, col++).Value = "בדיקת תקינות שעות";
            foreach (var (m, y) in monthSequence)
                ws.Cell(1, col++).Value = MonthColumnLabel(m, y);
            ws.Cell(1, col).Value = "הערות";

            ws.Row(1).Style.Font.Bold = true;

            var r = 2;
            foreach (var row in rows)
            {
                var c = 1;
                ws.Cell(r, c++).Value = row.InstitutionSymbol ?? "";
                ws.Cell(r, c++).Value = row.EmployeeDisplayName;
                ws.Cell(r, c++).Value = row.RoleDisplay ?? "";
                ws.Cell(r, c++).Value = row.HoursSummaryDisplay ?? "";
                ws.Cell(r, c++).Value = row.EducationDisplay ?? "";
                ws.Cell(r, c++).Value = row.HoursValidityNote ?? "";
                foreach (var (m, gy) in monthSequence)
                {
                    var label = MonthColumnLabel(m, gy);
                    row.MonthMarksByColumnLabel.TryGetValue(label, out var mark);
                    var monthCell = ws.Cell(r, c++);
                    monthCell.Value = mark ?? "";
                    if (row.MonthMismatchHighlightLabels.Contains(label))
                    {
                        monthCell.Style.Fill.PatternType = XLFillPatternValues.Solid;
                        monthCell.Style.Fill.BackgroundColor = XLColor.Yellow;
                    }
                }

                ws.Cell(r, c).Value = row.NotesCombined;
                r++;
            }

            ws.Columns().AdjustToContents(1, Math.Min(r - 1, 200));
            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }
    }
}
