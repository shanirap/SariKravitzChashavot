using AccountingProject.Models;
using Microsoft.AspNetCore.Identity;

namespace AccountingProject.Tests.Integration;

internal static class IntegrationSeed
{
    internal const string AdminUsername = "integration-admin";
    internal const string AdminPassword = "Aa1!integrationZZ";

    internal const string ViewerUsername = "integration-viewer";
    internal const string ViewerPassword = "Aa1!integrationVV";

    public static void EnsureUsers(PayrollDbContext db)
    {
        UpsertUser(db, AdminUsername, AdminPassword, UserRoles.Admin);
        UpsertUser(db, ViewerUsername, ViewerPassword, UserRoles.Viewer);
    }

    private static void UpsertUser(PayrollDbContext db, string username, string password, string role)
    {
        if (db.Users.Any(u => u.Username == username))
            return;

        var user = new User
        {
            Username = username,
            Role = role,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, password);
        db.Users.Add(user);
        db.SaveChanges();
    }
}
