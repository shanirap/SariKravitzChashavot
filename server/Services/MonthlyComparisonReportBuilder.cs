using AccountingProject.Models;

using ClosedXML.Excel;



namespace AccountingProject.Services;



/// <summary>בונה דוח השוואה חודשית — 3 שורות לכל מקטע: מצבת / עוקץ / V-X.</summary>

internal static class MonthlyComparisonReportBuilder

{

    private const string SheetName = "השוואה חודשית";

    private const string LabelMitzvat = "מצבת- מערכת שכר";

    private const string LabelOkets = "עוקץ- קלט";

    private const string LabelCompare = "השוואה- V/X";



    private static readonly string[] OutputHeaders =

    [

        "סמל מוסד",

        "מספר עובד בעוקץ",

        "ת\"ז",

        "שם פרטי+שם משפחה",

        "תפקיד",

        "דרגה",

        "ותק",

        "ש\"ש",

        "בסיס משרה",

        "אחוז משרה",

        "שעות גיל",

        "גמולי השתלמות",

        "כפל תואר",

        "קרן השתלמות",

        "הכפלה כללית",

    ];



    private const int LabelCol = 1;

    private const int DataColStart = 2;

    private const int ColSymbol = 2;

    private const int ColEmployeeNumber = 3;

    private const int ColIdNumber = 4;

    private const int ColName = 5;

    private const int ColRole = 6;

    private const int ColGrade = 7;

    private const int ColSeniority = 8;

    private const int ColHoursSum = 9;

    private const int ColJobBase = 10;

    private const int ColJobPercent = 11;

    private const int ColAgeHours = 12;

    private const int ColTrainingBenefits = 13;

    private const int ColDoubleDegree = 14;

    private const int ColTrainingFund = 15;

    private const int ColDoubleGeneral = 16;



    public static byte[] Build(

        IReadOnlyList<EmploymentData> records,

        string academicYear,

        int month,

        Stream uploadedFile,

        Func<string, int> parseSeptemberGregorianYear)

    {

        if (uploadedFile.CanSeek)

            uploadedFile.Position = 0;



        using var uploadedWb = new XLWorkbook(uploadedFile);

        var sheet = uploadedWb.Worksheets.FirstOrDefault()

                    ?? throw new InvalidOperationException("בחוברת Excel אין גיליונות נתונים.");



        var layout = PayrollComparisonUploadSupport.ParseLayout(sheet);

        var expectedYear = PayrollComparisonUploadSupport.ResolveExpectedGregorianYear(

            academicYear, month, parseSeptemberGregorianYear);

        var uploadRows = PayrollComparisonUploadSupport.ParseRowsForMonth(

            sheet, layout, month, expectedYear, academicYear);



        if (uploadRows.Count == 0)

            throw new InvalidOperationException($"לא נמצאו שורות נתונים בקובץ לחודש {month}.");



        var byTz = PayrollComparisonUploadSupport.IndexByTzMonth(uploadRows);

        var byNum = PayrollComparisonUploadSupport.IndexByEmpNumMonth(uploadRows);



        using var wb = new XLWorkbook();

        var ws = wb.Worksheets.Add(SheetName);

        WriteHeaderRow(ws);



        var outRow = 2;

        var isFirstBlock = true;

        foreach (var ed in records.OrderBy(e => e.Employee?.LastName).ThenBy(e => e.Employee?.FirstName))

        {

            var upload = PayrollComparisonUploadSupport.ResolveForEmployee(

                ed.Employee, month, expectedYear, byTz, byNum);



            foreach (var slot in ed.Slots

                         .Where(s => !PayrollComparisonUploadSupport.SlotIsEmpty(s))

                         .OrderBy(s => s.GradeBand)

                         .ThenBy(s => s.SlotIndex))

            {

                if (!isFirstBlock)

                    outRow++;

                isFirstBlock = false;



                var bandCols = PayrollComparisonUploadSupport.BandCols(layout, slot.GradeBand);

                var system = BuildSystemValues(ed, slot, upload, layout, bandCols);

                var input = BuildInputValues(ed, slot, upload, layout, bandCols);



                WriteDataRow(ws, outRow, LabelMitzvat, system);
                WriteDataRow(ws, outRow + 1, LabelOkets, input);
                HighlightInputDoubleGeneralIfNeeded(ws, outRow + 1, input.DoubleGeneral);
                WriteCompareRow(ws, outRow + 2, system, input);

                outRow += 3;

            }

        }



        if (outRow == 2)

            throw new InvalidOperationException("לא נמצאו מקטעי העסקה להשוואה.");



        ws.Columns(LabelCol, ColDoubleGeneral).AdjustToContents(1, Math.Max(outRow - 1, 2));

        using var ms = new MemoryStream();

        wb.SaveAs(ms);

        return ms.ToArray();

    }



    private static void WriteHeaderRow(IXLWorksheet ws)

    {

        for (var i = 0; i < OutputHeaders.Length; i++)

            ws.Cell(1, DataColStart + i).Value = OutputHeaders[i];

        ws.Row(1).Style.Font.Bold = true;

    }



    private static void WriteDataRow(IXLWorksheet ws, int row, string label, FieldValues values)

    {

        ws.Cell(row, LabelCol).Value = label;

        WriteValues(ws, row, values);

    }



    private static void WriteValues(IXLWorksheet ws, int row, FieldValues v)

    {

        ws.Cell(row, ColSymbol).Value = v.Symbol;

        SetText(ws, row, ColEmployeeNumber, v.EmployeeNumber);

        SetText(ws, row, ColIdNumber, v.IdNumber);

        SetText(ws, row, ColName, v.Name);

        SetText(ws, row, ColRole, v.Role);

        SetText(ws, row, ColGrade, v.Grade);

        SetText(ws, row, ColSeniority, v.Seniority);

        SetDecimal(ws, row, ColHoursSum, v.HoursSum);

        SetDecimal(ws, row, ColJobBase, v.JobBase);

        SetDecimal(ws, row, ColJobPercent, v.JobPercent);

        SetDecimal(ws, row, ColAgeHours, v.AgeHours);

        SetDecimal(ws, row, ColTrainingBenefits, v.TrainingBenefits);

        SetDecimal(ws, row, ColDoubleDegree, v.DoubleDegree);

        SetDecimal(ws, row, ColTrainingFund, v.TrainingFund);

        SetDecimal(ws, row, ColDoubleGeneral, v.DoubleGeneral);

    }



    private static void WriteCompareRow(IXLWorksheet ws, int row, FieldValues system, FieldValues input)

    {

        ws.Cell(row, LabelCol).Value = LabelCompare;

        WriteCompareCell(ws, row, ColSymbol, system.Symbol, input.Symbol);

        WriteCompareCell(ws, row, ColEmployeeNumber, system.EmployeeNumber, input.EmployeeNumber);

        WriteCompareCell(ws, row, ColIdNumber, system.IdNumber, input.IdNumber);

        WriteCompareCell(ws, row, ColName, system.Name, input.Name);

        WriteCompareCell(ws, row, ColRole, system.Role, input.Role);

        WriteCompareCell(ws, row, ColGrade, system.Grade, input.Grade);

        WriteCompareCell(ws, row, ColSeniority, system.Seniority, input.Seniority);

        WriteCompareCell(ws, row, ColHoursSum, system.HoursSum, input.HoursSum, numeric: true);

        WriteCompareCell(ws, row, ColJobBase, system.JobBase, input.JobBase, numeric: true);

        WriteCompareCell(ws, row, ColJobPercent, system.JobPercent, input.JobPercent, numeric: true);

        WriteCompareCell(ws, row, ColAgeHours, system.AgeHours, input.AgeHours, numeric: true);

        WriteCompareCell(ws, row, ColTrainingBenefits, system.TrainingBenefits, input.TrainingBenefits, numeric: true);

        WriteCompareCell(ws, row, ColDoubleDegree, system.DoubleDegree, input.DoubleDegree, numeric: true);

        WriteCompareCell(ws, row, ColTrainingFund, system.TrainingFund, input.TrainingFund, numeric: true);

        WriteCompareDoubleGeneralCompare(ws, row, input.DoubleGeneral);

    }



    private static void WriteCompareCell(

        IXLWorksheet ws,

        int row,

        int col,

        string? system,

        string? input,

        bool numeric = false)

    {

        var match = numeric

            ? PayrollComparisonUploadSupport.DecimalsEqual(

                ParseDecimalOrNull(system), ParseDecimalOrNull(input))

            : PayrollComparisonUploadSupport.TextEqual(system, input);

        ApplyCompareMark(ws.Cell(row, col), match);

    }



    private static void WriteCompareDoubleGeneralCompare(IXLWorksheet ws, int row, string? inputRaw)

    {

        var inputVal = ParseDecimalOrNull(inputRaw);

        var match = inputVal is null or 0;

        var cell = ws.Cell(row, ColDoubleGeneral);

        cell.Value = match ? "V" : "X";

        cell.Style.Fill.PatternType = XLFillPatternValues.Solid;

        cell.Style.Fill.BackgroundColor = match ? XLColor.LightGreen : XLColor.Yellow;

    }



    private static void HighlightInputDoubleGeneralIfNeeded(IXLWorksheet ws, int row, string? inputDoubleGeneral)
    {
        var val = ParseDecimalOrNull(inputDoubleGeneral);
        if (val is null or 0) return;
        var cell = ws.Cell(row, ColDoubleGeneral);
        cell.Style.Fill.PatternType = XLFillPatternValues.Solid;
        cell.Style.Fill.BackgroundColor = XLColor.Yellow;
    }

    private static void ApplyCompareMark(IXLCell cell, bool match)

    {

        cell.Value = match ? "V" : "X";

        cell.Style.Fill.PatternType = XLFillPatternValues.Solid;

        cell.Style.Fill.BackgroundColor = match ? XLColor.LightGreen : XLColor.LightYellow;

    }



    private static FieldValues BuildSystemValues(

        EmploymentData ed,

        EmploymentDataSlot slot,

        PayrollUploadRow? upload,

        PayrollUploadLayout layout,

        PayrollBandColumns bandCols)

    {

        var g1 = slot.GradeBand == 1;

        return new FieldValues(

            Symbol: slot.InstitutionSymbol?.Trim() ?? "",

            EmployeeNumber: FormatInt(ed.Employee?.EmployeeNumber),

            IdNumber: ed.Employee?.IdNumber?.Trim() ?? "",

            Name: ed.Employee?.FullName ?? "",

            Role: (g1 ? ed.Grade1Role : ed.Grade2Role) ?? "",

            Grade: (g1 ? ed.Grade1Grade : ed.Grade2Grade) ?? "",

            Seniority: (g1 ? ed.Grade1Seniority : ed.Grade2Seniority) ?? "",

            HoursSum: FormatDecimal(PayrollComparisonUploadSupport.SumSystemWeeklyHours(ed, slot.GradeBand)),

            JobBase: FormatDecimal(slot.JobBase),

            JobPercent: FormatDecimal(g1 ? ed.Grade1JobPercent : ed.Grade2JobPercent),

            AgeHours: FormatDecimal(g1 ? ed.Grade1AgeHours : ed.Grade2AgeHours),

            TrainingBenefits: FormatDecimal(g1 ? ed.Grade1TrainingBenefits : ed.Grade2TrainingBenefits),

            DoubleDegree: FormatDecimal(g1 ? ed.Grade1DoubleDegree : ed.Grade2DoubleDegree),

            TrainingFund: FormatDecimal(g1 ? ed.Grade1TrainingFundPercent : ed.Grade2TrainingFundPercent),

            DoubleGeneral: FormatDecimal(0m));

    }



    private static FieldValues BuildInputValues(

        EmploymentData ed,

        EmploymentDataSlot slot,

        PayrollUploadRow? upload,

        PayrollUploadLayout layout,

        PayrollBandColumns bandCols)

    {

        if (upload == null)

        {

            return new FieldValues(

                Symbol: "", EmployeeNumber: "", IdNumber: "", Name: "",

                Role: "", Grade: "", Seniority: "",

                HoursSum: "", JobBase: "", JobPercent: "",

                AgeHours: "", TrainingBenefits: "", DoubleDegree: "",

                TrainingFund: "", DoubleGeneral: "");

        }



        var empNum = upload.EmployeeNumber?.ToString()

                     ?? PayrollComparisonUploadSupport.GetCell(upload.Row, layout.ColEmployeeNumber);

        var tz = upload.RawIdNumber

                 ?? PayrollComparisonUploadSupport.GetCell(upload.Row, layout.ColTz);



        return new FieldValues(

            Symbol: slot.InstitutionSymbol?.Trim() ?? "",

            EmployeeNumber: NormalizeDisplay(empNum),

            IdNumber: NormalizeDisplay(tz),

            Name: PayrollComparisonUploadSupport.GetCell(upload.Row, layout.ColFullName),

            Role: PayrollComparisonUploadSupport.GetCell(upload.Row, layout.ColSugMisra),

            Grade: PayrollComparisonUploadSupport.GetCell(upload.Row, bandCols.Grade),

            Seniority: PayrollComparisonUploadSupport.GetCell(upload.Row, bandCols.Seniority),

            HoursSum: FormatDecimal(PayrollComparisonUploadSupport.SumMisraHours(upload, bandCols)),

            JobBase: FormatDecimal(PayrollComparisonUploadSupport.ReadSlotJobBase(upload.Row, bandCols, slot.SlotIndex)),

            JobPercent: FormatDecimal(PayrollComparisonUploadSupport.ParseDecimal(

                PayrollComparisonUploadSupport.GetCell(upload.Row, bandCols.JobPercent))),

            AgeHours: FormatDecimal(PayrollComparisonUploadSupport.ParseDecimal(

                PayrollComparisonUploadSupport.GetCell(upload.Row, bandCols.AgeHours))),

            TrainingBenefits: FormatDecimal(PayrollComparisonUploadSupport.ReadOptionalDecimal(upload.Row, bandCols.TrainingBenefits)),

            DoubleDegree: FormatDecimal(PayrollComparisonUploadSupport.ReadOptionalDecimal(upload.Row, bandCols.DoubleDegree)),

            TrainingFund: FormatDecimal(PayrollComparisonUploadSupport.ParseDecimal(

                PayrollComparisonUploadSupport.GetCell(upload.Row, bandCols.TrainingFund))),

            DoubleGeneral: FormatDecimal(PayrollComparisonUploadSupport.ParseDecimal(

                PayrollComparisonUploadSupport.GetCell(upload.Row, bandCols.DoubleGeneral))));

    }



    private static void SetText(IXLWorksheet ws, int row, int col, string? value)

    {

        if (!string.IsNullOrEmpty(value))

            ws.Cell(row, col).Value = value;

    }



    private static void SetDecimal(IXLWorksheet ws, int row, int col, string? value)

    {

        var d = ParseDecimalOrNull(value);

        if (d.HasValue)

            ws.Cell(row, col).Value = d.Value;

    }



    private static string FormatInt(int? value) =>

        value.HasValue ? value.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) : "";



    private static string FormatDecimal(decimal? value) =>

        PayrollComparisonUploadSupport.FormatDecimal(value);



    private static string NormalizeDisplay(string? value) =>

        string.IsNullOrWhiteSpace(value) ? "" : value.Trim();



    private static decimal? ParseDecimalOrNull(string? raw) =>

        PayrollComparisonUploadSupport.ParseDecimal(raw);



    private sealed record FieldValues(

        string Symbol,

        string EmployeeNumber,

        string IdNumber,

        string Name,

        string Role,

        string Grade,

        string Seniority,

        string HoursSum,

        string JobBase,

        string JobPercent,

        string AgeHours,

        string TrainingBenefits,

        string DoubleDegree,

        string TrainingFund,

        string DoubleGeneral);

}


