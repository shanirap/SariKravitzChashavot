using AccountingProject.Models;

namespace AccountingProject.Services
{
    public interface IJwtTokenService
    {
        (string Token, DateTimeOffset ExpiresAtUtc) CreateToken(User user);
    }
}
