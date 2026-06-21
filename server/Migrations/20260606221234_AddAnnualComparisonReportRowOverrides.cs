using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountingProject.Migrations
{
    /// <inheritdoc />
    public partial class AddAnnualComparisonReportRowOverrides : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "דריסות_דוח_השוואה_שנתי",
                columns: table => new
                {
                    מזהה = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    מזהה_מעסיק = table.Column<int>(type: "int", nullable: false),
                    שנת_לימודים = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    מזהה_מקטע = table.Column<int>(type: "int", nullable: false),
                    סמל_מוסד = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    שם_מלא = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    תפקיד = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    סוג_משרה_מעוקץ = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    דרגה = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ותק = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    שעות_שבועיות = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    בסיס_משרה = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    אחוז_משרה = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    הכפלה_כללית = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    תאי_חודש_json = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    נערך_ידנית = table.Column<bool>(type: "bit", nullable: false),
                    הערת_עריכה = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    נוצר_בתאריך = table.Column<DateTime>(type: "datetime2", nullable: false),
                    עודכן_בתאריך = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_דריסות_דוח_השוואה_שנתי", x => x.מזהה);
                    table.ForeignKey(
                        name: "FK_דריסות_דוח_השוואה_שנתי_מעסיקים_מזהה_מעסיק",
                        column: x => x.מזהה_מעסיק,
                        principalTable: "מעסיקים",
                        principalColumn: "מזהה_מעסיק",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_דריסות_דוח_השוואה_שנתי_נתוני_העסקה_מקטע_מזהה_מקטע",
                        column: x => x.מזהה_מקטע,
                        principalTable: "נתוני_העסקה_מקטע",
                        principalColumn: "מזהה_מקטע",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_דריסות_דוח_השוואה_שנתי_מזהה_מעסיק_שנת_לימודים_מזהה_מקטע",
                table: "דריסות_דוח_השוואה_שנתי",
                columns: new[] { "מזהה_מעסיק", "שנת_לימודים", "מזהה_מקטע" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_דריסות_דוח_השוואה_שנתי_מזהה_מקטע",
                table: "דריסות_דוח_השוואה_שנתי",
                column: "מזהה_מקטע");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "דריסות_דוח_השוואה_שנתי");
        }
    }
}
