using AccountingProject.Domain;
using AccountingProject.Services;
using AccountingProject.Tests.TestHelpers;
using ClosedXML.Excel;

namespace AccountingProject.Tests;

public sealed class ComparisonReportServiceTests
{
    [Fact]
    public async Task GenerateMonthlyPayrollComparison_EmptyDataRows_Throws()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("עוקץ");
        ws.Cell(1, 1).Value = "תז";
        ws.Cell(1, 2).Value = "חודש";
        ws.Cell(1, 3).Value = "שנה";
        var ms = new MemoryStream();
        wb.SaveAs(ms);
        ms.Position = 0;

        var sut = new ComparisonReportService(db);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.GenerateMonthlyPayrollComparisonExcelAsync(employer.Id, ms));
    }

    [Fact]
    public async Task GenerateMonthlyPayrollComparison_WithMatchingEmployee_ReturnsExcel()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "555666777");
        var academicYear = SchoolYearGregorian.GetSchoolYearFromGregorianMonth(9, 2025);
        await ReportTestData.SeedEmploymentWithSlotAsync(db, employer.Id, employee.Id, "SYM", academicYear);

        await using var upload = ExcelTestWorkbook.CreatePayrollComparisonUpload("555666777", 9, 2025);
        var bytes = await new ComparisonReportService(db).GenerateMonthlyPayrollComparisonExcelAsync(employer.Id, upload);

        Assert.True(bytes.Length > 64);
        using var wb = new XLWorkbook(new MemoryStream(bytes));
        Assert.True(wb.Worksheets.Count >= 1);
    }
}
