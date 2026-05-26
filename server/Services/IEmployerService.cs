using System.Collections.Generic;
using AccountingProject.Contracts;
using AccountingProject.Models;

namespace AccountingProject.Services
{
    public interface IEmployerService
    {
        Task<PagedResult<Employer>> GetPagedAsync(string? search, int page, int pageSize);
        Task<Employer?> GetByIdAsync(int id);
        Task<Employer> CreateAsync(EmployerDto dto);
        Task<bool> UpdateAsync(int id, EmployerDto dto);
        Task<(bool Success, string? Message)> DeleteAsync(int id);
        Task<PagedResult<Employee>> GetEmployeesAsync(int employerId, string? search, int page, int pageSize);
        Task<HashSet<int>> GetEmployeeIdsWithEmploymentDataAsync(int employerId, IReadOnlyList<int> employeeIds);
        Task<IReadOnlyList<EmployerInstitutionSymbol>> GetInstitutionSymbolsAsync(int employerId);
        Task<(EmployerInstitutionSymbol? Symbol, string? Message)> CreateInstitutionSymbolAsync(int employerId, EmployerInstitutionSymbolDto dto);
        Task<(EmployerInstitutionSymbol? Symbol, string? Message)> UpdateInstitutionSymbolAsync(int employerId, int symbolId, EmployerInstitutionSymbolUpdateDto dto);
        Task<(bool Success, string? Message)> DeleteInstitutionSymbolAsync(int employerId, int symbolId);

        /// <summary>קובץ Excel עם כל נתוני המעסיק (מעסיק, עובדים, סמלי מוסד, נתוני העסקה ומקטעים).</summary>
        Task<byte[]?> BuildFullEmployerExportExcelAsync(int employerId);
    }
}
