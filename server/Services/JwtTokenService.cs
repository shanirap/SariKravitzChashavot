using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AccountingProject.Infrastructure;
using AccountingProject.Models;
using Microsoft.IdentityModel.Tokens;

namespace AccountingProject.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _configuration;

        public JwtTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public (string Token, DateTimeOffset ExpiresAtUtc) CreateToken(User user)
        {
            var keyValue = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("Jwt:Key is not configured.");

            var issuer = _configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer is not configured.");
            var audience = _configuration["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt:Audience is not configured.");
            var minutes = int.TryParse(_configuration["Jwt:ExpirationMinutes"], out var m) ? m : 480;

            var signingKey = new SymmetricSecurityKey(JwtSigningKey.GetKeyBytes(keyValue));
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var expiresAt = DateTimeOffset.UtcNow.AddMinutes(minutes);

            // MapInbound Claims: use short "role" so [Authorize(Roles = ...)] matches RoleClaimType in Program.cs.
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.UniqueName, user.Username),
                new(ClaimTypes.Name, user.Username),
                new("role", user.Role),
            };

            var tokenDescriptor = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiresAt.UtcDateTime,
                signingCredentials: credentials);

            var handler = new JwtSecurityTokenHandler();
            return (handler.WriteToken(tokenDescriptor), expiresAt);
        }
    }
}
