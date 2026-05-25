using AccountingProject.Contracts;
using AccountingProject.Data;
using AccountingProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AccountingProject.Infrastructure
{
    /// <summary>
    /// Optional, explicit one-time creation of the first Admin when the Users table is empty.
    /// Does not run unless <c>BootstrapAdmin:Enabled</c> is true or the CLI switch is present.
    /// </summary>
    public static class ProductionAdminBootstrap
    {
        /// <summary>
        /// When present in process arguments, sets <c>BootstrapAdmin:Enabled</c> to true (in-memory override).
        /// </summary>
        public const string CommandLineSwitch = "--bootstrap-first-admin";

        private const string UsernameKey = "BootstrapAdmin:Username";
        private const string PasswordKey = "BootstrapAdmin:Password";
        private const string EnabledKey = "BootstrapAdmin:Enabled";

        /// <summary>
        /// Applies the CLI bootstrap switch before configuration is finalized.
        /// </summary>
        public static void ApplyCommandLineSwitch(string[] args, ConfigurationManager configuration)
        {
            if (args == null || args.Length == 0)
                return;

            if (!args.Any(a =>
                    string.Equals(a, CommandLineSwitch, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                [EnabledKey] = "true",
            });
        }

        /// <summary>
        /// Runs when bootstrap is enabled. No-op otherwise. Never creates a user if any row exists.
        /// </summary>
        /// <exception cref="InvalidOperationException">When bootstrap is enabled but credentials are missing or weak.</exception>
        public static async Task RunIfRequestedAsync(
            IConfiguration configuration,
            PayrollDbContext db,
            ILogger logger,
            CancellationToken cancellationToken = default)
        {
            if (!IsBootstrapEnabled(configuration))
                return;

            if (await db.Users.AnyAsync(cancellationToken))
            {
                logger.LogWarning(
                    "BootstrapAdmin: enabled but skipped because the Users table is not empty. Remove {EnabledKey}, the CLI flag, or environment equivalents after onboarding.",
                    EnabledKey);
                return;
            }

            var username = configuration[UsernameKey]?.Trim();
            var password = configuration[PasswordKey];

            if (string.IsNullOrEmpty(username))
            {
                logger.LogError(
                    "BootstrapAdmin: Enabled but BootstrapAdmin:Username is missing or empty. Set BootstrapAdmin__Username.");
                throw new InvalidOperationException(
                    "BootstrapAdmin is enabled but username is missing. Set BootstrapAdmin:Username.");
            }

            if (password == null || string.IsNullOrEmpty(password))
            {
                logger.LogError(
                    "BootstrapAdmin: Enabled but BootstrapAdmin:Password is missing or empty. Set BootstrapAdmin__Password.");
                throw new InvalidOperationException(
                    "BootstrapAdmin is enabled but password is missing. Set BootstrapAdmin:Password.");
            }

            var pwdErr = PasswordRules.ValidationError(password);
            if (pwdErr != null)
            {
                logger.LogError("BootstrapAdmin: Password rejected: {Reason}", pwdErr);
                throw new InvalidOperationException(
                    $"BootstrapAdmin password does not meet policy: {pwdErr}");
            }

            var dto = new CreateUserDto
            {
                Username = username,
                Password = password,
                Role = UserRoles.Admin,
            };

            try
            {
                UserAccountFactory.CreateUser(db, dto);
                await db.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(
                    ex,
                    "BootstrapAdmin: failed to persist the first admin (unique constraint or race). Validate that no concurrent process created users.");
                throw;
            }

            logger.LogInformation(
                "BootstrapAdmin: created first admin user '{Username}'. Disable BootstrapAdmin:Enabled and remove secrets before the next deployment.",
                username);
        }

        private static bool IsBootstrapEnabled(IConfiguration configuration)
        {
            var raw = configuration[EnabledKey];
            return bool.TryParse(raw, out var enabled) && enabled;
        }
    }
}
