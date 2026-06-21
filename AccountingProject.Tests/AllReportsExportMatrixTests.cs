using AccountingProject.Services;
using AccountingProject.Tests.TestHelpers;
using ClosedXML.Excel;

namespace AccountingProject.Tests;

/// <summary>מטריצה — כל 7 דוחות השרת מחזירים Excel תקין עם שם גיליון צפוי.</summary>
public sealed class AllReportsExportMatrixTests
{
    private const string Year = "תשפ\"ו";

    [Fact]
    public async Task AllSevenReports_WithSeededData_ProduceExpectedWorksheets()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "G-1");
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "S-1", "בית ספר");
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id, "123456789");
        await ReportTestData.SeedEmploymentWithSlotAsync(db, employer.Id, employee.Id, "G-1");

        var sut = new ReportExportService(db);

        await AssertSheetAsync(await sut.KindergartenAnnualAsync(employer.Id, Year), "מצבת גנים");
        await AssertSheetAsync(await sut.SchoolAnnualAsync(employer.Id, Year), "מצבת בית ספר");
        await AssertSheetAsync(await sut.EmployeesPersonalAsync(employer.Id), "עובדים אישיים");
        await AssertSheetAsync(await sut.EmployeesEmploymentDataAsync(employer.Id, Year), "עובדים נתוני העסקה");
        await AssertSheetAsync(await sut.InstitutionHoursAsync(employer.Id, Year, "G-1"), "בדיקת שעות לסמל");

        await using var upload = MonthlyComparisonUploadWorkbook.Create(
            "123456789", null, "Worker", 9, 2025, b => b.Band1(misra1Hours: 30m, misra1Base: 30m));
        await AssertSheetAsync(
            await sut.MonthlyComparisonAsync(employer.Id, Year, 9, upload),
            "השוואה חודשית");

        upload.Position = 0;
        await AssertSheetAsync(
            await sut.AnnualComparisonAsync(employer.Id, Year, upload),
            "השוואה שנתית");
    }

    private static Task AssertSheetAsync(byte[] bytes, string sheetName)
    {
        Assert.True(bytes.Length > 64);
        using var wb = new XLWorkbook(new MemoryStream(bytes));
        Assert.NotNull(wb.Worksheet(sheetName));
        return Task.CompletedTask;
    }
}
