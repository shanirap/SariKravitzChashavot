namespace AccountingProject.Domain
{
    public static class HebrewAcademicYear
    {
        private static readonly (int Value, string Letter)[] Parts =
        [
            (400, "ת"),
            (300, "ש"),
            (200, "ר"),
            (100, "ק"),
            (90, "צ"),
            (80, "פ"),
            (70, "ע"),
            (60, "ס"),
            (50, "נ"),
            (40, "מ"),
            (30, "ל"),
            (20, "כ"),
            (10, "י"),
            (9, "ט"),
            (8, "ח"),
            (7, "ז"),
            (6, "ו"),
            (5, "ה"),
            (4, "ד"),
            (3, "ג"),
            (2, "ב"),
            (1, "א")
        ];

        public static string? Normalize(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            var trimmed = value.Trim();
            if (!int.TryParse(trimmed, out var year)) return trimmed;

            if (year is >= 2000 and <= 2200)
                return Format(year + 3760);

            if (year is >= 5000 and <= 5999)
                return Format(year);

            return null;
        }

        public static string Format(int hebrewYear)
        {
            var remainder = hebrewYear % 1000;
            var letters = new List<string>();

            foreach (var (value, letter) in Parts)
            {
                while (remainder >= value)
                {
                    if (remainder == 15)
                    {
                        letters.Add("ט");
                        letters.Add("ו");
                        remainder = 0;
                        break;
                    }
                    if (remainder == 16)
                    {
                        letters.Add("ט");
                        letters.Add("ז");
                        remainder = 0;
                        break;
                    }

                    letters.Add(letter);
                    remainder -= value;
                }
            }

            return AddPunctuation(string.Concat(letters));
        }

        private static string AddPunctuation(string letters)
        {
            if (letters.Length == 0) return string.Empty;
            if (letters.Length == 1) return letters + "'";
            return letters[..^1] + "\"" + letters[^1];
        }
    }
}
