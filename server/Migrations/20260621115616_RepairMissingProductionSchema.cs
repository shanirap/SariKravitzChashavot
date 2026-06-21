using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountingProject.Migrations
{
    /// <summary>
    /// Idempotent repair for schema changes that were added to the model snapshot without
    /// being chained into EF migrations (missing Designer files). Safe on production even
    /// when columns/indexes were added manually.
    /// </summary>
    public partial class RepairMissingProductionSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'נתוני_העסקה_מקטע', N'מקטע_הורה_שעות_נוספות') IS NULL
                BEGIN
                    ALTER TABLE [נתוני_העסקה_מקטע]
                        ADD [מקטע_הורה_שעות_נוספות] tinyint NULL;
                END;

                IF COL_LENGTH(N'סמלי_מוסד_מעסיקים', N'סוג_מוסד') IS NULL
                BEGIN
                    ALTER TABLE [סמלי_מוסד_מעסיקים]
                        ADD [סוג_מוסד] nvarchar(20) NOT NULL
                            CONSTRAINT [DF_סמלי_מוסד_מעסיקים_סוג_מוסד] DEFAULT N'אחר';
                END;

                IF EXISTS (
                    SELECT 1
                    FROM sys.indexes i
                    WHERE i.[name] = N'IX_עובדים_מזהה_מעסיק_תז'
                      AND i.[object_id] = OBJECT_ID(N'[עובדים]')
                      AND (i.[has_filter] = 0 OR i.[filter_definition] NOT LIKE N'%IsDeleted%')
                )
                BEGIN
                    DROP INDEX [IX_עובדים_מזהה_מעסיק_תז] ON [עובדים];
                END;

                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.indexes i
                    WHERE i.[name] = N'IX_עובדים_מזהה_מעסיק_תז'
                      AND i.[object_id] = OBJECT_ID(N'[עובדים]')
                )
                BEGIN
                    CREATE UNIQUE INDEX [IX_עובדים_מזהה_מעסיק_תז]
                        ON [עובדים] ([מזהה_מעסיק], [תז])
                        WHERE [IsDeleted] = 0;
                END;

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
            // Forward-only repair migration for production safety.
        }
    }
}
