using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountingProject.Migrations
{
    /// <inheritdoc />
    public partial class MoveTrainingBenefitsDoubleDegreeToPerGrade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "גמולי_השתלמות",
                table: "נתוני_העסקה",
                newName: "דרגה1_גמולי_השתלמות");

            migrationBuilder.RenameColumn(
                name: "כפל_תואר",
                table: "נתוני_העסקה",
                newName: "דרגה1_כפל_תואר");

            migrationBuilder.AddColumn<decimal>(
                name: "דרגה2_גמולי_השתלמות",
                table: "נתוני_העסקה",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "דרגה2_כפל_תואר",
                table: "נתוני_העסקה",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "דרגה2_גמולי_השתלמות",
                table: "נתוני_העסקה");

            migrationBuilder.DropColumn(
                name: "דרגה2_כפל_תואר",
                table: "נתוני_העסקה");

            migrationBuilder.RenameColumn(
                name: "דרגה1_גמולי_השתלמות",
                table: "נתוני_העסקה",
                newName: "גמולי_השתלמות");

            migrationBuilder.RenameColumn(
                name: "דרגה1_כפל_תואר",
                table: "נתוני_העסקה",
                newName: "כפל_תואר");
        }
    }
}
