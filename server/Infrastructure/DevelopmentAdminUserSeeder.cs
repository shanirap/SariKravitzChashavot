using AccountingProject.Contracts;
using AccountingProject.Data;
using AccountingProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AccountingProject.Infrastructure
{
    public static class DevelopmentAdminUserSeeder
    {
        private const string SeedUsername = "admin";

        /// <summary>
        /// Creates a default admin user when running in Development if no users exist.
        /// Requires Jwt:SeedAdminPassword in configuration (set in appsettings.Development.json only, not Production).
        /// </summary>
        public static async Task SeedAsync(
            IHostEnvironment environment,
            IConfiguration configuration,
            PayrollDbContext db,
            ILogger logger,
            CancellationToken cancellationToken = default)
        {
            if (!environment.IsDevelopment())
                return;

            if (await db.Users.AnyAsync(cancellationToken))
                return;

            var password = configuration["Jwt:SeedAdminPassword"];
            if (string.IsNullOrWhiteSpace(password))
            {
                logger.LogWarning(
                    "Development seed skipped: Jwt:SeedAdminPassword is empty. No users exist; set Jwt:SeedAdminPassword in appsettings.Development.json to create the admin user.");
                return;
            }

            var dto = new CreateUserDto
            {
                Username = SeedUsername,
                Password = password.Trim(),
                Role = UserRoles.Admin,
            };

            UserAccountFactory.CreateUser(db, dto);
            await db.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Development seed: created user '{Username}' with role '{Role}'. Change the password after first login.",
                SeedUsername,
                UserRoles.Admin);
        }
    }

    /// <summary>
    /// Centralized user creation and password hashing for seeds and future admin tooling.
    /// </summary>
    public static class UserAccountFactory
    {
        public static void CreateUser(PayrollDbContext db, CreateUserDto dto)
        {
            var hasher = new PasswordHasher<User>();
            var normalizedUsername = dto.Username.Trim();

            var user = new User
            {
                Username = normalizedUsername,
                Role = UserRoles.Admin,
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow
            };
            user.PasswordHash = hasher.HashPassword(user, dto.Password);

            db.Users.Add(user);
        }
    }
}
