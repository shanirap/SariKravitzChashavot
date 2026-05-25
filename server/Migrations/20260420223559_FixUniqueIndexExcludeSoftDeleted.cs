using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountingProject.Migrations
{
    /// <inheritdoc />
    public partial class FixUniqueIndexExcludeSoftDeleted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_מעסיקים_חפ",
                table: "מעסיקים");

            migrationBuilder.CreateIndex(
                name: "IX_מעסיקים_חפ",
                table: "מעסיקים",
                column: "חפ",
                unique: true,
                filter: "[חפ] IS NOT NULL AND [IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_מעסיקים_חפ",
                table: "מעסיקים");

            migrationBuilder.CreateIndex(
                name: "IX_מעסיקים_חפ",
                table: "מעסיקים",
                column: "חפ",
                unique: true,
                filter: "[חפ] IS NOT NULL");
        }
    }
}
