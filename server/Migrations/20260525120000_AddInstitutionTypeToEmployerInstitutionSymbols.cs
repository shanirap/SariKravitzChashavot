using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountingProject.Migrations
{
    /// <inheritdoc />
    public partial class AddInstitutionTypeToEmployerInstitutionSymbols : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "סוג_מוסד",
                table: "סמלי_מוסד_מעסיקים",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "אחר");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "סוג_מוסד",
                table: "סמלי_מוסד_מעסיקים");
        }
    }
}
