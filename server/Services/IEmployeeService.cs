using AccountingProject.Contracts;
using AccountingProject.Models;

namespace AccountingProject.Services
{
    public interface IEmployeeService
    {
        Task<Employee?> GetByIdAsync(int id);
        /// <summary>Active employee for the given employer and national id (soft-deleted excluded by global query filter).</summary>
        Task<Employee?> GetByEmployerAndIdNumberAsync(int employerId, string idNumber);
        Task<EmployeePrecreateHint> GetPrecreateHintAsync(int employerId, string? idNumberRaw, CancellationToken cancellationToken = default);
        Task<EmployeeCreateOrGetResult> CreateOrGetAsync(EmployeeDto dto);
        Task<bool> UpdateAsync(int id, EmployeeDto dto);
        Task<bool> SetManualActiveStatusAsync(int id, bool isActive);
        Task<(bool Success, string? Message)> DeleteAsync(int id);
    }
}
