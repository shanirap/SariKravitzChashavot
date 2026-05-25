namespace AccountingProject.Services
{
    public interface IComparisonReportService
    {
        /// <summary>Parses uploaded payroll workbook and returns comparison Excel bytes.</summary>
        Task<byte[]> GenerateMonthlyPayrollComparisonExcelAsync(int employerId, Stream excelStream, CancellationToken cancellationToken = default);
    }
}
