using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountingProject.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployerIdToEmployees : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_עובדים_תז' AND object_id = OBJECT_ID(N'[עובדים]'))
    DROP INDEX [IX_עובדים_תז] ON [עובדים];
");

            migrationBuilder.AddColumn<int>(
                name: "מזהה_מעסיק",
                table: "עובדים",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(@"
UPDATE e
SET [מזהה_מעסיק] = x.[מזהה_מעסיק]
FROM [עובדים] e
OUTER APPLY (
    SELECT TOP (1) ed.[מזהה_מעסיק]
    FROM [נתוני_העסקה] ed
    WHERE ed.[מזהה_עובד] = e.[מזהה_עובד]
    ORDER BY ed.[UpdatedAtUtc] DESC, ed.[מזהה_נתון_העסקה] DESC
) x
WHERE e.[מזהה_מעסיק] IS NULL AND x.[מזהה_מעסיק] IS NOT NULL;

UPDATE e
SET [מזהה_מעסיק] = (SELECT TOP (1) [מזהה_מעסיק] FROM [מעסיקים] ORDER BY [מזהה_מעסיק])
FROM [עובדים] e
WHERE e.[מזהה_מעסיק] IS NULL;

IF EXISTS (SELECT 1 FROM [עובדים] WHERE [מזהה_מעסיק] IS NULL)
    THROW 51000, N'לא ניתן לשייך עובדים קיימים למעסיק כי לא קיים מעסיק במערכת.', 1;
");

            migrationBuilder.AlterColumn<int>(
                name: "מזהה_מעסיק",
                table: "עובדים",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_עובדים_מזהה_מעסיק_תז",
                table: "עובדים",
                columns: new[] { "מזהה_מעסיק", "תז" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_עובדים_מעסיקים_מזהה_מעסיק",
                table: "עובדים",
                column: "מזהה_מעסיק",
                principalTable: "מעסיקים",
                principalColumn: "מזהה_מעסיק",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_עובדים_מעסיקים_מזהה_מעסיק",
                table: "עובדים");

            migrationBuilder.DropIndex(
                name: "IX_עובדים_מזהה_מעסיק_תז",
                table: "עובדים");

            migrationBuilder.DropColumn(
                name: "מזהה_מעסיק",
                table: "עובדים");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_עובדים_תז' AND object_id = OBJECT_ID(N'[עובדים]'))
    CREATE UNIQUE INDEX [IX_עובדים_תז] ON [עובדים] ([תז]);
");
        }
    }
}
