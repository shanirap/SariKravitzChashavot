using AccountingProject.Models;
using AccountingProject.Services;
using AccountingProject.Tests.TestHelpers;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace AccountingProject.Tests;

public sealed class BulkImportEmployeeFieldsTests
{
    private const string Year = "תשפ\"ו";

    [Fact]
    public void EmployeesTemplate_IncludesUketzEmployeeNumberColumn()
    {
        using var db = DbTestFactory.CreateContext();
        using var wb = new BulkImportService(db, Microsoft.Extensions.Logging.Abstractions.NullLogger<BulkImportService>.Instance)
            .BuildEmployeesTemplate(includeEmployerName: false);
        var ws = wb.Worksheet("עובדים");
        var headers = ws.Row(1).CellsUsed().Select(c => c.GetString()).ToList();
        Assert.Contains("מספר_עובד_בעוקץ", headers);
        Assert.Contains("טל", headers);
        Assert.Contains("דרגה1_גמולי_השתלמות", headers);
        Assert.Contains("דרגה1_1_סמל_מוסד", headers);
    }

    [Fact]
    public async Task ImportEmployees_SetsEmployeeNumber_OnNewEmployee()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db, "Import Emp Employer");
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "G-1");

        await using var stream = BulkImportEmployeeWorkbook.CreateMinimalRow(
            employer.Name, "111222333", Year, fillRow: (ws, row) =>
            {
                var col = BulkImportEmployeeWorkbook.ColumnIndex(true, "מספר_עובד_בעוקץ");
                ws.Cell(row, col).Value = 4242;
                var phoneCol = BulkImportEmployeeWorkbook.ColumnIndex(true, "טל");
                ws.Cell(row, phoneCol).Value = "050-9998877";
            });

        var file = FormFileFromStream(stream, "import.xlsx");
        var sut = new BulkImportService(db, Microsoft.Extensions.Logging.Abstractions.NullLogger<BulkImportService>.Instance);
        var result = await sut.ImportEmployeesAsync(file);

        Assert.Equal(1, result.Imported);
        var emp = await db.Employees.SingleAsync(e => e.IdNumber == "111222333");
        Assert.Equal(4242, emp.EmployeeNumber);
        Assert.Equal("050-9998877", emp.Phone);
    }

    [Fact]
    public async Task ImportEmployees_UpdatesEmployeeNumber_WhenEmployeeAlreadyExists()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db, "Update Emp Employer");
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "G-1");
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "444555666");
        employee.EmployeeNumber = null;
        employee.Phone = "050-1111111";
        await db.SaveChangesAsync();

        await using var stream = BulkImportEmployeeWorkbook.CreateMinimalRow(
            employer.Name, "444555666", "תשפ\"ה", fillRow: (ws, row) =>
            {
                ws.Cell(row, BulkImportEmployeeWorkbook.ColumnIndex(true, "מספר_עובד_בעוקץ")).Value = 7777;
                ws.Cell(row, BulkImportEmployeeWorkbook.ColumnIndex(true, "טל")).Value = "050-2222222";
            });

        var file = FormFileFromStream(stream, "import.xlsx");
        var sut = new BulkImportService(db, Microsoft.Extensions.Logging.Abstractions.NullLogger<BulkImportService>.Instance);
        var result = await sut.ImportEmployeesAsync(file);

        Assert.Equal(1, result.Imported);
        await db.Entry(employee).ReloadAsync();
        Assert.Equal(7777, employee.EmployeeNumber);
        Assert.Equal("050-2222222", employee.Phone);
    }

    [Fact]
    public async Task ImportEmployees_InvalidEmployeeNumber_ReturnsRowError()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db, "Invalid Num Employer");
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "G-1");

        await using var stream = BulkImportEmployeeWorkbook.CreateMinimalRow(
            employer.Name, "888999000", fillRow: (ws, row) =>
            {
                ws.Cell(row, BulkImportEmployeeWorkbook.ColumnIndex(true, "מספר_עובד_בעוקץ")).Value = "לא-מספר";
            });

        var file = FormFileFromStream(stream, "import.xlsx");
        var sut = new BulkImportService(db, Microsoft.Extensions.Logging.Abstractions.NullLogger<BulkImportService>.Instance);
        var result = await sut.ImportEmployeesAsync(file);

        Assert.Equal(0, result.Imported);
        Assert.Equal(1, result.Errors);
        Assert.Contains("מספר_עובד_בעוקץ", result.Rows[0].Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ImportEmployees_AcceptsLegacyHeader_מספר_עובד()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db, "Legacy Header Employer");
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "G-1");

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("עובדים");
        ws.Cell(1, 1).Value = "שם_מעסיק";
        ws.Cell(1, 2).Value = "תז";
        ws.Cell(1, 3).Value = "מספר_עובד";
        ws.Cell(1, 4).Value = "שם_פרטי";
        ws.Cell(1, 5).Value = "שם_משפחה";
        ws.Cell(1, 6).Value = "מין";
        ws.Cell(1, 7).Value = "תאריך_לידה";
        ws.Cell(1, 8).Value = "שנת_לימודים";
        ws.Cell(2, 1).Value = employer.Name;
        ws.Cell(2, 2).Value = "121212121";
        ws.Cell(2, 3).Value = 3333;
        ws.Cell(2, 4).Value = "דן";
        ws.Cell(2, 5).Value = "לוי";
        ws.Cell(2, 6).Value = "זכר";
        ws.Cell(2, 7).Value = "1985-06-01";
        ws.Cell(2, 8).Value = Year;
        var ms = new MemoryStream();
        wb.SaveAs(ms);
        ms.Position = 0;

        var file = FormFileFromStream(ms, "legacy.xlsx");
        var sut = new BulkImportService(db, Microsoft.Extensions.Logging.Abstractions.NullLogger<BulkImportService>.Instance);
        var result = await sut.ImportEmployeesAsync(file);

        Assert.Equal(1, result.Imported);
        var emp = await db.Employees.SingleAsync(e => e.IdNumber == "121212121");
        Assert.Equal(3333, emp.EmployeeNumber);
    }

    private static IFormFile FormFileFromStream(Stream stream, string fileName)
    {
        stream.Position = 0;
        return new FormFile(stream, 0, stream.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        };
    }
}
