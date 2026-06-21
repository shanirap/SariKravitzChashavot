using System.Globalization;
using ClosedXML.Excel;

namespace AccountingProject.Infrastructure
{
    public static class ExcelDateParser
    {
        private static readonly string[] TextFormats =
        [
            "yyyy-MM-dd",
            "yyyy-M-d",
            "dd/MM/yyyy",
            "d/M/yyyy",
            "dd/M/yyyy",
            "d/MM/yyyy",
            "dd-MM-yyyy",
            "d-M-yyyy",
            "dd-M-yyyy",
            "d-MM-yyyy",
        ];

        public static bool TryParse(IXLCell cell, out DateOnly date)
        {
            date = default;
            var v = cell.Value;
            if (v.IsBlank)
                return false;
            if (v.IsDateTime)
            {
                date = DateOnly.FromDateTime(v.GetDateTime());
                return true;
            }

            return TryParseText(ExcelCellText.Get(cell), out date);
        }

        public static bool TryParseText(string? raw, out DateOnly date)
        {
            date = default;
            if (string.IsNullOrWhiteSpace(raw))
                return false;

            var s = raw.Trim();
            if (DateOnly.TryParseExact(s, TextFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                return true;

            if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var roundtrip))
            {
                date = DateOnly.FromDateTime(roundtrip);
                return true;
            }

            if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            {
                date = DateOnly.FromDateTime(dt);
                return true;
            }

            return TryParseDayFirstParts(s, out date);
        }

        private static bool TryParseDayFirstParts(string s, out DateOnly date)
        {
            date = default;
            var parts = s.Split(['/', '.'], StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 3)
                return false;

            if (!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var d) ||
                !int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var m) ||
                !int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var y))
            {
                return false;
            }

            if (y < 1000 || m is < 1 or > 12)
                return false;

            date = new DateOnly(y, m, Math.Clamp(d, 1, DateTime.DaysInMonth(y, m)));
            return true;
        }
    }
}
