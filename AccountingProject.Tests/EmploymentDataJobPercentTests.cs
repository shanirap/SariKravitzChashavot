using AccountingProject.Contracts;
using AccountingProject.Services;
using AccountingProject.Tests.TestHelpers;
using Microsoft.EntityFrameworkCore;

namespace AccountingProject.Tests;

public sealed class EmploymentDataJobPercentTests
{
    [Fact]
    public async Task CreateAsync_StoresGrossJobBase_ComputesJobPercentWithAgeHoursDeduction()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "JB-1");
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id);

        var sut = ServiceTestFactory.CreateEmploymentDataService(db);
        var dto = new EmploymentDataDto
        {
            EmployeeId = employee.Id,
            EmployerId = employer.Id,
            AcademicYear = ReportTestData.DefaultAcademicYear,
            Grade1GradeName = "יסודי וגנים",
            Grade1Grade = "ב",
            Grade1Role = "מורה מקצועי",
            Grade1Seniority = "1",
            Grade1AgeHours = 2m,
            Slots =
            [
                new EmploymentDataSlotDto
                {
                    GradeBand = 1,
                    SlotIndex = 1,
                    InstitutionSymbol = "JB-1",
                    WeeklyHours = 28m,
                    JobBase = 30m,
                },
            ],
        };

        var (record, message) = await sut.CreateAsync(dto);

        Assert.Null(message);
        Assert.NotNull(record);
        Assert.Equal(100m, record!.Grade1JobPercent);

        var slot = await db.EmploymentData
            .Include(e => e.Slots)
            .Where(e => e.Id == record.Id)
            .SelectMany(e => e.Slots)
            .SingleAsync();
        Assert.Equal(30m, slot.JobBase);
    }
}
