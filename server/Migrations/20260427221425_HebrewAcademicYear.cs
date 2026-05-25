using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountingProject.Migrations
{
    /// <inheritdoc />
    public partial class HebrewAcademicYear : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_נתוני_העסקה_מזהה_עובד_מזהה_מעסיק_שנת_לימודים",
                table: "נתוני_העסקה");

            migrationBuilder.AlterColumn<string>(
                name: "שנת_לימודים",
                table: "נתוני_העסקה",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.Sql(BuildConversionSql(toHebrew: true));

            migrationBuilder.CreateIndex(
                name: "IX_נתוני_העסקה_מזהה_עובד_מזהה_מעסיק_שנת_לימודים",
                table: "נתוני_העסקה",
                columns: new[] { "מזהה_עובד", "מזהה_מעסיק", "שנת_לימודים" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_נתוני_העסקה_מזהה_עובד_מזהה_מעסיק_שנת_לימודים",
                table: "נתוני_העסקה");

            migrationBuilder.Sql(BuildConversionSql(toHebrew: false));

            migrationBuilder.AlterColumn<int>(
                name: "שנת_לימודים",
                table: "נתוני_העסקה",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.CreateIndex(
                name: "IX_נתוני_העסקה_מזהה_עובד_מזהה_מעסיק_שנת_לימודים",
                table: "נתוני_העסקה",
                columns: new[] { "מזהה_עובד", "מזהה_מעסיק", "שנת_לימודים" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        private static string BuildConversionSql(bool toHebrew)
        {
            var cases = Enumerable.Range(2000, 201)
                .Select(year =>
                {
                    var hebrew = FormatHebrewYear(year + 3760).Replace("'", "''");
                    return toHebrew
                        ? $"WHEN N'{year}' THEN N'{hebrew}'"
                        : $"WHEN N'{hebrew}' THEN N'{year}'";
                });

            return $@"
UPDATE [נתוני_העסקה]
SET [שנת_לימודים] = CASE [שנת_לימודים]
  {string.Join(Environment.NewLine + "  ", cases)}
  ELSE [שנת_לימודים]
END;";
        }

        private static string FormatHebrewYear(int hebrewYear)
        {
            var parts = new (int Value, string Letter)[]
            {
                (400, "ת"), (300, "ש"), (200, "ר"), (100, "ק"),
                (90, "צ"), (80, "פ"), (70, "ע"), (60, "ס"), (50, "נ"),
                (40, "מ"), (30, "ל"), (20, "כ"), (10, "י"),
                (9, "ט"), (8, "ח"), (7, "ז"), (6, "ו"), (5, "ה"),
                (4, "ד"), (3, "ג"), (2, "ב"), (1, "א")
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
