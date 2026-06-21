using AccountingProject.Contracts;
using AccountingProject.Data;
using AccountingProject.Domain;
using AccountingProject.Infrastructure;
using AccountingProject.Models;
using AccountingProject.Services;
using AccountingProject.Tests.TestHelpers;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;

namespace AccountingProject.Tests;

public sealed class AnnualComparisonSavedReportOverrideTests
{
    private const string SheetName = "השוואה שנתית";
    private const string Year = "תשפ\"ו";
    private const int Month = 9;
    private const int GregorianYear = 2025;
    private const int FirstMonthColumn = 11;

    [Fact]
    public async Task Preview_ReturnsComputedAndDisplay_ForSavedInput()
    {
        await using var db = DbTestFactory.CreateContext();
        var employerId = await SeedWithSeptemberImportAsync(db);

        var slotId = await db.EmploymentDataSlots.Select(s => s.Id).SingleAsync();
        var preview = await new AnnualComparisonSavedReportService(db).GetPreviewAsync(employerId, Year);

        Assert.Equal(Year, preview.AcademicYear);
        Assert.Equal(12, preview.MonthHeaders.Count);
        Assert.Single(preview.Rows);
        var row = preview.Rows[0];
        Assert.Equal(slotId, row.SlotId);
        Assert.Equal("Worker Import", row.FullName.Display);
        Assert.Equal("Worker Import", row.FullName.Computed);
        Assert.False(row.FullName.IsOverridden);
        Assert.Equal("V", row.MonthCells["9.2025"].Display);
    }

    [Fact]
    public async Task SaveOverride_PreviewAndExport_ReflectDisplayValue()
    {
        await using var db = DbTestFactory.CreateContext();
        var employerId = await SeedWithSeptemberImportAsync(db);
        var slotId = await db.EmploymentDataSlots.Select(s => s.Id).SingleAsync();

        var service = new AnnualComparisonSavedReportService(db);
        await service.SaveOverridesAsync(employerId, Year,
        [
            new AnnualComparisonOverrideRowSaveDto
            {
                SlotId = slotId,
                FullName = "שם מותאם לדוח",
                MonthCells = new Dictionary<string, string> { ["9.2025"] = "V ידני" },
            },
        ]);

        var preview = await service.GetPreviewAsync(employerId, Year);
        var row = preview.Rows[0];
        Assert.True(row.FullName.IsOverridden);
        Assert.Equal("שם מותאם לדוח", row.FullName.Display);
        Assert.Equal("Worker Import", row.FullName.Computed);
        Assert.True(row.MonthCells["9.2025"].IsOverridden);
        Assert.Equal("V ידני", row.MonthCells["9.2025"].Display);

        var bytes = await new ReportExportService(db).AnnualComparisonFromSavedDataAsync(employerId, Year);
        using var wb = new XLWorkbook(new MemoryStream(bytes));
        var ws = wb.Worksheet(SheetName);
        Assert.Equal("שם מותאם לדוח", ws.Cell(2, 2).GetString());
        Assert.Equal("V ידני", ws.Cell(2, FirstMonthColumn).GetString());
    }

    [Fact]
    public async Task ClearOverride_RestoresComputed_InPreviewAndExport()
    {
        await using var db = DbTestFactory.CreateContext();
        var employerId = await SeedWithSeptemberImportAsync(db);
        var slotId = await db.EmploymentDataSlots.Select(s => s.Id).SingleAsync();

        var service = new AnnualComparisonSavedReportService(db);
        await service.SaveOverridesAsync(employerId, Year,
        [
            new AnnualComparisonOverrideRowSaveDto
            {
                SlotId = slotId,
                Role = "תפקיד מותאם",
            },
        ]);
        Assert.Single(await db.AnnualComparisonReportRowOverrides.ToListAsync());

        await service.ClearOverridesAsync(employerId, Year, slotId);
        Assert.Empty(await db.AnnualComparisonReportRowOverrides.ToListAsync());

        var preview = await service.GetPreviewAsync(employerId, Year);
        Assert.False(preview.Rows[0].Role.IsOverridden);
        Assert.Equal("גננת", preview.Rows[0].Role.Display);

        var bytes = await new ReportExportService(db).AnnualComparisonFromSavedDataAsync(employerId, Year);
        using var wb = new XLWorkbook(new MemoryStream(bytes));
        Assert.Equal("גננת", wb.Worksheet(SheetName).Cell(2, 3).GetString());
    }

    [Fact]
    public async Task SaveMatchingComputed_RemovesExistingOverride()
    {
        await using var db = DbTestFactory.CreateContext();
        var employerId = await SeedWithSeptemberImportAsync(db);
        var slotId = await db.EmploymentDataSlots.Select(s => s.Id).SingleAsync();
        var service = new AnnualComparisonSavedReportService(db);

        await service.SaveOverridesAsync(employerId, Year,
        [
            new AnnualComparisonOverrideRowSaveDto { SlotId = slotId, FullName = "שם זמני" },
        ]);
        Assert.Single(await db.AnnualComparisonReportRowOverrides.ToListAsync());

        var preview = await service.GetPreviewAsync(employerId, Year);
        var row = preview.Rows[0];
        await service.SaveOverridesAsync(employerId, Year,
        [
            new AnnualComparisonOverrideRowSaveDto
            {
                SlotId = slotId,
                FullName = row.FullName.Computed,
                Role = row.Role.Computed,
                InstitutionSymbol = row.InstitutionSymbol.Computed,
                SugMisraFromPayroll = row.SugMisraFromPayroll.Computed,
                Grade = row.Grade.Computed,
                Seniority = row.Seniority.Computed,
                WeeklyHours = decimal.TryParse(row.WeeklyHours.Computed, out var wh) ? wh : null,
                JobBase = decimal.TryParse(row.JobBase.Computed, out var jb) ? jb : null,
                JobPercent = decimal.TryParse(row.JobPercent.Computed, out var jp) ? jp : null,
                DoubleGeneral = decimal.TryParse(row.DoubleGeneral.Computed, out var dg) ? dg : null,
                MonthCells = row.MonthCells.ToDictionary(
                    kv => kv.Key,
                    kv => kv.Value.Computed ?? ""),
            },
        ]);

        Assert.Empty(await db.AnnualComparisonReportRowOverrides.ToListAsync());
    }

    private static async Task<int> SeedWithSeptemberImportAsync(PayrollDbContext db)
    {
        var employer = await ReportTestData.SeedEmployerAsync(db);
        var employee = await ReportTestData.SeedEmployeeAsync(
            db, employer.Id, "123456789", "Import", "Worker");
        var ed = await ReportTestData.SeedEmploymentWithSlotAsync(
            db, employer.Id, employee.Id, "SYM-1", grade1Role: "גננת", weeklyHours: 30m);
        ed.Grade1JobPercent = 100m;
        await db.SaveChangesAsync();

        var importService = new PayrollMonthlyInputService(db, new TestCurrentUser());
        await using var upload = MonthlyComparisonUploadWorkbook.Create(
            "123456789",
            1001,
            "Import Worker",
            Month,
            GregorianYear,
            b => b.Band1(misra1Hours: 30m, misra1Base: 30m, jobPercent: 100m));
        await importService.ImportMonthAsync(employer.Id, Year, Month, upload, "sept.xlsx");

        return employer.Id;
    }

    private sealed class TestCurrentUser : ICurrentUserService
    {
        public string? UserId => "1";
        public string? Username => "override-test";
        public string? Role => UserRoles.Admin;
        public string GetAuditActor() => "override-test";
    }
}
