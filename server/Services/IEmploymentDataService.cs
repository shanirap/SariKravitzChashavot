using AccountingProject.Contracts;
using AccountingProject.Models;

namespace AccountingProject.Services
{
    public interface IEmploymentDataService
    {
        Task<IReadOnlyList<EmploymentData>> GetByEmployeeAndEmployerAsync(int employeeId, int employerId);
        Task<(EmploymentData? Record, string? Message)> CreateAsync(EmploymentDataDto dto);
        Task<(EmploymentData? Record, string? Message)> UpdateAsync(int id, EmploymentDataDto dto);
        Task<(bool Success, string? Message)> DeleteAsync(int id);
    }
}
