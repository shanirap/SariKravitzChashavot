using AccountingProject.Models;
using AccountingProject.Services;
using AccountingProject.Tests.TestHelpers;
using ClosedXML.Excel;

namespace AccountingProject.Tests;

/// <summary>תרחישי כשל משותפים לדוחות השוואה (חודשי / שנתי).</summary>
public sealed class ReportExportServiceErrorTests
{
    private const string Year = "תשפ\"ו";

    [Fact]
    public async Task MonthlyComparison_NoPayrollHeaders_ThrowsNoDataRows()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        await using var upload = InvalidUploadWorkbooks.NoPayrollHeaders();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new ReportExportService(db).MonthlyComparisonAsync(employer.Id, Year, 9, upload));

        Assert.Contains("כותרות", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AnnualComparison_NoPayrollHeaders_ThrowsHeaderError()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        await using var upload = InvalidUploadWorkbooks.NoPayrollHeaders();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new ReportExportService(db).AnnualComparisonAsync(employer.Id, Year, upload));

        Assert.Contains("כותרות", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task MonthlyComparison_NoMatchingRows_ThrowsMonthMessage()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        await using var upload = InvalidUploadWorkbooks.WrongMonthOnly();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new ReportExportService(db).MonthlyComparisonAsync(employer.Id, Year, 9, upload));

        Assert.Contains("לא נמצאו שורות נתונים", ex.Message, StringComparison.Ordinal);
        Assert.Contains("9", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task MonthlyComparison_NoEmploymentSlots_ThrowsSlotsMessage()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "111222333");
        db.EmploymentData.Add(new EmploymentData
        {
            EmployeeId = employee.Id,
            EmployerId = employer.Id,
            AcademicYear = Year,
            Grade1Role = "גננת",
            Slots = [],
        });
        await db.SaveChangesAsync();

        await using var upload = MonthlyComparisonUploadWorkbook.Create(
            "111222333", null, "Worker", 9, 2025, b => b.Band1());

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new ReportExportService(db).MonthlyComparisonAsync(employer.Id, Year, 9, upload));

        Assert.Equal("לא נמצאו מקטעי העסקה להשוואה.", ex.Message);
    }

    [Fact]
    public async Task AnnualComparison_NoEmploymentSlots_ThrowsSlotsMessage()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "222333444");
        db.EmploymentData.Add(new EmploymentData
        {
            EmployeeId = employee.Id,
            EmployerId = employer.Id,
            AcademicYear = Year,
            Grade1Role = "גננת",
            Slots = [],
        });
        await db.SaveChangesAsync();

        await using var upload = MonthlyComparisonUploadWorkbook.Create(
            "222333444", null, "Worker", 9, 2025, b => b.Band1());

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new ReportExportService(db).AnnualComparisonAsync(employer.Id, Year, upload));

        Assert.Equal("לא נמצאו מקטעי העסקה להשוואה.", ex.Message);
    }

    [Fact]
    public async Task KindergartenAnnual_EmptyEmployer_ReturnsHeaderOnlyWorkbook()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);

        var bytes = await new ReportExportService(db).KindergartenAnnualAsync(employer.Id, Year);

        using var wb = new XLWorkbook(new MemoryStream(bytes));
        var ws = wb.Worksheet("מצבת גנים");
        Assert.Equal("סמל מוסד", ws.Cell(1, 1).GetString());
        Assert.Equal(1, ws.LastRowUsed()?.RowNumber());
    }

    [Fact]
    public async Task EmployeesPersonal_EmptyEmployer_ReturnsHeadersOnly()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);

        var bytes = await new ReportExportService(db).EmployeesPersonalAsync(employer.Id);

        using var wb = new XLWorkbook(new MemoryStream(bytes));
        var ws = wb.Worksheet("עובדים אישיים");
        Assert.Equal("שם פרטי", ws.Cell(1, 1).GetString());
        Assert.Equal(1, ws.LastRowUsed()?.RowNumber());
    }

    [Fact]
    public async Task AnnualComparisonFromSaved_NoEmploymentSlots_ThrowsSlotsMessage()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "555666777");
        db.EmploymentData.Add(new EmploymentData
        {
            EmployeeId = employee.Id,
            EmployerId = employer.Id,
            AcademicYear = Year,
            Grade1Role = "גננת",
            Slots = [],
        });
        await db.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new ReportExportService(db).AnnualComparisonFromSavedDataAsync(employer.Id, Year));

        Assert.Equal("לא נמצאו מקטעי העסקה להשוואה.", ex.Message);
    }

    [Fact]
    public async Task AnnualComparisonFromSaved_UnknownEmployer_ThrowsNotFoundMessage()
    {
        await using var db = DbTestFactory.CreateContext();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new ReportExportService(db).AnnualComparisonFromSavedDataAsync(99999, Year));

        Assert.Equal("המעסיק לא נמצא במערכת.", ex.Message);
    }

    [Fact]
    public async Task InstitutionHours_UnknownSymbol_ReturnsStandardAndZeroRoster()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "333444555");
        await ReportTestData.SeedEmploymentWithSlotAsync(db, employer.Id, employee.Id, "KNOWN");

        var bytes = await new ReportExportService(db).InstitutionHoursAsync(
            employer.Id, Year, "UNKNOWN-SYM");

        using var wb = new XLWorkbook(new MemoryStream(bytes));
        var ws = wb.Worksheet("בדיקת שעות לסמל");
        Assert.Equal("UNKNOWN-SYM", ws.Cell(2, 1).GetString());
        Assert.Equal(0m, ws.Cell(3, 2).GetValue<decimal>());
        Assert.Equal(0m, ws.Cell(3, 3).GetValue<decimal>());
    }
}
