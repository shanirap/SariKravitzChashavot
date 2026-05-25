using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountingProject.Migrations
{
    public partial class EmploymentDataRestructure : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DECLARE @rid int = OBJECT_ID(N'[נתוני_העסקה]', N'U');
IF @rid IS NOT NULL
BEGIN
  DECLARE @dropFk nvarchar(max) = N'';
  SELECT @dropFk = @dropFk + N'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(fk.parent_object_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(10)
  FROM sys.foreign_keys fk
  WHERE fk.referenced_object_id = @rid;
  IF (@dropFk <> N'') EXEC sp_executesql @dropFk;
END
IF OBJECT_ID(N'[נתוני_העסקה_מקטע]', N'U') IS NOT NULL DROP TABLE [נתוני_העסקה_מקטע];
IF OBJECT_ID(N'[נתוני_העסקה]', N'U') IS NOT NULL DROP TABLE [נתוני_העסקה];
");

            migrationBuilder.CreateTable(
                name: "נתוני_העסקה",
                columns: table => new
                {
                    מזהה_נתון_העסקה = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    מזהה_עובד = table.Column<int>(type: "int", nullable: false),
                    מזהה_מעסיק = table.Column<int>(type: "int", nullable: false),
                    שנת_לימודים = table.Column<int>(type: "int", nullable: false),
                    דרגה1_סהכ = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    דרגה1_אחוז_משרה = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    דרגה1_קרן_השתלמות_אחוז = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    דרגה1_שעות_גיל = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    דרגה1_אחוז_הטבה_אם = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    דרגה2_סהכ = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    דרגה2_אחוז_משרה = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    דרגה2_קרן_השתלמות_אחוז = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    דרגה2_שעות_גיל = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    דרגה2_אחוז_הטבה_אם = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_נתוני_העסקה", x => x.מזהה_נתון_העסקה);
                    table.ForeignKey(
                        name: "FK_נתוני_העסקה_מעסיקים_מזהה_מעסיק",
                        column: x => x.מזהה_מעסיק,
                        principalTable: "מעסיקים",
                        principalColumn: "מזהה_מעסיק",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_נתוני_העסקה_עובדים_מזהה_עובד",
                        column: x => x.מזהה_עובד,
                        principalTable: "עובדים",
                        principalColumn: "מזהה_עובד",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "נתוני_העסקה_מקטע",
                columns: table => new
                {
                    מזהה_מקטע = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    מזהה_נתון_העסקה = table.Column<int>(type: "int", nullable: false),
                    רמת_דרגה = table.Column<byte>(type: "tinyint", nullable: false),
                    אינדקס_מקטע = table.Column<byte>(type: "tinyint", nullable: false),
                    שם_הדירוג = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    דרגה = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    תפקיד = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ותק = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    סמל_מוסד = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    שבוע_שעות = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    בסיס_משרה = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_נתוני_העסקה_מקטע", x => x.מזהה_מקטע);
                    table.ForeignKey(
                        name: "FK_נתוני_העסקה_מקטע_נתוני_העסקה_מזהה_נתון_העסקה",
                        column: x => x.מזהה_נתון_העסקה,
                        principalTable: "נתוני_העסקה",
                        principalColumn: "מזהה_נתון_העסקה",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_נתוני_העסקה_מזהה_מעסיק",
                table: "נתוני_העסקה",
                column: "מזהה_מעסיק");

            migrationBuilder.CreateIndex(
                name: "IX_נתוני_העסקה_מזהה_עובד_מזהה_מעסיק_שנת_לימודים",
                table: "נתוני_העסקה",
                columns: new[] { "מזהה_עובד", "מזהה_מעסיק", "שנת_לימודים" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_נתוני_העסקה_מקטע_מזהה_נתון_העסקה_רמת_דרגה_אינדקס_מקטע",
                table: "נתוני_העסקה_מקטע",
                columns: new[] { "מזהה_נתון_העסקה", "רמת_דרגה", "אינדקס_מקטע" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "נתוני_העסקה_מקטע");

            migrationBuilder.DropTable(
                name: "נתוני_העסקה");
        }
    }
}
