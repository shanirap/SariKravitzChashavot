using ClosedXML.Excel;

namespace AccountingProject.Tests.TestHelpers;

internal static class BulkImportEmployerWorkbook
{
    public static MemoryStream Create(params (string BusinessNumber, string Name)[] rows)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("מעסיקים");
        ws.Cell(1, 1).Value = "חפ";
        ws.Cell(1, 2).Value = "שם_מעסיק";
        ws.Cell(1, 3).Value = "סמל_מוטב";
        ws.Cell(1, 4).Value = "מספר_עוקץ";

        for (var i = 0; i < rows.Length; i++)
        {
            var row = i + 2;
            ws.Cell(row, 1).Value = rows[i].BusinessNumber;
            ws.Cell(row, 2).Value = rows[i].Name;
        }

        var ms = new MemoryStream();
        wb.SaveAs(ms);
        ms.Position = 0;
        return ms;
    }
}
