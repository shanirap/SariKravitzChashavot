using AccountingProject.Data;
using Microsoft.EntityFrameworkCore;

namespace AccountingProject.Tests.TestHelpers;

internal static class DbTestFactory
{
    public static PayrollDbContext CreateContext(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<PayrollDbContext>()
            .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString("N"))
            .Options;
        return new PayrollDbContext(options);
    }
}
