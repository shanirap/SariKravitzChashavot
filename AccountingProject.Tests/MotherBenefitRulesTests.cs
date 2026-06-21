using AccountingProject.Contracts;
using AccountingProject.Domain;
using AccountingProject.Models;
using AccountingProject.Services;
using AccountingProject.Tests.TestHelpers;

namespace AccountingProject.Tests;

public sealed class MotherBenefitRulesTests
{
    private static readonly DateOnly RefDate = HebrewAcademicYear.GetSchoolYearStartDate("תשפ\"ו");

    [Fact]
    public void GetSchoolYearStartDate_TashpaV_IsSeptemberFirst2025()
    {
        var start = HebrewAcademicYear.GetSchoolYearStartDate("תשפ\"ו");
        Assert.Equal(new DateOnly(2025, 9, 1), start);
    }

    [Fact]
    public void ComputePercent_FemaleYisodiChild13HighJob_Returns10()
    {
        var children = new DateOnly?[] { new DateOnly(2012, 3, 15) };
        var result = MotherBenefitRules.ComputePercent(
            "יסודי וגנים", true, children, RefDate, 80m);
        Assert.Equal(10m, result);
    }

    [Fact]
    public void ComputePercent_FemaleOzLeTmuraChild14HighJob_Returns7()
    {
        var children = new DateOnly?[] { new DateOnly(2011, 9, 1) };
        var result = MotherBenefitRules.ComputePercent(
            "עוז לתמורה", true, children, RefDate, 80m);
        Assert.Equal(7m, result);
    }

    [Fact]
    public void ComputePercent_FemaleAhidWithChild_Returns0()
    {
        var children = new DateOnly?[] { new DateOnly(2012, 3, 15) };
        var result = MotherBenefitRules.ComputePercent(
            GradeOptions.UnifiedEducationSupportGradeName, true, children, RefDate, 80m);
        Assert.Equal(0m, result);
    }

    [Fact]
    public void ComputePercent_FemaleYisodiNoChildren_Returns0()
    {
        var result = MotherBenefitRules.ComputePercent(
            "יסודי וגנים", true, Array.Empty<DateOnly?>(), RefDate, 80m);
        Assert.Equal(0m, result);
    }

    [Fact]
    public void ComputePercent_FemaleYisodiChild15_Returns0()
    {
        var children = new DateOnly?[] { new DateOnly(2010, 8, 31) };
        var result = MotherBenefitRules.ComputePercent(
            "יסודי וגנים", true, children, RefDate, 80m);
        Assert.Equal(0m, result);
    }

    [Fact]
    public void ComputePercent_MaleYisodiWithChild_Returns0()
    {
        var children = new DateOnly?[] { new DateOnly(2012, 3, 15) };
        var result = MotherBenefitRules.ComputePercent(
            "יסודי וגנים", false, children, RefDate, 80m);
        Assert.Equal(0m, result);
    }

    [Fact]
    public void ComputePercent_JobAtThreshold_Returns0()
    {
        var children = new DateOnly?[] { new DateOnly(2012, 3, 15) };
        var result = MotherBenefitRules.ComputePercent(
            "יסודי וגנים", true, children, RefDate, 79m);
        Assert.Equal(0m, result);
    }

    [Fact]
    public void ComputePercent_EmptyGradeName_ReturnsNull()
    {
        var result = MotherBenefitRules.ComputePercent(
            "", true, Array.Empty<DateOnly?>(), RefDate, 80m);
        Assert.Null(result);
    }

    [Fact]
    public void ComputePercent_UnknownGradeName_ReturnsNull()
    {
        var children = new DateOnly?[] { new DateOnly(2012, 3, 15) };
        var result = MotherBenefitRules.ComputePercent(
            "לא קיים", true, children, RefDate, 80m);
        Assert.Null(result);
    }

    [Fact]
    public void ComputePercent_FemaleOfekHadashChild14HighJob_Returns10()
    {
        var children = new DateOnly?[] { new DateOnly(2011, 9, 1) };
        var result = MotherBenefitRules.ComputePercent(
            "אופק חדש", true, children, RefDate, 80m);
        Assert.Equal(10m, result);
    }

    [Theory]
    [InlineData("יסודי וגנים", 10)]
    [InlineData("עוז לתמורה", 7)]
    [InlineData("אופק חדש", 10)]
    [InlineData("אופק גנים", 10)]
    public void TryGetRateForGradeName_KnownGrades_ReturnExpected(string grade, decimal expected)
    {
        Assert.True(MotherBenefitRules.TryGetRateForGradeName(grade, out var rate));
        Assert.Equal(expected, rate);
    }

    [Theory]
    [InlineData("אחיד/תומכות חינוך")]
    [InlineData("אחיד")]
    [InlineData("")]
    [InlineData("לא קיים")]
    public void TryGetRateForGradeName_ExcludedOrUnknown_ReturnsFalse(string grade)
    {
        Assert.False(MotherBenefitRules.TryGetRateForGradeName(grade, out _));
    }

    [Fact]
    public void HasChildUpToAgeInclusive_ChildExactly14_ReturnsTrue()
    {
        var children = new DateOnly?[] { new DateOnly(2011, 9, 1) };
        Assert.True(MotherBenefitRules.HasChildUpToAgeInclusive(children, RefDate));
    }

    [Fact]
    public void HasChildUpToAgeInclusive_Child15_ReturnsFalse()
    {
        var children = new DateOnly?[] { new DateOnly(2010, 8, 31) };
        Assert.False(MotherBenefitRules.HasChildUpToAgeInclusive(children, RefDate));
    }

    [Fact]
    public void AgeInFullYearsAtDate_ComputesFullYears()
    {
        Assert.Equal(13, MotherBenefitRules.AgeInFullYearsAtDate(new DateOnly(2012, 3, 15), RefDate));
    }
}

public sealed class EmploymentDataMotherBenefitTests
{
    [Fact]
    public async Task CreateAsync_FemaleWithEligibleChild_ComputesMotherBenefitPercent()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "SYM-1");
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id);
        employee.ChildBirthDate1 = new DateOnly(2012, 3, 15);
        await db.SaveChangesAsync();

        var sut = ServiceTestFactory.CreateEmploymentDataService(db);
        var dto = new EmploymentDataDto
        {
            EmployeeId = employee.Id,
            EmployerId = employer.Id,
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

        var (record, message) = await sut.CreateAsync(dto);

        Assert.Null(message);
        Assert.NotNull(record);
        Assert.Equal(10m, record!.Grade1MotherBenefitPercent);
    }

    [Fact]
    public async Task CreateAsync_FemaleAhidWithChild_MotherBenefitIsZero()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "SYM-1");
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "987654321");
        employee.ChildBirthDate1 = new DateOnly(2012, 3, 15);
        await db.SaveChangesAsync();

        var sut = ServiceTestFactory.CreateEmploymentDataService(db);
        var dto = new EmploymentDataDto
        {
            EmployeeId = employee.Id,
            EmployerId = employer.Id,
            AcademicYear = ReportTestData.DefaultAcademicYear,
            Grade1GradeName = GradeOptions.UnifiedEducationSupportGradeName,
            Grade1Grade = "תומכת חינוך",
            Grade1Role = "סייעת ראשית",
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

        var (record, message) = await sut.CreateAsync(dto);

        Assert.Null(message);
        Assert.NotNull(record);
        Assert.Equal(0m, record!.Grade1MotherBenefitPercent);
        Assert.Equal(GradeOptions.UnifiedEducationSupportGradeName, record.Grade1GradeName);
    }

    [Fact]
    public async Task CreateAsync_LegacyAhidGradeName_NormalizesToNewLabel()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "SYM-1");
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "987654322");
        await db.SaveChangesAsync();

        var sut = ServiceTestFactory.CreateEmploymentDataService(db);
        var dto = new EmploymentDataDto
        {
            EmployeeId = employee.Id,
            EmployerId = employer.Id,
            AcademicYear = ReportTestData.DefaultAcademicYear,
            Grade1GradeName = GradeOptions.LegacyUnifiedGradeName,
            Grade1Grade = "תומכת חינוך",
            Grade1Role = "סייעת ראשית",
            Grade1Seniority = "2",
            Grade1Total = 30m,
            Slots =
            [
                new EmploymentDataSlotDto
                {
                    GradeBand = 1,
                    SlotIndex = 1,
                    InstitutionSymbol = "SYM-1",
                    WeeklyHours = 30m,
                    JobBase = 40m,
                },
            ],
        };

        var (record, message) = await sut.CreateAsync(dto);

        Assert.Null(message);
        Assert.NotNull(record);
        Assert.Equal(GradeOptions.UnifiedEducationSupportGradeName, record!.Grade1GradeName);
        Assert.Equal(7.5m, record.Grade1TrainingFundPercent);
    }

    [Fact]
    public async Task CreateAsync_FemaleOzLeTmuraWithEligibleChild_Computes7Percent()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "SYM-1");
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "555444333");
        employee.ChildBirthDate1 = new DateOnly(2012, 6, 1);
        await db.SaveChangesAsync();

        var sut = ServiceTestFactory.CreateEmploymentDataService(db);
        var dto = new EmploymentDataDto
        {
            EmployeeId = employee.Id,
            EmployerId = employer.Id,
            AcademicYear = ReportTestData.DefaultAcademicYear,
            Grade1GradeName = "עוז לתמורה",
            Grade1Grade = "ב",
            Grade1Role = "מורה מחנך",
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

        var (record, message) = await sut.CreateAsync(dto);

        Assert.Null(message);
        Assert.Equal(7m, record!.Grade1MotherBenefitPercent);
    }

    [Fact]
    public async Task UpdateAsync_WithoutEligibleChild_RecalculatesMotherBenefitToZero()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "SYM-1");
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "666777888");
        employee.ChildBirthDate1 = new DateOnly(2012, 3, 15);
        await db.SaveChangesAsync();

        var sut = ServiceTestFactory.CreateEmploymentDataService(db);
        var createDto = new EmploymentDataDto
        {
            EmployeeId = employee.Id,
            EmployerId = employer.Id,
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
        var (created, createMsg) = await sut.CreateAsync(createDto);
        Assert.Null(createMsg);
        Assert.Equal(10m, created!.Grade1MotherBenefitPercent);

        employee.ChildBirthDate1 = new DateOnly(2010, 8, 31);
        await db.SaveChangesAsync();

        var updateDto = new EmploymentDataDto
        {
            EmployeeId = employee.Id,
            EmployerId = employer.Id,
            AcademicYear = ReportTestData.DefaultAcademicYear,
            Grade1GradeName = "יסודי וגנים",
            Grade1Grade = "ב",
            Grade1Role = "גננת ראשית",
            Grade1Seniority = "1",
            Slots = createDto.Slots,
        };
        var (updated, updateMsg) = await sut.UpdateAsync(created.Id, updateDto);

        Assert.Null(updateMsg);
        Assert.Equal(0m, updated!.Grade1MotherBenefitPercent);
    }
}
