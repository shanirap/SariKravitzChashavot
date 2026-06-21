using AccountingProject.Contracts;
using AccountingProject.Domain;
using AccountingProject.Models;
using AccountingProject.Services;
using AccountingProject.Tests.TestHelpers;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace AccountingProject.Tests;

/// <summary>
/// Additional edge-case coverage for centralized employment calculations (import + manual save).
/// </summary>
public sealed class EmploymentCalculationEdgeCaseTests
{
    private const string Year = "תשפ\"ו";
    private static readonly DateOnly RefDate = HebrewAcademicYear.GetSchoolYearStartDate(Year);

    private static EmploymentCalculationService Calc => ServiceTestFactory.CreateEmploymentCalculations();

    // ── UpdateAsync / persistence ───────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ChangingRoleFromQualifying_RemovesSupplementaryFromDb()
    {
        await using var db = DbTestFactory.CreateContext();
        var (employer, employee, symbol) = await SeedAsync(db);
        var sut = ServiceTestFactory.CreateEmploymentDataService(db);

        var createDto = Dto(employee, employer, symbol, "יסודי וגנים", "גננת ראשית", 20m);
        var (created, err) = await sut.CreateAsync(createDto);
        Assert.Null(err);
        await db.Entry(created!).Collection(r => r.Slots).LoadAsync();
        Assert.Contains(created!.Slots, s => s.SupplementaryParentSlotIndex == 1);

        var updateDto = Dto(employee, employer, symbol, "יסודי וגנים", "מורה מקצועי", 20m);
        updateDto.Grade1AgeHours = created.Grade1AgeHours;
        var (updated, err2) = await sut.UpdateAsync(created.Id, updateDto);
        Assert.Null(err2);
        await db.Entry(updated!).Collection(r => r.Slots).LoadAsync();
        Assert.DoesNotContain(updated!.Slots, s => s.SupplementaryParentSlotIndex.HasValue);
    }

    [Fact]
    public async Task UpdateAsync_ClearingParentSlot_RemovesSupplementaryOnSave()
    {
        await using var db = DbTestFactory.CreateContext();
        var (employer, employee, symbol) = await SeedAsync(db);
        var sut = ServiceTestFactory.CreateEmploymentDataService(db);

        var (created, err) = await sut.CreateAsync(Dto(employee, employer, symbol, "יסודי וגנים", "גננת ראשית", 20m));
        Assert.Null(err);

        var updateDto = Dto(employee, employer, symbol, "יסודי וגנים", "גננת ראשית", 0m);
        updateDto.Slots =
        [
            new EmploymentDataSlotDto
            {
                GradeBand = 1,
                SlotIndex = 1,
                InstitutionSymbol = null,
                WeeklyHours = null,
                JobBase = null,
            },
        ];
        var (updated, err2) = await sut.UpdateAsync(created!.Id, updateDto);
        Assert.Null(err2);
        await db.Entry(updated!).Collection(r => r.Slots).LoadAsync();
        Assert.DoesNotContain(updated!.Slots, s => s.SupplementaryParentSlotIndex.HasValue);
        Assert.Null(updated.Grade1Total);
    }

    [Fact]
    public async Task UpdateAsync_ChangingWeeklyHours_RecalculatesTotalIncludingSupplementary()
    {
        await using var db = DbTestFactory.CreateContext();
        var (employer, employee, symbol) = await SeedAsync(db);
        var sut = ServiceTestFactory.CreateEmploymentDataService(db);

        var (created, err) = await sut.CreateAsync(Dto(employee, employer, symbol, "יסודי וגנים", "גננת ראשית", 20m));
        Assert.Null(err);
        Assert.Equal(23m, created!.Grade1Total);

        var updateDto = Dto(employee, employer, symbol, "יסודי וגנים", "גננת ראשית", 25m);
        var (updated, err2) = await sut.UpdateAsync(created.Id, updateDto);
        Assert.Null(err2);
        Assert.Equal(28m, updated!.Grade1Total);
    }

    // ── Multi-slot supplementary sync ─────────────────────────────────────────

    [Fact]
    public void PrepareForSave_TwoParentSlots_CreatesSupplementaryOnSlots2And4()
    {
        var dto = QualifyingBand1Dto("גננת ראשית");
        SetSlot(dto, 1, 1, "SYM-A", 10m, 30m);
        SetSlot(dto, 1, 3, "SYM-B", 8m, 30m);

        Calc.PrepareForSave(dto, new DateOnly(1990, 1, 1), false, []);

        Assert.Equal(24m, dto.Grade1Total);
        var slot2 = dto.Slots!.Single(s => s.GradeBand == 1 && s.SlotIndex == 2);
        var slot4 = dto.Slots!.Single(s => s.GradeBand == 1 && s.SlotIndex == 4);
        Assert.Equal((byte)1, slot2.SupplementaryParentSlotIndex);
        Assert.Equal((byte)3, slot4.SupplementaryParentSlotIndex);
        Assert.Equal(3m, slot2.WeeklyHours);
        Assert.Equal(3m, slot4.WeeklyHours);
    }

    [Fact]
    public void PrepareForSave_Band2Qualifying_AddsSupplementaryOnBand2Only()
    {
        var dto = EmptyDto();
        dto.Grade1GradeName = "יסודי וגנים";
        dto.Grade1Role = "מורה מקצועי";
        dto.Grade2GradeName = "עוז לתמורה";
        dto.Grade2Role = "מורה מחנך";
        dto.Grade2Grade = "ב";
        dto.Grade2Seniority = "1";
        SetSlot(dto, 1, 1, "G1", 20m, 30m);
        SetSlot(dto, 2, 1, "G2", 18m, 38m);

        Calc.PrepareForSave(dto, new DateOnly(1990, 1, 1), false, []);

        Assert.Null(dto.Slots!.Single(s => s.GradeBand == 1 && s.SlotIndex == 2).SupplementaryParentSlotIndex);
        var band2Child = dto.Slots.Single(s => s.GradeBand == 2 && s.SlotIndex == 2);
        Assert.Equal((byte)1, band2Child.SupplementaryParentSlotIndex);
        Assert.Equal(21m, dto.Grade2Total);
        Assert.Equal(20m, dto.Grade1Total);
    }

    [Fact]
    public void PrepareForSave_BothBandsQualifying_IndependentSupplementaryAndTotals()
    {
        var dto = EmptyDto();
        dto.Grade1GradeName = "יסודי וגנים";
        dto.Grade1Role = "גננת ראשית";
        dto.Grade1Grade = "ב";
        dto.Grade1Seniority = "1";
        dto.Grade2GradeName = "עוז לתמורה";
        dto.Grade2Role = "מורה מחנך";
        dto.Grade2Grade = "ב";
        dto.Grade2Seniority = "1";
        SetSlot(dto, 1, 1, "G1", 10m, 30m);
        SetSlot(dto, 2, 1, "G2", 15m, 38m);

        Calc.PrepareForSave(dto, new DateOnly(1990, 1, 1), false, []);

        Assert.Equal(13m, dto.Grade1Total);
        Assert.Equal(18m, dto.Grade2Total);
        Assert.Equal((byte)1, dto.Slots!.Single(s => s.GradeBand == 1 && s.SlotIndex == 2).SupplementaryParentSlotIndex);
        Assert.Equal((byte)1, dto.Slots.Single(s => s.GradeBand == 2 && s.SlotIndex == 2).SupplementaryParentSlotIndex);
    }

    [Fact]
    public void Sync_SupplementaryParent_DoesNotCreateNestedSupplementary()
    {
        var dto = QualifyingBand1Dto("גננת ראשית");
        SetSlot(dto, 1, 1, "SYM", 10m, 30m);
        SetSlot(dto, 1, 2, "SYM", 3m, 30m, supplementaryParent: 1);
        SetSlot(dto, 1, 3, "SYM-B", 8m, 30m);

        TeacherSupplementarySlotSync.Sync(dto);

        var slot4 = dto.Slots!.Single(s => s.GradeBand == 1 && s.SlotIndex == 4);
        Assert.Equal((byte)3, slot4.SupplementaryParentSlotIndex);
        Assert.Equal(3m, slot4.WeeklyHours);
    }

    [Fact]
    public void Sync_Slot6Standalone_DoesNotCreateSlot7()
    {
        var dto = QualifyingBand1Dto("גננת ראשית");
        SetSlot(dto, 1, 6, "SYM", 12m, 30m);

        TeacherSupplementarySlotSync.Sync(dto);

        Assert.Equal(12m, dto.Slots!.Single(s => s.GradeBand == 1 && s.SlotIndex == 6).WeeklyHours);
        Assert.Null(dto.Slots.Single(s => s.GradeBand == 1 && s.SlotIndex == 6).SupplementaryParentSlotIndex);
        Assert.Equal(6, dto.Slots.Count(s => s.GradeBand == 1));
    }

    [Fact]
    public void Sync_Slot5Parent_CreatesSlot6Supplementary_NoSlotBeyond6()
    {
        var dto = QualifyingBand1Dto("גננת ראשית");
        SetSlot(dto, 1, 5, "SYM", 15m, 30m);

        TeacherSupplementarySlotSync.Sync(dto);

        var slot6 = dto.Slots!.Single(s => s.GradeBand == 1 && s.SlotIndex == 6);
        Assert.Equal((byte)5, slot6.SupplementaryParentSlotIndex);
        Assert.Equal(3m, slot6.WeeklyHours);
    }

    // ── Age hours boundaries ──────────────────────────────────────────────────

    [Theory]
    [InlineData(1976, 9, 1, 0)]  // age 49
    [InlineData(1975, 9, 1, 2)]  // age 50
    [InlineData(1970, 9, 1, 4)]  // age 55
    public void AgeHours_BoundaryAtRefDate(int year, int month, int day, decimal expected)
    {
        var result = EmploymentAgeHoursDefaults.Compute(new DateOnly(year, month, day), RefDate);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void PrepareForSave_NullBirthDate_AgeHoursRemainNull()
    {
        var dto = BaseDto("יסודי וגנים", "מורה מקצועי", "SYM", 20m, 30m);
        Calc.PrepareForSave(dto, null, false, []);
        Assert.Null(dto.Grade1AgeHours);
        Assert.Null(dto.Grade2AgeHours);
    }

    [Fact]
    public async Task CreateAsync_Grade1AgeHoursExplicit_Grade2ComputedIndependently()
    {
        await using var db = DbTestFactory.CreateContext();
        var (employer, employee, symbol) = await SeedAsync(db);
        employee.BirthDate = new DateOnly(1973, 1, 15);
        await db.SaveChangesAsync();

        var sut = ServiceTestFactory.CreateEmploymentDataService(db);
        var dto = Dto(employee, employer, symbol, "יסודי וגנים", "מורה מקצועי", 20m);
        dto.Grade1AgeHours = 5m;
        dto.Grade2GradeName = "עוז לתמורה";
        dto.Grade2Grade = "ב";
        dto.Grade2Role = "מורה מקצועי";
        dto.Grade2Seniority = "1";
        dto.Slots!.Add(new EmploymentDataSlotDto
        {
            GradeBand = 2,
            SlotIndex = 1,
            InstitutionSymbol = symbol,
            WeeklyHours = 10m,
            JobBase = 30m,
        });

        var (record, err) = await sut.CreateAsync(dto);
        Assert.Null(err);
        Assert.Equal(5m, record!.Grade1AgeHours);
        Assert.Equal(2m, record.Grade2AgeHours);
    }

    // ── Mother benefit & training fund thresholds ─────────────────────────────

    [Fact]
    public void MotherBenefit_BaseJobPercentExactly79_IsZero()
    {
        var dto = BaseDto("יסודי וגנים", "מורה מקצועי", "SYM", 23.7m, 30m);
        dto.Grade1AgeHours = 0m;
        var childBirth = new DateOnly(2015, 1, 1);
        Calc.PrepareForSave(dto, new DateOnly(1990, 1, 1), isFemaleEmployee: true, [childBirth]);
        Assert.Equal(79m, dto.Grade1JobPercent);
        Assert.Equal(0m, dto.Grade1MotherBenefitPercent);
    }

    [Fact]
    public void MotherBenefit_BaseJobPercentJustAbove79_AppliesRate()
    {
        var dto = BaseDto("יסודי וגנים", "מורה מקצועי", "SYM", 23.71m, 30m);
        dto.Grade1AgeHours = 0m;
        var childBirth = new DateOnly(2015, 1, 1);
        Calc.PrepareForSave(dto, new DateOnly(1990, 1, 1), isFemaleEmployee: true, [childBirth]);
        Assert.True(dto.Grade1JobPercent > 79m);
        Assert.Equal(10m, dto.Grade1MotherBenefitPercent);
    }

    [Fact]
    public void TrainingFund_JobPercentJustBelowOneThird_IsZero()
    {
        var dto = BaseDto("יסודי וגנים", "מורה מקצועי", "SYM", 9.99m, 30m);
        dto.Grade1AgeHours = 0m;
        Calc.PrepareForSave(dto, new DateOnly(1990, 1, 1), false, []);
        Assert.Equal(33.3m, dto.Grade1JobPercent);
        Assert.Equal(0m, dto.Grade1TrainingFundPercent);
    }

    [Fact]
    public void TrainingFund_JobPercentAtOneThird_Is84()
    {
        var dto = BaseDto("יסודי וגנים", "מורה מקצועי", "SYM", 10.01m, 30m);
        dto.Grade1AgeHours = 0m;
        Calc.PrepareForSave(dto, new DateOnly(1990, 1, 1), false, []);
        Assert.True(dto.Grade1JobPercent!.Value >= 100m / 3m);
        Assert.Equal(8.4m, dto.Grade1TrainingFundPercent);
    }

    [Fact]
    public async Task CreateAsync_LegacyAhidGrade_NormalizesAndCalculatesTrainingFund()
    {
        await using var db = DbTestFactory.CreateContext();
        var (employer, employee, symbol) = await SeedAsync(db);
        var sut = ServiceTestFactory.CreateEmploymentDataService(db);
        var dto = Dto(employee, employer, symbol, GradeOptions.LegacyUnifiedGradeName, "סייעת ראשית", 30m);
        dto.Grade1Grade = "תומכת חינוך";
        dto.Grade1Seniority = "2";

        var (record, err) = await sut.CreateAsync(dto);
        Assert.Null(err);
        Assert.Equal(GradeOptions.UnifiedEducationSupportGradeName, record!.Grade1GradeName);
        Assert.Equal(7.5m, record.Grade1TrainingFundPercent);
    }

    [Fact]
    public async Task Import_LegacyAhidGrade_NormalizesAndCalculatesTrainingFund()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db, "Legacy Ahid Import");
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "AH-1");

        await using var stream = ImportRow(employer.Name, "181818181", (ws, row, map) =>
        {
            ws.Cell(row, map("דרגה1_שם_הדירוג")).Value = GradeOptions.LegacyUnifiedGradeName;
            ws.Cell(row, map("דרגה1_דרגה")).Value = "תומכת חינוך";
            ws.Cell(row, map("דרגה1_תפקיד")).Value = "סייעת ראשית";
            ws.Cell(row, map("דרגה1_ותק")).Value = 2;
            ws.Cell(row, map("דרגה1_1_סמל_מוסד")).Value = "AH-1";
            ws.Cell(row, map("דרגה1_1_שעות_שבועיות")).Value = 30m;
        });

        var result = await ServiceTestFactory.CreateBulkImportService(db)
            .ImportEmployeesAsync(FormFile(stream, "ahid.xlsx"));
        Assert.Equal(1, result.Imported);

        var ed = await db.EmploymentData.SingleAsync(e => e.AcademicYear == Year);
        Assert.Equal(GradeOptions.UnifiedEducationSupportGradeName, ed.Grade1GradeName);
        Assert.Equal(7.5m, ed.Grade1TrainingFundPercent);
    }

    [Fact]
    public async Task Import_ExplicitJobBaseInExcel_UsedInJobPercentCalculation()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db, "Custom Job Base");
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "JB-1");

        await using var stream = ImportRow(employer.Name, "333444555", (ws, row, map) =>
        {
            ws.Cell(row, map("דרגה1_שם_הדירוג")).Value = "יסודי וגנים";
            ws.Cell(row, map("דרגה1_דרגה")).Value = "ב";
            ws.Cell(row, map("דרגה1_תפקיד")).Value = "מורה מקצועי";
            ws.Cell(row, map("דרגה1_שעות_גיל")).Value = 0;
            ws.Cell(row, map("דרגה1_1_סמל_מוסד")).Value = "JB-1";
            ws.Cell(row, map("דרגה1_1_שעות_שבועיות")).Value = 20m;
            ws.Cell(row, map("דרגה1_1_בסיס_משרה")).Value = 25m;
        });

        var result = await ServiceTestFactory.CreateBulkImportService(db)
            .ImportEmployeesAsync(FormFile(stream, "job-base.xlsx"));
        Assert.Equal(1, result.Imported);

        var ed = await db.EmploymentData.SingleAsync(e => e.AcademicYear == Year);
        Assert.Equal(80m, ed.Grade1JobPercent);
        Assert.Equal(25m, (await db.EmploymentDataSlots.SingleAsync(s => s.SlotIndex == 1)).JobBase);
    }

    // ── Bulk import edge cases ────────────────────────────────────────────────

    [Fact]
    public async Task Import_ReimportSameYear_RejectedAndLeavesOriginalData()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db, "Reimport Employer");
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "RE-1");
        const string idNumber = "121212121";

        await using var first = ImportRow(employer.Name, idNumber, FillGanenetRashit("RE-1", 20.5m));
        var sut = ServiceTestFactory.CreateBulkImportService(db);
        Assert.Equal(1, (await sut.ImportEmployeesAsync(FormFile(first, "first.xlsx"))).Imported);

        var originalTotal = await db.EmploymentData.Where(e => e.AcademicYear == Year).Select(e => e.Grade1Total).SingleAsync();

        await using var second = ImportRow(employer.Name, idNumber, FillGanenetRashit("RE-1", 28m));
        var secondResult = await sut.ImportEmployeesAsync(FormFile(second, "second.xlsx"));

        Assert.Equal(0, secondResult.Imported);
        Assert.Equal(1, secondResult.Errors);
        Assert.Contains("כבר קיימת רשומה", secondResult.Rows[0].Message);
        Assert.Equal(originalTotal, await db.EmploymentData.Where(e => e.AcademicYear == Year).Select(e => e.Grade1Total).SingleAsync());
    }

    [Fact]
    public async Task Import_DuplicateRowInSameFile_SecondRowRejected()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db, "Dup File Employer");
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "DUP-1");

        await using var stream = CreateTwoRowImport(
            (employer.Name, "131313131", FillGanenetRashit("DUP-1", 20m)),
            (employer.Name, "131313131", FillGanenetRashit("DUP-1", 25m)));

        var result = await ServiceTestFactory.CreateBulkImportService(db)
            .ImportEmployeesAsync(FormFile(stream, "dup.xlsx"));

        Assert.Equal(1, result.Imported);
        Assert.Equal(1, result.Errors);
        Assert.Contains("כבר קיימת בקובץ", result.Rows[1].Message);
    }

    [Fact]
    public async Task Import_ExcelWrongSupplementaryRow_OverwrittenBySync()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db, "Wrong Supp");
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "WS-1");

        await using var stream = ImportRow(employer.Name, "141414141", (ws, row, map) =>
        {
            FillGanenetRashit("WS-1", 20.5m)(ws, row, map);
            ws.Cell(row, map("דרגה1_2_סמל_מוסד")).Value = "WS-1";
            ws.Cell(row, map("דרגה1_2_שעות_שבועיות")).Value = 99m;
            ws.Cell(row, map("דרגה1_2_בסיס_משרה")).Value = 99m;
        });

        var result = await ServiceTestFactory.CreateBulkImportService(db)
            .ImportEmployeesAsync(FormFile(stream, "wrong-supp.xlsx"));
        Assert.Equal(1, result.Imported);

        var slots = await db.EmploymentData
            .Include(e => e.Slots)
            .SelectMany(e => e.Slots)
            .Where(s => s.GradeBand == 1 && s.SlotIndex == 2)
            .ToListAsync();
        var supplementary = Assert.Single(slots);
        Assert.Equal("WS-1", supplementary.InstitutionSymbol);
        Assert.Equal(3m, supplementary.WeeklyHours);
        Assert.Equal(30m, supplementary.JobBase);
    }

    [Fact]
    public async Task Import_ViaEmployerId_ComputesSupplementaryAndTotals()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db, "Scoped Import");
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "SC-1");

        await using var stream = ImportRowForEmployer("151515151", FillGanenetRashit("SC-1", 20.5m),
            birthDate: new DateOnly(1973, 1, 15));

        var result = await ServiceTestFactory.CreateBulkImportService(db)
            .ImportEmployeesAsync(FormFile(stream, "scoped.xlsx"), employer.Id);
        Assert.Equal(1, result.Imported);

        var ed = await db.EmploymentData.Include(e => e.Slots).SingleAsync(e => e.AcademicYear == Year);
        Assert.Equal(23.50m, ed.Grade1Total);
        Assert.Equal(2m, ed.Grade1AgeHours);
        Assert.Contains(ed.Slots, s => s.SlotIndex == 2 && s.SupplementaryParentSlotIndex == 1);
    }

    [Fact]
    public async Task Import_MixedFile_ValidRowGetsCalculationsInvalidRowFails()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db, "Mixed Calc");
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "OK-1");

        await using var stream = CreateTwoRowImport(
            (employer.Name, "161616161", FillGanenetRashit("OK-1", 20.5m)),
            (employer.Name, "171717171", (ws, row, map) =>
            {
                ws.Cell(row, map("דרגה1_1_סמל_מוסד")).Value = "BAD-SYM";
                ws.Cell(row, map("דרגה1_1_שעות_שבועיות")).Value = 20m;
            }));

        var result = await ServiceTestFactory.CreateBulkImportService(db)
            .ImportEmployeesAsync(FormFile(stream, "mixed-calc.xlsx"));

        Assert.Equal(1, result.Imported);
        Assert.Equal(1, result.Errors);
        var ed = await db.EmploymentData.Include(e => e.Slots).SingleAsync();
        Assert.Equal(23.50m, ed.Grade1Total);
        Assert.Contains(ed.Slots, s => s.SlotIndex == 2 && s.WeeklyHours == 3m);
        Assert.False(await db.Employees.AnyAsync(e => e.IdNumber == "171717171"));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Action<IXLWorksheet, int, Func<string, int>> FillGanenetRashit(string symbol, decimal hours) =>
        (ws, row, map) =>
        {
            ws.Cell(row, map("דרגה1_שם_הדירוג")).Value = "יסודי וגנים";
            ws.Cell(row, map("דרגה1_דרגה")).Value = "ב";
            ws.Cell(row, map("דרגה1_תפקיד")).Value = "גננת ראשית";
            ws.Cell(row, map("דרגה1_ותק")).Value = 1;
            ws.Cell(row, map("דרגה1_1_סמל_מוסד")).Value = symbol;
            ws.Cell(row, map("דרגה1_1_שעות_שבועיות")).Value = hours;
            ws.Cell(row, map("דרגה1_1_בסיס_משרה")).Value = 30;
        };

    private static EmploymentDataDto QualifyingBand1Dto(string role)
    {
        var dto = EmptyDto();
        dto.Grade1GradeName = "יסודי וגנים";
        dto.Grade1Role = role;
        dto.Grade1Grade = "ב";
        dto.Grade1Seniority = "1";
        return dto;
    }

    private static EmploymentDataDto BaseDto(
        string gradeName,
        string role,
        string? symbol,
        decimal? hours,
        decimal? jobBase)
    {
        var dto = EmptyDto();
        dto.Grade1GradeName = gradeName;
        dto.Grade1Grade = "ב";
        dto.Grade1Role = role;
        dto.Grade1Seniority = "1";
        if (symbol != null || hours != null || jobBase != null)
            SetSlot(dto, 1, 1, symbol, hours, jobBase);
        return dto;
    }

    private static EmploymentDataDto EmptyDto() => new()
    {
        AcademicYear = Year,
        Slots = [],
    };

    private static void SetSlot(
        EmploymentDataDto dto,
        int band,
        int slotIndex,
        string? symbol,
        decimal? hours,
        decimal? jobBase,
        int? supplementaryParent = null)
    {
        dto.Slots ??= [];
        dto.Slots.Add(new EmploymentDataSlotDto
        {
            GradeBand = band,
            SlotIndex = slotIndex,
            InstitutionSymbol = symbol,
            WeeklyHours = hours,
            JobBase = jobBase,
            SupplementaryParentSlotIndex = supplementaryParent,
        });
    }

    private static EmploymentDataDto Dto(
        Employee employee,
        Employer employer,
        string symbol,
        string gradeName,
        string role,
        decimal hours) =>
        new()
        {
            EmployeeId = employee.Id,
            EmployerId = employer.Id,
            AcademicYear = Year,
            Grade1GradeName = gradeName,
            Grade1Grade = "ב",
            Grade1Role = role,
            Grade1Seniority = "1",
            Slots =
            [
                new EmploymentDataSlotDto
                {
                    GradeBand = 1,
                    SlotIndex = 1,
                    InstitutionSymbol = symbol,
                    WeeklyHours = hours,
                    JobBase = 30m,
                },
            ],
        };

    private static async Task<(Employer employer, Employee employee, string symbol)> SeedAsync(PayrollDbContext db)
    {
        var employer = await ReportTestData.SeedEmployerAsync(db, $"Edge-{Guid.NewGuid():N}"[..12]);
        const string symbol = "EDGE-SYM";
        await ReportTestData.SeedSymbolAsync(db, employer.Id, symbol);
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id, Guid.NewGuid().ToString("N")[..9]);
        return (employer, employee, symbol);
    }

    private static MemoryStream ImportRow(
        string employerName,
        string idNumber,
        Action<IXLWorksheet, int, Func<string, int>> fill,
        DateOnly? birthDate = null) =>
        BuildImportWorkbook(includeEmployer: true, employerName, idNumber, fill, birthDate);

    private static MemoryStream ImportRowForEmployer(
        string idNumber,
        Action<IXLWorksheet, int, Func<string, int>> fill,
        DateOnly? birthDate = null) =>
        BuildImportWorkbook(includeEmployer: false, null, idNumber, fill, birthDate);

    private static MemoryStream CreateTwoRowImport(
        (string Employer, string IdNumber, Action<IXLWorksheet, int, Func<string, int>> Fill) row1,
        (string Employer, string IdNumber, Action<IXLWorksheet, int, Func<string, int>> Fill) row2)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("עובדים");
        var headers = BuildImportHeaders(includeEmployer: true);
        WriteHeaderRow(ws, headers);
        WriteImportDataRow(ws, 2, headers, row1.Employer, row1.IdNumber, row1.Fill);
        WriteImportDataRow(ws, 3, headers, row2.Employer, row2.IdNumber, row2.Fill);
        return SaveWorkbook(wb);
    }

    private static MemoryStream BuildImportWorkbook(
        bool includeEmployer,
        string? employerName,
        string idNumber,
        Action<IXLWorksheet, int, Func<string, int>> fill,
        DateOnly? birthDate)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("עובדים");
        var headers = BuildImportHeaders(includeEmployer);
        WriteHeaderRow(ws, headers);
        WriteImportDataRow(ws, 2, headers, employerName, idNumber, fill, birthDate);
        return SaveWorkbook(wb);
    }

    private static void WriteImportDataRow(
        IXLWorksheet ws,
        int row,
        List<string> headers,
        string? employerName,
        string idNumber,
        Action<IXLWorksheet, int, Func<string, int>> fill,
        DateOnly? birthDate = null)
    {
        int Map(string h)
        {
            var index = headers.IndexOf(h);
            if (index < 0)
                throw new InvalidOperationException($"Missing header {h}");
            return index + 1;
        }

        var col = 1;
        if (headers[0] == "שם_מעסיק")
        {
            ws.Cell(row, col++).Value = employerName ?? "";
            col++; // חפ
        }

        ws.Cell(row, col++).Value = idNumber;
        col++; // מספר_עובד
        ws.Cell(row, col++).Value = "כהן";
        ws.Cell(row, col++).Value = "רחל";
        ws.Cell(row, col++).Value = "נקבה";
        ws.Cell(row, col++).Value = (birthDate ?? new DateOnly(1990, 1, 15)).ToString("yyyy-MM-dd");
        col += 11; // טל + children
        ws.Cell(row, col++).Value = Year;
        fill(ws, row, Map);
    }

    private static void WriteHeaderRow(IXLWorksheet ws, List<string> headers)
    {
        for (var i = 0; i < headers.Count; i++)
            ws.Cell(1, i + 1).Value = headers[i];
    }

    private static MemoryStream SaveWorkbook(XLWorkbook wb)
    {
        var ms = new MemoryStream();
        wb.SaveAs(ms);
        ms.Position = 0;
        return ms;
    }

    private static List<string> BuildImportHeaders(bool includeEmployer)
    {
        var headers = new List<string>();
        if (includeEmployer)
        {
            headers.Add("שם_מעסיק");
            headers.Add("חפ");
        }

        headers.AddRange(
        [
            "תז", "מספר_עובד_בעוקץ", "שם_משפחה", "שם_פרטי", "מין", "תאריך_לידה", "טל",
            "תאריך_לידה_ילד_1", "תאריך_לידה_ילד_2", "תאריך_לידה_ילד_3", "תאריך_לידה_ילד_4",
            "תאריך_לידה_ילד_5", "תאריך_לידה_ילד_6", "תאריך_לידה_ילד_7", "תאריך_לידה_ילד_8",
            "תאריך_לידה_ילד_9", "תאריך_לידה_ילד_10", "שנת_לימודים",
            "דרגה1_שם_הדירוג", "דרגה1_דרגה", "דרגה1_תפקיד", "דרגה1_ותק",
            "דרגה1_סהכ", "דרגה1_אחוז_משרה", "דרגה1_קרן_השתלמות_אחוז", "דרגה1_שעות_גיל",
            "דרגה1_אחוז_תוספת_אם", "דרגה1_גמולי_השתלמות", "דרגה1_כפל_תואר",
        ]);

        for (var s = 1; s <= 6; s++)
        {
            headers.Add($"דרגה1_{s}_סמל_מוסד");
            headers.Add($"דרגה1_{s}_שעות_שבועיות");
            headers.Add($"דרגה1_{s}_בסיס_משרה");
        }

        return headers;
    }

    private static IFormFile FormFile(Stream stream, string name)
    {
        stream.Position = 0;
        return new FormFile(stream, 0, stream.Length, "file", name)
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        };
    }
}
