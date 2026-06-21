using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountingProject.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeAllUsersToAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE [Users]
                SET [Role] = N'Admin'
                WHERE [Role] <> N'Admin';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Role normalization is not reversible.
        }
    }
}
