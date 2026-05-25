using AccountingProject.Data;
using AccountingProject.Models;
using AccountingProject.Services;
using AccountingProject.Tests.TestHelpers;
using Microsoft.AspNetCore.Identity;

namespace AccountingProject.Tests;

public sealed class AuthServiceTests
{
    [Fact]
    public async Task LoginAsync_InactiveUser_ReturnsNull()
    {
        await using var db = DbTestFactory.CreateContext();
        var user = SeedUser(db, "inactive", "Aa1!aaaaaaaa", isActive: false);

        var sut = new AuthService(db, new FakeJwtTokenService());
        var result = await sut.LoginAsync(user.Username, "Aa1!aaaaaaaa");

        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_ValidUser_ReturnsTokenAndIdentity()
    {
        await using var db = DbTestFactory.CreateContext();
        var user = SeedUser(db, "active", "Aa1!bbbbbbbb", isActive: true);

        var sut = new AuthService(db, new FakeJwtTokenService());
        var result = await sut.LoginAsync(user.Username, "Aa1!bbbbbbbb");

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result!.Token));
        Assert.Equal(user.Username, result.Username);
        Assert.Equal(user.Role, result.Role);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ReturnsNull()
    {
        await using var db = DbTestFactory.CreateContext();
        var user = SeedUser(db, "alice", "Aa1!cccccccc", isActive: true);

        var sut = new AuthService(db, new FakeJwtTokenService());
        var result = await sut.LoginAsync(user.Username, "WrongPass1!");

        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_UnknownUser_ReturnsNull()
    {
        await using var db = DbTestFactory.CreateContext();
        var sut = new AuthService(db, new FakeJwtTokenService());

        Assert.Null(await sut.LoginAsync("nobody", "Aa1!dddddddd"));
    }

    [Theory]
    [InlineData("", "Aa1!eeeeeeee")]
    [InlineData("   ", "Aa1!eeeeeeee")]
    [InlineData("user", "")]
    [InlineData("user", "   ")]
    public async Task LoginAsync_EmptyOrWhitespaceCredentials_ReturnsNull(string username, string password)
    {
        await using var db = DbTestFactory.CreateContext();
        SeedUser(db, "user", "Aa1!eeeeeeee", isActive: true);
        var sut = new AuthService(db, new FakeJwtTokenService());

        Assert.Null(await sut.LoginAsync(username, password));
    }

    [Fact]
    public async Task LoginAsync_TrimsUsernameForSuccessfulLookup()
    {
        await using var db = DbTestFactory.CreateContext();
        SeedUser(db, "trimmed", "Aa1!ffffffff", isActive: true);

        var sut = new AuthService(db, new FakeJwtTokenService());
        var result = await sut.LoginAsync("  trimmed  ", "Aa1!ffffffff");

        Assert.NotNull(result);
        Assert.Equal("trimmed", result!.Username);
    }

    [Fact]
    public async Task LoginAsync_PasswordDoesNotTrimTrailingSpace_FailsVerification()
    {
        await using var db = DbTestFactory.CreateContext();
        SeedUser(db, "bob", "Aa1!gggggggg", isActive: true);

        var sut = new AuthService(db, new FakeJwtTokenService());
        Assert.Null(await sut.LoginAsync("bob", "Aa1!gggggggg "));
    }

    private static User SeedUser(PayrollDbContext db, string username, string password, bool isActive)
    {
        var user = new User
        {
            Username = username,
            Role = UserRoles.Admin,
            IsActive = isActive,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, password);
        db.Users.Add(user);
        db.SaveChanges();
        return user;
    }
}
