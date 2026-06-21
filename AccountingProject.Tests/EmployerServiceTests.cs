using AccountingProject.Contracts;
using AccountingProject.Models;
using AccountingProject.Services;
using AccountingProject.Tests.TestHelpers;

namespace AccountingProject.Tests;

public sealed class EmployerServiceTests
{
    [Fact]
    public async Task GetPagedAsync_SearchFiltersByName()
    {
        await using var db = DbTestFactory.CreateContext();
        db.Employers.AddRange(
            new Employer { Name = "Alpha Corp" },
            new Employer { Name = "Beta Ltd" });
        await db.SaveChangesAsync();

        var sut = new EmployerService(db);
        var result = await sut.GetPagedAsync("Alpha", 1, 10);

        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal("Alpha Corp", result.Items[0].Name);
    }

    [Fact]
    public async Task GetPagedAsync_SearchFiltersByEketzNumber()
    {
        await using var db = DbTestFactory.CreateContext();
        db.Employers.AddRange(
            new Employer { Name = "Employer A", EketzNumber = "EK-12345" },
            new Employer { Name = "Employer B", EketzNumber = "EK-99999" });
        await db.SaveChangesAsync();

        var sut = new EmployerService(db);
        var result = await sut.GetPagedAsync("12345", 1, 10);

        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal("Employer A", result.Items[0].Name);
    }

    [Fact]
    public async Task DeleteInstitutionSymbol_WhenUsedInEmployment_ReturnsConflictMessage()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        var symbol = await ReportTestData.SeedSymbolAsync(db, employer.Id, "USED");
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id);
        await ReportTestData.SeedEmploymentWithSlotAsync(db, employer.Id, employee.Id, "USED");

        var sut = new EmployerService(db);
        var (success, message) = await sut.DeleteInstitutionSymbolAsync(employer.Id, symbol.Id);

        Assert.False(success);
        Assert.Contains("בשימוש", message);
        Assert.Equal(1, db.EmployerInstitutionSymbols.Count());
    }

    [Fact]
    public async Task DeleteInstitutionSymbol_WhenUnused_RemovesRow()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        var symbol = await ReportTestData.SeedSymbolAsync(db, employer.Id, "FREE");

        var sut = new EmployerService(db);
        var (success, message) = await sut.DeleteInstitutionSymbolAsync(employer.Id, symbol.Id);

        Assert.True(success);
        Assert.Null(message);
        Assert.Equal(0, db.EmployerInstitutionSymbols.Count());
    }

    [Fact]
    public async Task UpdateAsync_ChangesEmployerFields()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db, "Old Name");

        var sut = new EmployerService(db);
        var ok = await sut.UpdateAsync(employer.Id, new EmployerDto
        {
            Name = "New Name",
            BusinessNumber = "514111111",
        });

        Assert.True(ok);
        var saved = await db.Employers.FindAsync(employer.Id);
        Assert.Equal("New Name", saved!.Name);
        Assert.Equal("514111111", saved.BusinessNumber);
    }
}

public sealed class EmployerEmployeeFilterTests
{
    [Fact]
    public async Task GetEmployeesAsync_FiltersByActiveStatus()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        var activeWithEd = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "111111111", "Active", "One");
        var inactiveManual = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "222222222", "Inactive", "Two");
        inactiveManual.ManualActiveStatus = false;
        var noEd = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "333333333", "No", "Data");
        await ReportTestData.SeedEmploymentWithSlotAsync(db, employer.Id, activeWithEd.Id, "SYM-A");
        await db.SaveChangesAsync();

        var sut = new EmployerService(db);

        var activeOnly = await sut.GetEmployeesAsync(employer.Id, null, 1, 50, isActive: true);
        Assert.Equal(1, activeOnly.TotalCount);
        Assert.Equal(activeWithEd.Id, activeOnly.Items[0].Id);

        var inactiveOnly = await sut.GetEmployeesAsync(employer.Id, null, 1, 50, isActive: false);
        Assert.Equal(2, inactiveOnly.TotalCount);
        Assert.Contains(inactiveOnly.Items, e => e.Id == inactiveManual.Id);
        Assert.Contains(inactiveOnly.Items, e => e.Id == noEd.Id);
    }

    [Fact]
    public async Task GetEmployeesAsync_FiltersByInstitutionSymbol()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        var atSymA = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "444444444", "At", "SymA");
        var atSymB = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "555555555", "At", "SymB");
        await ReportTestData.SeedEmploymentWithSlotAsync(db, employer.Id, atSymA.Id, "SYM-A");
        await ReportTestData.SeedEmploymentWithSlotAsync(db, employer.Id, atSymB.Id, "SYM-B");

        var sut = new EmployerService(db);
        var filtered = await sut.GetEmployeesAsync(employer.Id, null, 1, 50, institutionSymbol: "SYM-A");

        Assert.Equal(1, filtered.TotalCount);
        Assert.Equal(atSymA.Id, filtered.Items[0].Id);
    }

    [Fact]
    public async Task GetEmployeesAsync_FiltersByActiveAndInstitutionSymbolTogether()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        var activeAtA = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "111111111", "Active", "A");
        var activeAtB = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "222222222", "Active", "B");
        var inactiveAtA = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "333333333", "Inactive", "A");
        inactiveAtA.ManualActiveStatus = false;
        await ReportTestData.SeedEmploymentWithSlotAsync(db, employer.Id, activeAtA.Id, "SYM-A");
        await ReportTestData.SeedEmploymentWithSlotAsync(db, employer.Id, activeAtB.Id, "SYM-B");
        await ReportTestData.SeedEmploymentWithSlotAsync(db, employer.Id, inactiveAtA.Id, "SYM-A");
        await db.SaveChangesAsync();

        var sut = new EmployerService(db);
        var filtered = await sut.GetEmployeesAsync(
            employer.Id, null, 1, 50, isActive: true, institutionSymbol: "SYM-A");

        Assert.Equal(1, filtered.TotalCount);
        Assert.Equal(activeAtA.Id, filtered.Items[0].Id);
    }

    [Fact]
    public async Task GetEmployeesAsync_SearchWithActiveFilter_AppliesBoth()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        var activeCohen = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "444444444", "Active", "Cohen");
        var inactiveCohen = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "555555555", "Inactive", "Cohen");
        inactiveCohen.ManualActiveStatus = false;
        await ReportTestData.SeedEmploymentWithSlotAsync(db, employer.Id, activeCohen.Id, "SYM-A");
        await ReportTestData.SeedEmploymentWithSlotAsync(db, employer.Id, inactiveCohen.Id, "SYM-A");
        await db.SaveChangesAsync();

        var sut = new EmployerService(db);
        var filtered = await sut.GetEmployeesAsync(
            employer.Id, "Cohen", 1, 50, isActive: true);

        Assert.Equal(1, filtered.TotalCount);
        Assert.Equal(activeCohen.Id, filtered.Items[0].Id);
    }
}
