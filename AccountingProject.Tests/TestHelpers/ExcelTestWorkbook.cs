using ClosedXML.Excel;

namespace AccountingProject.Tests.TestHelpers;

internal static class ExcelTestWorkbook
{
    /// <summary>קובץ מינימלי להשוואת שכר: תז + חודש + שנה גרגוריאניים.</summary>
    public static MemoryStream CreatePayrollComparisonUpload(string idNumber, int month, int gregorianYear)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("עוקץ");
        ws.Cell(1, 1).Value = "תז";
        ws.Cell(1, 2).Value = "חודש";
        ws.Cell(1, 3).Value = "שנה";
        ws.Cell(2, 1).Value = idNumber;
        ws.Cell(2, 2).Value = month;
        ws.Cell(2, 3).Value = gregorianYear;
        var ms = new MemoryStream();
        wb.SaveAs(ms);
        ms.Position = 0;
        return ms;
    }
}
