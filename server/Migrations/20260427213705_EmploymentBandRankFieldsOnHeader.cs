using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountingProject.Migrations
{
    /// <inheritdoc />
    public partial class EmploymentBandRankFieldsOnHeader : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "דרגה1_דרגה",
                table: "נתוני_העסקה",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "דרגה1_ותק",
                table: "נתוני_העסקה",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "דרגה1_שם_הדירוג",
                table: "נתוני_העסקה",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "דרגה1_תפקיד",
                table: "נתוני_העסקה",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "דרגה2_דרגה",
                table: "נתוני_העסקה",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "דרגה2_ותק",
                table: "נתוני_העסקה",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "דרגה2_שם_הדירוג",
                table: "נתוני_העסקה",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "דרגה2_תפקיד",
                table: "נתוני_העסקה",
                type: "nvarchar(max)",
                nullable: true);

            // העתקה ממקטע 1 בכל דרגה (נתונים קודמים היו per-slot; לרוב זהים)
            migrationBuilder.Sql(@"
UPDATE ed SET
  [דרגה1_שם_הדירוג] = s.[שם_הדירוג],
  [דרגה1_דרגה] = s.[דרגה],
  [דרגה1_תפקיד] = s.[תפקיד],
  [דרגה1_ותק] = s.[ותק]
FROM [נתוני_העסקה] ed
INNER JOIN [נתוני_העסקה_מקטע] s ON s.[מזהה_נתון_העסקה] = ed.[מזהה_נתון_העסקה] AND s.[רמת_דרגה] = 1 AND s.[אינדקס_מקטע] = 1;

UPDATE ed SET
  [דרגה2_שם_הדירוג] = s.[שם_הדירוג],
  [דרגה2_דרגה] = s.[דרגה],
  [דרגה2_תפקיד] = s.[תפקיד],
  [דרגה2_ותק] = s.[ותק]
FROM [נתוני_העסקה] ed
INNER JOIN [נתוני_העסקה_מקטע] s ON s.[מזהה_נתון_העסקה] = ed.[מזהה_נתון_העסקה] AND s.[רמת_דרגה] = 2 AND s.[אינדקס_מקטע] = 1;
");

            migrationBuilder.DropColumn(
                name: "דרגה",
                table: "נתוני_העסקה_מקטע");

            migrationBuilder.DropColumn(
                name: "ותק",
                table: "נתוני_העסקה_מקטע");

            migrationBuilder.DropColumn(
                name: "שם_הדירוג",
                table: "נתוני_העסקה_מקטע");

            migrationBuilder.DropColumn(
                name: "תפקיד",
                table: "נתוני_העסקה_מקטע");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "דרגה",
                table: "נתוני_העסקה_מקטע",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ותק",
                table: "נתוני_העסקה_מקטע",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "שם_הדירוג",
                table: "נתוני_העסקה_מקטע",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "תפקיד",
                table: "נתוני_העסקה_מקטע",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.Sql(@"
UPDATE s SET
  [שם_הדירוג] = ed.[דרגה1_שם_הדירוג],
  [דרגה] = ed.[דרגה1_דרגה],
  [תפקיד] = ed.[דרגה1_תפקיד],
  [ותק] = ed.[דרגה1_ותק]
FROM [נתוני_העסקה_מקטע] s
INNER JOIN [נתוני_העסקה] ed ON ed.[מזהה_נתון_העסקה] = s.[מזהה_נתון_העסקה]
WHERE s.[רמת_דרגה] = 1 AND s.[אינדקס_מקטע] = 1;

UPDATE s SET
  [שם_הדירוג] = ed.[דרגה2_שם_הדירוג],
  [דרגה] = ed.[דרגה2_דרגה],
  [תפקיד] = ed.[דרגה2_תפקיד],
  [ותק] = ed.[דרגה2_ותק]
FROM [נתוני_העסקה_מקטע] s
INNER JOIN [נתוני_העסקה] ed ON ed.[מזהה_נתון_העסקה] = s.[מזהה_נתון_העסקה]
WHERE s.[רמת_דרגה] = 2 AND s.[אינדקס_מקטע] = 1;
");

            migrationBuilder.DropColumn(
                name: "דרגה1_דרגה",
                table: "נתוני_העסקה");

            migrationBuilder.DropColumn(
                name: "דרגה1_ותק",
                table: "נתוני_העסקה");

            migrationBuilder.DropColumn(
                name: "דרגה1_שם_הדירוג",
                table: "נתוני_העסקה");

            migrationBuilder.DropColumn(
                name: "דרגה1_תפקיד",
                table: "נתוני_העסקה");

            migrationBuilder.DropColumn(
                name: "דרגה2_דרגה",
                table: "נתוני_העסקה");

            migrationBuilder.DropColumn(
                name: "דרגה2_ותק",
                table: "נתוני_העסקה");

            migrationBuilder.DropColumn(
                name: "דרגה2_שם_הדירוג",
                table: "נתוני_העסקה");

            migrationBuilder.DropColumn(
                name: "דרגה2_תפקיד",
                table: "נתוני_העסקה");
        }
    }
}
