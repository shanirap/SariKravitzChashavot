using AccountingProject.Domain;
using AccountingProject.Models;
using ClosedXML.Excel;

namespace AccountingProject.Services;

/// <summary>בונה דוח השוואה שנתית — עמודת חודש עם V או פירוט פערים.</summary>
internal static class AnnualComparisonReportBuilder
{
    private const string SheetName = "השוואה שנתית";
    internal const string NotFoundInInput = "לא נמצא בקלט";
    internal const string NotCapturedInInput = "לא נקלט";

    private static readonly string[] StaticHeaders =
    [
        "סמל מוסד",
        "שם משפחה+שם פרטי",
        "תפקיד",
        "סוג משרה (מעוקץ)",
        "דרגה",
        "ותק",
        "ש\"ש",
        "בסיס משרה",
        "אחוז משרה",
        "הכפלה כללית",
    ];

    public static byte[] Build(
        IReadOnlyList<EmploymentData> records,
        string academicYear,
        Stream uploadedFile,
        Func<string, int> parseSeptemberGregorianYear)
    {
        if (uploadedFile.CanSeek)
            uploadedFile.Position = 0;

        using var uploadedWb = new XLWorkbook(uploadedFile);
        var sheet = uploadedWb.Worksheets.FirstOrDefault()
                    ?? throw new InvalidOperationException("בחוברת Excel אין גיליונות נתונים.");

        int sepYear;
        if (!HebrewAcademicYear.TryParseSeptemberGregorianYear(academicYear, out sepYear))
            throw new InvalidOperationException(HebrewAcademicYear.InvalidMessage);

        var monthSequence = SchoolYearGregorian.GetSchoolYearMonthSequence(sepYear);
        var layout = PayrollComparisonUploadSupport.ParseLayout(sheet);
        var allRows = PayrollComparisonUploadSupport.ParseAllRows(sheet, layout, academicYear, monthSequence);
        if (allRows.Count == 0)
            throw new InvalidOperationException("לא נמצאו שורות נתונים בקובץ לשנת הלימודים שנבחרה.");

        var source = AnnualComparisonUploadMonthSource.FromParsedUpload(layout, allRows);
        return BuildReport(records, monthSequence, source);
    }

    public static byte[] BuildFromSavedData(
        IReadOnlyList<EmploymentData> records,
        IReadOnlyList<(int Month, int GregorianYear)> monthSequence,
        IReadOnlyList<PayrollMonthlyInputBatch> activeBatches,
        IReadOnlyList<PayrollComparisonInputRow> comparisonRows,
        IReadOnlyDictionary<int, AnnualComparisonReportRowOverride>? overridesBySlotId = null) =>
        BuildReport(
            records,
            monthSequence,
            AnnualComparisonSavedMonthSource.FromSavedRows(activeBatches, comparisonRows),
            overridesBySlotId);

    private static byte[] BuildReport(
        IReadOnlyList<EmploymentData> records,
        IReadOnlyList<(int Month, int GregorianYear)> monthSequence,
        IAnnualComparisonMonthSource monthSource,
        IReadOnlyDictionary<int, AnnualComparisonReportRowOverride>? overridesBySlotId = null)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add(SheetName);
        var monthHeaders = monthSequence.Select(m => $"{m.Month}.{m.GregorianYear}").ToArray();
        var headers = StaticHeaders.Concat(monthHeaders).ToArray();
        for (var i = 0; i < headers.Length; i++)
            ws.Cell(1, i + 1).Value = headers[i];
        ws.Row(1).Style.Font.Bold = true;

        var outRow = 2;
        foreach (var ed in records.OrderBy(e => e.Employee?.LastName).ThenBy(e => e.Employee?.FirstName))
        {
            foreach (var slot in ed.Slots
                         .Where(s => !PayrollComparisonUploadSupport.SlotIsEmpty(s))
                         .OrderBy(s => s.GradeBand)
                         .ThenBy(s => s.SlotIndex))
            {
                WriteRow(ws, outRow, ed, slot, monthSequence, monthSource, overridesBySlotId);
                outRow++;
            }
        }

        if (outRow == 2)
            throw new InvalidOperationException("לא נמצאו מקטעי העסקה להשוואה.");

        var monthColStart = StaticHeaders.Length + 1;
        ws.Columns(1, headers.Length).AdjustToContents(1, Math.Max(outRow - 1, 2));
        for (var c = monthColStart; c <= headers.Length; c++)
            ws.Column(c).Style.Alignment.WrapText = true;

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    private static void WriteRow(
        IXLWorksheet ws,
        int row,
        EmploymentData ed,
        EmploymentDataSlot slot,
        IReadOnlyList<(int Month, int GregorianYear)> monthSequence,
        IAnnualComparisonMonthSource monthSource,
        IReadOnlyDictionary<int, AnnualComparisonReportRowOverride>? overridesBySlotId)
    {
        var computed = AnnualComparisonRowComputer.Compute(ed, slot, monthSequence, monthSource);
        AnnualComparisonReportRowOverride? ovr = null;
        overridesBySlotId?.TryGetValue(slot.Id, out ovr);
        var display = AnnualComparisonRowComputer.ToDisplay(computed, ovr);

        ws.Cell(row, 1).Value = display.InstitutionSymbol;
        ws.Cell(row, 2).Value = display.FullName;
        ws.Cell(row, 3).Value = display.Role;
        ws.Cell(row, 4).Value = display.SugMisraFromPayroll;
        ws.Cell(row, 5).Value = display.Grade;
        ws.Cell(row, 6).Value = display.Seniority;
        if (display.WeeklyHours.HasValue) ws.Cell(row, 7).Value = display.WeeklyHours.Value;
        if (display.JobBase.HasValue) ws.Cell(row, 8).Value = display.JobBase.Value;
        if (display.JobPercent.HasValue) ws.Cell(row, 9).Value = display.JobPercent.Value;
        ws.Cell(row, 10).Value = display.DoubleGeneral;

        var col = StaticHeaders.Length + 1;
        foreach (var (month, gregYear) in monthSequence)
        {
            var key = $"{month}.{gregYear}";
            var cellText = display.MonthCells.GetValueOrDefault(key) ?? NotCapturedInInput;
            var cell = ws.Cell(row, col);
            cell.Value = cellText;
            ApplyMonthCellStyle(cell, cellText);
            col++;
        }
    }

    internal static string BuildMonthCell(
        EmploymentData ed,
        EmploymentDataSlot slot,
        AnnualComparisonInputValues? input)
    {
        if (input == null)
            return NotFoundInInput;

        var mismatches = new List<string>();
        var values = input.Value;
        var g1 = slot.GradeBand == 1;

        if (!PayrollComparisonUploadSupport.IsMonthlyJobType(values.Role))
        {
            mismatches.Add(
                $"סוג משרה: קלט={FormatText(values.Role)}, נדרש={PayrollComparisonUploadSupport.ExpectedMonthlyJobType}");
        }

        var dbGrade = g1 ? ed.Grade1Grade : ed.Grade2Grade;
        if (!PayrollComparisonUploadSupport.TextEqual(dbGrade, values.Grade))
            mismatches.Add($"דרגה: מערכת={FormatText(dbGrade)}, קלט={FormatText(values.Grade)}");

        var dbSeniority = g1 ? ed.Grade1Seniority : ed.Grade2Seniority;
        if (!PayrollComparisonUploadSupport.SeniorityEqual(dbSeniority, values.Seniority))
            mismatches.Add($"ותק: מערכת={FormatText(dbSeniority)}, קלט={FormatText(values.Seniority)}");

        if (values.CompareJobBase
            && !PayrollComparisonUploadSupport.DecimalsEqual(slot.JobBase, values.JobBase))
        {
            mismatches.Add(
                $"בסיס משרה: מערכת={PayrollComparisonUploadSupport.FormatDecimal(slot.JobBase)}, קלט={PayrollComparisonUploadSupport.FormatDecimal(values.JobBase)}");
        }

        var dbPercent = g1 ? ed.Grade1JobPercent : ed.Grade2JobPercent;
        if (!PayrollComparisonUploadSupport.DecimalsEqual(dbPercent, values.JobPercent))
        {
            mismatches.Add(
                $"אחוז משרה: מערכת={PayrollComparisonUploadSupport.FormatDecimal(dbPercent)}, קלט={PayrollComparisonUploadSupport.FormatDecimal(values.JobPercent)}");
        }

        if (values.GeneralMultiplier is not (null or 0))
        {
            mismatches.Add(
                $"הכפלה כללית: קלט={PayrollComparisonUploadSupport.FormatDecimal(values.GeneralMultiplier)}, מערכת=0");
        }

        var sysHoursSum = PayrollComparisonUploadSupport.SumSystemWeeklyHours(ed, slot.GradeBand);
        if (!PayrollComparisonUploadSupport.DecimalsEqual(sysHoursSum, values.WeeklyHours))
        {
            mismatches.Add(
                $"ש\"ש: מערכת={PayrollComparisonUploadSupport.FormatDecimal(sysHoursSum)}, קלט={PayrollComparisonUploadSupport.FormatDecimal(values.WeeklyHours)}");
        }

        return mismatches.Count == 0 ? "V" : string.Join("; ", mismatches);
    }

    private static void ApplyMonthCellStyle(IXLCell cell, string text)
    {
        if (text is NotFoundInInput or NotCapturedInInput)
        {
            cell.Style.Fill.PatternType = XLFillPatternValues.Solid;
            cell.Style.Fill.BackgroundColor = XLColor.LightYellow;
            return;
        }

        if (text == "V")
        {
            cell.Style.Fill.PatternType = XLFillPatternValues.Solid;
            cell.Style.Fill.BackgroundColor = XLColor.LightGreen;
            return;
        }

        cell.Style.Fill.PatternType = XLFillPatternValues.Solid;
        cell.Style.Fill.BackgroundColor = XLColor.LightPink;
    }

    private static string FormatText(string? v) =>
        string.IsNullOrWhiteSpace(v) ? "ריק" : v.Trim();

}
