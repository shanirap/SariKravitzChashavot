using AccountingProject.Models;
using AccountingProject.Services;
using AccountingProject.Tests.TestHelpers;
using ClosedXML.Excel;

namespace AccountingProject.Tests;

public sealed class InstitutionHoursReportTests
{
    private const string SheetName = "בדיקת שעות לסמל";
    private const decimal RequiredTeacher = 34.5m;
    private const decimal RequiredAssistant = 34.5m;
    private const decimal RequiredSecondAssistant = 40m;

    private static readonly string[] ExpectedHeaders =
    [
        "סמל מוסד",
        "מס' שעות גננת סה\"כ",
        "מס' שעות סייעת סה\"כ",
        "סייעת שניה",
    ];

    [Fact]
    public async Task InstitutionHours_HasExactHeadersAndStandardRow()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "SYM-1");
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id);
        await ReportTestData.SeedEmploymentWithSlotAsync(db, employer.Id, employee.Id, "SYM-1");

        var bytes = await new ReportExportService(db).InstitutionHoursAsync(
            employer.Id, ReportTestData.DefaultAcademicYear, "SYM-1");

        using var wb = new XLWorkbook(new MemoryStream(bytes));
        var ws = wb.Worksheet(SheetName);
        Assert.Single(wb.Worksheets);

        for (var c = 0; c < ExpectedHeaders.Length; c++)
            Assert.Equal(ExpectedHeaders[c], ws.Cell(1, c + 1).GetString());

        Assert.Equal("SYM-1", ws.Cell(2, 1).GetString());
        Assert.Equal(RequiredTeacher, ws.Cell(2, 2).GetValue<decimal>());
        Assert.Equal(RequiredAssistant, ws.Cell(2, 3).GetValue<decimal>());
        Assert.Equal(RequiredSecondAssistant, ws.Cell(2, 4).GetValue<decimal>());
    }

    [Fact]
    public async Task InstitutionHours_RosterRow_SumsRolesCorrectly()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "SYM-1");

        var gannetEmp = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "111111111");
        await ReportTestData.SeedEmploymentWithSlotAsync(
            db, employer.Id, gannetEmp.Id, "SYM-1", weeklyHours: 30m, grade1Role: "גננת");

        var assistantEmp = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "222222222");
        var assistantEd = await ReportTestData.SeedEmploymentWithSlotAsync(
            db, employer.Id, assistantEmp.Id, "SYM-1", weeklyHours: 34.5m, grade1Role: "גננת");
        assistantEd.Slots.First().GradeBand = 2;
        assistantEd.Grade1Role = null;
        assistantEd.Grade2Role = "סייעת";
        await db.SaveChangesAsync();

        var secondEmp = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "333333333");
        var secondEd = await ReportTestData.SeedEmploymentWithSlotAsync(
            db, employer.Id, secondEmp.Id, "SYM-1", weeklyHours: 40m, grade1Role: "גננת");
        secondEd.Slots.First().GradeBand = 2;
        secondEd.Grade1Role = null;
        secondEd.Grade2Role = "סייעת שניה";
        await db.SaveChangesAsync();

        var bytes = await new ReportExportService(db).InstitutionHoursAsync(
            employer.Id, ReportTestData.DefaultAcademicYear, "SYM-1");

        using var wb = new XLWorkbook(new MemoryStream(bytes));
        var ws = wb.Worksheet(SheetName);

        Assert.Equal("מצבת", ws.Cell(3, 1).GetString());
        Assert.Equal(30m, ws.Cell(3, 2).GetValue<decimal>());
        Assert.Equal(34.5m, ws.Cell(3, 3).GetValue<decimal>());
        Assert.Equal(40m, ws.Cell(3, 4).GetValue<decimal>());
    }

    [Fact]
    public async Task InstitutionHours_DifferenceRow_IsStandardMinusRoster()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "SYM-1");
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id);
        await ReportTestData.SeedEmploymentWithSlotAsync(
            db, employer.Id, employee.Id, "SYM-1", weeklyHours: 30m);

        var bytes = await new ReportExportService(db).InstitutionHoursAsync(
            employer.Id, ReportTestData.DefaultAcademicYear, "SYM-1");

        using var wb = new XLWorkbook(new MemoryStream(bytes));
        var ws = wb.Worksheet(SheetName);

        Assert.Equal("הפרש", ws.Cell(4, 1).GetString());
        Assert.Equal(4.5m, ws.Cell(4, 2).GetValue<decimal>());
        Assert.Equal(34.5m, ws.Cell(4, 3).GetValue<decimal>());
        Assert.Equal(40m, ws.Cell(4, 4).GetValue<decimal>());
    }

    [Fact]
    public async Task InstitutionHours_Difference_ZeroWhenRosterMatchesStandard()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "SYM-1");
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id);
        await ReportTestData.SeedEmploymentWithSlotAsync(
            db, employer.Id, employee.Id, "SYM-1", weeklyHours: 34.5m, grade1Role: "גננת");

        var assistantEmp = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "222222222");
        var assistantEd = await ReportTestData.SeedEmploymentWithSlotAsync(
            db, employer.Id, assistantEmp.Id, "SYM-1", weeklyHours: 34.5m, grade1Role: "גננת");
        assistantEd.Slots.First().GradeBand = 2;
        assistantEd.Grade1Role = null;
        assistantEd.Grade2Role = "סייעת";
        await db.SaveChangesAsync();

        var secondEmp = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "333333333");
        var secondEd = await ReportTestData.SeedEmploymentWithSlotAsync(
            db, employer.Id, secondEmp.Id, "SYM-1", weeklyHours: 40m, grade1Role: "גננת");
        secondEd.Slots.First().GradeBand = 2;
        secondEd.Grade1Role = null;
        secondEd.Grade2Role = "סייעת שניה";
        await db.SaveChangesAsync();

        var bytes = await new ReportExportService(db).InstitutionHoursAsync(
            employer.Id, ReportTestData.DefaultAcademicYear, "SYM-1");

        using var wb = new XLWorkbook(new MemoryStream(bytes));
        var ws = wb.Worksheet(SheetName);
        Assert.Equal(0m, ws.Cell(4, 2).GetValue<decimal>());
        Assert.Equal(0m, ws.Cell(4, 3).GetValue<decimal>());
        Assert.Equal(0m, ws.Cell(4, 4).GetValue<decimal>());
    }

    [Fact]
    public async Task InstitutionHours_Difference_PositiveWhenBelowStandard_NegativeWhenAbove()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "SYM-1");
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id);
        await ReportTestData.SeedEmploymentWithSlotAsync(
            db, employer.Id, employee.Id, "SYM-1", weeklyHours: 30m);

        var underBytes = await new ReportExportService(db).InstitutionHoursAsync(
            employer.Id, ReportTestData.DefaultAcademicYear, "SYM-1");
        using (var wb = new XLWorkbook(new MemoryStream(underBytes)))
        {
            var ws = wb.Worksheet(SheetName);
            Assert.True(ws.Cell(4, 2).GetValue<decimal>() > 0);
        }

        var ed = db.EmploymentData.First();
        ed.Slots.First().WeeklyHours = 40m;
        await db.SaveChangesAsync();

        var overBytes = await new ReportExportService(db).InstitutionHoursAsync(
            employer.Id, ReportTestData.DefaultAcademicYear, "SYM-1");
        using (var wb = new XLWorkbook(new MemoryStream(overBytes)))
        {
            var ws = wb.Worksheet(SheetName);
            Assert.True(ws.Cell(4, 2).GetValue<decimal>() < 0);
        }
    }

    [Fact]
    public async Task InstitutionHours_SecondAssistant_NotCountedAsRegularAssistant()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "SYM-1");
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id);
        var ed = await ReportTestData.SeedEmploymentWithSlotAsync(
            db, employer.Id, employee.Id, "SYM-1", weeklyHours: 15m, grade1Role: "גננת");
        ed.Slots.Clear();
        ed.Slots.Add(new EmploymentDataSlot
        {
            GradeBand = 2,
            SlotIndex = 1,
            InstitutionSymbol = "SYM-1",
            WeeklyHours = 15m,
            JobBase = 28m,
        });
        ed.Grade2Role = "סייעת שניה";
        await db.SaveChangesAsync();

        var bytes = await new ReportExportService(db).InstitutionHoursAsync(
            employer.Id, ReportTestData.DefaultAcademicYear, "SYM-1");

        using var wb = new XLWorkbook(new MemoryStream(bytes));
        var ws = wb.Worksheet(SheetName);
        Assert.Equal(0m, ws.Cell(3, 3).GetValue<decimal>());
        Assert.Equal(15m, ws.Cell(3, 4).GetValue<decimal>());
        Assert.Equal(25m, ws.Cell(4, 4).GetValue<decimal>());
    }

    [Fact]
    public async Task InstitutionHours_IgnoresOtherInstitutionSymbols()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "TARGET");
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id);
        await ReportTestData.SeedEmploymentWithSlotAsync(db, employer.Id, employee.Id, "OTHER-SYM", weeklyHours: 99m);

        var bytes = await new ReportExportService(db).InstitutionHoursAsync(
            employer.Id, ReportTestData.DefaultAcademicYear, "TARGET");

        using var wb = new XLWorkbook(new MemoryStream(bytes));
        var ws = wb.Worksheet(SheetName);
        Assert.Equal(0m, ws.Cell(3, 2).GetValue<decimal>());
        Assert.Equal(0m, ws.Cell(3, 3).GetValue<decimal>());
        Assert.Equal(0m, ws.Cell(3, 4).GetValue<decimal>());
        Assert.Equal(RequiredTeacher, ws.Cell(4, 2).GetValue<decimal>());
    }
}
