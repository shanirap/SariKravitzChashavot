using AccountingProject.Contracts;

namespace AccountingProject.Services
{
    public interface IUserManagementService
    {
        Task<IReadOnlyList<UserSummaryDto>> ListAsync(CancellationToken cancellationToken);

        Task<(UserSummaryDto? User, string? Error)> CreateAsync(AdminCreateUserRequestDto dto, CancellationToken cancellationToken);

        Task<(bool Success, string? Error)> SetPasswordAsync(int userId, string password, CancellationToken cancellationToken);

        Task<(bool Success, string? Error)> SetActiveAsync(int userId, bool isActive, int currentAdminUserId, CancellationToken cancellationToken);
    }
}
