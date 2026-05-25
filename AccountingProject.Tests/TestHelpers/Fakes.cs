using AccountingProject.Contracts;
using AccountingProject.Models;
using AccountingProject.Services;

namespace AccountingProject.Tests.TestHelpers;

internal sealed class FakeJwtTokenService : IJwtTokenService
{
    public (string Token, DateTimeOffset ExpiresAtUtc) CreateToken(User user) =>
        ($"fake-token-{user.Username}", DateTimeOffset.UtcNow.AddHours(1));
}

internal sealed class NoopEmployeeService : IEmployeeService
{
    public Task<Employee?> GetByIdAsync(int id) => Task.FromResult<Employee?>(null);
    public Task<Employee?> GetByEmployerAndIdNumberAsync(int employerId, string idNumber) => Task.FromResult<Employee?>(null);
    public Task<EmployeePrecreateHint> GetPrecreateHintAsync(int employerId, string? idNumberRaw, CancellationToken cancellationToken = default) =>
        Task.FromResult(new EmployeePrecreateHint(false, false, false));
    public Task<EmployeeCreateOrGetResult> CreateOrGetAsync(EmployeeDto dto) => throw new NotSupportedException();
    public Task<bool> UpdateAsync(int id, EmployeeDto dto) => Task.FromResult(false);
    public Task<bool> SetManualActiveStatusAsync(int id, bool isActive) => Task.FromResult(false);
    public Task<(bool Success, string? Message)> DeleteAsync(int id) => Task.FromResult((false, (string?)null));
}
