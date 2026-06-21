using AccountingProject.Contracts;

namespace AccountingProject.Services
{
    public interface IPayrollMonthlyInputService
    {
        Task<PayrollImportResultDto> ImportMonthAsync(
            int employerId,
            string academicYear,
            int month,
            Stream file,
            string originalFileName);

        Task<IReadOnlyList<PayrollMonthStatusDto>> GetYearStatusAsync(
            int employerId,
            string academicYear);

        Task<IReadOnlyList<PayrollMonthlyInputRowDto>> GetRowsAsync(
            int employerId,
            string academicYear,
            int month);

        Task<PayrollMonthlyInputRowDto> UpdateRowAsync(
            int employerId,
            int rowId,
            PayrollMonthlyInputRowEditDto dto);

        Task DeleteRowAsync(int employerId, int rowId);
    }
}
