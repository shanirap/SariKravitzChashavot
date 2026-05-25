using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountingProject.Migrations
{
    /// <inheritdoc />
    public partial class CleanupEmptyEmploymentDataSlots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM [נתוני_העסקה_מקטע]
                WHERE [מקטע_הורה_שעות_נוספות] IS NULL
                  AND ([סמל_מוסד] IS NULL OR LTRIM(RTRIM([סמל_מוסד])) = '')
                  AND ([שעות_שבועיות] IS NULL OR [שעות_שבועיות] = 0);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Data cleanup is not reversible.
        }
    }
}
