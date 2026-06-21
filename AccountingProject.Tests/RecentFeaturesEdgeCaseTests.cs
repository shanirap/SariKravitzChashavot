using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AccountingProject.Contracts;
using AccountingProject.Domain;
using AccountingProject.Models;
using AccountingProject.Services;
using AccountingProject.Tests.Integration;
using AccountingProject.Tests.TestHelpers;

namespace AccountingProject.Tests;

public sealed class MotherBenefitRulesEdgeCaseTests
{
    private static readonly DateOnly RefDate = HebrewAcademicYear.GetSchoolYearStartDate("תשפ\"ו");

    [Fact]
    public void ComputePercent_JobJustAboveThreshold_ReturnsRate()
    {
        var children = new DateOnly?[] { new DateOnly(2012, 3, 15) };
        var result = MotherBenefitRules.ComputePercent(
            "יסודי וגנים", true, children, RefDate, 79.01m);
        Assert.Equal(10m, result);
    }

    [Fact]
    public void ComputePercent_NullBaseJobPercent_Returns0()
    {
        var children = new DateOnly?[] { new DateOnly(2012, 3, 15) };
        var result = MotherBenefitRules.ComputePercent(
            "יסודי וגנים", true, children, RefDate, null);
        Assert.Equal(0m, result);
    }

    [Fact]
    public void HasChildUpToAgeInclusive_FirstChildTooOldSecondEligible_ReturnsTrue()
    {
        var children = new DateOnly?[]
        {
            new DateOnly(2010, 8, 31),
            new DateOnly(2012, 3, 15),
        };
        Assert.True(MotherBenefitRules.HasChildUpToAgeInclusive(children, RefDate));
    }

    [Fact]
    public void HasChildUpToAgeInclusive_AllNullBirthDates_ReturnsFalse()
    {
        var children = new DateOnly?[] { null, null, null };
        Assert.False(MotherBenefitRules.HasChildUpToAgeInclusive(children, RefDate));
    }

    [Fact]
    public void AgeInFullYearsAtDate_BirthdayDayAfterRefDate_IsStill14()
    {
        Assert.Equal(14, MotherBenefitRules.AgeInFullYearsAtDate(new DateOnly(2010, 9, 2), RefDate));
    }

    [Fact]
    public void AgeInFullYearsAtDate_BirthdayOnRefDate_Is15()
    {
        Assert.Equal(15, MotherBenefitRules.AgeInFullYearsAtDate(new DateOnly(2010, 9, 1), RefDate));
    }

    [Fact]
    public void ComputePercent_ChildTurning15OnRefDate_Returns0()
    {
        var children = new DateOnly?[] { new DateOnly(2010, 9, 1) };
        var result = MotherBenefitRules.ComputePercent(
            "יסודי וגנים", true, children, RefDate, 80m);
        Assert.Equal(0m, result);
    }

    [Fact]
    public void ComputePercent_ChildDayAfterRefBirthdayStill14_ReturnsRate()
    {
        var children = new DateOnly?[] { new DateOnly(2010, 9, 2) };
        var result = MotherBenefitRules.ComputePercent(
            "אופק גנים", true, children, RefDate, 80m);
        Assert.Equal(10m, result);
    }
}

public sealed class EmployerEmployeeFilterEdgeCaseTests
{
    [Fact]
    public async Task GetEmployeesAsync_UnknownInstitutionSymbol_ReturnsEmpty()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id);
        await ReportTestData.SeedEmploymentWithSlotAsync(db, employer.Id, employee.Id, "SYM-A");

        var sut = new EmployerService(db);
        var filtered = await sut.GetEmployeesAsync(employer.Id, null, 1, 50, institutionSymbol: "NO-MATCH");

        Assert.Equal(0, filtered.TotalCount);
        Assert.Empty(filtered.Items);
    }

    [Fact]
    public async Task GetEmployeesAsync_WhitespaceInstitutionSymbol_ReturnsAllEmployees()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        var first = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "111111111", "A", "One");
        var second = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "222222222", "B", "Two");
        await ReportTestData.SeedEmploymentWithSlotAsync(db, employer.Id, first.Id, "SYM-A");

        var sut = new EmployerService(db);
        var filtered = await sut.GetEmployeesAsync(employer.Id, null, 1, 50, institutionSymbol: "   ");

        Assert.Equal(2, filtered.TotalCount);
    }

    [Fact]
    public async Task GetEmployeesAsync_ManualActiveTrueWithoutEmployment_InActiveFilter()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        var manualActive = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "333333333", "Manual", "Active");
        manualActive.ManualActiveStatus = true;
        await db.SaveChangesAsync();

        var sut = new EmployerService(db);
        var activeOnly = await sut.GetEmployeesAsync(employer.Id, null, 1, 50, isActive: true);

        Assert.Equal(1, activeOnly.TotalCount);
        Assert.Equal(manualActive.Id, activeOnly.Items[0].Id);
    }

    [Fact]
    public async Task GetEmployeesAsync_ActiveFilterWithPagination_RespectsPageSize()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        for (var i = 0; i < 3; i++)
        {
            var emp = await ReportTestData.SeedEmployeeAsync(db, employer.Id, $"10000000{i}", "Active", $"User{i}");
            await ReportTestData.SeedEmploymentWithSlotAsync(db, employer.Id, emp.Id, "SYM-A");
        }

        var sut = new EmployerService(db);
        var page1 = await sut.GetEmployeesAsync(employer.Id, null, 1, 2, isActive: true);
        var page2 = await sut.GetEmployeesAsync(employer.Id, null, 2, 2, isActive: true);

        Assert.Equal(3, page1.TotalCount);
        Assert.Equal(2, page1.Items.Count);
        Assert.Single(page2.Items);
    }
}

public sealed class EmploymentDataMotherBenefitEdgeCaseTests
{
    [Fact]
    public async Task CreateAsync_EligibleChildOnlyInSlot10_ComputesBenefit()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "SYM-1");
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "121212121");
        employee.ChildBirthDate10 = new DateOnly(2012, 3, 15);
        await db.SaveChangesAsync();

        var sut = ServiceTestFactory.CreateEmploymentDataService(db);
        var dto = BuildFemaleYisodiDto(employee.Id, employer.Id);

        var (record, message) = await sut.CreateAsync(dto);

        Assert.Null(message);
        Assert.Equal(10m, record!.Grade1MotherBenefitPercent);
    }

    [Fact]
    public async Task CreateAsync_JobJustAbove79Percent_ComputesBenefit()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "SYM-1");
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "131313131");
        employee.ChildBirthDate1 = new DateOnly(2012, 3, 15);
        await db.SaveChangesAsync();

        var sut = ServiceTestFactory.CreateEmploymentDataService(db);
        var dto = BuildFemaleYisodiDto(employee.Id, employer.Id);
        dto.Slots![0].WeeklyHours = 23.71m;
        dto.Slots[0].JobBase = 30m;

        var (record, message) = await sut.CreateAsync(dto);

        Assert.Null(message);
        Assert.Equal(10m, record!.Grade1MotherBenefitPercent);
    }

    [Fact]
    public async Task CreateAsync_MaleWithAllEligibleConditions_StillZero()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "SYM-1");
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "141414141");
        employee.Gender = "זכר";
        employee.ChildBirthDate1 = new DateOnly(2012, 3, 15);
        await db.SaveChangesAsync();

        var sut = ServiceTestFactory.CreateEmploymentDataService(db);
        var dto = BuildFemaleYisodiDto(employee.Id, employer.Id);

        var (record, message) = await sut.CreateAsync(dto);

        Assert.Null(message);
        Assert.Equal(0m, record!.Grade1MotherBenefitPercent);
    }

    [Fact]
    public async Task CreateAsync_FirstChildTooOldSecondEligible_ComputesBenefit()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "SYM-1");
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "151515151");
        employee.ChildBirthDate1 = new DateOnly(2010, 8, 31);
        employee.ChildBirthDate2 = new DateOnly(2012, 6, 1);
        await db.SaveChangesAsync();

        var sut = ServiceTestFactory.CreateEmploymentDataService(db);
        var dto = BuildFemaleYisodiDto(employee.Id, employer.Id);

        var (record, message) = await sut.CreateAsync(dto);

        Assert.Null(message);
        Assert.Equal(10m, record!.Grade1MotherBenefitPercent);
    }

    private static EmploymentDataDto BuildFemaleYisodiDto(int employeeId, int employerId) =>
        new()
        {
            EmployeeId = employeeId,
            EmployerId = employerId,
            AcademicYear = ReportTestData.DefaultAcademicYear,
            Grade1GradeName = "יסודי וגנים",
            Grade1Grade = "ב",
            Grade1Role = "גננת ראשית",
            Grade1Seniority = "1",
            Slots =
            [
                new EmploymentDataSlotDto
                {
                    GradeBand = 1,
                    SlotIndex = 1,
                    InstitutionSymbol = "SYM-1",
                    WeeklyHours = 30m,
                    JobBase = 30m,
                },
            ],
        };
}

public sealed class AnnualComparisonSavedOverridesEdgeCaseTests
{
    private const string Year = "תשפ\"ו";

    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    [Fact]
    public async Task SaveOverrides_UnknownSlotId_ReturnsBadRequest()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);
        var employerId = await ReportsApiIntegrationTests.SeedEmployerWithDataViaApiAsync(client);

        var saveResp = await client.PutAsJsonAsync("/api/reports/annual-comparison-saved/overrides",
            new AnnualComparisonOverrideSaveRequest
            {
                EmployerId = employerId,
                AcademicYear = Year,
                Rows =
                [
                    new AnnualComparisonOverrideRowSaveDto
                    {
                        SlotId = 999999,
                        FullName = "לא קיים",
                    },
                ],
            });

        Assert.Equal(HttpStatusCode.BadRequest, saveResp.StatusCode);
        var body = await saveResp.Content.ReadAsStringAsync();
        Assert.Contains("999999", body);
    }
}
