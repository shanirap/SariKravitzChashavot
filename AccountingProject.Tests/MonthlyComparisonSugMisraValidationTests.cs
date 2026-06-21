using AccountingProject.Data;
using AccountingProject.Services;
using AccountingProject.Tests.TestHelpers;
using ClosedXML.Excel;

namespace AccountingProject.Tests;

/// <summary>Regression tests for monthly-comparison validation of payroll "סוג משרה".</summary>
public sealed class MonthlyComparisonSugMisraValidationTests
{
    private const string SheetName = "השוואה חודשית";
    private const string Year = "תשפ\"ו";
    private const int ColRole = 6;
    private const int ColSugMisra = 7;
    private const int ColHoursSum = 10;
    private const int CompareRow = 4;
    private const int InputRow = 3;

    [Fact]
    public void Change1_IsMonthlyJobType_RequiresExactMonthlyJobType()
    {
        Assert.True(PayrollComparisonUploadSupport.IsMonthlyJobType("משרה חודשית"));
        Assert.True(PayrollComparisonUploadSupport.IsMonthlyJobType("  משרה   חודשית  "));
        Assert.False(PayrollComparisonUploadSupport.IsMonthlyJobType("משרה שעתית"));
        Assert.False(PayrollComparisonUploadSupport.IsMonthlyJobType("גננת"));
        Assert.False(PayrollComparisonUploadSupport.IsMonthlyJobType(null));
        Assert.False(PayrollComparisonUploadSupport.IsMonthlyJobType(""));
        Assert.Equal("משרה חודשית", PayrollComparisonUploadSupport.ExpectedMonthlyJobType);
    }

    [Fact]
    public async Task Change2_MonthlyComparison_ValidSugMisra_CompareRowShowsV()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "101010101");
        await ReportTestData.SeedEmploymentWithSlotAsync(
            db, employer.Id, employee.Id, "SYM-1", weeklyHours: 30m);

        await using var upload = MonthlyComparisonUploadWorkbook.Create(
            "101010101", null, "Valid Monthly", 9, 2025, b => b.Band1(misra1Hours: 30m));

        var bytes = await new ReportExportService(db).MonthlyComparisonAsync(employer.Id, Year, 9, upload);
        using var wb = new XLWorkbook(new MemoryStream(bytes));
        var ws = wb.Worksheet(SheetName);

        Assert.Equal("משרה חודשית", ws.Cell(InputRow, ColSugMisra).GetString());
        Assert.Equal("V", ws.Cell(CompareRow, ColSugMisra).GetString());
        Assert.Equal("V", ws.Cell(CompareRow, ColHoursSum).GetString());
    }

    [Fact]
    public async Task Change3_MonthlyComparison_NonMonthlySugMisra_CompareRowShowsX()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "202020202");
        await ReportTestData.SeedEmploymentWithSlotAsync(
            db, employer.Id, employee.Id, "SYM-1", weeklyHours: 30m);

        await using var upload = MonthlyComparisonUploadWorkbook.Create(
            "202020202", null, "Hourly Job", 9, 2025, b => b.Band1(misra1Hours: 30m));

        using var wbIn = new XLWorkbook(upload);
        wbIn.Worksheet(1).Cell(4, 6).Value = "משרה שעתית";
        var ms = new MemoryStream();
        wbIn.SaveAs(ms);
        ms.Position = 0;

        var bytes = await new ReportExportService(db).MonthlyComparisonAsync(employer.Id, Year, 9, ms);
        using var wb = new XLWorkbook(new MemoryStream(bytes));
        var ws = wb.Worksheet(SheetName);

        Assert.Equal("משרה שעתית", ws.Cell(InputRow, ColSugMisra).GetString());
        Assert.Equal("X", ws.Cell(CompareRow, ColSugMisra).GetString());
        Assert.Equal("V", ws.Cell(CompareRow, ColHoursSum).GetString());
    }

    [Fact]
    public async Task Change4_MonthlyComparison_NoUploadRow_EmptySugMisraCompareRowShowsX()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        await ReportTestData.SeedEmploymentWithSlotAsync(
            db, employer.Id,
            (await ReportTestData.SeedEmployeeAsync(db, employer.Id, "303030303")).Id,
            "SYM-1");

        await using var upload = MonthlyComparisonUploadWorkbook.Create(
            "404040404", null, "Other Employee", 9, 2025, b => b.Band1());

        var bytes = await new ReportExportService(db).MonthlyComparisonAsync(employer.Id, Year, 9, upload);
        using var wb = new XLWorkbook(new MemoryStream(bytes));
        var ws = wb.Worksheet(SheetName);

        Assert.Equal("", ws.Cell(InputRow, ColSugMisra).GetString());
        Assert.Equal("X", ws.Cell(CompareRow, ColSugMisra).GetString());
        Assert.Equal("X", ws.Cell(CompareRow, ColHoursSum).GetString());
    }

    [Fact]
    public async Task Change5_MonthlyComparison_RoleNotCompared_WrongSugMisraStillShowsX()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "505050505");
        await ReportTestData.SeedEmploymentWithSlotAsync(
            db, employer.Id, employee.Id, "SYM-1", weeklyHours: 30m, grade1Role: "גננת");

        await using var upload = MonthlyComparisonUploadWorkbook.Create(
            "505050505", null, "Role Diff", 9, 2025, b => b.Band1(misra1Hours: 30m));

        using var wbIn = new XLWorkbook(upload);
        wbIn.Worksheet(1).Cell(4, 6).Value = "סייעת";
        var ms = new MemoryStream();
        wbIn.SaveAs(ms);
        ms.Position = 0;

        var bytes = await new ReportExportService(db).MonthlyComparisonAsync(employer.Id, Year, 9, ms);
        using var wb = new XLWorkbook(new MemoryStream(bytes));
        var ws = wb.Worksheet(SheetName);

        Assert.Equal("גננת", ws.Cell(2, ColRole).GetString());
        Assert.Equal("סייעת", ws.Cell(InputRow, ColSugMisra).GetString());
        Assert.Equal("", ws.Cell(CompareRow, ColRole).GetString());
        Assert.Equal("X", ws.Cell(CompareRow, ColSugMisra).GetString());
        Assert.Equal("V", ws.Cell(CompareRow, ColHoursSum).GetString());
    }
}
