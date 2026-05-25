using System.Globalization;
using AccountingProject.Models;
using ClosedXML.Excel;

namespace AccountingProject.Services
{
    internal static class EmployerFullExcelExport
    {
        public static byte[] Build(
            Employer employer,
            IReadOnlyList<Employee> employees,
            IReadOnlyList<EmployerInstitutionSymbol> symbols,
            IReadOnlyList<EmploymentData> employmentRows,
            HashSet<int> employeeIdsWithEmploymentData)
        {
            using var wb = new XLWorkbook();

            AddEmployerSheet(wb, employer);
            AddEmployeesSheet(wb, employees, employeeIdsWithEmploymentData);
            AddSymbolsSheet(wb, symbols);
            AddEmploymentHeadersSheet(wb, employmentRows);
            AddEmploymentSlotsSheet(wb, employmentRows);

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }

        private static void AddEmployerSheet(XLWorkbook wb, Employer employer)
        {
            var ws = wb.Worksheets.Add("מעסיק");
            string[] h = ["מזהה", "שם מעסיק", "ח.פ.", "סמל מוטב", "מספר עוקץ"];
            for (var i = 0; i < h.Length; i++)
                ws.Cell(1, i + 1).Value = h[i];
            ws.Cell(2, 1).Value = employer.Id;
            ws.Cell(2, 2).Value = employer.Name;
            ws.Cell(2, 3).Value = employer.BusinessNumber ?? "";
            ws.Cell(2, 4).Value = employer.BeneficiarySymbol ?? "";
            ws.Cell(2, 5).Value = employer.EketzNumber ?? "";
            ws.Row(1).Style.Font.Bold = true;
            ws.Columns().AdjustToContents(1, 2);
        }

        private static void AddEmployeesSheet(XLWorkbook wb, IReadOnlyList<Employee> employees, HashSet<int> employeeIdsWithEmploymentData)
        {
            var ws = wb.Worksheets.Add("עובדים");
            var headers = new[]
            {
                "מזהה עובד", "ת.ז.", "שם פרטי", "שם משפחה", "שם מלא", "מספר עובד בעוקץ", "תאריך לידה", "מין", "טלפון",
                "פעיל (מחושב)", "סטטוס פעילות ידני"
            };
            var childHeaders = Enumerable.Range(1, 10).Select(i => $"תאריך לידה ילד {i}").ToArray();
            var allH = headers.Concat(childHeaders).ToArray();
            for (var i = 0; i < allH.Length; i++)
                ws.Cell(1, i + 1).Value = allH[i];

            var r = 2;
            foreach (var e in employees)
            {
                var hasEd = employeeIdsWithEmploymentData.Contains(e.Id);
                var activeComputed = e.ManualActiveStatus ?? hasEd;
                var c = 1;
                ws.Cell(r, c++).Value = e.Id;
                ws.Cell(r, c++).Value = e.IdNumber;
                ws.Cell(r, c++).Value = e.FirstName ?? "";
                ws.Cell(r, c++).Value = e.LastName ?? "";
                ws.Cell(r, c++).Value = e.FullName;
                if (e.EmployeeNumber.HasValue) ws.Cell(r, c).Value = e.EmployeeNumber.Value;
                c++;
                ws.Cell(r, c++).Value = e.BirthDate.HasValue ? e.BirthDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : "";
                ws.Cell(r, c++).Value = e.Gender ?? "";
                ws.Cell(r, c++).Value = e.Phone ?? "";
                ws.Cell(r, c++).Value = activeComputed ? "כן" : "לא";
                ws.Cell(r, c++).Value = e.ManualActiveStatus.HasValue ? (e.ManualActiveStatus.Value ? "כן" : "לא") : "";
                var children = new[]
                {
                    e.ChildBirthDate1, e.ChildBirthDate2, e.ChildBirthDate3, e.ChildBirthDate4, e.ChildBirthDate5,
                    e.ChildBirthDate6, e.ChildBirthDate7, e.ChildBirthDate8, e.ChildBirthDate9, e.ChildBirthDate10
                };
                foreach (var cd in children)
                {
                    ws.Cell(r, c++).Value = cd.HasValue ? cd.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : "";
                }
                r++;
            }
            ws.Row(1).Style.Font.Bold = true;
            ws.Columns().AdjustToContents(1, Math.Min(r - 1, 500));
        }

        private static void AddSymbolsSheet(XLWorkbook wb, IReadOnlyList<EmployerInstitutionSymbol> symbols)
        {
            var ws = wb.Worksheets.Add("סמלי מוסד");
            ws.Cell(1, 1).Value = "מזהה";
            ws.Cell(1, 2).Value = "סמל מוסד";
            ws.Cell(1, 3).Value = "שם מוסד";
            var r = 2;
            foreach (var s in symbols)
            {
                ws.Cell(r, 1).Value = s.Id;
                ws.Cell(r, 2).Value = s.InstitutionSymbol;
                ws.Cell(r, 3).Value = s.InstitutionSymbolName ?? "";
                r++;
            }
            ws.Row(1).Style.Font.Bold = true;
            ws.Columns().AdjustToContents(1, Math.Min(r - 1, 500));
        }

        private static void AddEmploymentHeadersSheet(XLWorkbook wb, IReadOnlyList<EmploymentData> employmentRows)
        {
            var ws = wb.Worksheets.Add("נתוני עסקה");
            var headers = new[]
            {
                "מזהה רשומה", "מזהה עובד", "ת.ז. עובד", "שם עובד", "שנת לימודים",
                "דרגה1 סהכ", "דרגה1 אחוז משרה", "דרגה1 קרן השתלמות %", "דרגה1 שעות גיל", "דרגה1 אחוז תוספת אם",
                "דרגה1 גמולי השתלמות", "דרגה1 כפל תואר",
                "דרגה2 סהכ", "דרגה2 אחוז משרה", "דרגה2 קרן השתלמות %", "דרגה2 שעות גיל", "דרגה2 אחוז תוספת אם",
                "דרגה2 גמולי השתלמות", "דרגה2 כפל תואר",
                "דרגה1 שם דירוג", "דרגה1 דרגה", "דרגה1 תפקיד", "דרגה1 ותק",
                "דרגה2 שם דירוג", "דרגה2 דרגה", "דרגה2 תפקיד", "דרגה2 ותק"
            };
            for (var i = 0; i < headers.Length; i++)
                ws.Cell(1, i + 1).Value = headers[i];

            var r = 2;
            foreach (var ed in employmentRows.OrderBy(x => x.Employee?.LastName).ThenBy(x => x.Employee?.FirstName).ThenBy(x => x.AcademicYear))
            {
                var emp = ed.Employee;
                var c = 1;
                ws.Cell(r, c++).Value = ed.Id;
                ws.Cell(r, c++).Value = ed.EmployeeId;
                ws.Cell(r, c++).Value = emp?.IdNumber ?? "";
                ws.Cell(r, c++).Value = emp?.FullName ?? "";
                ws.Cell(r, c++).Value = ed.AcademicYear;
                SetDecimal(ws, r, ref c, ed.Grade1Total);
                SetDecimal(ws, r, ref c, ed.Grade1JobPercent);
                SetDecimal(ws, r, ref c, ed.Grade1TrainingFundPercent);
                SetDecimal(ws, r, ref c, ed.Grade1AgeHours);
                SetDecimal(ws, r, ref c, ed.Grade1MotherBenefitPercent);
                SetDecimal(ws, r, ref c, ed.Grade1TrainingBenefits);
                SetDecimal(ws, r, ref c, ed.Grade1DoubleDegree);
                SetDecimal(ws, r, ref c, ed.Grade2Total);
                SetDecimal(ws, r, ref c, ed.Grade2JobPercent);
                SetDecimal(ws, r, ref c, ed.Grade2TrainingFundPercent);
                SetDecimal(ws, r, ref c, ed.Grade2AgeHours);
                SetDecimal(ws, r, ref c, ed.Grade2MotherBenefitPercent);
                SetDecimal(ws, r, ref c, ed.Grade2TrainingBenefits);
                SetDecimal(ws, r, ref c, ed.Grade2DoubleDegree);
                ws.Cell(r, c++).Value = ed.Grade1GradeName ?? "";
                ws.Cell(r, c++).Value = ed.Grade1Grade ?? "";
                ws.Cell(r, c++).Value = ed.Grade1Role ?? "";
                ws.Cell(r, c++).Value = ed.Grade1Seniority ?? "";
                ws.Cell(r, c++).Value = ed.Grade2GradeName ?? "";
                ws.Cell(r, c++).Value = ed.Grade2Grade ?? "";
                ws.Cell(r, c++).Value = ed.Grade2Role ?? "";
                ws.Cell(r, c++).Value = ed.Grade2Seniority ?? "";
                r++;
            }
            ws.Row(1).Style.Font.Bold = true;
            ws.Columns().AdjustToContents(1, Math.Min(r - 1, 500));
        }

        private static void AddEmploymentSlotsSheet(XLWorkbook wb, IReadOnlyList<EmploymentData> employmentRows)
        {
            var ws = wb.Worksheets.Add("מקטעים");
            string[] h =
            [
                "מזהה נתון העסקה", "מזהה עובד", "ת.ז. עובד", "שנת לימודים", "רמת דרגה", "אינדקס מקטע",
                "סמל מוסד", "שעות שבועיות", "בסיס משרה", "מקטע הורה (שעות נוספות)"
            ];
            for (var i = 0; i < h.Length; i++)
                ws.Cell(1, i + 1).Value = h[i];

            var r = 2;
            foreach (var ed in employmentRows.OrderBy(x => x.Id))
            {
                var emp = ed.Employee;
                foreach (var slot in ed.Slots.OrderBy(s => s.GradeBand).ThenBy(s => s.SlotIndex))
                {
                    ws.Cell(r, 1).Value = ed.Id;
                    ws.Cell(r, 2).Value = ed.EmployeeId;
                    ws.Cell(r, 3).Value = emp?.IdNumber ?? "";
                    ws.Cell(r, 4).Value = ed.AcademicYear;
                    ws.Cell(r, 5).Value = slot.GradeBand;
                    ws.Cell(r, 6).Value = slot.SlotIndex;
                    ws.Cell(r, 7).Value = slot.InstitutionSymbol ?? "";
                    SetDecimal(ws, r, 8, slot.WeeklyHours);
                    SetDecimal(ws, r, 9, slot.JobBase);
                    if (slot.SupplementaryParentSlotIndex.HasValue)
                        ws.Cell(r, 10).Value = slot.SupplementaryParentSlotIndex.Value;
                    r++;
                }
            }
            ws.Row(1).Style.Font.Bold = true;
            ws.Columns().AdjustToContents(1, Math.Min(r - 1, 1000));
        }

        private static void SetDecimal(IXLWorksheet ws, int row, ref int col, decimal? value)
        {
            if (value.HasValue) ws.Cell(row, col).Value = value.Value;
            col++;
        }

        private static void SetDecimal(IXLWorksheet ws, int row, int col, decimal? value)
        {
            if (value.HasValue) ws.Cell(row, col).Value = value.Value;
        }
    }
}
