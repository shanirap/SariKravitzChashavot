using ClosedXML.Excel;

namespace AccountingProject.Tests.TestHelpers;

internal static class BulkImportEmployeeWorkbook
{
    public static MemoryStream CreateMinimalRow(
        string employerName,
        string idNumber,
        string academicYear = "תשפ\"ו",
        bool includeEmployerColumn = true,
        Action<IXLWorksheet, int>? fillRow = null)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("עובדים");
        var headers = BuildHeaders(includeEmployerColumn);
        for (var i = 0; i < headers.Count; i++)
            ws.Cell(1, i + 1).Value = headers[i];

        var row = 2;
        var col = 1;
        if (includeEmployerColumn)
        {
            ws.Cell(row, col++).Value = employerName;
            col++; // חפ — optional; fillRow may set via ColumnIndex
        }
        ws.Cell(row, col++).Value = idNumber;
        col++; // מספר_עובד_בעוקץ — optional, filled by callback
        ws.Cell(row, col++).Value = "כהן";
        ws.Cell(row, col++).Value = "רחל";
        ws.Cell(row, col++).Value = "נקבה";
        ws.Cell(row, col++).Value = "1990-01-15";
        col++; // טל
        col += 10; // children
        ws.Cell(row, col++).Value = academicYear;

        fillRow?.Invoke(ws, row);

        var ms = new MemoryStream();
        wb.SaveAs(ms);
        ms.Position = 0;
        return ms;
    }

    private static List<string> BuildHeaders(bool includeEmployer)
    {
        var cols = new List<string>
        {
            "תז", "מספר_עובד_בעוקץ", "שם_משפחה", "שם_פרטי", "מין", "תאריך_לידה", "טל",
            "תאריך_לידה_ילד_1", "תאריך_לידה_ילד_2", "תאריך_לידה_ילד_3", "תאריך_לידה_ילד_4",
            "תאריך_לידה_ילד_5", "תאריך_לידה_ילד_6", "תאריך_לידה_ילד_7", "תאריך_לידה_ילד_8",
            "תאריך_לידה_ילד_9", "תאריך_לידה_ילד_10", "שנת_לימודים",
        };
        if (includeEmployer)
        {
            cols.Insert(0, "שם_מעסיק");
            cols.Insert(1, "חפ");
        }
        return cols;
    }

    public static int ColumnIndex(bool includeEmployer, string header)
    {
        var headers = BuildHeaders(includeEmployer);
        return headers.IndexOf(header) + 1;
    }
}
