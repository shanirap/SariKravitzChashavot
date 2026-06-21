namespace AccountingProject.Domain
{
    public static class HebrewAcademicYear
    {
        public const string InvalidMessage = "שנת לימודים לא תקינה.";

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

        private static readonly Dictionary<char, int> LetterValues = new()
        {
            ['ת'] = 400, ['ש'] = 300, ['ר'] = 200, ['ק'] = 100,
            ['צ'] = 90,  ['פ'] = 80,  ['ע'] = 70,  ['ס'] = 60,
            ['נ'] = 50,  ['מ'] = 40,  ['ל'] = 30,  ['כ'] = 20,
            ['י'] = 10,  ['ט'] = 9,   ['ח'] = 8,   ['ז'] = 7,
            ['ו'] = 6,   ['ה'] = 5,   ['ד'] = 4,   ['ג'] = 3,
            ['ב'] = 2,   ['א'] = 1
        };

        /// <summary>
        /// Normalize input (including numeric 5786 / 2026) and validate that it maps to a school year.
        /// </summary>
        public static bool TryValidateAndCanonicalize(string? value, out string canonical)
        {
            canonical = string.Empty;
            var normalized = Normalize(value);
            if (string.IsNullOrWhiteSpace(normalized))
                return false;
            if (!TryParseSeptemberGregorianYear(normalized, out _))
                return false;
            canonical = normalized;
            return true;
        }

        /// <summary>
        /// Canonical label for comparing stored values with API input (handles numeric aliases and punctuation variants).
        /// </summary>
        public static string CanonicalForComparison(string? value)
        {
            if (TryValidateAndCanonicalize(value, out var canonical))
                return canonical;
            var normalized = Normalize(value);
            return string.IsNullOrWhiteSpace(normalized) ? (value ?? "").Trim() : normalized;
        }

        /// <summary>Gregorian calendar year of September that starts the school year labeled by the Hebrew academic year.</summary>
        public static bool TryParseSeptemberGregorianYear(string? hebrewYear, out int septemberGregorianYear)
        {
            septemberGregorianYear = default;
            if (string.IsNullOrWhiteSpace(hebrewYear)) return false;
            var sum = hebrewYear.Where(c => LetterValues.ContainsKey(c)).Sum(c => LetterValues[c]);
            if (sum == 0) return false;
            septemberGregorianYear = 5000 + sum - 3761;
            return true;
        }

        /// <summary>1 September of the school year for the given Hebrew academic year label.</summary>
        public static DateOnly GetSchoolYearStartDate(string academicYear)
        {
            if (!TryParseSeptemberGregorianYear(academicYear, out var sepYear))
                throw new ArgumentException(InvalidMessage, nameof(academicYear));
            return new DateOnly(sepYear, 9, 1);
        }
    }
}
