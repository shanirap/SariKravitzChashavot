using AccountingProject.Data;
using AccountingProject.Infrastructure;
using AccountingProject.Models;
using AccountingProject.Services;
using AccountingProject.Tests.TestHelpers;
using Microsoft.EntityFrameworkCore;

namespace AccountingProject.Tests;

public sealed class PayrollMonthlyInputImportTests
{
    private const string Year = "תשפ\"ו";
    private const int Month = 9;
    private const int GregorianYear = 2025;

    [Fact]
    public async Task Import_ValidFile_CreatesActiveBatch()
    {
        await using var db = DbTestFactory.CreateContext();
        var (service, employerId) = await CreateServiceAsync(db);

        var result = await ImportValidFileAsync(service, employerId, "first.xlsx");

        var batch = await db.PayrollMonthlyInputBatches.FindAsync(result.BatchId);
        Assert.NotNull(batch);
        Assert.True(batch!.IsActive);
        Assert.False(batch.IsDeleted);
        Assert.Equal(employerId, batch.EmployerId);
        Assert.Equal(Month, batch.Month);
        Assert.Equal(GregorianYear, batch.GregorianYear);
    }

    [Fact]
    public async Task Import_ValidFile_CreatesRows()
    {
        await using var db = DbTestFactory.CreateContext();
        var (service, employerId) = await CreateServiceAsync(db);

        var result = await ImportValidFileAsync(service, employerId, "rows.xlsx");

        var rows = await db.PayrollMonthlyInputRows
            .Where(r => r.BatchId == result.BatchId && !r.IsDeleted)
            .ToListAsync();
        Assert.NotEmpty(rows);
        Assert.Contains(rows, r => r.IdNumber == "123456789");
        Assert.Contains(rows, r => r.FullName == "Import Worker");
    }

    [Fact]
    public async Task Import_ValidFile_RowsCountMatchesSavedRows()
    {
        await using var db = DbTestFactory.CreateContext();
        var (service, employerId) = await CreateServiceAsync(db);

        var result = await ImportValidFileAsync(service, employerId, "count.xlsx");

        var savedCount = await db.PayrollMonthlyInputRows
            .CountAsync(r => r.BatchId == result.BatchId && !r.IsDeleted);
        Assert.Equal(savedCount, result.RowsCount);
        Assert.True(result.RowsCount > 0);
    }

    [Fact]
    public async Task Import_SecondFile_DeactivatesPreviousBatch()
    {
        await using var db = DbTestFactory.CreateContext();
        var (service, employerId) = await CreateServiceAsync(db);

        var first = await ImportValidFileAsync(service, employerId, "v1.xlsx");
        var second = await ImportValidFileAsync(service, employerId, "v2.xlsx");

        var firstBatch = await db.PayrollMonthlyInputBatches.FindAsync(first.BatchId);
        Assert.NotNull(firstBatch);
        Assert.False(firstBatch!.IsActive);
        Assert.NotEqual(first.BatchId, second.BatchId);
    }

    [Fact]
    public async Task Import_SecondFile_OnlyNewestBatchActive()
    {
        await using var db = DbTestFactory.CreateContext();
        var (service, employerId) = await CreateServiceAsync(db);

        await ImportValidFileAsync(service, employerId, "old.xlsx");
        var second = await ImportValidFileAsync(service, employerId, "new.xlsx");

        var activeBatches = await db.PayrollMonthlyInputBatches
            .Where(b =>
                b.EmployerId == employerId
                && b.Month == Month
                && b.GregorianYear == GregorianYear
                && b.IsActive
                && !b.IsDeleted)
            .ToListAsync();

        Assert.Single(activeBatches);
        Assert.Equal(second.BatchId, activeBatches[0].Id);
    }

    [Fact]
    public async Task Import_GetYearStatus_ShowsCaptured()
    {
        await using var db = DbTestFactory.CreateContext();
        var (service, employerId) = await CreateServiceAsync(db);

        await ImportValidFileAsync(service, employerId, "status.xlsx");

        var status = await service.GetYearStatusAsync(employerId, Year);
        var september = status.First(s => s.Month == Month && s.GregorianYear == GregorianYear);
        Assert.Equal("נקלט", september.Status);
        Assert.NotNull(september.BatchId);
        Assert.True(september.RowsCount > 0);
    }

    [Fact]
    public async Task Import_ParseFailure_NoBatchCreated()
    {
        await using var db = DbTestFactory.CreateContext();
        var (service, employerId) = await CreateServiceAsync(db);

        await using var invalid = InvalidUploadWorkbooks.NoPayrollHeaders();
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ImportMonthAsync(employerId, Year, Month, invalid, "bad.xlsx"));

        Assert.Equal(0, await db.PayrollMonthlyInputBatches.CountAsync());
        Assert.Equal(0, await db.PayrollMonthlyInputRows.CountAsync());
    }

    private static async Task<(PayrollMonthlyInputService Service, int EmployerId)> CreateServiceAsync(PayrollDbContext db)
    {
        var employer = await ReportTestData.SeedEmployerAsync(db);
        return (new PayrollMonthlyInputService(db, new ImportTestCurrentUser()), employer.Id);
    }

    private static async Task<Contracts.PayrollImportResultDto> ImportValidFileAsync(
        PayrollMonthlyInputService service,
        int employerId,
        string fileName)
    {
        await using var upload = ValidUploadStream();
        return await service.ImportMonthAsync(employerId, Year, Month, upload, fileName);
    }

    private static MemoryStream ValidUploadStream() =>
        MonthlyComparisonUploadWorkbook.Create(
            "123456789",
            1001,
            "Import Worker",
            Month,
            GregorianYear,
            b => b.Band1());

    private sealed class ImportTestCurrentUser : ICurrentUserService
    {
        public string? UserId => "1";
        public string? Username => "import-test";
        public string? Role => UserRoles.Admin;
        public string GetAuditActor() => "import-test";
    }
}
