using AccountingProject.Domain;
using AccountingProject.Services;
using AccountingProject.Tests.TestHelpers;
using ClosedXML.Excel;

namespace AccountingProject.Tests;

public sealed class AnnualComparisonReportTests
{
    private const string SheetName = "השוואה שנתית";
    private const string Year = "תשפ\"ו";
    /// <summary>First school-year month column (10 static headers + 1).</summary>
    private const int FirstMonthColumn = 11;

    [Fact]
    public async Task AnnualComparison_WithoutYearColumn_InfersGregorianYear()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "123456789");
        await ReportTestData.SeedEmploymentWithSlotAsync(db, employer.Id, employee.Id, "SYM-1");

        await using var upload = MonthlyComparisonUploadWorkbook.Create(
            "123456789", null, "Test User", 9, 2025, b => b.Band1(), includeYearColumn: false);

        var bytes = await new ReportExportService(db).AnnualComparisonAsync(employer.Id, Year, upload);

        using var wb = new XLWorkbook(new MemoryStream(bytes));
        var ws = wb.Worksheet(SheetName);
        Assert.True(ws.LastRowUsed()?.RowNumber() >= 2);
    }

    [Fact]
    public async Task AnnualComparison_FindsHeaderRow_NotOnFirstRow()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "123456789");
        await ReportTestData.SeedEmploymentWithSlotAsync(db, employer.Id, employee.Id, "SYM-1");

        await using var upload = MonthlyComparisonUploadWorkbook.Create(
            "123456789", null, "Test User", 9, 2025, b => b.Band1());

        var bytes = await new ReportExportService(db).AnnualComparisonAsync(employer.Id, Year, upload);

        using var wb = new XLWorkbook(new MemoryStream(bytes));
        Assert.NotNull(wb.Worksheet(SheetName));
        Assert.Single(wb.Worksheets);
    }

    [Fact]
    public async Task AnnualComparison_MonthColumns_BuiltFromAcademicYear()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "123456789");
        await ReportTestData.SeedEmploymentWithSlotAsync(db, employer.Id, employee.Id, "SYM-1");

        await using var upload = MonthlyComparisonUploadWorkbook.Create(
            "123456789", null, "Test User", 9, 2025, b => b.Band1());

        var bytes = await new ReportExportService(db).AnnualComparisonAsync(employer.Id, Year, upload);
        using var wb = new XLWorkbook(new MemoryStream(bytes));
        var ws = wb.Worksheet(SheetName);

        var sepYear = 2025;
        var seq = SchoolYearGregorian.GetSchoolYearMonthSequence(sepYear);
        var col = 11;
        foreach (var (m, y) in seq)
        {
            Assert.Equal($"{m}.{y}", ws.Cell(1, col).GetString());
            col++;
        }

        Assert.Equal(22, col - 1);
    }

    [Fact]
    public async Task AnnualComparison_AllMatch_ShowsVForMonth()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "111222333");
        var ed = await ReportTestData.SeedEmploymentWithSlotAsync(
            db, employer.Id, employee.Id, "SYM-1", weeklyHours: 30m, grade1Role: "גננת");
        ed.Grade1JobPercent = 100m;
        ed.Grade1AgeHours = 2m;
        await db.SaveChangesAsync();

        await using var upload = MonthlyComparisonUploadWorkbook.Create(
            "111222333", null, "Test User", 9, 2025,
            b => b.Band1(misra1Hours: 30m, misra1Base: 30m, jobPercent: 100m, ageHours: 2m));

        var bytes = await new ReportExportService(db).AnnualComparisonAsync(employer.Id, Year, upload);
        using var wb = new XLWorkbook(new MemoryStream(bytes));
        Assert.Equal("V", wb.Worksheet(SheetName).Cell(2, FirstMonthColumn).GetString());
    }

    [Fact]
    public async Task AnnualComparison_PayrollSugMisraDiffersFromSystemRole_ShowsVAndStaticSugMisra()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "222333444");
        var ed = await ReportTestData.SeedEmploymentWithSlotAsync(
            db, employer.Id, employee.Id, "SYM-1", weeklyHours: 30m, grade1Role: "גננת");
        ed.Grade1JobPercent = 100m;
        await db.SaveChangesAsync();

        await using var upload = MonthlyComparisonUploadWorkbook.Create(
            "222333444", null, "Test User", 9, 2025,
            b => b.Band1(misra1Hours: 30m));

        using var wbIn = new XLWorkbook(upload);
        var wsIn = wbIn.Worksheet(1);
        wsIn.Cell(4, 6).Value = "סייעת";
        var ms = new MemoryStream();
        wbIn.SaveAs(ms);
        ms.Position = 0;

        var bytes = await new ReportExportService(db).AnnualComparisonAsync(employer.Id, Year, ms);
        using var wb = new XLWorkbook(new MemoryStream(bytes));
        var ws = wb.Worksheet(SheetName);
        Assert.Equal("גננת", ws.Cell(2, 3).GetString());
        Assert.Equal("סייעת", ws.Cell(2, 4).GetString());
        var cell = ws.Cell(2, FirstMonthColumn).GetString();
        Assert.Contains("סוג משרה:", cell);
        Assert.Contains("נדרש=משרה חודשית", cell);
        Assert.DoesNotContain("תפקיד:", cell);
    }

    [Fact]
    public async Task AnnualComparison_GradeMismatch_ShowsDetail()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "333444555");
        await ReportTestData.SeedEmploymentWithSlotAsync(
            db, employer.Id, employee.Id, "SYM-1", weeklyHours: 30m);

        await using var upload = MonthlyComparisonUploadWorkbook.Create(
            "333444555", null, "Test User", 9, 2025,
            b => b.Band1(misra1Hours: 30m, grade: "3"));

        var bytes = await new ReportExportService(db).AnnualComparisonAsync(employer.Id, Year, upload);
        using var wb = new XLWorkbook(new MemoryStream(bytes));
        var cell = wb.Worksheet(SheetName).Cell(2, FirstMonthColumn).GetString();
        Assert.Contains("דרגה:", cell);
    }

    [Fact]
    public async Task AnnualComparison_DoubleGeneralMismatch_ShowsDetail()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "444555666");
        await ReportTestData.SeedEmploymentWithSlotAsync(db, employer.Id, employee.Id, "SYM-1");

        await using var upload = MonthlyComparisonUploadWorkbook.Create(
            "444555666", null, "Test User", 9, 2025,
            b => b.Band1(doubleGeneral: 10m));

        var bytes = await new ReportExportService(db).AnnualComparisonAsync(employer.Id, Year, upload);
        using var wb = new XLWorkbook(new MemoryStream(bytes));
        var cell = wb.Worksheet(SheetName).Cell(2, FirstMonthColumn).GetString();
        Assert.Contains("הכפלה כללית:", cell);
    }

    [Fact]
    public async Task AnnualComparison_HoursSum_ComparedAcrossAllSlots()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "555666777");
        var ed = await ReportTestData.SeedEmploymentWithSlotAsync(
            db, employer.Id, employee.Id, "SYM-A", weeklyHours: 20m);
        ed.Slots.Add(new Models.EmploymentDataSlot
        {
            GradeBand = 1,
            SlotIndex = 2,
            InstitutionSymbol = "SYM-B",
            WeeklyHours = 10m,
            JobBase = 30m,
        });
        await db.SaveChangesAsync();

        await using var upload = MonthlyComparisonUploadWorkbook.Create(
            "555666777", null, "Test User", 9, 2025,
            b => b.Band1(misra1Hours: 20m, misra1Base: 30m, misra2Hours: 10m, misra2Base: 30m, jobPercent: 0m));

        var bytes = await new ReportExportService(db).AnnualComparisonAsync(employer.Id, Year, upload);
        using var wb = new XLWorkbook(new MemoryStream(bytes));
        Assert.Equal("V", wb.Worksheet(SheetName).Cell(2, FirstMonthColumn).GetString());
        Assert.Equal("V", wb.Worksheet(SheetName).Cell(3, FirstMonthColumn).GetString());
    }

    [Fact]
    public async Task AnnualComparison_HoursSumMismatch_ShowsDetail()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "666777888");
        await ReportTestData.SeedEmploymentWithSlotAsync(
            db, employer.Id, employee.Id, "SYM-1", weeklyHours: 30m);

        await using var upload = MonthlyComparisonUploadWorkbook.Create(
            "666777888", null, "Test User", 9, 2025,
            b => b.Band1(misra1Hours: 25m));

        var bytes = await new ReportExportService(db).AnnualComparisonAsync(employer.Id, Year, upload);
        using var wb = new XLWorkbook(new MemoryStream(bytes));
        var cell = wb.Worksheet(SheetName).Cell(2, FirstMonthColumn).GetString();
        Assert.Contains("ש\"ש:", cell);
        Assert.Contains("30", cell);
        Assert.Contains("25", cell);
    }

    [Fact]
    public async Task AnnualComparison_SeniorityMismatch_ShowsSeniorityDetail()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "444333222");
        var ed = await ReportTestData.SeedEmploymentWithSlotAsync(db, employer.Id, employee.Id, "SYM-1");
        ed.Grade1Seniority = "10";
        await db.SaveChangesAsync();

        await using var upload = MonthlyComparisonUploadWorkbook.Create(
            "444333222", null, "Test User", 9, 2025,
            b => b.Band1(seniority: "2"));

        var bytes = await new ReportExportService(db).AnnualComparisonAsync(employer.Id, Year, upload);
        using var wb = new XLWorkbook(new MemoryStream(bytes));
        var cell = wb.Worksheet(SheetName).Cell(2, FirstMonthColumn).GetString();
        Assert.Contains("ותק:", cell, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AnnualComparison_NoInputRow_ShowsNotFound()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "777888999");
        await ReportTestData.SeedEmploymentWithSlotAsync(db, employer.Id, employee.Id, "SYM-1");

        await using var upload = MonthlyComparisonUploadWorkbook.Create(
            "000000000", null, "Other", 9, 2025, b => b.Band1());

        var bytes = await new ReportExportService(db).AnnualComparisonAsync(employer.Id, Year, upload);
        using var wb = new XLWorkbook(new MemoryStream(bytes));
        Assert.Equal("לא נמצא בקלט", wb.Worksheet(SheetName).Cell(2, FirstMonthColumn).GetString());
    }

    [Fact]
    public async Task AnnualComparison_GradeBand2_UsesSecondColumnGroup()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "888999000");
        var ed = await ReportTestData.SeedEmploymentWithSlotAsync(
            db, employer.Id, employee.Id, "SYM-2", weeklyHours: 0m);
        ed.Slots.Clear();
        ed.Slots.Add(new Models.EmploymentDataSlot
        {
            GradeBand = 2,
            SlotIndex = 1,
            InstitutionSymbol = "SYM-2",
            WeeklyHours = 15m,
            JobBase = 9m,
        });
        ed.Grade2Role = "סייעת";
        ed.Grade2Grade = "ב";
        ed.Grade2Seniority = "3";
        await db.SaveChangesAsync();

        await using var upload = MonthlyComparisonUploadWorkbook.Create(
            "888999000", null, "Test User", 9, 2025,
            b => b.Band1().Band2(misra1Hours: 15m, misra1Base: 9m, jobPercent: 0m));

        using var wbIn = new XLWorkbook(upload);
        var ms = new MemoryStream();
        wbIn.SaveAs(ms);
        ms.Position = 0;

        var bytes = await new ReportExportService(db).AnnualComparisonAsync(employer.Id, Year, ms);
        using var wb = new XLWorkbook(new MemoryStream(bytes));
        Assert.Equal("V", wb.Worksheet(SheetName).Cell(2, FirstMonthColumn).GetString());
    }

    [Fact]
    public async Task AnnualComparison_NonMonthlySugMisra_ShowsDetail()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "707080809");
        await ReportTestData.SeedEmploymentWithSlotAsync(
            db, employer.Id, employee.Id, "SYM-1", weeklyHours: 30m);

        await using var upload = MonthlyComparisonUploadWorkbook.Create(
            "707080809", null, "Test User", 9, 2025,
            b => b.Band1(misra1Hours: 30m));

        using var wbIn = new XLWorkbook(upload);
        wbIn.Worksheet(1).Cell(4, 6).Value = "משרה שעתית";
        var ms = new MemoryStream();
        wbIn.SaveAs(ms);
        ms.Position = 0;

        var bytes = await new ReportExportService(db).AnnualComparisonAsync(employer.Id, Year, ms);
        using var wb = new XLWorkbook(new MemoryStream(bytes));
        var cell = wb.Worksheet(SheetName).Cell(2, FirstMonthColumn).GetString();
        Assert.Contains("סוג משרה:", cell);
        Assert.Contains("נדרש=משרה חודשית", cell);
    }
}
