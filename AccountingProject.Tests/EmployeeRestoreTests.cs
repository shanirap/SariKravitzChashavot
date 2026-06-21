using AccountingProject.Contracts;
using AccountingProject.Services;
using AccountingProject.Tests.TestHelpers;
using Microsoft.EntityFrameworkCore;

namespace AccountingProject.Tests;

/// <summary>
/// CreateOrGetAsync restores a soft-deleted employee when employer+ת.ז. match; idempotent for active duplicates.
/// </summary>
public sealed class EmployeeRestoreTests
{
    [Fact]
    public async Task CreateOrGetAsync_WithDeletedSameEmployerAndTz_RestoresSameRowAndUpdatesFields()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        var sut = new EmployeeService(db);

        var created = await sut.CreateOrGetAsync(BasicEmployeeDto(employer.Id, "555666777", "Old", "Name"));
        var id = created.Employee.Id;
        Assert.True(created.CreatedNew);
        Assert.False(created.RestoredFromSoftDelete);

        var deleted = await sut.DeleteAsync(id);
        Assert.True(deleted.Success);

        Assert.Null(await sut.GetByEmployerAndIdNumberAsync(employer.Id, "555666777"));

        var restored = await sut.CreateOrGetAsync(BasicEmployeeDto(employer.Id, "555666777", "New", "Person"));

        Assert.Equal(id, restored.Employee.Id);
        Assert.False(restored.CreatedNew);
        Assert.True(restored.RestoredFromSoftDelete);
        Assert.Equal("New", restored.Employee.FirstName);
        Assert.Equal("Person", restored.Employee.LastName);
        Assert.False(restored.Employee.IsDeleted);
        Assert.Null(restored.Employee.DeletedAtUtc);

        var all = await db.Employees.IgnoreQueryFilters()
            .Where(e => e.EmployerId == employer.Id && e.IdNumber == "555666777")
            .ToListAsync();
        Assert.Single(all);
    }

    [Fact]
    public async Task GetPrecreateHint_AfterSoftDelete_IndicatesWillRestore()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        var sut = new EmployeeService(db);

        var created = await sut.CreateOrGetAsync(BasicEmployeeDto(employer.Id, "444555666"));
        await sut.DeleteAsync(created.Employee.Id);

        var hint = await sut.GetPrecreateHintAsync(employer.Id, "444555666");

        Assert.False(hint.EmployerMissing);
        Assert.False(hint.HasActiveEmployeeWithSameTz);
        Assert.True(hint.WillRestoreSoftDeletedEmployee);
    }

    [Fact]
    public async Task GetPrecreateHint_WhenActiveExists_IndicatesHasActive()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        var sut = new EmployeeService(db);

        await sut.CreateOrGetAsync(BasicEmployeeDto(employer.Id, "333444555"));

        var hint = await sut.GetPrecreateHintAsync(employer.Id, "333444555");

        Assert.False(hint.EmployerMissing);
        Assert.True(hint.HasActiveEmployeeWithSameTz);
        Assert.False(hint.WillRestoreSoftDeletedEmployee);
    }

    [Fact]
    public async Task CreateOrGetAsync_DuplicateActiveSameEmployerTz_ReturnsExistingWithoutSecondRow()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        var sut = new EmployeeService(db);
        var dto = BasicEmployeeDto(employer.Id, "222333444", "First", "Create");

        var first = await sut.CreateOrGetAsync(dto);
        var secondDto = BasicEmployeeDto(employer.Id, "222333444", "Ignored", "Create");
        var second = await sut.CreateOrGetAsync(secondDto);

        Assert.True(first.CreatedNew);
        Assert.False(second.CreatedNew);
        Assert.False(second.RestoredFromSoftDelete);
        Assert.Equal(first.Employee.Id, second.Employee.Id);
        Assert.Equal("First", second.Employee.FirstName);

        Assert.Equal(1, await db.Employees.CountAsync(e =>
            e.EmployerId == employer.Id && e.IdNumber == "222333444"));
    }

    private static EmployeeDto BasicEmployeeDto(
        int employerId,
        string idNumber,
        string firstName = "Test",
        string lastName = "User") =>
        new()
        {
            EmployerId = employerId,
            IdNumber = idNumber,
            FirstName = firstName,
            LastName = lastName,
            Gender = "זכר",
            BirthDate = "1990-01-01",
        };
}
