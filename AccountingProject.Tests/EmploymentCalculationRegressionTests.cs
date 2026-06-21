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
/// 30 regression tests verifying existing functionality still works after centralized employment calculations.
/// </summary>
public sealed class EmploymentCalculationRegressionTests
{
    private const string Year = "תשפ\"ו";
    private static readonly DateOnly RefDate = HebrewAcademicYear.GetSchoolYearStartDate(Year);

    private static EmploymentCalculationService Calc => ServiceTestFactory.CreateEmploymentCalculations();

    [Fact]
    public void Regression01_OzLeTmuraMorehMehanek_AddsSupplementarySlot()
    {
        var dto = BaseDto("עוז לתמורה", "מורה מחנך", "SYM", 18m, 38m);
        Calc.PrepareForSave(dto, new DateOnly(1990, 1, 1), false, []);
        var child = dto.Slots!.Single(s => s.GradeBand == 1 && s.SlotIndex == 2);
        Assert.Equal((byte)1, child.SupplementaryParentSlotIndex);
        Assert.Equal(3m, child.WeeklyHours);
    }

    [Fact]
    public void Regression02_MorehMiktsoi_DoesNotAddSupplementary()
    {
        var dto = BaseDto("יסודי וגנים", "מורה מקצועי", "SYM", 20m, 30m);
        Calc.PrepareForSave(dto, new DateOnly(1990, 1, 1), false, []);
        Assert.Null(dto.Slots!.Single(s => s.GradeBand == 1 && s.SlotIndex == 2).SupplementaryParentSlotIndex);
    }

    [Fact]
    public void Regression03_PreservesExplicitAgeHours()
    {
        var dto = BaseDto("יסודי וגנים", "מורה מקצועי", "SYM", 20m, 30m);
        dto.Grade1AgeHours = 3m;
        dto.Grade2AgeHours = 3m;
        Calc.PrepareForSave(dto, new DateOnly(1973, 1, 1), false, []);
        Assert.Equal(3m, dto.Grade1AgeHours);
        Assert.Equal(3m, dto.Grade2AgeHours);
    }

    [Fact]
    public void Regression04_ComputesAgeHoursWhenMissing_Birth1976()
    {
        var dto = BaseDto("יסודי וגנים", "מורה מקצועי", "SYM", 20m, 30m);
        Calc.PrepareForSave(dto, new DateOnly(1976, 9, 2), false, []);
        Assert.Equal(0m, dto.Grade1AgeHours);
    }

    [Fact]
    public void Regression05_ComputesAgeHoursWhenMissing_Birth1968()
    {
        var dto = BaseDto("יסודי וגנים", "מורה מקצועי", "SYM", 20m, 30m);
        Calc.PrepareForSave(dto, new DateOnly(1968, 8, 31), false, []);
        Assert.Equal(4m, dto.Grade1AgeHours);
    }

    [Fact]
    public void Regression06_UnifiedGrade_TrainingFundZeroSeniority1()
    {
        var dto = BaseDto(GradeOptions.UnifiedEducationSupportGradeName, "סייעת ראשית", "SYM", 30m, 40m);
        dto.Grade1Seniority = "1";
        Calc.PrepareForSave(dto, new DateOnly(1990, 1, 1), false, []);
        Assert.Equal(0m, dto.Grade1TrainingFundPercent);
    }

    [Fact]
    public void Regression07_UnifiedGrade_TrainingFund75Seniority2()
    {
        var dto = BaseDto(GradeOptions.UnifiedEducationSupportGradeName, "סייעת ראשית", "SYM", 30m, 40m);
        dto.Grade1Seniority = "2";
        Calc.PrepareForSave(dto, new DateOnly(1990, 1, 1), false, []);
        Assert.Equal(7.5m, dto.Grade1TrainingFundPercent);
    }

    [Fact]
    public void Regression08_LegacyAhidGradeName_Normalized()
    {
        var dto = BaseDto(GradeOptions.LegacyUnifiedGradeName, "סייעת ראשית", "SYM", 30m, 40m);
        dto.Grade1Seniority = "2";
        Calc.PrepareForSave(dto, new DateOnly(1990, 1, 1), false, []);
        Assert.Equal(7.5m, dto.Grade1TrainingFundPercent);
    }

    [Fact]
    public void Regression09_OfekGanimGanenetRashit_JobBase304()
    {
        var dto = BaseDto("אופק גנים", "גננת ראשית", "SYM", 20m, null);
        Calc.ApplyDefaultJobBases(dto);
        Assert.Equal(30.4m, dto.Slots!.Single(s => s.SlotIndex == 1).JobBase);
    }

    [Fact]
    public void Regression10_EmptyParentSlot_NoSupplementaryChild()
    {
        var dto = BaseDto("יסודי וגנים", "גננת ראשית", null, null, null);
        Calc.PrepareForSave(dto, new DateOnly(1990, 1, 1), false, []);
        Assert.Null(dto.Slots!.Single(s => s.GradeBand == 1 && s.SlotIndex == 2).WeeklyHours);
    }

    [Fact]
    public async Task Regression11_CreateAsync_MorehMiktsoi_NoSupplementaryPersisted()
    {
        await using var db = DbTestFactory.CreateContext();
        var (employer, employee, symbol) = await SeedAsync(db);
        var sut = ServiceTestFactory.CreateEmploymentDataService(db);
        var (record, err) = await sut.CreateAsync(Dto(employee, employer, symbol, "יסודי וגנים", "מורה מקצועי", 25m));
        Assert.Null(err);
        Assert.DoesNotContain(record!.Slots, s => s.SupplementaryParentSlotIndex != null);
    }

    [Fact]
    public async Task Regression12_CreateAsync_GanenetRashit_AutoSupplementaryPersisted()
    {
        await using var db = DbTestFactory.CreateContext();
        var (employer, employee, symbol) = await SeedAsync(db);
        var sut = ServiceTestFactory.CreateEmploymentDataService(db);
        var (record, err) = await sut.CreateAsync(Dto(employee, employer, symbol, "יסודי וגנים", "גננת ראשית", 20m));
        Assert.Null(err);
        Assert.Contains(record!.Slots, s => s.SlotIndex == 2 && s.SupplementaryParentSlotIndex == 1);
    }

    [Fact]
    public async Task Regression13_CreateAsync_FemaleWithChild_MotherBenefitInJobPercent()
    {
        await using var db = DbTestFactory.CreateContext();
        var (employer, employee, symbol) = await SeedAsync(db);
        employee.ChildBirthDate1 = new DateOnly(2012, 3, 15);
        await db.SaveChangesAsync();
        var sut = ServiceTestFactory.CreateEmploymentDataService(db);
        var dto = Dto(employee, employer, symbol, "יסודי וגנים", "מורה מקצועי", 28m);
        dto.Grade1AgeHours = 2m;
        var (record, err) = await sut.CreateAsync(dto);
        Assert.Null(err);
        Assert.Equal(10m, record!.Grade1MotherBenefitPercent);
        Assert.True(record.Grade1JobPercent > 100m);
    }

    [Fact]
    public async Task Regression14_CreateAsync_MaleEmployee_MotherBenefitZero()
    {
        await using var db = DbTestFactory.CreateContext();
        var (employer, employee, symbol) = await SeedAsync(db);
        employee.Gender = "זכר";
        employee.ChildBirthDate1 = new DateOnly(2012, 3, 15);
        await db.SaveChangesAsync();
        var sut = ServiceTestFactory.CreateEmploymentDataService(db);
        var (record, err) = await sut.CreateAsync(Dto(employee, employer, symbol, "יסודי וגנים", "מורה מקצועי", 28m));
        Assert.Null(err);
        Assert.Equal(0m, record!.Grade1MotherBenefitPercent);
    }

    [Fact]
    public async Task Regression15_UpdateAsync_RecalculatesTotalsAfterHoursChange()
    {
        await using var db = DbTestFactory.CreateContext();
        var (employer, employee, symbol) = await SeedAsync(db);
        var sut = ServiceTestFactory.CreateEmploymentDataService(db);
        var createDto = Dto(employee, employer, symbol, "יסודי וגנים", "מורה מקצועי", 20m);
        var (created, err) = await sut.CreateAsync(createDto);
        Assert.Null(err);

        var updateDto = Dto(employee, employer, symbol, "יסודי וגנים", "מורה מקצועי", 30m);
        var (updated, err2) = await sut.UpdateAsync(created!.Id, updateDto);
        Assert.Null(err2);
        Assert.Equal(30m, updated!.Grade1Total);
    }

    [Fact]
    public async Task Regression16_Import_MorehMiktsoi_NoSupplementarySlot()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db, "Reg Import 16");
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "R16");
        await using var stream = ImportRow(employer.Name, "161616161", (ws, row, map) =>
        {
            ws.Cell(row, map("דרגה1_שם_הדירוג")).Value = "יסודי וגנים";
            ws.Cell(row, map("דרגה1_דרגה")).Value = "ב";
            ws.Cell(row, map("דרגה1_תפקיד")).Value = "מורה מקצועי";
            ws.Cell(row, map("דרגה1_1_סמל_מוסד")).Value = "R16";
            ws.Cell(row, map("דרגה1_1_שעות_שבועיות")).Value = 22;
        });
        var result = await ServiceTestFactory.CreateBulkImportService(db)
            .ImportEmployeesAsync(FormFile(stream, "r16.xlsx"));
        Assert.Equal(1, result.Imported);
        var slots = await db.EmploymentData.Include(e => e.Slots).SelectMany(e => e.Slots).ToListAsync();
        Assert.DoesNotContain(slots, s => s.SupplementaryParentSlotIndex != null);
    }

    [Fact]
    public async Task Regression17_Import_PreservesExcelAgeHours()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db, "Reg Import 17");
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "R17");
        await using var stream = ImportRow(employer.Name, "171717171", (ws, row, map) =>
        {
            ws.Cell(row, map("דרגה1_שם_הדירוג")).Value = "יסודי וגנים";
            ws.Cell(row, map("דרגה1_דרגה")).Value = "ב";
            ws.Cell(row, map("דרגה1_תפקיד")).Value = "מורה מקצועי";
            ws.Cell(row, map("דרגה1_שעות_גיל")).Value = 4;
            ws.Cell(row, map("דרגה1_1_סמל_מוסד")).Value = "R17";
            ws.Cell(row, map("דרגה1_1_שעות_שבועיות")).Value = 22;
        }, birthDate: new DateOnly(1973, 1, 15));
        await ServiceTestFactory.CreateBulkImportService(db).ImportEmployeesAsync(FormFile(stream, "r17.xlsx"));
        var ed = await db.EmploymentData.SingleAsync();
        Assert.Equal(4m, ed.Grade1AgeHours);
    }

    [Fact]
    public async Task Regression18_Import_OzLeTmura_AddsSupplementary()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db, "Reg Import 18");
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "R18");
        await using var stream = ImportRow(employer.Name, "181818181", (ws, row, map) =>
        {
            ws.Cell(row, map("דרגה1_שם_הדירוג")).Value = "עוז לתמורה";
            ws.Cell(row, map("דרגה1_דרגה")).Value = "ב";
            ws.Cell(row, map("דרגה1_תפקיד")).Value = "מורה מחנך";
            ws.Cell(row, map("דרגה1_1_סמל_מוסד")).Value = "R18";
            ws.Cell(row, map("דרגה1_1_שעות_שבועיות")).Value = 18;
        });
        await ServiceTestFactory.CreateBulkImportService(db).ImportEmployeesAsync(FormFile(stream, "r18.xlsx"));
        var slots = await db.EmploymentData.Include(e => e.Slots).SelectMany(e => e.Slots).ToListAsync();
        Assert.Contains(slots, s => s.SupplementaryParentSlotIndex == 1 && s.WeeklyHours == 3m);
    }

    [Fact]
    public async Task Regression19_Import_ComputesAgeHoursFromBirthDate()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db, "Reg Import 19");
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "R19");
        await using var stream = ImportRow(employer.Name, "191919191", (ws, row, map) =>
        {
            ws.Cell(row, map("דרגה1_שם_הדירוג")).Value = "יסודי וגנים";
            ws.Cell(row, map("דרגה1_דרגה")).Value = "ב";
            ws.Cell(row, map("דרגה1_תפקיד")).Value = "מורה מקצועי";
            ws.Cell(row, map("דרגה1_1_סמל_מוסד")).Value = "R19";
            ws.Cell(row, map("דרגה1_1_שעות_שבועיות")).Value = 22;
        }, birthDate: new DateOnly(1973, 1, 15));
        await ServiceTestFactory.CreateBulkImportService(db).ImportEmployeesAsync(FormFile(stream, "r19.xlsx"));
        var ed = await db.EmploymentData.SingleAsync();
        Assert.Equal(2m, ed.Grade1AgeHours);
    }

    [Fact]
    public async Task Regression20_Import_TrainingBenefitsAndDoubleDegreePreserved()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db, "Reg Import 20");
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "R20");
        await using var stream = ImportRow(employer.Name, "202020202", (ws, row, map) =>
        {
            ws.Cell(row, map("דרגה1_שם_הדירוג")).Value = "יסודי וגנים";
            ws.Cell(row, map("דרגה1_דרגה")).Value = "ב";
            ws.Cell(row, map("דרגה1_תפקיד")).Value = "מורה מקצועי";
            ws.Cell(row, map("דרגה1_גמולי_השתלמות")).Value = 11.5m;
            ws.Cell(row, map("דרגה1_כפל_תואר")).Value = 1.5m;
            ws.Cell(row, map("דרגה1_1_סמל_מוסד")).Value = "R20";
            ws.Cell(row, map("דרגה1_1_שעות_שבועיות")).Value = 22;
        });
        await ServiceTestFactory.CreateBulkImportService(db).ImportEmployeesAsync(FormFile(stream, "r20.xlsx"));
        var ed = await db.EmploymentData.SingleAsync();
        Assert.Equal(11.5m, ed.Grade1TrainingBenefits);
        Assert.Equal(1.5m, ed.Grade1DoubleDegree);
    }

    [Theory]
    [InlineData(1976, 9, 2, 0)]
    [InlineData(1973, 1, 15, 2)]
    [InlineData(1968, 8, 31, 4)]
    public void Regression21_23_AgeHoursDefaults(int y, int m, int d, decimal expected)
    {
        Assert.Equal(expected, EmploymentAgeHoursDefaults.Compute(new DateOnly(y, m, d), RefDate));
    }

    [Fact]
    public void Regression24_JobBaseDefaults_YisodiIs30()
    {
        Assert.Equal(30m, EmploymentJobBaseDefaults.GetJobBaseValue("יסודי וגנים", "מורה מקצועי"));
    }

    [Fact]
    public void Regression25_JobBaseDefaults_UnifiedIs40()
    {
        Assert.Equal(40m, EmploymentJobBaseDefaults.GetJobBaseValue(GradeOptions.UnifiedEducationSupportGradeName, "סייעת ראשית"));
    }

    [Fact]
    public void Regression26_SupplementarySync_RemovesWhenRoleNotQualifying()
    {
        var dto = BaseDto("יסודי וגנים", "גננת ראשית", "SYM", 20m, 30m);
        TeacherSupplementarySlotSync.Sync(dto);
        Assert.NotNull(dto.Slots!.Single(s => s.GradeBand == 1 && s.SlotIndex == 2).WeeklyHours);

        dto.Grade1Role = "מורה מקצועי";
        TeacherSupplementarySlotSync.Sync(dto);
        Assert.Null(dto.Slots!.Single(s => s.GradeBand == 1 && s.SlotIndex == 2).SupplementaryParentSlotIndex);
    }

    [Fact]
    public void Regression27_SupplementarySync_Slot5Parent_CreatesSlot6()
    {
        var dto = EmptyDto();
        dto.Grade1GradeName = "יסודי וגנים";
        dto.Grade1Role = "גננת ראשית";
        SetSlot(dto, 1, 5, "SYM", 15m, 30m);
        TeacherSupplementarySlotSync.Sync(dto);
        var slot6 = dto.Slots!.Single(s => s.GradeBand == 1 && s.SlotIndex == 6);
        Assert.Equal((byte)5, slot6.SupplementaryParentSlotIndex);
        Assert.Equal(3m, slot6.WeeklyHours);
    }

    [Fact]
    public async Task Regression28_CreateAsync_Grade2Band_IndependentCalculations()
    {
        await using var db = DbTestFactory.CreateContext();
        var (employer, employee, symbol) = await SeedAsync(db);
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "G2-SYM");
        var sut = ServiceTestFactory.CreateEmploymentDataService(db);
        var dto = Dto(employee, employer, symbol, "יסודי וגנים", "מורה מקצועי", 20m);
        dto.Slots!.Add(new EmploymentDataSlotDto
        {
            GradeBand = 2,
            SlotIndex = 1,
            InstitutionSymbol = "G2-SYM",
            WeeklyHours = 10m,
            JobBase = 30m,
        });
        dto.Grade2GradeName = "עוז לתמורה";
        dto.Grade2Grade = "ב";
        dto.Grade2Role = "מורה מקצועי";
        dto.Grade2Seniority = "1";
        var (record, err) = await sut.CreateAsync(dto);
        Assert.Null(err);
        Assert.Equal(20m, record!.Grade1Total);
        Assert.Equal(10m, record.Grade2Total);
    }

    [Fact]
    public async Task Regression29_Import_InvalidSymbol_DoesNotCreateEmployee()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db, "Reg Import 29");
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "OK-SYM");
        await using var stream = ImportRow(employer.Name, "292929292", (ws, row, map) =>
        {
            ws.Cell(row, map("דרגה1_שם_הדירוג")).Value = "יסודי וגנים";
            ws.Cell(row, map("דרגה1_דרגה")).Value = "ב";
            ws.Cell(row, map("דרגה1_תפקיד")).Value = "מורה מקצועי";
            ws.Cell(row, map("דרגה1_1_סמל_מוסד")).Value = "BAD-SYM";
            ws.Cell(row, map("דרגה1_1_שעות_שבועיות")).Value = 22;
        });
        var result = await ServiceTestFactory.CreateBulkImportService(db).ImportEmployeesAsync(FormFile(stream, "r29.xlsx"));
        Assert.Equal(0, result.Imported);
        Assert.False(await db.Employees.AnyAsync(e => e.IdNumber == "292929292"));
    }

    [Fact]
    public async Task Regression30_CreateAsync_LowJobPercent_TrainingFundZero()
    {
        await using var db = DbTestFactory.CreateContext();
        var (employer, employee, symbol) = await SeedAsync(db);
        var sut = ServiceTestFactory.CreateEmploymentDataService(db);
        var dto = Dto(employee, employer, symbol, "יסודי וגנים", "מורה מקצועי", 5m);
        var (record, err) = await sut.CreateAsync(dto);
        Assert.Null(err);
        Assert.Equal(0m, record!.Grade1TrainingFundPercent);
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
        decimal? jobBase)
    {
        dto.Slots ??= [];
        dto.Slots.Add(new EmploymentDataSlotDto
        {
            GradeBand = band,
            SlotIndex = slotIndex,
            InstitutionSymbol = symbol,
            WeeklyHours = hours,
            JobBase = jobBase,
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
        var employer = await ReportTestData.SeedEmployerAsync(db, $"Reg-{Guid.NewGuid():N}"[..12]);
        const string symbol = "REG-SYM";
        await ReportTestData.SeedSymbolAsync(db, employer.Id, symbol);
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id, Guid.NewGuid().ToString("N")[..9]);
        return (employer, employee, symbol);
    }

    private static MemoryStream ImportRow(
        string employerName,
        string idNumber,
        Action<IXLWorksheet, int, Func<string, int>> fill,
        DateOnly? birthDate = null)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("עובדים");
        var headers = new[]
        {
            "שם_מעסיק", "חפ", "תז", "מספר_עובד_בעוקץ", "שם_משפחה", "שם_פרטי", "מין", "תאריך_לידה", "טל",
            "תאריך_לידה_ילד_1", "תאריך_לידה_ילד_2", "תאריך_לידה_ילד_3", "תאריך_לידה_ילד_4",
            "תאריך_לידה_ילד_5", "תאריך_לידה_ילד_6", "תאריך_לידה_ילד_7", "תאריך_לידה_ילד_8",
            "תאריך_לידה_ילד_9", "תאריך_לידה_ילד_10", "שנת_לימודים",
            "דרגה1_שם_הדירוג", "דרגה1_דרגה", "דרגה1_תפקיד", "דרגה1_ותק",
            "דרגה1_סהכ", "דרגה1_אחוז_משרה", "דרגה1_קרן_השתלמות_אחוז", "דרגה1_שעות_גיל",
            "דרגה1_אחוז_תוספת_אם", "דרגה1_גמולי_השתלמות", "דרגה1_כפל_תואר",
            "דרגה1_1_סמל_מוסד", "דרגה1_1_שעות_שבועיות", "דרגה1_1_בסיס_משרה",
        };
        for (var i = 0; i < headers.Length; i++)
            ws.Cell(1, i + 1).Value = headers[i];
        const int row = 2;
        var col = 1;
        ws.Cell(row, col++).Value = employerName;
        col++; // חפ
        ws.Cell(row, col++).Value = idNumber;
        col++; // מספר_עובד_בעוקץ
        ws.Cell(row, col++).Value = "כהן";
        ws.Cell(row, col++).Value = "ישrael";
        ws.Cell(row, col++).Value = "זכר";
        ws.Cell(row, col++).Value = (birthDate ?? new DateOnly(1990, 1, 15)).ToString("yyyy-MM-dd");
        col += 11; // טל + children
        ws.Cell(row, col++).Value = Year;
        int Map(string h) => Array.IndexOf(headers, h) + 1;
        fill(ws, row, Map);
        var ms = new MemoryStream();
        wb.SaveAs(ms);
        ms.Position = 0;
        return ms;
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
