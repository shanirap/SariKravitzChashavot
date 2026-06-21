using AccountingProject.Data;
using AccountingProject.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace AccountingProject.Tests.TestHelpers;

internal static class ServiceTestFactory
{
    public static EmploymentCalculationService CreateEmploymentCalculations() => new();

    public static EmploymentDataService CreateEmploymentDataService(PayrollDbContext db) =>
        new(db, CreateEmploymentCalculations());

    public static BulkImportService CreateBulkImportService(PayrollDbContext db) =>
        new(db, CreateEmploymentCalculations(), NullLogger<BulkImportService>.Instance);
}
