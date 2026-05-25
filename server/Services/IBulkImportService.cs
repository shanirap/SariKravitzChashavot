using AccountingProject.Contracts;
using ClosedXML.Excel;

namespace AccountingProject.Services
{
    public interface IBulkImportService
    {
        Task<ImportResult> ImportEmployeesAsync(IFormFile file, int? employerId = null);
        Task<ImportResult> ImportEmployersAsync(IFormFile file);
        XLWorkbook BuildEmployeesTemplate(bool includeEmployerName = true, int? employerId = null);
        XLWorkbook BuildEmployersTemplate();
    }
}
