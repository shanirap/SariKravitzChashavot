using AccountingProject.Data;
using AccountingProject.Domain;
using AccountingProject.Infrastructure;
using AccountingProject.Models;
using AccountingProject.Services;
using AccountingProject.Tests.TestHelpers;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;

namespace AccountingProject.Tests;

public sealed class AnnualComparisonFromSavedReportTests
{
    private const string SheetName = "השוואה שנתית";
    private const string Year = "תשפ\"ו";
    private const int Month = 9;
    private const int GregorianYear = 2025;
    private const int FirstMonthColumn = 10;
    private const int LastMonthColumn = 21;

    [Fact]
    public async Task AnnualComparisonFromSaved_NoBatches_AllMonthColumnsShowNotCaptured()
    {
        await using var db = DbTestFactory.CreateContext();
        var employerId = await SeedMatchingEmploymentAsync(db);

        using var wb = await OpenSavedReportAsync(db, employerId, importSeptember: false);
        var ws = wb.Worksheet(SheetName);

        for (var col = FirstMonthColumn; col <= LastMonthColumn; col++)
            Assert.Equal(AnnualComparisonReportBuilder.NotCapturedInInput, ws.Cell(2, col).GetString());
    }

    [Fact]
    public async Task AnnualComparisonFromSaved_OneImportedMonth_ComparedOthersNotCaptured()
    {
        await using var db = DbTestFactory.CreateContext();
        var employerId = await SeedMatchingEmploymentAsync(db);

        using var wb = await OpenSavedReportAsync(db, employerId, importSeptember: true);
        var ws = wb.Worksheet(SheetName);

        Assert.Equal("V", ws.Cell(2, FirstMonthColumn).GetString());
        for (var col = FirstMonthColumn + 1; col <= LastMonthColumn; col++)
            Assert.Equal(AnnualComparisonReportBuilder.NotCapturedInInput, ws.Cell(2, col).GetString());
    }

    [Fact]
    public async Task AnnualComparisonFromSaved_EditedSavedRow_AffectsReport()
    {
        await using var db = DbTestFactory.CreateContext();
        var employerId = await SeedMatchingEmploymentAsync(db);
        var importService = new PayrollMonthlyInputService(db, new TestCurrentUser());
        await using var upload = ValidSeptemberUpload();
        await importService.ImportMonthAsync(employerId, Year, Month, upload, "base.xlsx");

        var savedRow = await db.PayrollMonthlyInputRows.SingleAsync(r => r.IdNumber == "123456789" && !r.IsDeleted);
        savedRow.Role = "סייעת";
        await db.SaveChangesAsync();

        using var wb = await OpenSavedReportAsync(db, employerId, importSeptember: false);
        var cell = wb.Worksheet(SheetName).Cell(2, FirstMonthColumn).GetString();
        Assert.Contains("תפקיד:", cell);
        Assert.Contains("סייעת", cell);
    }

    [Fact]
    public async Task AnnualComparisonFromSaved_GeneralMultiplierNonZero_ShowsDifference()
    {
        await using var db = DbTestFactory.CreateContext();
        var employerId = await SeedMatchingEmploymentAsync(db);
        var importService = new PayrollMonthlyInputService(db, new TestCurrentUser());
        await using var upload = MonthlyComparisonUploadWorkbook.Create(
            "123456789",
            1001,
            "Import Worker",
            Month,
            GregorianYear,
            b => b.Band1(misra1Hours: 30m, misra1Base: 28m, jobPercent: 100m, doubleGeneral: 10m));
        await importService.ImportMonthAsync(employerId, Year, Month, upload, "mult.xlsx");

        using var wb = await OpenSavedReportAsync(db, employerId, importSeptember: false);
        var cell = wb.Worksheet(SheetName).Cell(2, FirstMonthColumn).GetString();
        Assert.Contains("הכפלה כללית:", cell);
    }

    private static async Task<int> SeedMatchingEmploymentAsync(PayrollDbContext db)
    {
        var employer = await ReportTestData.SeedEmployerAsync(db);
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "123456789", "Import", "Worker");
        var ed = await ReportTestData.SeedEmploymentWithSlotAsync(
            db, employer.Id, employee.Id, "SYM-1", grade1Role: "גננת", weeklyHours: 30m);
        ed.Grade1JobPercent = 100m;
        await db.SaveChangesAsync();
        return employer.Id;
    }

    private static async Task<XLWorkbook> OpenSavedReportAsync(
        PayrollDbContext db,
        int employerId,
        bool importSeptember)
    {
        if (importSeptember)
        {
            var importService = new PayrollMonthlyInputService(db, new TestCurrentUser());
            await using var upload = ValidSeptemberUpload();
            await importService.ImportMonthAsync(employerId, Year, Month, upload, "sept.xlsx");
        }

        var bytes = await new ReportExportService(db).AnnualComparisonFromSavedDataAsync(employerId, Year);
        return new XLWorkbook(new MemoryStream(bytes));
    }

    private static MemoryStream ValidSeptemberUpload() =>
        MonthlyComparisonUploadWorkbook.Create(
            "123456789",
            1001,
            "Import Worker",
            Month,
            GregorianYear,
            b => b.Band1(misra1Hours: 30m, misra1Base: 28m, jobPercent: 100m));

    private sealed class TestCurrentUser : ICurrentUserService
    {
        public string? UserId => "1";
        public string? Username => "saved-report-test";
        public string? Role => UserRoles.Admin;
        public string GetAuditActor() => "saved-report-test";
    }
}
