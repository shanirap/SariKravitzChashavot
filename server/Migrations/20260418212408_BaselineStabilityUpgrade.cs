using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountingProject.Migrations
{
    /// <inheritdoc />
    public partial class BaselineStabilityUpgrade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "מעסיקים",
                columns: table => new
                {
                    מזהה_מעסיק = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    שם_מעסיק = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    חפ = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    מספר_שכר = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    מספר_עוקץ = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_מעסיקים", x => x.מזהה_מעסיק);
                });

            migrationBuilder.CreateTable(
                name: "עובדים",
                columns: table => new
                {
                    מזהה_עובד = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    מספר_עובד = table.Column<int>(type: "int", nullable: true),
                    תז = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    שם_פרטי = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    שם_משפחה = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    תאריך_לידה = table.Column<DateOnly>(type: "date", nullable: true),
                    מין = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    טל = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    תאריך_לידה_ילד_1 = table.Column<DateOnly>(type: "date", nullable: true),
                    תאריך_לידה_ילד_2 = table.Column<DateOnly>(type: "date", nullable: true),
                    תאריך_לידה_ילד_3 = table.Column<DateOnly>(type: "date", nullable: true),
                    תאריך_לידה_ילד_4 = table.Column<DateOnly>(type: "date", nullable: true),
                    תאריך_לידה_ילד_5 = table.Column<DateOnly>(type: "date", nullable: true),
                    תאריך_לידה_ילד_6 = table.Column<DateOnly>(type: "date", nullable: true),
                    תאריך_לידה_ילד_7 = table.Column<DateOnly>(type: "date", nullable: true),
                    תאריך_לידה_ילד_8 = table.Column<DateOnly>(type: "date", nullable: true),
                    תאריך_לידה_ילד_9 = table.Column<DateOnly>(type: "date", nullable: true),
                    תאריך_לידה_ילד_10 = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_עובדים", x => x.מזהה_עובד);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntityKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ChangesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChangedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ChangedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "נתוני_העסקה",
                columns: table => new
                {
                    מזהה_נתון_העסקה = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    מזהה_עובד = table.Column<int>(type: "int", nullable: false),
                    מזהה_מעסיק = table.Column<int>(type: "int", nullable: false),
                    חודש_שכר = table.Column<int>(type: "int", nullable: false),
                    שנת_שכר = table.Column<int>(type: "int", nullable: false),
                    סמל_מוסד = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    שם_דרגה = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    דרגה = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    תפקיד = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ותק = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    שעות_תקן_1 = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    שעות_בפועל_1 = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    שעות_ד1_2 = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    שעות_תקן_3 = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    שעות_תקן_2 = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    שעות_בפועל_2 = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    שעות_ד2_2 = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    שעות_גמלא_2 = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    קרן_השתלמות_סכום = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    קרן_השתלמות_אחוז = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    פנסיה_סכום = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    סוג_משרה = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    הכפלה_כללית_באחוז = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    גמול_חינוך_כיתה = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    גמול_הכשרה_ומקצוע = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    כפל_תואר = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    גמולי_השתלמות = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    שעות_גיל = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    אחוז_תוספת_אם = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
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

            migrationBuilder.CreateIndex(
                name: "IX_מעסיקים_חפ",
                table: "מעסיקים",
                column: "חפ",
                unique: true,
                filter: "[חפ] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_מעסיקים_שם_מעסיק",
                table: "מעסיקים",
                column: "שם_מעסיק");

            migrationBuilder.CreateIndex(
                name: "IX_נתוני_העסקה_מזהה_מעסיק_שנת_שכר_חודש_שכר",
                table: "נתוני_העסקה",
                columns: new[] { "מזהה_מעסיק", "שנת_שכר", "חודש_שכר" });

            migrationBuilder.CreateIndex(
                name: "IX_נתוני_העסקה_מזהה_עובד_מזהה_מעסיק_שנת_שכר_חודש_שכר",
                table: "נתוני_העסקה",
                columns: new[] { "מזהה_עובד", "מזהה_מעסיק", "שנת_שכר", "חודש_שכר" });

            migrationBuilder.CreateIndex(
                name: "IX_עובדים_מספר_עובד",
                table: "עובדים",
                column: "מספר_עובד");

            migrationBuilder.CreateIndex(
                name: "IX_עובדים_תז",
                table: "עובדים",
                column: "תז",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_ChangedAtUtc",
                table: "AuditLogs",
                column: "ChangedAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "נתוני_העסקה");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "מעסיקים");

            migrationBuilder.DropTable(
                name: "עובדים");
        }
    }
}
