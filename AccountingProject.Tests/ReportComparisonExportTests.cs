using AccountingProject.Services;
using AccountingProject.Tests.TestHelpers;
using ClosedXML.Excel;

namespace AccountingProject.Tests;

public sealed class ReportComparisonExportTests
{
    [Fact]
    public async Task MonthlyComparison_ProducesComparisonWorksheetWithVXMarks()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "123456789");
        await ReportTestData.SeedEmploymentWithSlotAsync(
            db, employer.Id, employee.Id, "SYM-1", weeklyHours: 30m, grade1Role: "גננת");

        await using var upload = MonthlyComparisonUploadWorkbook.Create(
            "123456789", null, employee.FirstName + " " + employee.LastName, 9, 2025,
            b => b.Band1(misra1Hours: 30m, misra1Base: 28m));

        var bytes = await new ReportExportService(db).MonthlyComparisonAsync(
            employer.Id, ReportTestData.DefaultAcademicYear, 9, upload);

        using var wb = new XLWorkbook(new MemoryStream(bytes));
        var ws = wb.Worksheet("השוואה חודשית");
        Assert.Equal("סמל מוסד", ws.Cell(1, 2).GetString());
        Assert.Equal("מספר עובד בעוקץ", ws.Cell(1, 3).GetString());
        Assert.Equal("השוואה- V/X", ws.Cell(4, 1).GetString());
        Assert.Single(wb.Worksheets);
        Assert.True(ws.Cell(4, 9).GetString() is "V" or "X");
    }

    [Fact]
    public async Task AnnualComparison_IncludesStaticAndMonthColumns()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "123456789");
        await ReportTestData.SeedEmploymentWithSlotAsync(db, employer.Id, employee.Id, "SYM-2");

        await using var upload = MonthlyComparisonUploadWorkbook.Create(
            "123456789", null, "Test User", 9, 2025, b => b.Band1());

        var bytes = await new ReportExportService(db).AnnualComparisonAsync(
            employer.Id, ReportTestData.DefaultAcademicYear, upload);

        using var wb = new XLWorkbook(new MemoryStream(bytes));
        var ws = wb.Worksheet("השוואה שנתית");
        Assert.Equal("הכפלה כללית", ws.Cell(1, 9).GetString());
        Assert.Equal("9.2025", ws.Cell(1, 10).GetString());
        Assert.Single(wb.Worksheets);
        Assert.True(ws.RowsUsed().Count() >= 2);
    }
}
