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
                IF COL_LENGTH(N'נתוני_העסקה_מקטע', N'מקטע_הורה_שעות_נוספות') IS NOT NULL
                BEGIN
                    DELETE FROM [נתוני_העסקה_מקטע]
                    WHERE [מקטע_הורה_שעות_נוספות] IS NULL
                      AND ([סמל_מוסד] IS NULL OR LTRIM(RTRIM([סמל_מוסד])) = '')
                      AND ([שעות_שבועיות] IS NULL OR [שעות_שבועיות] = 0);
                END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Data cleanup is not reversible.
        }
    }
}
