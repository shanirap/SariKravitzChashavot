using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace AccountingProject.Infrastructure
{
    public sealed class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? UserId => ReadUserId();
        public string? Username => ReadUsername();
        public string? Role => ReadClaim("role") ?? ReadClaim(ClaimTypes.Role);

        public string GetAuditActor()
        {
            var username = ReadUsername();
            if (!string.IsNullOrWhiteSpace(username))
                return username;

            var userId = ReadUserId();
            if (!string.IsNullOrWhiteSpace(userId))
                return userId;

            return "system";
        }

        private string? ReadUserId() =>
            ReadClaim(JwtRegisteredClaimNames.Sub)
            ?? ReadClaim(ClaimTypes.NameIdentifier);

        private string? ReadUsername() =>
            ReadClaim(ClaimTypes.Name)
            ?? ReadClaim(JwtRegisteredClaimNames.UniqueName);

        private string? ReadClaim(string claimType)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
                return null;

            var value = user.FindFirst(claimType)?.Value;
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
