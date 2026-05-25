using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountingProject.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployerInstitutionSymbols : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "סמלי_מוסד_מעסיקים",
                columns: table => new
                {
                    מזהה_סמל_מוסד_מעסיק = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    מעסיק = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    סמל_מוטב = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    סמל_מוסד = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    שם_סמל_מוסד = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_סמלי_מוסד_מעסיקים", x => x.מזהה_סמל_מוסד_מעסיק);
                });

            migrationBuilder.CreateIndex(
                name: "IX_סמלי_מוסד_מעסיקים_מעסיק_סמל_מוטב_סמל_מוסד",
                table: "סמלי_מוסד_מעסיקים",
                columns: new[] { "מעסיק", "סמל_מוטב", "סמל_מוסד" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "סמלי_מוסד_מעסיקים");
        }
    }
}
