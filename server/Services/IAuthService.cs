using AccountingProject.Contracts;

namespace AccountingProject.Services
{
    public interface IAuthService
    {
        Task<LoginResponseDto?> LoginAsync(string username, string password, CancellationToken cancellationToken = default);
    }
}
