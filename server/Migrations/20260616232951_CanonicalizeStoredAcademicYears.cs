using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountingProject.Migrations
{
    /// <inheritdoc />
    /// <summary>
    /// Normalizes safely parseable academic-year aliases (numeric Gregorian/Hebrew) to canonical Hebrew labels.
    /// Unrecognized values are left unchanged for manual review.
    /// </summary>
    public partial class CanonicalizeStoredAcademicYears : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var conversionSql = BuildNumericYearConversionSql();
            var tables = new[]
            {
                "[נתוני_העסקה]",
                "[קלט_עוקץ_חודשי_אצווה]",
                "[קלט_עוקץ_חודשי_שורה]",
                "[דריסות_דוח_השוואה_שנתי]",
            };

            foreach (var table in tables)
            {
                migrationBuilder.Sql($"""
                    UPDATE {table}
                    SET [שנת_לימודים] = LTRIM(RTRIM([שנת_לימודים]));

                    {conversionSql.Replace("[נתוני_העסקה]", table)}
                    """);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Data normalization is not reversible.
        }

        private static string BuildNumericYearConversionSql()
        {
            var gregorianCases = Enumerable.Range(2000, 201)
                .Select(year =>
                {
                    var hebrew = FormatHebrewYear(year + 3760).Replace("'", "''");
                    return $"WHEN N'{year}' THEN N'{hebrew}'";
                });

            var hebrewNumberCases = Enumerable.Range(5001, 999)
                .Select(year =>
                {
                    var hebrew = FormatHebrewYear(year).Replace("'", "''");
                    return $"WHEN N'{year}' THEN N'{hebrew}'";
                });

            return $"""
                UPDATE [נתוני_העסקה]
                SET [שנת_לימודים] = CASE [שנת_לימודים]
                  {string.Join(Environment.NewLine + "  ", gregorianCases)}
                  {string.Join(Environment.NewLine + "  ", hebrewNumberCases)}
                  ELSE [שנת_לימודים]
                END;
                """;
        }

        private static string FormatHebrewYear(int hebrewYear)
        {
            var parts = new (int Value, string Letter)[]
            {
                (400, "ת"), (300, "ש"), (200, "ר"), (100, "ק"),
                (90, "צ"), (80, "פ"), (70, "ע"), (60, "ס"), (50, "נ"),
                (40, "מ"), (30, "ל"), (20, "כ"), (10, "י"),
                (9, "ט"), (8, "ח"), (7, "ז"), (6, "ו"), (5, "ה"),
                (4, "ד"), (3, "ג"), (2, "ב"), (1, "א"),
            };

            var remainder = hebrewYear % 1000;
            var letters = new List<string>();
            foreach (var (value, letter) in parts)
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

            var raw = string.Concat(letters);
            return raw.Length <= 1 ? raw + "'" : raw[..^1] + "\"" + raw[^1];
        }
    }
}
