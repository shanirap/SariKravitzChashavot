using AccountingProject.Contracts;
using AccountingProject.Data;
using AccountingProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AccountingProject.Services
{
    public class AuthService : IAuthService
    {
        private readonly PayrollDbContext _db;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthService(PayrollDbContext db, IJwtTokenService jwtTokenService)
        {
            _db = db;
            _jwtTokenService = jwtTokenService;
            _passwordHasher = new PasswordHasher<User>();
        }

        public async Task<LoginResponseDto?> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return null;

            var normalizedUsername = username.Trim();
            var user = await _db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == normalizedUsername, cancellationToken);

            if (user == null || !user.IsActive)
                return null;

            var verify = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            if (verify is PasswordVerificationResult.Failed)
                return null;

            var (token, expiresAtUtc) = _jwtTokenService.CreateToken(user);

            return new LoginResponseDto
            {
                Token = token,
                Username = user.Username,
                Role = user.Role,
                ExpiresAtUtc = expiresAtUtc
            };
        }
    }
}
