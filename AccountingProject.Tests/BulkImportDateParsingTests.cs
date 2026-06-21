using System.Globalization;
using AccountingProject.Contracts;
using AccountingProject.Data;
using AccountingProject.Models;
using AccountingProject.Services;
using AccountingProject.Tests.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace AccountingProject.Tests;

public sealed class BulkImportDateParsingTests
{
    private const string Year = "תשפ\"ו";

    [Fact]
    public async Task ImportEmployees_ParsesDdMmYyyyText_UnderEnUsCulture()
    {
        using var _ = EnUsCultureScope.Create();
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db, "Date Import Employer");
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "G-1");

        await using var stream = BulkImportEmployeeWorkbook.CreateMinimalRow(
            employer.Name, "111222333", Year, fillRow: (ws, row) =>
            {
                ws.Cell(row, BulkImportEmployeeWorkbook.ColumnIndex(true, "תאריך_לידה")).Value = "25/08/1970";
            });

        var result = await ImportAsync(db, stream);

        Assert.Equal(1, result.Imported);
        var emp = await db.Employees.SingleAsync(e => e.IdNumber == "111222333");
        Assert.Equal(new DateOnly(1970, 8, 25), emp.BirthDate);
    }

    [Fact]
    public async Task ImportEmployees_ParsesNativeDateTimeCell_UnderEnUsCulture()
    {
        using var _ = EnUsCultureScope.Create();
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db, "DateTime Cell Employer");
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "G-1");

        await using var stream = BulkImportEmployeeWorkbook.CreateMinimalRow(
            employer.Name, "222333444", Year, fillRow: (ws, row) =>
            {
                ws.Cell(row, BulkImportEmployeeWorkbook.ColumnIndex(true, "תאריך_לידה")).Value = new DateTime(1970, 8, 25);
            });

        var result = await ImportAsync(db, stream);

        Assert.Equal(1, result.Imported);
        var emp = await db.Employees.SingleAsync(e => e.IdNumber == "222333444");
        Assert.Equal(new DateOnly(1970, 8, 25), emp.BirthDate);
    }

    [Fact]
    public async Task ImportEmployees_ParsesIsoText_UnderEnUsCulture()
    {
        using var _ = EnUsCultureScope.Create();
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db, "Iso Date Employer");
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "G-1");

        await using var stream = BulkImportEmployeeWorkbook.CreateMinimalRow(
            employer.Name, "333444555", Year, fillRow: (ws, row) =>
            {
                ws.Cell(row, BulkImportEmployeeWorkbook.ColumnIndex(true, "תאריך_לידה")).Value = "1990-01-15";
            });

        var result = await ImportAsync(db, stream);

        Assert.Equal(1, result.Imported);
        var emp = await db.Employees.SingleAsync(e => e.IdNumber == "333444555");
        Assert.Equal(new DateOnly(1990, 1, 15), emp.BirthDate);
    }

    [Fact]
    public async Task ImportEmployees_InvalidBirthDate_ReturnsRowError_UnderEnUsCulture()
    {
        using var _ = EnUsCultureScope.Create();
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db, "Invalid Date Employer");
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "G-1");

        await using var stream = BulkImportEmployeeWorkbook.CreateMinimalRow(
            employer.Name, "444555666", Year, fillRow: (ws, row) =>
            {
                ws.Cell(row, BulkImportEmployeeWorkbook.ColumnIndex(true, "תאריך_לידה")).Value = "לא-תאריך";
            });

        var result = await ImportAsync(db, stream);

        Assert.Equal(0, result.Imported);
        Assert.Equal(1, result.Errors);
        Assert.Equal("תאריך_לידה לא תקין.", result.Rows[0].Message);
    }

    [Fact]
    public async Task ImportEmployees_ParsesChildBirthDateDdMmYyyy_UnderEnUsCulture()
    {
        using var _ = EnUsCultureScope.Create();
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db, "Child Date Employer");
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "G-1");

        await using var stream = BulkImportEmployeeWorkbook.CreateMinimalRow(
            employer.Name, "555666777", Year, fillRow: (ws, row) =>
            {
                ws.Cell(row, BulkImportEmployeeWorkbook.ColumnIndex(true, "תאריך_לידה")).Value = "25/08/1970";
                ws.Cell(row, BulkImportEmployeeWorkbook.ColumnIndex(true, "תאריך_לידה_ילד_1")).Value = "15/03/2015";
            });

        var result = await ImportAsync(db, stream);

        Assert.Equal(1, result.Imported);
        var emp = await db.Employees.SingleAsync(e => e.IdNumber == "555666777");
        Assert.Equal(new DateOnly(2015, 3, 15), emp.ChildBirthDate1);
    }

    [Fact]
    public async Task ImportEmployees_ParsesChildBirthDateNativeDateTimeCell_UnderEnUsCulture()
    {
        using var _ = EnUsCultureScope.Create();
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db, "Child DateTime Employer");
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "G-1");

        await using var stream = BulkImportEmployeeWorkbook.CreateMinimalRow(
            employer.Name, "666777888", Year, fillRow: (ws, row) =>
            {
                ws.Cell(row, BulkImportEmployeeWorkbook.ColumnIndex(true, "תאריך_לידה")).Value = new DateTime(1970, 8, 25);
                ws.Cell(row, BulkImportEmployeeWorkbook.ColumnIndex(true, "תאריך_לידה_ילד_1")).Value = new DateTime(2015, 3, 15);
            });

        var result = await ImportAsync(db, stream);

        Assert.Equal(1, result.Imported);
        var emp = await db.Employees.SingleAsync(e => e.IdNumber == "666777888");
        Assert.Equal(new DateOnly(2015, 3, 15), emp.ChildBirthDate1);
    }

    [Fact]
    public async Task ImportEmployees_ParsesDdMmYyyyText_UnderHeIlCulture()
    {
        using var _ = HeIlCultureScope.Create();
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db, "HeIL Date Employer");
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "G-1");

        await using var stream = BulkImportEmployeeWorkbook.CreateMinimalRow(
            employer.Name, "777888999", Year, fillRow: (ws, row) =>
            {
                ws.Cell(row, BulkImportEmployeeWorkbook.ColumnIndex(true, "תאריך_לידה")).Value = "25/08/1970";
                ws.Cell(row, BulkImportEmployeeWorkbook.ColumnIndex(true, "תאריך_לידה_ילד_1")).Value = "15/03/2015";
            });

        var result = await ImportAsync(db, stream);

        Assert.Equal(1, result.Imported);
        var emp = await db.Employees.SingleAsync(e => e.IdNumber == "777888999");
        Assert.Equal(new DateOnly(1970, 8, 25), emp.BirthDate);
        Assert.Equal(new DateOnly(2015, 3, 15), emp.ChildBirthDate1);
    }

    private static async Task<ImportResult> ImportAsync(PayrollDbContext db, Stream stream)
    {
        stream.Position = 0;
        var file = new FormFile(stream, 0, stream.Length, "file", "import.xlsx")
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        };
        var sut = ServiceTestFactory.CreateBulkImportService(db);
        return await sut.ImportEmployeesAsync(file);
    }

    private sealed class EnUsCultureScope : IDisposable
    {
        private readonly CultureInfo _previousCulture;
        private readonly CultureInfo _previousUiCulture;

        private EnUsCultureScope(CultureInfo previousCulture, CultureInfo previousUiCulture)
        {
            _previousCulture = previousCulture;
            _previousUiCulture = previousUiCulture;
        }

        public static EnUsCultureScope Create()
        {
            var previousCulture = CultureInfo.CurrentCulture;
            var previousUiCulture = CultureInfo.CurrentUICulture;
            var enUs = new CultureInfo("en-US");
            CultureInfo.CurrentCulture = enUs;
            CultureInfo.CurrentUICulture = enUs;
            return new EnUsCultureScope(previousCulture, previousUiCulture);
        }

        public void Dispose()
        {
            CultureInfo.CurrentCulture = _previousCulture;
            CultureInfo.CurrentUICulture = _previousUiCulture;
        }
    }

    private sealed class HeIlCultureScope : IDisposable
    {
        private readonly CultureInfo _previousCulture;
        private readonly CultureInfo _previousUiCulture;

        private HeIlCultureScope(CultureInfo previousCulture, CultureInfo previousUiCulture)
        {
            _previousCulture = previousCulture;
            _previousUiCulture = previousUiCulture;
        }

        public static HeIlCultureScope Create()
        {
            var previousCulture = CultureInfo.CurrentCulture;
            var previousUiCulture = CultureInfo.CurrentUICulture;
            var heIl = new CultureInfo("he-IL");
            CultureInfo.CurrentCulture = heIl;
            CultureInfo.CurrentUICulture = heIl;
            return new HeIlCultureScope(previousCulture, previousUiCulture);
        }

        public void Dispose()
        {
            CultureInfo.CurrentCulture = _previousCulture;
            CultureInfo.CurrentUICulture = _previousUiCulture;
        }
    }
}
