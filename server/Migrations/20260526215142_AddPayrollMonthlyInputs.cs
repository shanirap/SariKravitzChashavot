using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountingProject.Migrations
{
    /// <inheritdoc />
    public partial class AddPayrollMonthlyInputs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "קלט_עוקץ_חודשי_אצווה",
                columns: table => new
                {
                    מזהה_אצווה = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    מזהה_מעסיק = table.Column<int>(type: "int", nullable: false),
                    שנת_לימודים = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    חודש = table.Column<int>(type: "int", nullable: false),
                    שנה_גרגוריאנית = table.Column<int>(type: "int", nullable: false),
                    שם_קובץ_מקורי = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    הועלה_בתאריך = table.Column<DateTime>(type: "datetime2", nullable: false),
                    הועלה_על_ידי = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    מספר_שורות = table.Column<int>(type: "int", nullable: false),
                    פעיל = table.Column<bool>(type: "bit", nullable: false),
                    נמחק = table.Column<bool>(type: "bit", nullable: false),
                    נמחק_בתאריך = table.Column<DateTime>(type: "datetime2", nullable: true),
                    נוצר_בתאריך = table.Column<DateTime>(type: "datetime2", nullable: false),
                    עודכן_בתאריך = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_קלט_עוקץ_חודשי_אצווה", x => x.מזהה_אצווה);
                    table.ForeignKey(
                        name: "FK_קלט_עוקץ_חודשי_אצווה_מעסיקים_מזהה_מעסיק",
                        column: x => x.מזהה_מעסיק,
                        principalTable: "מעסיקים",
                        principalColumn: "מזהה_מעסיק",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "קלט_עוקץ_חודשי_שורה",
                columns: table => new
                {
                    מזהה_שורה = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    מזהה_אצווה = table.Column<int>(type: "int", nullable: false),
                    מזהה_מעסיק = table.Column<int>(type: "int", nullable: false),
                    שנת_לימודים = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    חודש = table.Column<int>(type: "int", nullable: false),
                    שנה_גרגוריאנית = table.Column<int>(type: "int", nullable: false),
                    מספר_שורה_באקסל = table.Column<int>(type: "int", nullable: true),
                    סמל_מוסד = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    מספר_עובד_בעוקץ = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    תז = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    שם_מלא = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    תפקיד = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    דרגה = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ותק = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    שעות_שבועיות = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    בסיס_משרה = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    אחוז_משרה = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    שעות_גיל = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    גמולי_השתלמות = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    כפל_תואר = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    קרן_השתלמות = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    הכפלה_כללית = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    נערך_ידנית = table.Column<bool>(type: "bit", nullable: false),
                    הערת_עריכה_ידנית = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    תאים_גולמיים_json = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    נמחק = table.Column<bool>(type: "bit", nullable: false),
                    נמחק_בתאריך = table.Column<DateTime>(type: "datetime2", nullable: true),
                    נוצר_בתאריך = table.Column<DateTime>(type: "datetime2", nullable: false),
                    עודכן_בתאריך = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_קלט_עוקץ_חודשי_שורה", x => x.מזהה_שורה);
                    table.ForeignKey(
                        name: "FK_קלט_עוקץ_חודשי_שורה_קלט_עוקץ_חודשי_אצווה_מזהה_אצווה",
                        column: x => x.מזהה_אצווה,
                        principalTable: "קלט_עוקץ_חודשי_אצווה",
                        principalColumn: "מזהה_אצווה",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_קלט_עוקץ_חודשי_אצווה_מזהה_מעסיק_שנת_לימודים_חודש_שנה_גרגוריאנית",
                table: "קלט_עוקץ_חודשי_אצווה",
                columns: new[] { "מזהה_מעסיק", "שנת_לימודים", "חודש", "שנה_גרגוריאנית" },
                unique: true,
                filter: "[פעיל] = 1 AND [נמחק] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_קלט_עוקץ_חודשי_שורה_מזהה_אצווה",
                table: "קלט_עוקץ_חודשי_שורה",
                column: "מזהה_אצווה");

            migrationBuilder.CreateIndex(
                name: "IX_קלט_עוקץ_חודשי_שורה_מזהה_מעסיק_שנת_לימודים_חודש_שנה_גרגוריאנית",
                table: "קלט_עוקץ_חודשי_שורה",
                columns: new[] { "מזהה_מעסיק", "שנת_לימודים", "חודש", "שנה_גרגוריאנית" });

            migrationBuilder.CreateIndex(
                name: "IX_קלט_עוקץ_חודשי_שורה_מספר_עובד_בעוקץ",
                table: "קלט_עוקץ_חודשי_שורה",
                column: "מספר_עובד_בעוקץ");

            migrationBuilder.CreateIndex(
                name: "IX_קלט_עוקץ_חודשי_שורה_סמל_מוסד",
                table: "קלט_עוקץ_חודשי_שורה",
                column: "סמל_מוסד");

            migrationBuilder.CreateIndex(
                name: "IX_קלט_עוקץ_חודשי_שורה_תז",
                table: "קלט_עוקץ_חודשי_שורה",
                column: "תז");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "קלט_עוקץ_חודשי_שורה");

            migrationBuilder.DropTable(
                name: "קלט_עוקץ_חודשי_אצווה");
        }
    }
}
