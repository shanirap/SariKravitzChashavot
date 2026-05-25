using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountingProject.Migrations
{
    /// <inheritdoc />
    public partial class RenameEmploymentHebrewFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "שבוע_שעות",
                table: "נתוני_העסקה_מקטע",
                newName: "שעות_שבועיות");

            migrationBuilder.RenameColumn(
                name: "דרגה2_אחוז_הטבה_אם",
                table: "נתוני_העסקה",
                newName: "דרגה2_אחוז_תוספת_אם");

            migrationBuilder.RenameColumn(
                name: "דרגה1_אחוז_הטבה_אם",
                table: "נתוני_העסקה",
                newName: "דרגה1_אחוז_תוספת_אם");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "שעות_שבועיות",
                table: "נתוני_העסקה_מקטע",
                newName: "שבוע_שעות");

            migrationBuilder.RenameColumn(
                name: "דרגה2_אחוז_תוספת_אם",
                table: "נתוני_העסקה",
                newName: "דרגה2_אחוז_הטבה_אם");

            migrationBuilder.RenameColumn(
                name: "דרגה1_אחוז_תוספת_אם",
                table: "נתוני_העסקה",
                newName: "דרגה1_אחוז_הטבה_אם");
        }
    }
}
