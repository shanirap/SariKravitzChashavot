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
