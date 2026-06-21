using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AccountingProject.Contracts;
using AccountingProject.Models;
using AccountingProject.Services;
using AccountingProject.Tests.Integration;
using AccountingProject.Tests.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace AccountingProject.Tests;

public sealed class ProductionCriticalFixesTests
{
    private const string Year = "תשפ\"ו";

    [Fact]
    public async Task UpdateAsync_ChangingEmployerId_ThrowsHebrewValidationError()
    {
        await using var db = DbTestFactory.CreateContext();
        var employerA = await ReportTestData.SeedEmployerAsync(db, "Employer A");
        var employerB = await ReportTestData.SeedEmployerAsync(db, "Employer B");
        var employee = await ReportTestData.SeedEmployeeAsync(db, employerA.Id);
        await ReportTestData.SeedSymbolAsync(db, employerA.Id, "SYM1");
        await ReportTestData.SeedEmploymentWithSlotAsync(db, employerA.Id, employee.Id, "SYM1");

        var sut = new EmployeeService(db);
        var dto = new EmployeeDto
        {
            EmployerId = employerB.Id,
            IdNumber = employee.IdNumber,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Gender = employee.Gender!,
            BirthDate = "1990-01-01",
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => sut.UpdateAsync(employee.Id, dto));

        Assert.Contains("לא ניתן לשנות את המעסיק", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task UpdateAsync_ChangingEmployerId_WithoutEmployment_StillThrows()
    {
        await using var db = DbTestFactory.CreateContext();
        var employerA = await ReportTestData.SeedEmployerAsync(db, "Employer A");
        var employerB = await ReportTestData.SeedEmployerAsync(db, "Employer B");
        var employee = await ReportTestData.SeedEmployeeAsync(db, employerA.Id);

        var sut = new EmployeeService(db);
        var dto = new EmployeeDto
        {
            EmployerId = employerB.Id,
            IdNumber = employee.IdNumber,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Gender = employee.Gender!,
            BirthDate = "1990-01-01",
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => sut.UpdateAsync(employee.Id, dto));

        Assert.Contains("לא ניתן לשנות את המעסיק", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ImportEmployees_NonUniqueEmployerName_ReturnsRowError()
    {
        await using var db = DbTestFactory.CreateContext();
        db.Employers.AddRange(
            new Employer { Name = "Duplicate Name Ltd", BusinessNumber = "111111111" },
            new Employer { Name = "Duplicate Name Ltd", BusinessNumber = "222222222" });
        await db.SaveChangesAsync();
        await ReportTestData.SeedSymbolAsync(db, (await db.Employers.FirstAsync()).Id, "G-1");

        await using var stream = BulkImportEmployeeWorkbook.CreateMinimalRow("Duplicate Name Ltd", "123123123", Year);
        var file = FormFileFromStream(stream, "import.xlsx");
        var sut = ServiceTestFactory.CreateBulkImportService(db);

        var result = await sut.ImportEmployeesAsync(file);

        Assert.Equal(0, result.Imported);
        Assert.Equal(1, result.Errors);
        Assert.Contains("אינו ייחודי", result.Rows[0].Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ImportEmployees_ResolvesEmployerByBusinessNumber_WhenNameIsDuplicate()
    {
        await using var db = DbTestFactory.CreateContext();
        var target = new Employer { Name = "Shared Name", BusinessNumber = "514999888" };
        db.Employers.AddRange(
            target,
            new Employer { Name = "Shared Name", BusinessNumber = "514888777" });
        await db.SaveChangesAsync();
        await ReportTestData.SeedSymbolAsync(db, target.Id, "G-1");

        await using var stream = BulkImportEmployeeWorkbook.CreateMinimalRow(
            "Shared Name",
            "321321321",
            Year,
            fillRow: (ws, row) =>
            {
                var col = BulkImportEmployeeWorkbook.ColumnIndex(true, "חפ");
                if (col > 0)
                    ws.Cell(row, col).Value = "514999888";
            });

        var file = FormFileFromStream(stream, "import.xlsx");
        var sut = ServiceTestFactory.CreateBulkImportService(db);
        var result = await sut.ImportEmployeesAsync(file);

        Assert.Equal(1, result.Imported);
        var employee = await db.Employees.SingleAsync(e => e.IdNumber == "321321321");
        Assert.Equal(target.Id, employee.EmployerId);
    }

    [Fact]
    public async Task ImportEmployees_InvalidAcademicYear_ReturnsRowError()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db, "Year Validation Employer");
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "G-1");

        await using var stream = BulkImportEmployeeWorkbook.CreateMinimalRow(
            employer.Name, "555444333", "not-a-valid-year");

        var file = FormFileFromStream(stream, "import.xlsx");
        var sut = ServiceTestFactory.CreateBulkImportService(db);
        var result = await sut.ImportEmployeesAsync(file);

        Assert.Equal(0, result.Imported);
        Assert.Equal(1, result.Errors);
        Assert.Contains("שנת_לימודים", result.Rows[0].Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CreateAsync_NullSlots_ReturnsValidationMessage()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id);

        var sut = ServiceTestFactory.CreateEmploymentDataService(db);
        var dto = new EmploymentDataDto
        {
            EmployeeId = employee.Id,
            EmployerId = employer.Id,
            AcademicYear = Year,
            Slots = null!,
        };

        var (record, message) = await sut.CreateAsync(dto);

        Assert.Null(record);
        Assert.Contains("מקטעי העסקה", message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task EmploymentData_Create_WithNullSlots_Returns400()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);

        var er = await client.PostAsJsonAsync("/api/employers", new EmployerDto { Name = "Slots Null Employer" });
        er.EnsureSuccessStatusCode();
        var employer = await er.Content.ReadFromJsonAsync<IdJson>(JsonOptions());
        Assert.NotNull(employer);

        var empResp = await client.PostAsJsonAsync("/api/employees", new EmployeeDto
        {
            EmployerId = employer!.Id,
            IdNumber = "909808707",
            FirstName = "Test",
            LastName = "Worker",
            Gender = "זכר",
            BirthDate = "1990-01-01",
        });
        empResp.EnsureSuccessStatusCode();
        var employee = await empResp.Content.ReadFromJsonAsync<IdJson>(JsonOptions());
        Assert.NotNull(employee);

        var payload = new
        {
            employeeId = employee!.Id,
            employerId = employer.Id,
            academicYear = Year,
            slots = (object?)null,
        };
        var resp = await client.PostAsJsonAsync("/api/employment-data", payload);

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        var body = await resp.Content.ReadAsStringAsync();
        Assert.Contains("מקטעי העסקה", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Employees_Create_AsViewer_Returns403()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateViewerClientAsync(factory);

        var resp = await client.PostAsJsonAsync("/api/employees", new EmployeeDto
        {
            EmployerId = 1,
            IdNumber = "111000999",
            FirstName = "Viewer",
            LastName = "Attempt",
            Gender = "זכר",
            BirthDate = "1990-01-01",
        });

        Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
    }

    private static JsonSerializerOptions JsonOptions() => new() { PropertyNameCaseInsensitive = true };

    private static IFormFile FormFileFromStream(Stream stream, string fileName)
    {
        stream.Position = 0;
        return new FormFile(stream, 0, stream.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        };
    }

    private sealed class IdJson
    {
        public int Id { get; set; }
    }
}
