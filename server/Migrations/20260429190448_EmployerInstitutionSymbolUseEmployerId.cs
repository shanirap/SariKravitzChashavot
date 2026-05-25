using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountingProject.Migrations
{
    /// <inheritdoc />
    public partial class EmployerInstitutionSymbolUseEmployerId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "מזהה_מעסיק",
                table: "סמלי_מוסד_מעסיקים",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE s
                SET s.[מזהה_מעסיק] = e.[מזהה_מעסיק]
                FROM [סמלי_מוסד_מעסיקים] s
                INNER JOIN [מעסיקים] e
                    ON e.[שם_מעסיק] = s.[מעסיק]
                   AND ISNULL(e.[סמל_מוטב], N'') = ISNULL(s.[סמל_מוטב], N'');
                """
            );

            migrationBuilder.Sql(
                """
                IF EXISTS (
                    SELECT 1
                    FROM [סמלי_מוסד_מעסיקים]
                    WHERE [מזהה_מעסיק] IS NULL
                )
                BEGIN
                    THROW 50001, N'לא ניתן להשלים מעבר ל-EmployerId: קיימים סמלי מוסד ללא התאמה למעסיק.', 1;
                END
                """
            );

            migrationBuilder.AlterColumn<int>(
                name: "מזהה_מעסיק",
                table: "סמלי_מוסד_מעסיקים",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.DropIndex(
                name: "IX_סמלי_מוסד_מעסיקים_מעסיק_סמל_מוטב_סמל_מוסד",
                table: "סמלי_מוסד_מעסיקים");

            migrationBuilder.DropColumn(
                name: "מעסיק",
                table: "סמלי_מוסד_מעסיקים");

            migrationBuilder.DropColumn(
                name: "סמל_מוטב",
                table: "סמלי_מוסד_מעסיקים");

            migrationBuilder.CreateIndex(
                name: "IX_סמלי_מוסד_מעסיקים_מזהה_מעסיק_סמל_מוסד",
                table: "סמלי_מוסד_מעסיקים",
                columns: new[] { "מזהה_מעסיק", "סמל_מוסד" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_סמלי_מוסד_מעסיקים_מעסיקים_מזהה_מעסיק",
                table: "סמלי_מוסד_מעסיקים",
                column: "מזהה_מעסיק",
                principalTable: "מעסיקים",
                principalColumn: "מזהה_מעסיק",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "מעסיק",
                table: "סמלי_מוסד_מעסיקים",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "סמל_מוטב",
                table: "סמלי_מוסד_מעסיקים",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                """
                UPDATE s
                SET s.[מעסיק] = e.[שם_מעסיק],
                    s.[סמל_מוטב] = ISNULL(e.[סמל_מוטב], N'')
                FROM [סמלי_מוסד_מעסיקים] s
                INNER JOIN [מעסיקים] e ON e.[מזהה_מעסיק] = s.[מזהה_מעסיק];
                """
            );

            migrationBuilder.DropForeignKey(
                name: "FK_סמלי_מוסד_מעסיקים_מעסיקים_מזהה_מעסיק",
                table: "סמלי_מוסד_מעסיקים");

            migrationBuilder.DropIndex(
                name: "IX_סמלי_מוסד_מעסיקים_מזהה_מעסיק_סמל_מוסד",
                table: "סמלי_מוסד_מעסיקים");

            migrationBuilder.DropColumn(
                name: "מזהה_מעסיק",
                table: "סמלי_מוסד_מעסיקים");

            migrationBuilder.CreateIndex(
                name: "IX_סמלי_מוסד_מעסיקים_מעסיק_סמל_מוטב_סמל_מוסד",
                table: "סמלי_מוסד_מעסיקים",
                columns: new[] { "מעסיק", "סמל_מוטב", "סמל_מוסד" },
                unique: true);
        }
    }
}
