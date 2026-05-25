using AccountingProject.Contracts;
using AccountingProject.Models;
using AccountingProject.Services;
using AccountingProject.Tests.TestHelpers;
using Microsoft.AspNetCore.Identity;

namespace AccountingProject.Tests;

public sealed class UserManagementTests
{
    private const string ValidPassword = "Aa1!zzzzzzzz";

    [Fact]
    public async Task AdminCanCreateUser()
    {
        await using var db = DbTestFactory.CreateContext();
        var sut = new UserManagementService(db);

        var (user, error) = await sut.CreateAsync(new AdminCreateUserRequestDto
        {
            Username = "office-user",
            Password = ValidPassword,
            Role = UserRoles.Viewer
        }, CancellationToken.None);

        Assert.Null(error);
        Assert.NotNull(user);
        Assert.Equal("office-user", user!.Username);
        Assert.Equal(UserRoles.Viewer, user.Role);
    }

    [Fact]
    public async Task DuplicateUsernameRejected()
    {
        await using var db = DbTestFactory.CreateContext();
        SeedUser(db, "duplicate-user", "Aa1!yyyyyyyy", isActive: true);
        var sut = new UserManagementService(db);

        var (user, error) = await sut.CreateAsync(new AdminCreateUserRequestDto
        {
            Username = "duplicate-user",
            Password = "Aa1!xxxxxxxx",
            Role = UserRoles.Admin
        }, CancellationToken.None);

        Assert.Null(user);
        Assert.Equal("Username is already in use.", error);
    }

    [Fact]
    public async Task InactiveUserCannotLogin()
    {
        await using var db = DbTestFactory.CreateContext();
        SeedUser(db, "inactive-user", "Aa1!qqqqqqqq", isActive: false);

        var auth = new AuthService(db, new FakeJwtTokenService());
        var result = await auth.LoginAsync("inactive-user", "Aa1!qqqqqqqq");

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_EmptyUsernameRejected()
    {
        await using var db = DbTestFactory.CreateContext();
        var sut = new UserManagementService(db);

        var (user, error) = await sut.CreateAsync(new AdminCreateUserRequestDto
        {
            Username = "   ",
            Password = ValidPassword,
            Role = UserRoles.Viewer
        }, CancellationToken.None);

        Assert.Null(user);
        Assert.Equal("Username is required.", error);
    }

    [Fact]
    public async Task CreateAsync_UsernameTooLongRejected()
    {
        await using var db = DbTestFactory.CreateContext();
        var sut = new UserManagementService(db);

        var (user, error) = await sut.CreateAsync(new AdminCreateUserRequestDto
        {
            Username = new string('x', 129),
            Password = ValidPassword,
            Role = UserRoles.Viewer
        }, CancellationToken.None);

        Assert.Null(user);
        Assert.Equal("Username exceeds maximum length.", error);
    }

    [Fact]
    public async Task CreateAsync_InvalidRoleRejected()
    {
        await using var db = DbTestFactory.CreateContext();
        var sut = new UserManagementService(db);

        var (user, error) = await sut.CreateAsync(new AdminCreateUserRequestDto
        {
            Username = "role-test-user",
            Password = ValidPassword,
            Role = "NotARealRole"
        }, CancellationToken.None);

        Assert.Null(user);
        Assert.NotNull(error);
        Assert.StartsWith("Role must be one of:", error);
    }

    [Fact]
    public async Task CreateAsync_WeakPasswordRejected()
    {
        await using var db = DbTestFactory.CreateContext();
        var sut = new UserManagementService(db);

        var (user, error) = await sut.CreateAsync(new AdminCreateUserRequestDto
        {
            Username = "weak-pass-user",
            Password = "short1!A",
            Role = UserRoles.Viewer
        }, CancellationToken.None);

        Assert.Null(user);
        Assert.NotNull(error);
    }

    [Fact]
    public async Task CreateAsync_DuplicateUsernameAfterTrimRejected()
    {
        await using var db = DbTestFactory.CreateContext();
        SeedUser(db, "reserved", ValidPassword, isActive: true);
        var sut = new UserManagementService(db);

        var (user, error) = await sut.CreateAsync(new AdminCreateUserRequestDto
        {
            Username = "  reserved  ",
            Password = "Aa1!yyyyyyyy",
            Role = UserRoles.Admin
        }, CancellationToken.None);

        Assert.Null(user);
        Assert.Equal("Username is already in use.", error);
    }

    [Fact]
    public async Task CreateAsync_PayrollManagerRoleAccepted()
    {
        await using var db = DbTestFactory.CreateContext();
        var sut = new UserManagementService(db);

        var (user, error) = await sut.CreateAsync(new AdminCreateUserRequestDto
        {
            Username = "payroll-mgr",
            Password = ValidPassword,
            Role = $"  {UserRoles.PayrollManager}  "
        }, CancellationToken.None);

        Assert.Null(error);
        Assert.NotNull(user);
        Assert.Equal(UserRoles.PayrollManager, user!.Role);
    }

    [Fact]
    public async Task SetPasswordAsync_UserNotFound_ReturnsError()
    {
        await using var db = DbTestFactory.CreateContext();
        var sut = new UserManagementService(db);

        var (ok, err) = await sut.SetPasswordAsync(99999, ValidPassword, CancellationToken.None);

        Assert.False(ok);
        Assert.Equal("User not found.", err);
    }

    [Fact]
    public async Task SetPasswordAsync_WeakPassword_ReturnsError()
    {
        await using var db = DbTestFactory.CreateContext();
        var u = SeedUser(db, "pwd-target", ValidPassword, isActive: true);
        var sut = new UserManagementService(db);

        var (ok, err) = await sut.SetPasswordAsync(u.Id, "Aa1!short", CancellationToken.None);

        Assert.False(ok);
        Assert.NotNull(err);
    }

    [Fact]
    public async Task SetPasswordAsync_Success_AllowsLoginWithNewPassword()
    {
        await using var db = DbTestFactory.CreateContext();
        var u = SeedUser(db, "rotate-me", ValidPassword, isActive: true);
        var sut = new UserManagementService(db);
        const string newPassword = "Bb2@bbbbbbbb";

        var (ok, err) = await sut.SetPasswordAsync(u.Id, newPassword, CancellationToken.None);
        Assert.True(ok);
        Assert.Null(err);

        var auth = new AuthService(db, new FakeJwtTokenService());
        var login = await auth.LoginAsync("rotate-me", newPassword);
        Assert.NotNull(login);

        Assert.Null(await auth.LoginAsync("rotate-me", ValidPassword));
    }

    [Fact]
    public async Task SetActiveAsync_UserNotFound_ReturnsError()
    {
        await using var db = DbTestFactory.CreateContext();
        var sut = new UserManagementService(db);

        var (ok, err) = await sut.SetActiveAsync(424242, true, currentAdminUserId: 1, CancellationToken.None);

        Assert.False(ok);
        Assert.Equal("User not found.", err);
    }

    [Fact]
    public async Task SetActiveAsync_CannotDeactivateSelf_ReturnsError()
    {
        await using var db = DbTestFactory.CreateContext();
        var admin = SeedUser(db, "self-admin", ValidPassword, isActive: true);
        var sut = new UserManagementService(db);

        var (ok, err) = await sut.SetActiveAsync(admin.Id, false, currentAdminUserId: admin.Id, CancellationToken.None);

        Assert.False(ok);
        Assert.Equal("Cannot deactivate your own account.", err);
    }

    [Fact]
    public async Task SetActiveAsync_DeactivateOtherUser_SucceedsAndBlocksTheirLogin()
    {
        await using var db = DbTestFactory.CreateContext();
        var admin = SeedUser(db, "admin-act", ValidPassword, isActive: true);
        var victim = SeedUser(db, "victim-u", "Aa1!wwwwwwww", isActive: true);
        var sut = new UserManagementService(db);

        var (ok, err) = await sut.SetActiveAsync(victim.Id, false, currentAdminUserId: admin.Id, CancellationToken.None);
        Assert.True(ok);
        Assert.Null(err);

        var auth = new AuthService(db, new FakeJwtTokenService());
        Assert.Null(await auth.LoginAsync("victim-u", "Aa1!wwwwwwww"));
        Assert.NotNull(await auth.LoginAsync("admin-act", ValidPassword));
    }

    [Fact]
    public async Task ListAsync_ReturnsUsersOrderedByUsername()
    {
        await using var db = DbTestFactory.CreateContext();
        SeedUser(db, "zebra", ValidPassword, isActive: true);
        SeedUser(db, "alpha", ValidPassword, isActive: true);
        var sut = new UserManagementService(db);

        var list = await sut.ListAsync(CancellationToken.None);

        Assert.Equal(new[] { "alpha", "zebra" }, list.Select(u => u.Username).ToArray());
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
