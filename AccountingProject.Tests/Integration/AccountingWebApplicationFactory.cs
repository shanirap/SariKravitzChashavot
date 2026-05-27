using AccountingProject.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace AccountingProject.Tests.Integration;

/// <summary>
/// Full ASP.NET pipeline with EF Core InMemory (isolates per factory instance via database name).
/// </summary>
internal sealed class AccountingWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = "Integration_" + Guid.NewGuid().ToString("N");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.UseSetting("Jwt:Key", "TEST_INTEGRATION_JWT_KEY_32_CHARS_MIN!!");
        builder.UseSetting("Jwt:Issuer", "AccountingProject");
        builder.UseSetting("Jwt:Audience", "AccountingProjectUsers");
        builder.UseSetting("Jwt:SeedAdminPassword", "");
        builder.UseSetting("AllowedOrigins:0", "http://localhost:5173");

        builder.ConfigureServices(services =>
        {
            foreach (var d in services.ToList())
            {
                if (d.ServiceType == typeof(DbContextOptions<PayrollDbContext>))
                    services.Remove(d);
            }

            services.AddDbContext<PayrollDbContext>(options =>
                options
                    .UseInMemoryDatabase(_databaseName)
                    .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning)));

            // Align with Program.cs RoleClaimType = "role"; inbound claim mapping can strip short claim types.
            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.MapInboundClaims = false;
            });
        });
    }
}
