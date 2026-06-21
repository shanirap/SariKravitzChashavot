using AccountingProject.Models;
using AccountingProject.Services;
using AccountingProject.Tests.TestHelpers;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace AccountingProject.Tests;

public sealed class BulkImportPartialRowSaveTests
{
    private const string Year = "תשפ\"ו";

    [Fact]
    public async Task ImportEmployees_InvalidInstitutionOnNewEmployee_DoesNotCreateEmployee()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db, "Partial Import Employer");
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "VALID-1");

        await using var stream = CreateRowWithInstitutionSymbol(employer.Name, "999888777", "BAD-SYMBOL");
        var sut = ServiceTestFactory.CreateBulkImportService(db);

        var result = await sut.ImportEmployeesAsync(FormFileFromStream(stream, "import.xlsx"));

        Assert.Equal(0, result.Imported);
        Assert.Equal(1, result.Errors);
        Assert.False(result.Rows[0].Success);
        Assert.False(await db.Employees.AnyAsync(e => e.IdNumber == "999888777"));
        Assert.False(await db.EmploymentData.AnyAsync());
    }

    [Fact]
    public async Task ImportEmployees_InvalidEmploymentOnExistingEmployee_DoesNotChangeEmployee()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db, "Existing Emp Employer");
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "VALID-1");
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "555666777", "Original", "Name");

        await using var stream = CreateRowWithInstitutionSymbol(
            employer.Name,
            "555666777",
            "BAD-SYMBOL",
            fillRow: (ws, row) =>
            {
                ws.Cell(row, BulkImportEmployeeWorkbook.ColumnIndex(true, "שם_פרטי")).Value = "Changed";
                ws.Cell(row, BulkImportEmployeeWorkbook.ColumnIndex(true, "שם_משפחה")).Value = "Person";
            });

        var sut = ServiceTestFactory.CreateBulkImportService(db);
        var result = await sut.ImportEmployeesAsync(FormFileFromStream(stream, "import.xlsx"));

        Assert.Equal(0, result.Imported);
        Assert.Equal(1, result.Errors);
        await db.Entry(employee).ReloadAsync();
        Assert.Equal("Original", employee.FirstName);
        Assert.Equal("Name", employee.LastName);
        Assert.False(await db.EmploymentData.AnyAsync());
    }

    [Fact]
    public async Task ImportEmployees_ValidRow_StillImportsSuccessfully()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db, "Valid Row Employer");
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "VALID-1");

        await using var stream = CreateRowWithInstitutionSymbol(employer.Name, "111222333", "VALID-1");
        var sut = ServiceTestFactory.CreateBulkImportService(db);

        var result = await sut.ImportEmployeesAsync(FormFileFromStream(stream, "import.xlsx"));

        Assert.Equal(1, result.Imported);
        Assert.Equal(0, result.Errors);
        Assert.True(await db.Employees.AnyAsync(e => e.IdNumber == "111222333"));
        Assert.True(await db.EmploymentData.AnyAsync(ed => ed.AcademicYear == Year));
    }

    [Fact]
    public async Task ImportEmployees_PartialFile_ImportsValidRowsOnly()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db, "Mixed File Employer");
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "VALID-1");

        await using var stream = CreateTwoRowWorkbook(
            (employer.Name, "444555666", "VALID-1"),
            (employer.Name, "777888999", "BAD-SYMBOL"));

        var sut = ServiceTestFactory.CreateBulkImportService(db);
        var result = await sut.ImportEmployeesAsync(FormFileFromStream(stream, "mixed.xlsx"));

        Assert.Equal(1, result.Imported);
        Assert.Equal(1, result.Errors);
        Assert.True(await db.Employees.AnyAsync(e => e.IdNumber == "444555666"));
        Assert.False(await db.Employees.AnyAsync(e => e.IdNumber == "777888999"));
        Assert.Equal(1, await db.EmploymentData.CountAsync());
    }

    private static MemoryStream CreateRowWithInstitutionSymbol(
        string employerName,
        string idNumber,
        string institutionSymbol,
        Action<IXLWorksheet, int>? fillRow = null)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("עובדים");
        var headers = new List<string>
        {
            "שם_מעסיק", "חפ", "תז", "מספר_עובד_בעוקץ", "שם_משפחה", "שם_פרטי", "מין", "תאריך_לידה", "טל",
            "תאריך_לידה_ילד_1", "תאריך_לידה_ילד_2", "תאריך_לידה_ילד_3", "תאריך_לידה_ילד_4",
            "תאריך_לידה_ילד_5", "תאריך_לידה_ילד_6", "תאריך_לידה_ילד_7", "תאריך_לידה_ילד_8",
            "תאריך_לידה_ילד_9", "תאריך_לידה_ילד_10", "שנת_לימודים",
            "דרגה1_1_סמל_מוסד", "דרגה1_1_שעות_שבועיות", "דרגה1_1_בסיס_משרה",
        };
        for (var i = 0; i < headers.Count; i++)
            ws.Cell(1, i + 1).Value = headers[i];

        const int row = 2;
        var col = 1;
        ws.Cell(row, col++).Value = employerName;
        col++; // חפ
        ws.Cell(row, col++).Value = idNumber;
        col++; // מספר עובד
        ws.Cell(row, col++).Value = "כהן";
        ws.Cell(row, col++).Value = "רחל";
        ws.Cell(row, col++).Value = "נקבה";
        ws.Cell(row, col++).Value = "1990-01-15";
        col += 11; // טל + children
        ws.Cell(row, col++).Value = Year;
        ws.Cell(row, col++).Value = institutionSymbol;
        ws.Cell(row, col++).Value = 30;
        ws.Cell(row, col).Value = 1;

        fillRow?.Invoke(ws, row);

        var ms = new MemoryStream();
        wb.SaveAs(ms);
        ms.Position = 0;
        return ms;
    }

    private static MemoryStream CreateTwoRowWorkbook(
        (string Employer, string IdNumber, string Symbol) row1,
        (string Employer, string IdNumber, string Symbol) row2)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("עובדים");
        var headers = new List<string>
        {
            "שם_מעסיק", "חפ", "תז", "מספר_עובד_בעוקץ", "שם_משפחה", "שם_פרטי", "מין", "תאריך_לידה", "טל",
            "תאריך_לידה_ילד_1", "תאריך_לידה_ילד_2", "תאריך_לידה_ילד_3", "תאריך_לידה_ילד_4",
            "תאריך_לידה_ילד_5", "תאריך_לידה_ילד_6", "תאריך_לידה_ילד_7", "תאריך_לידה_ילד_8",
            "תאריך_לידה_ילד_9", "תאריך_לידה_ילד_10", "שנת_לימודים",
            "דרגה1_1_סמל_מוסד", "דרגה1_1_שעות_שבועיות", "דרגה1_1_בסיס_משרה",
        };
        for (var i = 0; i < headers.Count; i++)
            ws.Cell(1, i + 1).Value = headers[i];

        WriteImportRow(ws, 2, row1);
        WriteImportRow(ws, 3, row2);

        var ms = new MemoryStream();
        wb.SaveAs(ms);
        ms.Position = 0;
        return ms;
    }

    private static void WriteImportRow(
        IXLWorksheet ws,
        int row,
        (string Employer, string IdNumber, string Symbol) data)
    {
        var col = 1;
        ws.Cell(row, col++).Value = data.Employer;
        col++;
        ws.Cell(row, col++).Value = data.IdNumber;
        col++;
        ws.Cell(row, col++).Value = "כהן";
        ws.Cell(row, col++).Value = "רחל";
        ws.Cell(row, col++).Value = "נקבה";
        ws.Cell(row, col++).Value = "1990-01-15";
        col += 11;
        ws.Cell(row, col++).Value = Year;
        ws.Cell(row, col++).Value = data.Symbol;
        ws.Cell(row, col++).Value = 30;
        ws.Cell(row, col).Value = 1;
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
