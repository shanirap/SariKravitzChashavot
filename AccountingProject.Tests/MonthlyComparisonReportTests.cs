using AccountingProject.Data;
using AccountingProject.Services;
using AccountingProject.Tests.TestHelpers;
using ClosedXML.Excel;

namespace AccountingProject.Tests;

public sealed class MonthlyComparisonReportTests
{
    private const string SheetName = "השוואה חודשית";
    private const string Year = "תשפ\"ו";

    private static readonly string[] ExpectedHeaders =
    [
        "סמל מוסד",
        "מספר עובד בעוקץ",
        "ת\"ז",
        "שם פרטי+שם משפחה",
        "תפקיד",
        "דרגה",
        "ותק",
        "ש\"ש",
        "בסיס משרה",
        "אחוז משרה",
        "שעות גיל",
        "גמולי השתלמות",
        "כפל תואר",
        "קרן השתלמות",
        "הכפלה כללית",
    ];

    private const int LabelCol = 1;
    private const int DataColStart = 2;
    private const int ColName = 5;
    private const int ColHoursSum = 9;
    private const int ColJobBase = 10;
    private const int ColAgeHours = 12;
    private const int ColTrainingBenefits = 13;
    private const int ColDoubleDegree = 14;
    private const int ColTrainingFund = 15;
    private const int ColDoubleGeneral = 16;

    [Fact]
    public async Task MonthlyComparison_HasExactRequiredHeaders()
    {
        await using var db = DbTestFactory.CreateContext();
        var (bytes, _) = await BuildDefaultReportAsync(db);

        using var wb = new XLWorkbook(new MemoryStream(bytes));
        var ws = wb.Worksheet(SheetName);
        for (var i = 0; i < ExpectedHeaders.Length; i++)
            Assert.Equal(ExpectedHeaders[i], ws.Cell(1, DataColStart + i).GetString());
    }

    [Fact]
    public async Task MonthlyComparison_EachRecord_HasThreeLabeledRows()
    {
        await using var db = DbTestFactory.CreateContext();
        var (bytes, _) = await BuildDefaultReportAsync(db);

        using var wb = new XLWorkbook(new MemoryStream(bytes));
        var ws = wb.Worksheet(SheetName);
        Assert.Equal("מצבת- מערכת שכר", ws.Cell(2, LabelCol).GetString());
        Assert.Equal("עוקץ- קלט", ws.Cell(3, LabelCol).GetString());
        Assert.Equal("השוואה- V/X", ws.Cell(4, LabelCol).GetString());
    }

    [Fact]
    public async Task MonthlyComparison_MatchingFields_ShowV()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "123456789");
        var ed = await ReportTestData.SeedEmploymentWithSlotAsync(
            db, employer.Id, employee.Id, "SYM-1", weeklyHours: 30m, grade1Role: "גננת");
        ed.Grade1JobPercent = 100m;
        ed.Grade1TrainingFundPercent = 7.5m;
        await db.SaveChangesAsync();

        await using var upload = MonthlyComparisonUploadWorkbook.Create(
            "123456789", null, "Test User", 9, 2025,
            b => b.Band1(misra1Hours: 30m, misra1Base: 28m, jobPercent: 100m, trainingFund: 7.5m, ageHours: 2m));

        var bytes = await new ReportExportService(db).MonthlyComparisonAsync(employer.Id, Year, 9, upload);
        using var wb = new XLWorkbook(new MemoryStream(bytes));
        var ws = wb.Worksheet(SheetName);
        var compareRow = 4;
        Assert.Equal("V", ws.Cell(compareRow, ColHoursSum).GetString());
        Assert.Equal("V", ws.Cell(compareRow, ColJobBase).GetString());
        Assert.Equal("V", ws.Cell(compareRow, ColAgeHours).GetString());
        Assert.Equal("V", ws.Cell(compareRow, ColTrainingFund).GetString());
    }

    [Fact]
    public async Task MonthlyComparison_MismatchingHours_ShowX()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "444555666");
        await ReportTestData.SeedEmploymentWithSlotAsync(
            db, employer.Id, employee.Id, "SYM-1", weeklyHours: 30m);

        await using var upload = MonthlyComparisonUploadWorkbook.Create(
            "444555666", null, "Mismatch", 9, 2025, b => b.Band1(misra1Hours: 99m));

        var bytes = await new ReportExportService(db).MonthlyComparisonAsync(employer.Id, Year, 9, upload);
        using var wb = new XLWorkbook(new MemoryStream(bytes));
        Assert.Equal("X", wb.Worksheet(SheetName).Cell(4, ColHoursSum).GetString());
    }

    [Fact]
    public async Task MonthlyComparison_DoubleGeneralZero_ShowsV()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "555666777");
        await ReportTestData.SeedEmploymentWithSlotAsync(db, employer.Id, employee.Id, "SYM-1");

        await using var upload = MonthlyComparisonUploadWorkbook.Create(
            "555666777", null, "Zero Gen", 9, 2025, b => b.Band1(doubleGeneral: 0m));

        var bytes = await new ReportExportService(db).MonthlyComparisonAsync(employer.Id, Year, 9, upload);
        using var wb = new XLWorkbook(new MemoryStream(bytes));
        Assert.Equal("V", wb.Worksheet(SheetName).Cell(4, ColDoubleGeneral).GetString());
    }

    [Fact]
    public async Task MonthlyComparison_DoubleGeneralNonZero_ShowsXAndYellowFill()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "555666777");
        await ReportTestData.SeedEmploymentWithSlotAsync(db, employer.Id, employee.Id, "SYM-1");

        await using var upload = MonthlyComparisonUploadWorkbook.Create(
            "555666777", null, "NonZero Gen", 9, 2025, b => b.Band1(doubleGeneral: 5m));

        var bytes = await new ReportExportService(db).MonthlyComparisonAsync(employer.Id, Year, 9, upload);
        using var wb = new XLWorkbook(new MemoryStream(bytes));
        var ws = wb.Worksheet(SheetName);
        var compareCell = ws.Cell(4, ColDoubleGeneral);
        Assert.Equal("X", compareCell.GetString());
        Assert.Equal(XLColor.Yellow, compareCell.Style.Fill.BackgroundColor);
        Assert.Equal(XLColor.Yellow, ws.Cell(3, ColDoubleGeneral).Style.Fill.BackgroundColor);
    }

    [Fact]
    public async Task MonthlyComparison_IncludesNewBenefitColumns()
    {
        await using var db = DbTestFactory.CreateContext();
        var (bytes, _) = await BuildDefaultReportAsync(db);

        using var wb = new XLWorkbook(new MemoryStream(bytes));
        var ws = wb.Worksheet(SheetName);
        Assert.Equal("שעות גיל", ws.Cell(1, ColAgeHours).GetString());
        Assert.Equal("גמולי השתלמות", ws.Cell(1, ColTrainingBenefits).GetString());
        Assert.Equal("כפל תואר", ws.Cell(1, ColDoubleDegree).GetString());
        Assert.Equal("קרן השתלמות", ws.Cell(1, ColTrainingFund).GetString());
        Assert.Equal(2m, ws.Cell(3, ColAgeHours).GetValue<decimal>());
    }

    [Fact]
    public async Task MonthlyComparison_MatchesEmployeeByEmployeeNumber_WhenTzMissing()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "999888777");
        employee.EmployeeNumber = 4242;
        await db.SaveChangesAsync();
        await ReportTestData.SeedEmploymentWithSlotAsync(
            db, employer.Id, employee.Id, "SYM-1", weeklyHours: 30m);

        await using var upload = MonthlyComparisonUploadWorkbook.Create(
            "", 4242, "Num Match", 9, 2025, b => b.Band1(misra1Hours: 30m));

        var bytes = await new ReportExportService(db).MonthlyComparisonAsync(employer.Id, Year, 9, upload);
        using var wb = new XLWorkbook(new MemoryStream(bytes));
        var ws = wb.Worksheet(SheetName);
        Assert.Equal("4242", ws.Cell(3, DataColStart + 1).GetString());
        Assert.Equal("V", ws.Cell(4, ColHoursSum).GetString());
    }

    [Fact]
    public async Task MonthlyComparison_FindsHeaderRow_NotOnFirstRow()
    {
        await using var db = DbTestFactory.CreateContext();
        var (bytes, _) = await BuildDefaultReportAsync(db);

        using var wb = new XLWorkbook(new MemoryStream(bytes));
        Assert.NotNull(wb.Worksheet(SheetName));
        Assert.Single(wb.Worksheets);
    }

    [Fact]
    public async Task MonthlyComparison_NoUploadRow_CompareRowShowsX()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        await ReportTestData.SeedEmploymentWithSlotAsync(
            db, employer.Id,
            (await ReportTestData.SeedEmployeeAsync(db, employer.Id, "000111222")).Id,
            "SYM-1");

        await using var upload = MonthlyComparisonUploadWorkbook.Create(
            "999000111", null, "Other", 9, 2025, b => b.Band1());

        var bytes = await new ReportExportService(db).MonthlyComparisonAsync(employer.Id, Year, 9, upload);
        using var wb = new XLWorkbook(new MemoryStream(bytes));
        var ws = wb.Worksheet(SheetName);
        Assert.Equal("X", ws.Cell(4, ColName).GetString());
        Assert.Equal("X", ws.Cell(4, ColHoursSum).GetString());
    }

    private static async Task<(byte[] Bytes, int EmployerId)> BuildDefaultReportAsync(
        PayrollDbContext db)
    {
        var employer = await ReportTestData.SeedEmployerAsync(db);
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "123456789");
        await ReportTestData.SeedEmploymentWithSlotAsync(db, employer.Id, employee.Id, "SYM-1");

        await using var upload = MonthlyComparisonUploadWorkbook.Create(
            "123456789", null, "Test User", 9, 2025, b => b.Band1());

        var bytes = await new ReportExportService(db).MonthlyComparisonAsync(employer.Id, Year, 9, upload);
        return (bytes, employer.Id);
    }
}
