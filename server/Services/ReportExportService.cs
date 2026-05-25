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
        Task<byte[]> InstitutionHoursAsync(int employerId, string academicYear, string institutionSymbol);
        Task<byte[]> EmployeesPersonalAsync(int employerId);
        Task<byte[]> EmployeesEmploymentDataAsync(int employerId, string academicYear);
    }

    public sealed class ReportExportService : IReportExportService
    {
        private readonly PayrollDbContext _db;

        public ReportExportService(PayrollDbContext db) => _db = db;

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

        // ─────────────────────────────────────────────────────────────────
        //  TODO: Confirm business rule for kindergarten identification.
        //  Current heuristic: GradeName == "אופק גנים" (garden-only grade track),
        //  OR role contains "גננת" (covers יסודי וגנים / אופק חדש kindergarten roles).
        // ─────────────────────────────────────────────────────────────────
        private static bool IsKindergartenGrade(string? gradeName, string? role)
        {
            if (string.Equals(gradeName, "אופק גנים", StringComparison.Ordinal)) return true;
            if (role != null && role.Contains("גננת", StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        // ─────────────────────────────────────────────────────────────────
        //  TODO: Confirm business rule for school identification.
        //  Current heuristic: GradeName == "עוז לתמורה" (school-only track),
        //  OR role contains "מורה"/"מנהל" when not also a kindergarten grade.
        // ─────────────────────────────────────────────────────────────────
        private static bool IsSchoolGrade(string? gradeName, string? role)
        {
            if (IsKindergartenGrade(gradeName, role)) return false;
            if (string.Equals(gradeName, "עוז לתמורה", StringComparison.Ordinal)) return true;
            if (role != null && (role.Contains("מורה", StringComparison.OrdinalIgnoreCase) ||
                                  role.Contains("מנהל", StringComparison.OrdinalIgnoreCase))) return true;
            return false;
        }

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
        //  Report 1 — מצבת גנים שנתי
        // ─────────────────────────────────────────────────────────────────
        public async Task<byte[]> KindergartenAnnualAsync(int employerId, string academicYear)
        {
            var records = await GetEmploymentDataWithSlots(employerId, academicYear);
            var filtered = records
                .Where(ed => IsKindergartenGrade(ed.Grade1GradeName, ed.Grade1Role)
                          || IsKindergartenGrade(ed.Grade2GradeName, ed.Grade2Role))
                .OrderBy(ed => ed.Employee?.LastName)
                .ThenBy(ed => ed.Employee?.FirstName)
                .ToList();

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("מצבת גנים");
            string[] headers = [
                "שם העובדת", "ת\"ז", "שם דירוג", "תפקיד", "דרגה", "ותק",
                "סה\"כ ש\"ש", "אחוז משרה", "אחוז תוספת אם", "שעות גיל", "קרן השתלמות %"
            ];
            SetHeaders(ws, headers);

            var r = 2;
            foreach (var ed in filtered)
            {
                var g1 = IsKindergartenGrade(ed.Grade1GradeName, ed.Grade1Role);
                ws.Cell(r, 1).Value = ed.Employee?.FullName ?? "";
                ws.Cell(r, 2).Value = ed.Employee?.IdNumber ?? "";
                ws.Cell(r, 3).Value = (g1 ? ed.Grade1GradeName : ed.Grade2GradeName) ?? "";
                ws.Cell(r, 4).Value = (g1 ? ed.Grade1Role : ed.Grade2Role) ?? "";
                ws.Cell(r, 5).Value = (g1 ? ed.Grade1Grade : ed.Grade2Grade) ?? "";
                ws.Cell(r, 6).Value = (g1 ? ed.Grade1Seniority : ed.Grade2Seniority) ?? "";
                SetDec(ws, r, 7,  g1 ? ed.Grade1Total : ed.Grade2Total);
                SetDec(ws, r, 8,  g1 ? ed.Grade1JobPercent : ed.Grade2JobPercent);
                SetDec(ws, r, 9,  g1 ? ed.Grade1MotherBenefitPercent : ed.Grade2MotherBenefitPercent);
                SetDec(ws, r, 10, g1 ? ed.Grade1AgeHours : ed.Grade2AgeHours);
                SetDec(ws, r, 11, g1 ? ed.Grade1TrainingFundPercent : ed.Grade2TrainingFundPercent);
                r++;
            }
            BoldAndFit(ws, headers.Length, r - 1);
            return ToBytes(wb);
        }

        // ─────────────────────────────────────────────────────────────────
        //  Report 2 — מצבת בית ספר שנתי
        // ─────────────────────────────────────────────────────────────────
        public async Task<byte[]> SchoolAnnualAsync(int employerId, string academicYear)
        {
            var records = await GetEmploymentDataWithSlots(employerId, academicYear);
            var filtered = records
                .Where(ed => IsSchoolGrade(ed.Grade1GradeName, ed.Grade1Role)
                          || IsSchoolGrade(ed.Grade2GradeName, ed.Grade2Role))
                .OrderBy(ed => ed.Employee?.LastName)
                .ThenBy(ed => ed.Employee?.FirstName)
                .ToList();

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("מצבת בית ספר");
            string[] headers = [
                "שם העובדת", "ת\"ז", "שם דירוג", "תפקיד", "דרגה", "ותק",
                "סה\"כ ש\"ש", "אחוז משרה", "אחוז תוספת אם", "שעות גיל", "קרן השתלמות %"
            ];
            SetHeaders(ws, headers);

            var r = 2;
            foreach (var ed in filtered)
            {
                var g1 = IsSchoolGrade(ed.Grade1GradeName, ed.Grade1Role);
                ws.Cell(r, 1).Value = ed.Employee?.FullName ?? "";
                ws.Cell(r, 2).Value = ed.Employee?.IdNumber ?? "";
                ws.Cell(r, 3).Value = (g1 ? ed.Grade1GradeName : ed.Grade2GradeName) ?? "";
                ws.Cell(r, 4).Value = (g1 ? ed.Grade1Role : ed.Grade2Role) ?? "";
                ws.Cell(r, 5).Value = (g1 ? ed.Grade1Grade : ed.Grade2Grade) ?? "";
                ws.Cell(r, 6).Value = (g1 ? ed.Grade1Seniority : ed.Grade2Seniority) ?? "";
                SetDec(ws, r, 7,  g1 ? ed.Grade1Total : ed.Grade2Total);
                SetDec(ws, r, 8,  g1 ? ed.Grade1JobPercent : ed.Grade2JobPercent);
                SetDec(ws, r, 9,  g1 ? ed.Grade1MotherBenefitPercent : ed.Grade2MotherBenefitPercent);
                SetDec(ws, r, 10, g1 ? ed.Grade1AgeHours : ed.Grade2AgeHours);
                SetDec(ws, r, 11, g1 ? ed.Grade1TrainingFundPercent : ed.Grade2TrainingFundPercent);
                r++;
            }
            BoldAndFit(ws, headers.Length, r - 1);
            return ToBytes(wb);
        }

        // ─────────────────────────────────────────────────────────────────
        //  Report 3 — דוח השוואה לפי חודש מסוים
        //  TODO: "עוקץ" (monthly uploaded payroll) data is NOT stored in the DB.
        //        Only the "מצבת" (baseline employment data) rows are available.
        //        For full עוקץ comparison, use the existing "השוואה" upload feature.
        //  TODO: Columns תוספת מעונות / הכפלה כללית do not exist in the current model.
        // ─────────────────────────────────────────────────────────────────
        public async Task<byte[]> MonthlyComparisonAsync(int employerId, string academicYear, int month, Stream uploadedFile)
        {
            var records = await GetEmploymentDataWithSlots(employerId, academicYear);

            // Read uploaded file summary (row count) for the notes sheet.
            int uploadedRowCount = 0;
            try
            {
                using var uploadedWb = new XLWorkbook(uploadedFile);
                var uploadedWs = uploadedWb.Worksheets.FirstOrDefault();
                if (uploadedWs != null)
                    uploadedRowCount = uploadedWs.LastRowUsed()?.RowNumber() ?? 0;
            }
            catch { /* if file cannot be parsed, continue without uploaded data */ }

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("השוואה חודשית");
            string[] headers = [
                "סמל מוסד", "שם העובדת", "תפקיד", "דרגה", "ותק",
                "ש\"ש", "מתוך ש\"ש", "אחוז משרה", "אחוז תוספת אם", "שעות גיל",
                "מס' גמולים", "כפל תואר", "תוספת מעונות*",
                "הפרשה לקרן השתלמות", "הכפלה כללית*", "סוג שורה"
            ];
            SetHeaders(ws, headers);

            var r = 2;
            foreach (var ed in records.OrderBy(e => e.Employee?.LastName).ThenBy(e => e.Employee?.FirstName))
            {
                foreach (var slot in ed.Slots
                    .Where(s => !string.IsNullOrEmpty(s.InstitutionSymbol) || s.WeeklyHours > 0)
                    .OrderBy(s => s.GradeBand).ThenBy(s => s.SlotIndex))
                {
                    var g1 = slot.GradeBand == 1;
                    ws.Cell(r, 1).Value  = slot.InstitutionSymbol ?? "";
                    ws.Cell(r, 2).Value  = ed.Employee?.FullName ?? "";
                    ws.Cell(r, 3).Value  = (g1 ? ed.Grade1Role : ed.Grade2Role) ?? "";
                    ws.Cell(r, 4).Value  = (g1 ? ed.Grade1Grade : ed.Grade2Grade) ?? "";
                    ws.Cell(r, 5).Value  = (g1 ? ed.Grade1Seniority : ed.Grade2Seniority) ?? "";
                    SetDec(ws, r, 6,  slot.WeeklyHours);
                    SetDec(ws, r, 7,  slot.JobBase);
                    SetDec(ws, r, 8,  g1 ? ed.Grade1JobPercent : ed.Grade2JobPercent);
                    SetDec(ws, r, 9,  g1 ? ed.Grade1MotherBenefitPercent : ed.Grade2MotherBenefitPercent);
                    SetDec(ws, r, 10, g1 ? ed.Grade1AgeHours : ed.Grade2AgeHours);
                    SetDec(ws, r, 11, g1 ? ed.Grade1TrainingBenefits : ed.Grade2TrainingBenefits);
                    SetDec(ws, r, 12, g1 ? ed.Grade1DoubleDegree : ed.Grade2DoubleDegree);
                    // col 13, 15: not in model — empty
                    SetDec(ws, r, 14, g1 ? ed.Grade1TrainingFundPercent : ed.Grade2TrainingFundPercent);
                    ws.Cell(r, 16).Value = "מצבת";
                    r++;
                }
            }
            BoldAndFit(ws, headers.Length, r - 1);

            var noteWs = wb.Worksheets.Add("הערות");
            noteWs.Cell(1, 1).Value = $"דוח השוואה חודשי — חודש {month}, שנת לימודים {academicYear}";
            noteWs.Cell(2, 1).Value = "* עמודות המסומנות בכוכבית (תוספת מעונות, הכפלה כללית) אינן קיימות במודל הנתונים הנוכחי.";
            noteWs.Cell(3, 1).Value = $"קובץ עוקץ שהועלה: {uploadedRowCount} שורות נטענו (כולל כותרת).";
            noteWs.Cell(4, 1).Value = "TODO: יש לממש התאמה בין שורות העוקץ לשורות המצבת ולהוסיף שורות עוקץ והשוואה.";
            noteWs.Row(1).Style.Font.Bold = true;
            noteWs.Column(1).Width = 90;
            return ToBytes(wb);
        }

        // ─────────────────────────────────────────────────────────────────
        //  Report 4 — דוח השוואה שנתי
        //  Dynamic month columns: September–August for the selected academic year.
        //  TODO: Monthly data cells (columns per month) require uploaded monthly payroll
        //        data which is NOT stored in DB. Cells are empty pending implementation.
        // ─────────────────────────────────────────────────────────────────
        public async Task<byte[]> AnnualComparisonAsync(int employerId, string academicYear, Stream uploadedFile)
        {
            var records = await GetEmploymentDataWithSlots(employerId, academicYear);

            // Read uploaded file summary for the notes sheet.
            int uploadedRowCount = 0;
            try
            {
                using var uploadedWb = new XLWorkbook(uploadedFile);
                var uploadedWs = uploadedWb.Worksheets.FirstOrDefault();
                if (uploadedWs != null)
                    uploadedRowCount = uploadedWs.LastRowUsed()?.RowNumber() ?? 0;
            }
            catch { /* continue without uploaded data if file cannot be parsed */ }

            int sepYear;
            try { sepYear = ParseSeptemberGregorianYear(academicYear); }
            catch { sepYear = DateTime.UtcNow.Year - 1; }

            var monthSeq = SchoolYearGregorian.GetSchoolYearMonthSequence(sepYear);
            var monthHeaders = monthSeq.Select(m => $"{m.Month}.{m.GregorianYear}").ToArray();

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("השוואה שנתית");

            string[] staticHeaders = [
                "סמל מוסד", "שם העובדת", "תפקיד", "דרגה", "ותק",
                "ש\"ש", "מתוך ש\"ש", "אחוז משרה", "הכפלה כללית*"
            ];
            var allHeaders = staticHeaders.Concat(monthHeaders).ToArray();
            SetHeaders(ws, allHeaders);

            var r = 2;
            foreach (var ed in records.OrderBy(e => e.Employee?.LastName).ThenBy(e => e.Employee?.FirstName))
            {
                foreach (var slot in ed.Slots
                    .Where(s => !string.IsNullOrEmpty(s.InstitutionSymbol) || s.WeeklyHours > 0)
                    .OrderBy(s => s.GradeBand).ThenBy(s => s.SlotIndex))
                {
                    var g1 = slot.GradeBand == 1;
                    ws.Cell(r, 1).Value = slot.InstitutionSymbol ?? "";
                    ws.Cell(r, 2).Value = ed.Employee?.FullName ?? "";
                    ws.Cell(r, 3).Value = (g1 ? ed.Grade1Role : ed.Grade2Role) ?? "";
                    ws.Cell(r, 4).Value = (g1 ? ed.Grade1Grade : ed.Grade2Grade) ?? "";
                    ws.Cell(r, 5).Value = (g1 ? ed.Grade1Seniority : ed.Grade2Seniority) ?? "";
                    SetDec(ws, r, 6, slot.WeeklyHours);
                    SetDec(ws, r, 7, slot.JobBase);
                    SetDec(ws, r, 8, g1 ? ed.Grade1JobPercent : ed.Grade2JobPercent);
                    // col 9 (הכפלה כללית): not in model
                    // cols 10+: monthly data not in DB — left empty
                    r++;
                }
            }
            BoldAndFit(ws, allHeaders.Length, r - 1);

            var noteWs = wb.Worksheets.Add("הערות");
            noteWs.Cell(1, 1).Value = $"דוח השוואה שנתי — שנת לימודים {academicYear}";
            noteWs.Cell(2, 1).Value = "* 'הכפלה כללית' אינה קיימת במודל הנתונים הנוכחי.";
            noteWs.Cell(3, 1).Value = $"קובץ עוקץ שהועלה: {uploadedRowCount} שורות נטענו (כולל כותרת).";
            noteWs.Cell(4, 1).Value = "TODO: יש לממש התאמה בין שורות העוקץ לשורות המצבת ולמלא את עמודות החודשים.";
            noteWs.Row(1).Style.Font.Bold = true;
            noteWs.Column(1).Width = 90;
            return ToBytes(wb);
        }

        // ─────────────────────────────────────────────────────────────────
        //  Report 5 — בדיקת שעות לסמל
        //  TODO: "נדרש" (required hours per institution symbol) is NOT defined in the
        //        current data model. Only "מצבת אתר" (actual from employment slots) is computed.
        //        "הפרש" (difference) is left empty because נדרש is missing.
        //  TODO: "שעות חינוך" column is not defined in the model.
        //  Role classification: גננת = role contains "גננת"; סייעת = role contains "סייעת".
        // ─────────────────────────────────────────────────────────────────
        public async Task<byte[]> InstitutionHoursAsync(int employerId, string academicYear, string institutionSymbol)
        {
            var records = await GetEmploymentDataWithSlots(employerId, academicYear);

            decimal gannetHours = 0;
            decimal saayetHours = 0;

            foreach (var ed in records)
            {
                foreach (var slot in ed.Slots.Where(s =>
                    string.Equals(s.InstitutionSymbol, institutionSymbol, StringComparison.OrdinalIgnoreCase)
                    && s.WeeklyHours > 0))
                {
                    var role = (slot.GradeBand == 1 ? ed.Grade1Role : ed.Grade2Role) ?? "";
                    if (role.Contains("גננת", StringComparison.OrdinalIgnoreCase))
                        gannetHours += slot.WeeklyHours ?? 0;
                    else if (role.Contains("סייעת", StringComparison.OrdinalIgnoreCase))
                        saayetHours += slot.WeeklyHours ?? 0;
                }
            }

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("בדיקת שעות");
            ws.Cell(1, 1).Value = "סמל גן";
            ws.Cell(1, 2).Value = "מס' שעות גננת סה\"כ";
            ws.Cell(1, 3).Value = "שעות חינוך*";
            ws.Cell(1, 4).Value = "מס' שעות סייעת סה\"כ";
            ws.Cell(1, 5).Value = "סוג שורה";
            ws.Row(1).Style.Font.Bold = true;

            // נדרש — TODO
            ws.Cell(2, 1).Value = institutionSymbol;
            ws.Cell(2, 5).Value = "נדרש — TODO: לא מוגדר במודל";

            // מצבת אתר
            ws.Cell(3, 1).Value = institutionSymbol;
            ws.Cell(3, 2).Value = gannetHours;
            ws.Cell(3, 4).Value = saayetHours;
            ws.Cell(3, 5).Value = "מצבת אתר";

            // הפרש — TODO (נדרש missing)
            ws.Cell(4, 1).Value = institutionSymbol;
            ws.Cell(4, 5).Value = "הפרש — TODO: נדרש לא מוגדר";

            var noteWs = wb.Worksheets.Add("הערות");
            noteWs.Cell(1, 1).Value = $"בדיקת שעות לסמל {institutionSymbol} — שנת לימודים {academicYear}";
            noteWs.Cell(2, 1).Value = "* 'שעות חינוך' אינן מוגדרות במודל הנתונים הנוכחי.";
            noteWs.Cell(3, 1).Value = "שורת 'נדרש' וחישוב 'הפרש' דורשים הגדרת כלל עסקי שאינו במערכת כרגע.";
            noteWs.Row(1).Style.Font.Bold = true;
            noteWs.Column(1).Width = 80;

            ws.Columns().AdjustToContents();
            return ToBytes(wb);
        }

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
            string[] headers = ["שם פרטי", "שם משפחה", "ת\"ז", "תאריך לידה", "טלפון", "מין", "מעסיק", "סטטוס", .. childHeaders];
            SetHeaders(ws, headers);

            var r = 2;
            foreach (var emp in employees)
            {
                var active = emp.ManualActiveStatus ?? employeeIdsWithEd.Contains(emp.Id);
                ws.Cell(r, 1).Value = emp.FirstName ?? "";
                ws.Cell(r, 2).Value = emp.LastName ?? "";
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
        //  Report 7 — עובדים נתוני העסקה
        //  TODO: Columns תוספת מעונות / הכפלה כללית do not exist in the current data model.
        // ─────────────────────────────────────────────────────────────────
        public async Task<byte[]> EmployeesEmploymentDataAsync(int employerId, string academicYear)
        {
            var records = await GetEmploymentDataWithSlots(employerId, academicYear);

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("נתוני העסקה");
            string[] headers = [
                "שם העובדת", "ת\"ז", "מעסיק", "סמל מוסד",
                "שם הדירוג", "דרגה", "תפקיד", "ותק",
                "ש\"ש", "מתוך ש\"ש", "אחוז משרה", "אחוז תוספת אם", "שעות גיל",
                "מס' גמולים", "כפל תואר", "תוספת מעונות*",
                "הפרשה לקרן השתלמות", "הכפלה כללית*"
            ];
            SetHeaders(ws, headers);

            var r = 2;
            foreach (var ed in records.OrderBy(e => e.Employee?.LastName).ThenBy(e => e.Employee?.FirstName))
            {
                foreach (var slot in ed.Slots
                    .Where(s => !string.IsNullOrEmpty(s.InstitutionSymbol) || s.WeeklyHours > 0)
                    .OrderBy(s => s.GradeBand).ThenBy(s => s.SlotIndex))
                {
                    var g1 = slot.GradeBand == 1;
                    ws.Cell(r, 1).Value = ed.Employee?.FullName ?? "";
                    ws.Cell(r, 2).Value = ed.Employee?.IdNumber ?? "";
                    ws.Cell(r, 3).Value = ed.Employer?.Name ?? "";
                    ws.Cell(r, 4).Value = slot.InstitutionSymbol ?? "";
                    ws.Cell(r, 5).Value = (g1 ? ed.Grade1GradeName : ed.Grade2GradeName) ?? "";
                    ws.Cell(r, 6).Value = (g1 ? ed.Grade1Grade : ed.Grade2Grade) ?? "";
                    ws.Cell(r, 7).Value = (g1 ? ed.Grade1Role : ed.Grade2Role) ?? "";
                    ws.Cell(r, 8).Value = (g1 ? ed.Grade1Seniority : ed.Grade2Seniority) ?? "";
                    SetDec(ws, r, 9,  slot.WeeklyHours);
                    SetDec(ws, r, 10, slot.JobBase);
                    SetDec(ws, r, 11, g1 ? ed.Grade1JobPercent : ed.Grade2JobPercent);
                    SetDec(ws, r, 12, g1 ? ed.Grade1MotherBenefitPercent : ed.Grade2MotherBenefitPercent);
                    SetDec(ws, r, 13, g1 ? ed.Grade1AgeHours : ed.Grade2AgeHours);
                    SetDec(ws, r, 14, g1 ? ed.Grade1TrainingBenefits : ed.Grade2TrainingBenefits);
                    SetDec(ws, r, 15, g1 ? ed.Grade1DoubleDegree : ed.Grade2DoubleDegree);
                    // col 16 not in model — empty
                    SetDec(ws, r, 17, g1 ? ed.Grade1TrainingFundPercent : ed.Grade2TrainingFundPercent);
                    // col 18 not in model
                    r++;
                }
            }
            BoldAndFit(ws, headers.Length, r - 1);

            var noteWs = wb.Worksheets.Add("הערות");
            noteWs.Cell(1, 1).Value = $"נתוני העסקה — שנת לימודים {academicYear}";
            noteWs.Cell(2, 1).Value = "* עמודות המסומנות בכוכבית (תוספת מעונות, הכפלה כללית) אינן קיימות במודל הנתונים הנוכחי.";
            noteWs.Row(1).Style.Font.Bold = true;
            noteWs.Column(1).Width = 90;
            return ToBytes(wb);
        }

        // ─────────────────────────────────────────────────────────────────
        //  Private — shared DB query
        // ─────────────────────────────────────────────────────────────────
        private async Task<List<Models.EmploymentData>> GetEmploymentDataWithSlots(int employerId, string academicYear)
        {
            return await _db.EmploymentData
                .Include(e => e.Employee)
                .Include(e => e.Employer)
                .Include(e => e.Slots)
                .Where(e => e.EmployerId == employerId
                         && e.AcademicYear == academicYear
                         && !e.IsDeleted)
                .ToListAsync();
        }
    }
}
