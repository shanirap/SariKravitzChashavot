using AccountingProject.Contracts;
using AccountingProject.Data;
using AccountingProject.Infrastructure;
using AccountingProject.Models;
using AccountingProject.Services;
using AccountingProject.Tests.TestHelpers;
using Microsoft.EntityFrameworkCore;

namespace AccountingProject.Tests;

public sealed class PayrollMonthlyInputServiceEdgeCaseTests
{
    private const string Year = "תשפ\"ו";
    private const int Month = 9;

    [Fact]
    public async Task ImportMonthAsync_UnknownEmployer_ThrowsEmployerNotFound()
    {
        await using var db = DbTestFactory.CreateContext();
        var service = new PayrollMonthlyInputService(db, new EdgeTestUser());
        await using var upload = ValidUploadStream();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ImportMonthAsync(99999, Year, Month, upload, "x.xlsx"));

        Assert.Equal("המעסיק לא נמצא במערכת.", ex.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ImportMonthAsync_InvalidEmployerId_ThrowsInvalidEmployer(int employerId)
    {
        await using var db = DbTestFactory.CreateContext();
        var service = new PayrollMonthlyInputService(db, new EdgeTestUser());
        await using var upload = ValidUploadStream();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ImportMonthAsync(employerId, Year, Month, upload, "x.xlsx"));

        Assert.Equal("מזהה מעסיק לא תקין.", ex.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ImportMonthAsync_MissingAcademicYear_ThrowsYearRequired(string year)
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        var service = new PayrollMonthlyInputService(db, new EdgeTestUser());
        await using var upload = ValidUploadStream();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ImportMonthAsync(employer.Id, year, Month, upload, "x.xlsx"));

        Assert.Equal("שנת לימודים נדרשת.", ex.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    public async Task ImportMonthAsync_InvalidMonth_ThrowsMonthRange(int month)
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        var service = new PayrollMonthlyInputService(db, new EdgeTestUser());
        await using var upload = ValidUploadStream();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ImportMonthAsync(employer.Id, Year, month, upload, "x.xlsx"));

        Assert.Equal("חודש חייב להיות בין 1 ל-12.", ex.Message);
    }

    [Fact]
    public async Task ImportMonthAsync_InvalidAcademicYear_ThrowsInvalidYearMessage()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        var service = new PayrollMonthlyInputService(db, new EdgeTestUser());
        await using var upload = ValidUploadStream();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ImportMonthAsync(employer.Id, "!!!", Month, upload, "y.xlsx"));

        Assert.Equal("שנת לימודים לא תקינה.", ex.Message);
    }

    [Fact]
    public async Task ImportMonthAsync_WrongMonthRows_ThrowsNoRowsForMonth()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        var service = new PayrollMonthlyInputService(db, new EdgeTestUser());
        await using var upload = InvalidUploadWorkbooks.WrongMonthOnly();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ImportMonthAsync(employer.Id, Year, Month, upload, "wrong.xlsx"));

        Assert.Contains("לא נמצאו שורות נתונים בקובץ לחודש 9", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ImportMonthAsync_CorruptBytes_ThrowsCorruptFileMessage()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        var service = new PayrollMonthlyInputService(db, new EdgeTestUser());
        await using var corrupt = new MemoryStream([0x00, 0x01, 0x02, 0x03]);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ImportMonthAsync(employer.Id, Year, Month, corrupt, "bad.xlsx"));

        Assert.Contains("קובץ ה-Excel פגום", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ImportMonthAsync_BlankFileName_StoresUploadXlsx()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        var service = new PayrollMonthlyInputService(db, new EdgeTestUser());
        await using var upload = ValidUploadStream();

        var result = await service.ImportMonthAsync(employer.Id, Year, Month, upload, "   ");

        var batch = await db.PayrollMonthlyInputBatches.FindAsync(result.BatchId);
        Assert.Equal("upload.xlsx", batch!.OriginalFileName);
    }

    [Fact]
    public async Task ImportMonthAsync_LongAuditActor_TruncatesUploadedByTo200()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        var longName = new string('א', 250);
        var service = new PayrollMonthlyInputService(db, new EdgeTestUser(longName));
        await using var upload = ValidUploadStream();

        var result = await service.ImportMonthAsync(employer.Id, Year, Month, upload, "long.xlsx");

        var batch = await db.PayrollMonthlyInputBatches.FindAsync(result.BatchId);
        Assert.NotNull(batch!.UploadedBy);
        Assert.Equal(200, batch.UploadedBy!.Length);
    }

    [Fact]
    public async Task UpdateRowAsync_UnknownRow_ThrowsRowNotFound()
    {
        await using var db = DbTestFactory.CreateContext();
        var service = new PayrollMonthlyInputService(db, new EdgeTestUser());

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UpdateRowAsync(999999, new PayrollMonthlyInputRowEditDto { Role = "x" }));

        Assert.Equal("שורת קלט עוקץ חודשי לא נמצאה.", ex.Message);
    }

    [Fact]
    public async Task DeleteRowAsync_UnknownRow_ThrowsRowNotFound()
    {
        await using var db = DbTestFactory.CreateContext();
        var service = new PayrollMonthlyInputService(db, new EdgeTestUser());

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.DeleteRowAsync(888888));

        Assert.Equal("שורת קלט עוקץ חודשי לא נמצאה.", ex.Message);
    }

    [Fact]
    public async Task GetRowsAsync_NoBatch_ReturnsEmptyList()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        var service = new PayrollMonthlyInputService(db, new EdgeTestUser());

        var rows = await service.GetRowsAsync(employer.Id, Year, Month);

        Assert.Empty(rows);
    }

    private static MemoryStream ValidUploadStream() =>
        MonthlyComparisonUploadWorkbook.Create(
            "123456789", 1001, "Edge Worker", Month, 2025, b => b.Band1());

    private sealed class EdgeTestUser(string? auditActor = "edge-test") : ICurrentUserService
    {
        public string? UserId => "1";
        public string? Username => "edge";
        public string? Role => UserRoles.Admin;
        public string GetAuditActor() => auditActor ?? "edge-test";
    }
}
