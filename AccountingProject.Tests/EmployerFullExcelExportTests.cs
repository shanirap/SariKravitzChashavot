using AccountingProject.Domain;
using AccountingProject.Services;
using AccountingProject.Tests.TestHelpers;
using ClosedXML.Excel;

namespace AccountingProject.Tests;

public sealed class EmployerFullExcelExportTests
{
    [Fact]
    public async Task BuildFullExport_ContainsExpectedWorksheetsAndInstitutionTypeColumn()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "EX-1", InstitutionTypes.School);
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id);
        await ReportTestData.SeedEmploymentWithSlotAsync(db, employer.Id, employee.Id, "EX-1");

        var bytes = await new EmployerService(db).BuildFullEmployerExportExcelAsync(employer.Id);
        Assert.NotNull(bytes);

        using var wb = new XLWorkbook(new MemoryStream(bytes!));
        Assert.NotNull(wb.Worksheet("מעסיק"));
        Assert.NotNull(wb.Worksheet("עובדים"));
        Assert.NotNull(wb.Worksheet("סמלי מוסד"));
        Assert.NotNull(wb.Worksheet("נתוני עסקה"));

        var symbols = wb.Worksheet("סמלי מוסד");
        Assert.Equal("סוג מוסד", symbols.Cell(1, 4).GetString());
        Assert.Equal(InstitutionTypes.School, symbols.Cell(2, 4).GetString());
    }
}
