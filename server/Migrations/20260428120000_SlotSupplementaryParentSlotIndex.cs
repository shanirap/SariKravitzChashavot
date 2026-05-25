using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountingProject.Migrations
{
    /// <inheritdoc />
    public partial class SlotSupplementaryParentSlotIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "מקטע_הורה_שעות_נוספות",
                table: "נתוני_העסקה_מקטע",
                type: "tinyint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "מקטע_הורה_שעות_נוספות",
                table: "נתוני_העסקה_מקטע");
        }
    }
}
