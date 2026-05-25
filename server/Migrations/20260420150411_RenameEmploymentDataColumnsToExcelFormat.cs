using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountingProject.Migrations
{
    /// <inheritdoc />
    public partial class RenameEmploymentDataColumnsToExcelFormat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "שעות_תקן_3",
                table: "נתוני_העסקה",
                newName: "שעות_משרה_3_דרוג_1");

            migrationBuilder.RenameColumn(
                name: "שעות_תקן_2",
                table: "נתוני_העסקה",
                newName: "שעות_משרה_1_דרוג_2");

            migrationBuilder.RenameColumn(
                name: "שעות_תקן_1",
                table: "נתוני_העסקה",
                newName: "שעות_משרה_1_דרוג_1");

            migrationBuilder.RenameColumn(
                name: "שעות_ד2_2",
                table: "נתוני_העסקה",
                newName: "שעות_משרה_2_דרוג_2");

            migrationBuilder.RenameColumn(
                name: "שעות_ד1_2",
                table: "נתוני_העסקה",
                newName: "שעות_משרה_2_דרוג_1");

            migrationBuilder.RenameColumn(
                name: "שעות_גמלא_2",
                table: "נתוני_העסקה",
                newName: "שעות_משרה_3_דרוג_2");

            migrationBuilder.RenameColumn(
                name: "שעות_בפועל_2",
                table: "נתוני_העסקה",
                newName: "מתוך_שעות_משרה_1_דרוג_2");

            migrationBuilder.RenameColumn(
                name: "שעות_בפועל_1",
                table: "נתוני_העסקה",
                newName: "מתוך_שעות_משרה_1_דרוג_1");

            migrationBuilder.RenameColumn(
                name: "שנת_שכר",
                table: "נתוני_העסקה",
                newName: "שנה");

            migrationBuilder.RenameColumn(
                name: "שם_דרגה",
                table: "נתוני_העסקה",
                newName: "שם_הדירוג");

            migrationBuilder.RenameColumn(
                name: "חודש_שכר",
                table: "נתוני_העסקה",
                newName: "חודש");

            migrationBuilder.RenameColumn(
                name: "דרגה",
                table: "נתוני_העסקה",
                newName: "דירוג");

            // NOTE: indexes reference columns by internal object-id, so column renames
            // don't invalidate existing indexes. The stored index name may become a bit
            // stale (based on old column names) but it keeps functioning.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "שעות_משרה_3_דרוג_2",
                table: "נתוני_העסקה",
                newName: "שעות_גמלא_2");

            migrationBuilder.RenameColumn(
                name: "שעות_משרה_3_דרוג_1",
                table: "נתוני_העסקה",
                newName: "שעות_תקן_3");

            migrationBuilder.RenameColumn(
                name: "שעות_משרה_2_דרוג_2",
                table: "נתוני_העסקה",
                newName: "שעות_ד2_2");

            migrationBuilder.RenameColumn(
                name: "שעות_משרה_2_דרוג_1",
                table: "נתוני_העסקה",
                newName: "שעות_ד1_2");

            migrationBuilder.RenameColumn(
                name: "שעות_משרה_1_דרוג_2",
                table: "נתוני_העסקה",
                newName: "שעות_תקן_2");

            migrationBuilder.RenameColumn(
                name: "שעות_משרה_1_דרוג_1",
                table: "נתוני_העסקה",
                newName: "שעות_תקן_1");

            migrationBuilder.RenameColumn(
                name: "שנה",
                table: "נתוני_העסקה",
                newName: "שנת_שכר");

            migrationBuilder.RenameColumn(
                name: "שם_הדירוג",
                table: "נתוני_העסקה",
                newName: "שם_דרגה");

            migrationBuilder.RenameColumn(
                name: "מתוך_שעות_משרה_1_דרוג_2",
                table: "נתוני_העסקה",
                newName: "שעות_בפועל_2");

            migrationBuilder.RenameColumn(
                name: "מתוך_שעות_משרה_1_דרוג_1",
                table: "נתוני_העסקה",
                newName: "שעות_בפועל_1");

            migrationBuilder.RenameColumn(
                name: "חודש",
                table: "נתוני_העסקה",
                newName: "חודש_שכר");

            migrationBuilder.RenameColumn(
                name: "דירוג",
                table: "נתוני_העסקה",
                newName: "דרגה");

        }
    }
}
