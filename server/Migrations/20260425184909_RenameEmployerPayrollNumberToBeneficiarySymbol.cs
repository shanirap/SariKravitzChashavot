using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountingProject.Migrations
{
    /// <inheritdoc />
    public partial class RenameEmployerPayrollNumberToBeneficiarySymbol : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "מספר_שכר",
                table: "מעסיקים",
                newName: "סמל_מוטב");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "סמל_מוטב",
                table: "מעסיקים",
                newName: "מספר_שכר");
        }
    }
}
