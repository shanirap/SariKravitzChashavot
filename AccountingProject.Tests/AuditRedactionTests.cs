using AccountingProject.Models;
using AccountingProject.Tests.TestHelpers;

namespace AccountingProject.Tests;

public sealed class AuditRedactionTests
{
    [Fact]
    public async Task PasswordHashIsRedactedInAuditChanges()
    {
        await using var db = DbTestFactory.CreateContext();

        var user = new User
        {
            Username = "audit-user",
            PasswordHash = "plain-secret-hash",
            Role = UserRoles.Admin,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var audit = await db.AuditLogs
            .OrderByDescending(a => a.Id)
            .FirstOrDefaultAsync(a => a.EntityName == nameof(User));

        Assert.NotNull(audit);
        Assert.NotNull(audit!.ChangesJson);
        Assert.Contains("REDACTED", audit.ChangesJson);
        Assert.DoesNotContain("plain-secret-hash", audit.ChangesJson);
    }
}
