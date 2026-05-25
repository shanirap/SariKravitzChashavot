using System.Net;
using System.Net.Http.Json;
using AccountingProject.Contracts;
using AccountingProject.Models;
using Microsoft.AspNetCore.Identity;

namespace AccountingProject.Tests.Integration;

public sealed class AuthApiIntegrationTests
{
    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);
        Assert.False(string.IsNullOrWhiteSpace(
            client.DefaultRequestHeaders.Authorization?.Parameter));
    }

    [Fact]
    public async Task Login_InvalidPassword_Returns401()
    {
        await using var factory = new AccountingWebApplicationFactory();
        using var scope = factory.Services.CreateScope();
        IntegrationSeed.EnsureUsers(scope.ServiceProvider.GetRequiredService<PayrollDbContext>());

        var client = factory.CreateClient();
        var resp = await client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto
        {
            Username = IntegrationSeed.AdminUsername,
            Password = "WrongPass9!",
        });

        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task Login_InactiveUser_Returns401()
    {
        await using var factory = new AccountingWebApplicationFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PayrollDbContext>();
        var inactive = new User
        {
            Username = "inactive-api",
            Role = UserRoles.Admin,
            IsActive = false,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        inactive.PasswordHash = new PasswordHasher<User>().HashPassword(inactive, "Aa1!inactiveXX");
        db.Users.Add(inactive);
        db.SaveChanges();

        var client = factory.CreateClient();
        var resp = await client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto
        {
            Username = "inactive-api",
            Password = "Aa1!inactiveXX",
        });

        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task Employers_WithoutAuth_Returns401()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var resp = await factory.CreateClient().GetAsync("/api/employers");

        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }
}
