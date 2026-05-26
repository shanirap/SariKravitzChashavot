using ClosedXML.Excel;

namespace AccountingProject.Tests.TestHelpers;

internal static class InvalidUploadWorkbooks
{
    /// <summary>גיליון ללא כותרות עוקץ מזוהות — לא יימצאו שורות נתונים.</summary>
    public static MemoryStream NoPayrollHeaders()
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("נתונים");
        ws.Cell(1, 1).Value = "עמודה א";
        ws.Cell(1, 2).Value = "עמודה ב";
        ws.Cell(2, 1).Value = "ערך";
        var ms = new MemoryStream();
        wb.SaveAs(ms);
        ms.Position = 0;
        return ms;
    }

    /// <summary>כותרות תקינות אך חודש שלא בקובץ (חודש 10 בלבד, בקשה לחודש 9).</summary>
    public static MemoryStream WrongMonthOnly()
    {
        return MonthlyComparisonUploadWorkbook.Create(
            "123456789", null, "Worker", 10, 2025, b => b.Band1());
    }
}
