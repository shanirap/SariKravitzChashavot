using AccountingProject.Contracts;
using AccountingProject.Services;
using AccountingProject.Tests.TestHelpers;

namespace AccountingProject.Tests;

public sealed class EmploymentDataInstitutionValidationTests
{
    [Fact]
    public async Task CreateAsync_RejectsInstitutionSymbolNotOwnedByEmployer()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "OWNED");
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id);

        var sut = ServiceTestFactory.CreateEmploymentDataService(db);
        var dto = new EmploymentDataDto
        {
            EmployeeId = employee.Id,
            EmployerId = employer.Id,
            AcademicYear = ReportTestData.DefaultAcademicYear,
            Slots =
            [
                new EmploymentDataSlotDto
                {
                    GradeBand = 1,
                    SlotIndex = 1,
                    InstitutionSymbol = "FOREIGN",
                    WeeklyHours = 20m,
                },
            ],
        };

        var (record, message) = await sut.CreateAsync(dto);

        Assert.Null(record);
        Assert.Contains("אינו שייך למעסיק", message);
    }
}
