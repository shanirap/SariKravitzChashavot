using AccountingProject.Contracts;
using AccountingProject.Data;
using AccountingProject.Infrastructure;
using AccountingProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AccountingProject.Services
{
    public sealed class UserManagementService : IUserManagementService
    {
        private readonly PayrollDbContext _db;
        private readonly PasswordHasher<User> _passwordHasher = new();

        public UserManagementService(PayrollDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<UserSummaryDto>> ListAsync(CancellationToken cancellationToken)
        {
            var list = await _db.Users.AsNoTracking()
                .OrderBy(u => u.Username)
                .Select(u => new UserSummaryDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Role = u.Role,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                })
                .ToListAsync(cancellationToken);

            return list;
        }

        public async Task<(UserSummaryDto? User, string? Error)> CreateAsync(
            AdminCreateUserRequestDto dto,
            CancellationToken cancellationToken)
        {
            var usernameTrimmed = dto.Username.Trim();
            if (string.IsNullOrEmpty(usernameTrimmed))
                return (null, "Username is required.");

            if (usernameTrimmed.Length > 128)
                return (null, "Username exceeds maximum length.");

            var pwdErr = PasswordRules.ValidationError(dto.Password);
            if (pwdErr != null)
                return (null, pwdErr);

            var roleNormalized = dto.Role.Trim();
            if (!UserRoles.All.Contains(roleNormalized))
                return (null, $"Role must be one of: {string.Join(", ", UserRoles.All)}.");

            if (roleNormalized.Length > 64)
                return (null, "Role exceeds maximum length.");

            if (await _db.Users.AsNoTracking().AnyAsync(u => u.Username == usernameTrimmed, cancellationToken))
                return (null, "Username is already in use.");

            var factoryDto = new CreateUserDto
            {
                Username = usernameTrimmed,
                Password = dto.Password,
                Role = roleNormalized,
            };

            try
            {
                UserAccountFactory.CreateUser(_db, factoryDto);
                await _db.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                return (null, "Username is already in use.");
            }

            var created = await _db.Users.AsNoTracking().FirstAsync(
                u => u.Username == usernameTrimmed,
                cancellationToken);

            return (new UserSummaryDto
            {
                Id = created.Id,
                Username = created.Username,
                Role = created.Role,
                IsActive = created.IsActive,
                CreatedAt = created.CreatedAt,
            }, null);
        }

        public async Task<(bool Success, string? Error)> SetPasswordAsync(
            int userId,
            string password,
            CancellationToken cancellationToken)
        {
            var pwdErr = PasswordRules.ValidationError(password);
            if (pwdErr != null)
                return (false, pwdErr);

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
            if (user == null)
                return (false, "User not found.");

            user.PasswordHash = _passwordHasher.HashPassword(user, password);
            await _db.SaveChangesAsync(cancellationToken);
            return (true, null);
        }

        public async Task<(bool Success, string? Error)> SetActiveAsync(
            int userId,
            bool isActive,
            int currentAdminUserId,
            CancellationToken cancellationToken)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
            if (user == null)
                return (false, "User not found.");

            if (!isActive && userId == currentAdminUserId)
                return (false, "Cannot deactivate your own account.");

            user.IsActive = isActive;
            await _db.SaveChangesAsync(cancellationToken);
            return (true, null);
        }
    }
}
