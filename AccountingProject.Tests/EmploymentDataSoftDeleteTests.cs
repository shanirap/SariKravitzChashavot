using AccountingProject.Contracts;
using AccountingProject.Services;
using AccountingProject.Tests.TestHelpers;
using Microsoft.EntityFrameworkCore;

namespace AccountingProject.Tests;

/// <summary>
/// Soft-deleted employment rows are excluded from duplicate-year checks; active rows still block duplicates.
/// </summary>
public sealed class EmploymentDataSoftDeleteTests
{
    [Fact]
    public async Task CreateAsync_AfterSoftDeletedSameYear_Succeeds()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id);
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "SYM1");

        var employmentSut = ServiceTestFactory.CreateEmploymentDataService(db);
        var firstDto = BasicEmploymentDto(employee.Id, employer.Id, ReportTestData.DefaultAcademicYear);

        var (first, firstMessage) = await employmentSut.CreateAsync(firstDto);
        Assert.NotNull(first);
        Assert.Null(firstMessage);
        var firstId = first!.Id;

        var deleted = await employmentSut.DeleteAsync(firstId);
        Assert.True(deleted.Success);

        var (second, secondMessage) = await employmentSut.CreateAsync(firstDto);
        Assert.NotNull(second);
        Assert.Null(secondMessage);
        Assert.NotEqual(firstId, second!.Id);
        Assert.Equal(ReportTestData.DefaultAcademicYear, second.AcademicYear);

        var activeCount = await db.EmploymentData.CountAsync(ed =>
            ed.EmployeeId == employee.Id
            && ed.EmployerId == employer.Id
            && ed.AcademicYear == ReportTestData.DefaultAcademicYear);
        Assert.Equal(1, activeCount);

        var totalIncludingDeleted = await db.EmploymentData.IgnoreQueryFilters()
            .CountAsync(ed =>
                ed.EmployeeId == employee.Id
                && ed.EmployerId == employer.Id
                && ed.AcademicYear == ReportTestData.DefaultAcademicYear);
        Assert.Equal(2, totalIncludingDeleted);
    }

    [Fact]
    public async Task CreateAsync_ActiveDuplicateSameYear_StillRejected()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id);
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "SYM1");

        var sut = ServiceTestFactory.CreateEmploymentDataService(db);
        var dto = BasicEmploymentDto(employee.Id, employer.Id, ReportTestData.DefaultAcademicYear);

        var (created, _) = await sut.CreateAsync(dto);
        Assert.NotNull(created);

        var (duplicate, message) = await sut.CreateAsync(dto);

        Assert.Null(duplicate);
        Assert.Contains("כבר קיימת רשומה", message);
    }

    private static EmploymentDataDto BasicEmploymentDto(int employeeId, int employerId, string academicYear) =>
        new()
        {
            EmployeeId = employeeId,
            EmployerId = employerId,
            AcademicYear = academicYear,
            Slots = [],
        };
}
