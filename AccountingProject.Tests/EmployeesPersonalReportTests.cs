using AccountingProject.Models;
using AccountingProject.Services;
using AccountingProject.Tests.TestHelpers;
using ClosedXML.Excel;

namespace AccountingProject.Tests;

public sealed class EmployeesPersonalReportTests
{
    [Fact]
    public async Task EmployeesPersonal_IncludesActiveStatusAndEmployerName()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db, "מעסיק לדוח");
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "987654321", "דנה", "לוי");
        await ReportTestData.SeedEmploymentWithSlotAsync(db, employer.Id, employee.Id, "G-1");

        var bytes = await new ReportExportService(db).EmployeesPersonalAsync(employer.Id);

        using var wb = new XLWorkbook(new MemoryStream(bytes));
        var ws = wb.Worksheet("עובדים אישיים");
        Assert.Equal("לוי", ws.Cell(2, 1).GetString());
        Assert.Equal("דנה", ws.Cell(2, 2).GetString());
        Assert.Equal("987654321", ws.Cell(2, 3).GetString());
        Assert.Equal("מעסיק לדוח", ws.Cell(2, 7).GetString());
        Assert.Equal("פעיל", ws.Cell(2, 8).GetString());
    }

    [Fact]
    public async Task EmployeesPersonal_ManualInactive_ShowsInactiveStatus()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id);
        employee.ManualActiveStatus = false;
        await db.SaveChangesAsync();

        var bytes = await new ReportExportService(db).EmployeesPersonalAsync(employer.Id);

        using var wb = new XLWorkbook(new MemoryStream(bytes));
        Assert.Equal("לא פעיל", wb.Worksheet("עובדים אישיים").Cell(2, 8).GetString());
    }

    [Fact]
    public async Task EmployeesPersonal_NoEmployment_ShowsInactiveWhenNotManual()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        await ReportTestData.SeedEmployeeAsync(db, employer.Id);

        var bytes = await new ReportExportService(db).EmployeesPersonalAsync(employer.Id);

        using var wb = new XLWorkbook(new MemoryStream(bytes));
        Assert.Equal("לא פעיל", wb.Worksheet("עובדים אישיים").Cell(2, 8).GetString());
    }
}
