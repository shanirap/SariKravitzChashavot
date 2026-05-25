using ClosedXML.Excel;

namespace AccountingProject.Infrastructure
{
    public static class ExcelCellText
    {
        public static string Get(IXLCell cell)
        {
            var v = cell.Value;
            if (v.IsBlank) return string.Empty;
            if (v.IsText) return v.GetText();
            if (v.IsNumber) return v.GetNumber().ToString(System.Globalization.CultureInfo.InvariantCulture);
            if (v.IsBoolean) return v.GetBoolean().ToString();
            if (v.IsDateTime) return v.GetDateTime().ToString("o");
            if (v.IsTimeSpan) return v.GetTimeSpan().ToString();
            if (v.IsError) return string.Empty;
            return v.ToString() ?? string.Empty;
        }
    }
}
