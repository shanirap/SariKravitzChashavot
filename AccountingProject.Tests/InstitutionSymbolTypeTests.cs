using AccountingProject.Contracts;
using AccountingProject.Data;
using AccountingProject.Domain;
using AccountingProject.Models;
using AccountingProject.Services;
using AccountingProject.Tests.TestHelpers;
using ClosedXML.Excel;

namespace AccountingProject.Tests;

public sealed class InstitutionSymbolTypeTests
{
    [Theory]
    [InlineData(InstitutionTypes.Kindergarten)]
    [InlineData(InstitutionTypes.School)]
    [InlineData(InstitutionTypes.Other)]
    public async Task CreateInstitutionSymbol_PersistsValidInstitutionType(string institutionType)
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = SeedEmployer(db);

        var sut = new EmployerService(db);
        var (symbol, message) = await sut.CreateInstitutionSymbolAsync(employer.Id, new EmployerInstitutionSymbolDto
        {
            InstitutionSymbol = $"SYM-{institutionType}",
            InstitutionSymbolName = "Test",
            InstitutionType = institutionType,
        });

        Assert.Null(message);
        Assert.NotNull(symbol);
        Assert.Equal(institutionType, symbol!.InstitutionType);

        var saved = await db.EmployerInstitutionSymbols.FindAsync(symbol.Id);
        Assert.Equal(institutionType, saved!.InstitutionType);
    }

    [Fact]
    public async Task CreateInstitutionSymbol_WithoutType_DefaultsToOther()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = SeedEmployer(db);

        var sut = new EmployerService(db);
        var (symbol, message) = await sut.CreateInstitutionSymbolAsync(employer.Id, new EmployerInstitutionSymbolDto
        {
            InstitutionSymbol = "SYM-DEFAULT",
            InstitutionSymbolName = "Default",
        });

        Assert.Null(message);
        Assert.Equal(InstitutionTypes.Other, symbol!.InstitutionType);
    }

    [Fact]
    public async Task CreateInstitutionSymbol_InvalidType_ReturnsValidationError()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = SeedEmployer(db);

        var sut = new EmployerService(db);
        var (symbol, message) = await sut.CreateInstitutionSymbolAsync(employer.Id, new EmployerInstitutionSymbolDto
        {
            InstitutionSymbol = "SYM-BAD",
            InstitutionType = "גן חובה",
        });

        Assert.Null(symbol);
        Assert.Contains("סוג מוסד", message);
        Assert.Equal(0, db.EmployerInstitutionSymbols.Count());
    }

    [Fact]
    public async Task UpdateInstitutionSymbol_ChangesInstitutionType()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = SeedEmployer(db);
        db.EmployerInstitutionSymbols.Add(new EmployerInstitutionSymbol
        {
            EmployerId = employer.Id,
            InstitutionSymbol = "UPD1",
            InstitutionType = InstitutionTypes.Other,
        });
        await db.SaveChangesAsync();
        var existing = db.EmployerInstitutionSymbols.First();

        var sut = new EmployerService(db);
        var (symbol, message) = await sut.UpdateInstitutionSymbolAsync(employer.Id, existing.Id, new EmployerInstitutionSymbolUpdateDto
        {
            InstitutionType = InstitutionTypes.School,
        });

        Assert.Null(message);
        Assert.Equal(InstitutionTypes.School, symbol!.InstitutionType);
    }

    [Fact]
    public async Task GetInstitutionSymbols_ReturnsInstitutionType()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = SeedEmployer(db);
        db.EmployerInstitutionSymbols.Add(new EmployerInstitutionSymbol
        {
            EmployerId = employer.Id,
            InstitutionSymbol = "G1",
            InstitutionType = InstitutionTypes.Kindergarten,
        });
        await db.SaveChangesAsync();

        var sut = new EmployerService(db);
        var list = await sut.GetInstitutionSymbolsAsync(employer.Id);

        Assert.Single(list);
        Assert.Equal(InstitutionTypes.Kindergarten, list[0].InstitutionType);
    }

    [Fact]
    public async Task FullEmployerExport_IncludesInstitutionTypeColumn()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = SeedEmployer(db);
        db.EmployerInstitutionSymbols.Add(new EmployerInstitutionSymbol
        {
            EmployerId = employer.Id,
            InstitutionSymbol = "EX1",
            InstitutionSymbolName = "Export Test",
            InstitutionType = InstitutionTypes.School,
        });
        await db.SaveChangesAsync();

        var sut = new EmployerService(db);
        var bytes = await sut.BuildFullEmployerExportExcelAsync(employer.Id);
        Assert.NotNull(bytes);

        using var wb = new XLWorkbook(new MemoryStream(bytes!));
        var ws = wb.Worksheet("סמלי מוסד");
        Assert.Equal("סוג מוסד", ws.Cell(1, 4).GetString());
        Assert.Equal(InstitutionTypes.School, ws.Cell(2, 4).GetString());
    }

    private static Employer SeedEmployer(PayrollDbContext db)
    {
        var employer = new Employer { Name = "Institution Type Employer" };
        db.Employers.Add(employer);
        db.SaveChanges();
        return employer;
    }
}
