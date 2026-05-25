using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountingProject.Migrations
{
    /// <inheritdoc />
    [Migration("20260503210000_EmployeeEmployerTzUnique_FilterIsDeleted")]
    public class EmployeeEmployerTzUnique_FilterIsDeleted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_עובדים_מזהה_מעסיק_תז",
                table: "עובדים");

            migrationBuilder.CreateIndex(
                name: "IX_עובדים_מזהה_מעסיק_תז",
                table: "עובדים",
                columns: new[] { "מזהה_מעסיק", "תז" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_עובדים_מזהה_מעסיק_תז",
                table: "עובדים");

            migrationBuilder.CreateIndex(
                name: "IX_עובדים_מזהה_מעסיק_תז",
                table: "עובדים",
                columns: new[] { "מזהה_מעסיק", "תז" },
                unique: true);
        }
    }
}
