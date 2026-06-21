using AccountingProject.Contracts;
using AccountingProject.Domain;
using AccountingProject.Models;
using AccountingProject.Services;
using AccountingProject.Tests.TestHelpers;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace AccountingProject.Tests;

public sealed class AcademicYearNormalizationTests
{
    private const string CanonicalYear = "תשפ\"ו";

    [Fact]
    public async Task KindergartenAnnual_NonCanonicalInput_FindsCanonicallyStoredEmployment()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "KG-1");
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id);
        await SeedEmploymentAsync(db, employer.Id, employee.Id, CanonicalYear, "KG-1");

        var bytes = await new ReportExportService(db).KindergartenAnnualAsync(employer.Id, "5786");

        using var wb = new XLWorkbook(new MemoryStream(bytes));
        var ws = wb.Worksheet("מצבת גנים");
        Assert.True(ws.LastRowUsed()?.RowNumber() > 1);
    }

    [Fact]
    public async Task ImportEmployees_StoresCanonicalAcademicYear()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db, "Year Canon Employer");
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "VALID-1");

        await using var stream = BulkImportEmployeeWorkbook.CreateMinimalRow(
            employer.Name,
            "321654987",
            academicYear: "5786");

        var sut = ServiceTestFactory.CreateBulkImportService(db);
        var result = await sut.ImportEmployeesAsync(FormFileFromStream(stream, "year.xlsx"));

        Assert.Equal(1, result.Imported);
        var ed = await db.EmploymentData.SingleAsync();
        Assert.Equal(CanonicalYear, ed.AcademicYear);
    }

    [Fact]
    public async Task ImportEmployees_InvalidAcademicYear_ReturnsErrorAndCreatesNothing()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db, "Bad Year Employer");

        await using var stream = BulkImportEmployeeWorkbook.CreateMinimalRow(
            employer.Name,
            "147258369",
            academicYear: "not-a-year");

        var sut = ServiceTestFactory.CreateBulkImportService(db);
        var result = await sut.ImportEmployeesAsync(FormFileFromStream(stream, "bad-year.xlsx"));

        Assert.Equal(0, result.Imported);
        Assert.Equal(1, result.Errors);
        Assert.Contains("שנת_לימודים", result.Rows[0].Message, StringComparison.Ordinal);
        Assert.False(await db.Employees.AnyAsync());
        Assert.False(await db.EmploymentData.AnyAsync());
    }

    [Fact]
    public void CanonicalForComparison_MatchesAlternateValidLabels()
    {
        var canonical = HebrewAcademicYear.CanonicalForComparison(CanonicalYear);
        Assert.Equal(CanonicalYear, HebrewAcademicYear.CanonicalForComparison("5786"));
        Assert.Equal(CanonicalYear, HebrewAcademicYear.CanonicalForComparison("2026"));
        Assert.Equal(canonical, HebrewAcademicYear.CanonicalForComparison("  תשפ\"ו  "));
    }

    [Fact]
    public async Task EmploymentDataService_Create_StoresCanonicalYear()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id);
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "SYM-1");

        var dto = new EmploymentDataDto
        {
            EmployeeId = employee.Id,
            EmployerId = employer.Id,
            AcademicYear = "5786",
            Grade1GradeName = "יסודי וגנים",
            Grade1Grade = "ב",
            Grade1Role = "גננת ראשית",
            Grade1Seniority = "5",
            Slots =
            [
                new EmploymentDataSlotDto
                {
                    GradeBand = 1,
                    SlotIndex = 1,
                    InstitutionSymbol = "SYM-1",
                    WeeklyHours = 30,
                },
            ],
        };

        var sut = ServiceTestFactory.CreateEmploymentDataService(db);
        var (created, error) = await sut.CreateAsync(dto);

        Assert.Null(error);
        Assert.NotNull(created);
        Assert.Equal(CanonicalYear, created!.AcademicYear);
    }

    private static async Task SeedEmploymentAsync(
        PayrollDbContext db,
        int employerId,
        int employeeId,
        string academicYear,
        string symbol)
    {
        db.EmploymentData.Add(new EmploymentData
        {
            EmployerId = employerId,
            EmployeeId = employeeId,
            AcademicYear = academicYear,
            Grade1Role = "גננת",
            Grade1Grade = "ב",
            Grade1Seniority = "5",
            Slots =
            [
                new EmploymentDataSlot
                {
                    GradeBand = 1,
                    SlotIndex = 1,
                    InstitutionSymbol = symbol,
                    WeeklyHours = 30,
                },
            ],
        });
        await db.SaveChangesAsync();
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
