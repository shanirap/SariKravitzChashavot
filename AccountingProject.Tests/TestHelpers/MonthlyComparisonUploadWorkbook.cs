using ClosedXML.Excel;

namespace AccountingProject.Tests.TestHelpers;

internal static class MonthlyComparisonUploadWorkbook
{
    /// <summary>קובץ עוקץ עם שורת כותרות בשורה 3 (כמו בקובץ לדוגמה).</summary>
    public static MemoryStream Create(
        string idNumber,
        int? employeeNumber,
        string fullName,
        int month,
        int year,
        Action<MonthlyComparisonUploadRowBuilder>? configureRow = null,
        bool includeYearColumn = true)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("עוקץ");
        ws.Cell(1, 1).Value = "כותרת עליונה";

        var headerRow = 3;
        var col = 1;
        ws.Cell(headerRow, col++).Value = "מספר עובד";
        ws.Cell(headerRow, col++).Value = "ת\"ז";
        ws.Cell(headerRow, col++).Value = "שם כולל";
        ws.Cell(headerRow, col++).Value = "חודש משכורת";
        if (includeYearColumn)
            ws.Cell(headerRow, col++).Value = "שנה";
        ws.Cell(headerRow, col++).Value = "סוג משרה";

        WriteBand1Headers(ws, headerRow, ref col);
        WriteBand2Headers(ws, headerRow, ref col);

        var dataRow = 4;
        col = 1;
        if (employeeNumber.HasValue) ws.Cell(dataRow, col).Value = employeeNumber.Value;
        col++;
        ws.Cell(dataRow, col++).Value = idNumber;
        ws.Cell(dataRow, col++).Value = fullName;
        ws.Cell(dataRow, col++).Value = month;
        if (includeYearColumn)
            ws.Cell(dataRow, col++).Value = year;
        ws.Cell(dataRow, col++).Value = "גננת";

        var builder = new MonthlyComparisonUploadRowBuilder(ws, dataRow, col);
        configureRow?.Invoke(builder);

        var ms = new MemoryStream();
        wb.SaveAs(ms);
        ms.Position = 0;
        return ms;
    }

    private static void WriteBand1Headers(IXLWorksheet ws, int headerRow, ref int col)
    {
        ws.Cell(headerRow, col++).Value = "שם הדירוג";
        ws.Cell(headerRow, col++).Value = "דרגה";
        ws.Cell(headerRow, col++).Value = "ותק רגיל/רמת מורכבות";
        ws.Cell(headerRow, col++).Value = "אחוז משרה מחושב";
        ws.Cell(headerRow, col++).Value = "אחוז תוספת אם";
        for (var i = 1; i <= 6; i++)
        {
            ws.Cell(headerRow, col++).Value = $"מישרה {i}-שעות";
            ws.Cell(headerRow, col++).Value = $"מישרה {i}-מתוך שעות";
        }
        ws.Cell(headerRow, col++).Value = "אחוז הפרשה לקרן השתלמות";
        ws.Cell(headerRow, col++).Value = "שעות גיל מחושב";
        ws.Cell(headerRow, col++).Value = "הכפלה כללית באחוז";
    }

    private static void WriteBand2Headers(IXLWorksheet ws, int headerRow, ref int col)
    {
        ws.Cell(headerRow, col++).Value = "שם הדירוג";
        ws.Cell(headerRow, col++).Value = "דרגה";
        ws.Cell(headerRow, col++).Value = "ותק רגיל/רמת מורכבות";
        ws.Cell(headerRow, col++).Value = "אחוז משרה מחושב";
        ws.Cell(headerRow, col++).Value = "אחוז תוספת אם";
        for (var i = 1; i <= 6; i++)
        {
            ws.Cell(headerRow, col++).Value = $"מישרה {i}-שעות";
            ws.Cell(headerRow, col++).Value = $"מישרה {i}-מתוך שעות";
        }
        ws.Cell(headerRow, col++).Value = "אחוז הפרשה לקרן השתלמות";
        ws.Cell(headerRow, col++).Value = "שעות גיל מחושב";
        ws.Cell(headerRow, col++).Value = "הכפלה כללית באחוז";
    }
}

internal sealed class MonthlyComparisonUploadRowBuilder(IXLWorksheet ws, int dataRow, int band1StartCol)
{
    private int _col = band1StartCol;

    public MonthlyComparisonUploadRowBuilder Band1(
        string grade = "ב",
        string seniority = "5",
        decimal jobPercent = 100m,
        decimal misra1Hours = 30m,
        decimal misra1Base = 28m,
        decimal misra2Hours = 0m,
        decimal misra2Base = 0m,
        decimal ageHours = 2m,
        decimal trainingFund = 7.5m,
        decimal doubleGeneral = 0m)
    {
        _col++; // שם הדירוג
        ws.Cell(dataRow, _col++).Value = grade;
        ws.Cell(dataRow, _col++).Value = seniority;
        ws.Cell(dataRow, _col++).Value = jobPercent;
        _col++; // אחוז תוספת אם
        ws.Cell(dataRow, _col++).Value = misra1Hours;
        ws.Cell(dataRow, _col++).Value = misra1Base;
        if (misra2Hours != 0m)
        {
            ws.Cell(dataRow, _col++).Value = misra2Hours;
            if (misra2Base != 0m)
                ws.Cell(dataRow, _col++).Value = misra2Base;
            else
                _col++;
        }
        for (var i = misra2Hours != 0m ? 3 : 2; i <= 6; i++)
            _col += 2;
        ws.Cell(dataRow, _col++).Value = trainingFund;
        ws.Cell(dataRow, _col++).Value = ageHours;
        ws.Cell(dataRow, _col++).Value = doubleGeneral;
        return this;
    }

    public MonthlyComparisonUploadRowBuilder Band2(
        string grade = "ב",
        string seniority = "3",
        decimal jobPercent = 50m,
        decimal misra1Hours = 10m,
        decimal misra1Base = 9m,
        decimal ageHours = 0m,
        decimal trainingFund = 5m,
        decimal doubleGeneral = 0m)
    {
        _col++; // שם הדירוג
        ws.Cell(dataRow, _col++).Value = grade;
        ws.Cell(dataRow, _col++).Value = seniority;
        ws.Cell(dataRow, _col++).Value = jobPercent;
        _col++; // אחוז תוספת אם
        ws.Cell(dataRow, _col++).Value = misra1Hours;
        ws.Cell(dataRow, _col++).Value = misra1Base;
        for (var i = 2; i <= 6; i++)
        {
            _col += 2;
        }
        ws.Cell(dataRow, _col++).Value = trainingFund;
        ws.Cell(dataRow, _col++).Value = ageHours;
        ws.Cell(dataRow, _col++).Value = doubleGeneral;
        return this;
    }
}
