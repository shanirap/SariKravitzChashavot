using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountingProject.Migrations
{
    /// <inheritdoc />
    public partial class AddEmploymentTrainingBenefitsAndDoubleDegree : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "גמולי_השתלמות",
                table: "נתוני_העסקה",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "כפל_תואר",
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
                name: "גמולי_השתלמות",
                table: "נתוני_העסקה");

            migrationBuilder.DropColumn(
                name: "כפל_תואר",
                table: "נתוני_העסקה");
        }
    }
}
