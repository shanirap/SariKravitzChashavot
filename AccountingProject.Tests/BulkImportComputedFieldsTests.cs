using AccountingProject.Models;
using AccountingProject.Services;
using AccountingProject.Tests.TestHelpers;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace AccountingProject.Tests;

public sealed class BulkImportComputedFieldsTests
{
    private const string Year = "תשפ\"ו";

    [Fact]
    public async Task ImportEmployees_SyncsTeacherSupplementaryAndAgeHours_MatchingManualScenario()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db, "Screenshot Scenario Employer");
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "471136");

        await using var stream = CreateEmploymentRow(
            employer.Name,
            "123123123",
            birthDate: new DateOnly(1973, 1, 15),
            fillRow: (ws, row, map) =>
            {
                ws.Cell(row, map("דרגה1_שם_הדירוג")).Value = "יסודי וגנים";
                ws.Cell(row, map("דרגה1_דרגה")).Value = "ב";
                ws.Cell(row, map("דרגה1_תפקיד")).Value = "גננת ראשית";
                ws.Cell(row, map("דרגה1_ותק")).Value = 17;
                ws.Cell(row, map("דרגה1_1_סמל_מוסד")).Value = "471136";
                ws.Cell(row, map("דרגה1_1_שעות_שבועיות")).Value = 20.50m;
                ws.Cell(row, map("דרגה1_1_בסיס_משרה")).Value = 30;
                ws.Cell(row, map("דרגה1_סהכ")).Value = 999;
                ws.Cell(row, map("דרגה1_אחוז_משרה")).Value = 50;
            });

        var sut = ServiceTestFactory.CreateBulkImportService(db);
        var result = await sut.ImportEmployeesAsync(FormFileFromStream(stream, "screenshot.xlsx"));

        Assert.Equal(1, result.Imported);
        var ed = await db.EmploymentData
            .Include(e => e.Slots)
            .SingleAsync(e => e.AcademicYear == Year);

        Assert.Equal(23.50m, ed.Grade1Total);
        Assert.Equal(2m, ed.Grade1AgeHours);
        Assert.Equal(83.93m, ed.Grade1JobPercent);
        Assert.Equal(8.4m, ed.Grade1TrainingFundPercent);

        var supplementary = ed.Slots.Single(s => s.SlotIndex == 2);
        Assert.Equal((byte)1, supplementary.SupplementaryParentSlotIndex);
        Assert.Equal(3m, supplementary.WeeklyHours);
        Assert.Equal("471136", supplementary.InstitutionSymbol);
    }

    [Fact]
    public async Task ImportEmployees_ComputesAllDerivedFields_OnNewRecord()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db, "Computed Import Employer");
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "CMP-1");

        await using var stream = CreateEmploymentRow(
            employer.Name,
            "123456789",
            fillRow: (ws, row, map) =>
            {
                ws.Cell(row, map("דרגה1_שם_הדירוג")).Value = "יסודי וגנים";
                ws.Cell(row, map("דרגה1_דרגה")).Value = "ב";
                ws.Cell(row, map("דרגה1_תפקיד")).Value = "גננת ראשית";
                ws.Cell(row, map("דרגה1_שעות_גיל")).Value = 2;
                ws.Cell(row, map("דרגה1_1_סמל_מוסד")).Value = "CMP-1";
                ws.Cell(row, map("דרגה1_1_שעות_שבועיות")).Value = 28;
                ws.Cell(row, map("תאריך_לידה_ילד_1")).Value = "2012-03-15";
            });

        var sut = ServiceTestFactory.CreateBulkImportService(db);
        var result = await sut.ImportEmployeesAsync(FormFileFromStream(stream, "computed.xlsx"));

        Assert.Equal(1, result.Imported);
        var ed = await db.EmploymentData
            .Include(e => e.Slots)
            .SingleAsync(e => e.AcademicYear == Year);

        Assert.Equal(31m, ed.Grade1Total);
        Assert.Equal(120.71m, ed.Grade1JobPercent);
        Assert.Equal(10m, ed.Grade1MotherBenefitPercent);
        Assert.Equal(8.4m, ed.Grade1TrainingFundPercent);
        Assert.Equal(30m, ed.Slots.Single(s => s.SlotIndex == 1).JobBase);
        Assert.Contains(ed.Slots, s => s.SlotIndex == 2 && s.SupplementaryParentSlotIndex == 1);
    }

    [Fact]
    public async Task ImportEmployees_IgnoresWrongComputedValuesFromExcel()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db, "Wrong Excel Computed");
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "WRG-1");

        await using var stream = CreateEmploymentRow(
            employer.Name,
            "987654321",
            fillRow: (ws, row, map) =>
            {
                ws.Cell(row, map("דרגה1_שם_הדירוג")).Value = "יסודי וגנים";
                ws.Cell(row, map("דרגה1_דרגה")).Value = "ב";
                ws.Cell(row, map("דרגה1_תפקיד")).Value = "גננת ראשית";
                ws.Cell(row, map("דרגה1_שעות_גיל")).Value = 2;
                ws.Cell(row, map("דרגה1_1_סמל_מוסד")).Value = "WRG-1";
                ws.Cell(row, map("דרגה1_1_שעות_שבועיות")).Value = 28;
                ws.Cell(row, map("דרגה1_1_בסיס_משרה")).Value = 30;
                ws.Cell(row, map("דרגה1_סהכ")).Value = 999;
                ws.Cell(row, map("דרגה1_אחוז_משרה")).Value = 50;
                ws.Cell(row, map("דרגה1_אחוז_תוספת_אם")).Value = 99;
                ws.Cell(row, map("דרגה1_קרן_השתלמות_אחוז")).Value = 1.1m;
            });

        var sut = ServiceTestFactory.CreateBulkImportService(db);
        var result = await sut.ImportEmployeesAsync(FormFileFromStream(stream, "wrong-computed.xlsx"));

        Assert.Equal(1, result.Imported);
        var ed = await db.EmploymentData.SingleAsync(e => e.AcademicYear == Year);
        Assert.Equal(31m, ed.Grade1Total);
        Assert.Equal(110.71m, ed.Grade1JobPercent);
        Assert.Equal(0m, ed.Grade1MotherBenefitPercent);
        Assert.Equal(8.4m, ed.Grade1TrainingFundPercent);
    }

    [Fact]
    public async Task ImportEmployees_AppliesDefaultJobBase_WhenMissingFromExcel()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db, "Default Job Base");
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "DEF-1");

        await using var stream = CreateEmploymentRow(
            employer.Name,
            "444333222",
            fillRow: (ws, row, map) =>
            {
                ws.Cell(row, map("דרגה1_שם_הדירוג")).Value = "יסודי וגנים";
                ws.Cell(row, map("דרגה1_דרגה")).Value = "ב";
                ws.Cell(row, map("דרגה1_תפקיד")).Value = "גננת ראשית";
                ws.Cell(row, map("דרגה1_1_סמל_מוסד")).Value = "DEF-1";
                ws.Cell(row, map("דרגה1_1_שעות_שבועיות")).Value = 28;
            });

        var sut = ServiceTestFactory.CreateBulkImportService(db);
        var result = await sut.ImportEmployeesAsync(FormFileFromStream(stream, "default-base.xlsx"));

        Assert.Equal(1, result.Imported);
        var slots = await db.EmploymentData
            .Include(e => e.Slots)
            .SelectMany(e => e.Slots)
            .ToListAsync();
        Assert.Equal(30m, slots.Single(s => s.SlotIndex == 1).JobBase);
        Assert.Equal(30m, slots.Single(s => s.SlotIndex == 2).JobBase);
    }

    [Fact]
    public async Task ImportEmployees_PreservesInputFields_WhenComputedColumnsMissing()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db, "Preserve Inputs");
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "PRS-1");

        await using var stream = CreateEmploymentRow(
            employer.Name,
            "111000999",
            fillRow: (ws, row, map) =>
            {
                ws.Cell(row, map("דרגה1_שם_הדירוג")).Value = "יסודי וגנים";
                ws.Cell(row, map("דרגה1_דרגה")).Value = "ב";
                ws.Cell(row, map("דרגה1_תפקיד")).Value = "גננת ראשית";
                ws.Cell(row, map("דרגה1_שעות_גיל")).Value = 3;
                ws.Cell(row, map("דרגה1_גמולי_השתלמות")).Value = 12.5m;
                ws.Cell(row, map("דרגה1_כפל_תואר")).Value = 2;
                ws.Cell(row, map("דרגה1_1_סמל_מוסד")).Value = "PRS-1";
                ws.Cell(row, map("דרגה1_1_שעות_שבועיות")).Value = 28;
                ws.Cell(row, map("דרגה1_1_בסיס_משרה")).Value = 30;
            });

        var sut = ServiceTestFactory.CreateBulkImportService(db);
        var result = await sut.ImportEmployeesAsync(FormFileFromStream(stream, "preserve.xlsx"));

        Assert.Equal(1, result.Imported);
        var ed = await db.EmploymentData.SingleAsync(e => e.AcademicYear == Year);
        Assert.Equal(3m, ed.Grade1AgeHours);
        Assert.Equal(12.5m, ed.Grade1TrainingBenefits);
        Assert.Equal(2m, ed.Grade1DoubleDegree);
        Assert.Equal(31m, ed.Grade1Total);
    }

    [Fact]
    public async Task CreateAsync_StillComputesDerivedFields_AfterRefactor()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "MAN-1");
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id);

        var sut = ServiceTestFactory.CreateEmploymentDataService(db);
        var dto = new Contracts.EmploymentDataDto
        {
            EmployeeId = employee.Id,
            EmployerId = employer.Id,
            AcademicYear = ReportTestData.DefaultAcademicYear,
            Grade1GradeName = "יסודי וגנים",
            Grade1Grade = "ב",
            Grade1Role = "גננת ראשית",
            Grade1Seniority = "1",
            Grade1AgeHours = 2m,
            Slots =
            [
                new Contracts.EmploymentDataSlotDto
                {
                    GradeBand = 1,
                    SlotIndex = 1,
                    InstitutionSymbol = "MAN-1",
                    WeeklyHours = 28m,
                    JobBase = 30m,
                },
            ],
        };

        var (record, message) = await sut.CreateAsync(dto);

        Assert.Null(message);
        Assert.NotNull(record);
        Assert.Equal(31m, record!.Grade1Total);
        Assert.Equal(110.71m, record.Grade1JobPercent);
    }

    [Fact]
    public async Task CreateAsync_CreatesSupplementarySlot_WhenQualifyingRole()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = await ReportTestData.SeedEmployerAsync(db);
        await ReportTestData.SeedSymbolAsync(db, employer.Id, "SUP-1");
        var employee = await ReportTestData.SeedEmployeeAsync(db, employer.Id);

        var sut = ServiceTestFactory.CreateEmploymentDataService(db);
        var dto = new Contracts.EmploymentDataDto
        {
            EmployeeId = employee.Id,
            EmployerId = employer.Id,
            AcademicYear = ReportTestData.DefaultAcademicYear,
            Grade1GradeName = "יסודי וגנים",
            Grade1Grade = "ב",
            Grade1Role = "גננת ראשית",
            Grade1Seniority = "1",
            Slots =
            [
                new Contracts.EmploymentDataSlotDto
                {
                    GradeBand = 1,
                    SlotIndex = 1,
                    InstitutionSymbol = "SUP-1",
                    WeeklyHours = 20.50m,
                    JobBase = 30m,
                },
            ],
        };

        var (record, message) = await sut.CreateAsync(dto);

        Assert.Null(message);
        Assert.NotNull(record);
        await db.Entry(record!).Collection(r => r.Slots).LoadAsync();
        var supplementary = record.Slots.Single(s => s.SlotIndex == 2);
        Assert.Equal((byte)1, supplementary.SupplementaryParentSlotIndex);
        Assert.Equal(3m, supplementary.WeeklyHours);
    }

    private static MemoryStream CreateEmploymentRow(
        string employerName,
        string idNumber,
        Action<IXLWorksheet, int, Func<string, int>> fillRow,
        DateOnly? birthDate = null)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("עובדים");
        var headers = BuildHeaders();
        for (var i = 0; i < headers.Count; i++)
            ws.Cell(1, i + 1).Value = headers[i];

        const int row = 2;
        var col = 1;
        ws.Cell(row, col++).Value = employerName;
        col++; // חפ
        ws.Cell(row, col++).Value = idNumber;
        col++; // מספר עובד
        ws.Cell(row, col++).Value = "כהן";
        ws.Cell(row, col++).Value = "רחל";
        ws.Cell(row, col++).Value = "נקבה";
        ws.Cell(row, col++).Value = (birthDate ?? new DateOnly(1990, 1, 15)).ToString("yyyy-MM-dd");
        col += 11; // טל + children
        ws.Cell(row, col++).Value = Year;

        int Map(string header)
        {
            var index = headers.IndexOf(header);
            if (index < 0)
                throw new InvalidOperationException($"Missing header {header}");
            return index + 1;
        }

        fillRow(ws, row, Map);

        var ms = new MemoryStream();
        wb.SaveAs(ms);
        ms.Position = 0;
        return ms;
    }

    private static List<string> BuildHeaders()
    {
        var headers = new List<string>
        {
            "שם_מעסיק", "חפ", "תז", "מספר_עובד_בעוקץ", "שם_משפחה", "שם_פרטי", "מין", "תאריך_לידה", "טל",
            "תאריך_לידה_ילד_1", "תאריך_לידה_ילד_2", "תאריך_לידה_ילד_3", "תאריך_לידה_ילד_4",
            "תאריך_לידה_ילד_5", "תאריך_לידה_ילד_6", "תאריך_לידה_ילד_7", "תאריך_לידה_ילד_8",
            "תאריך_לידה_ילד_9", "תאריך_לידה_ילד_10", "שנת_לימודים",
            "דרגה1_שם_הדירוג", "דרגה1_דרגה", "דרגה1_תפקיד", "דרגה1_ותק",
            "דרגה1_סהכ", "דרגה1_אחוז_משרה", "דרגה1_קרן_השתלמות_אחוז", "דרגה1_שעות_גיל",
            "דרגה1_אחוז_תוספת_אם", "דרגה1_גמולי_השתלמות", "דרגה1_כפל_תואר",
        };
        for (var s = 1; s <= 6; s++)
        {
            headers.Add($"דרגה1_{s}_סמל_מוסד");
            headers.Add($"דרגה1_{s}_שעות_שבועיות");
            headers.Add($"דרגה1_{s}_בסיס_משרה");
        }

        return headers;
    }

    private static IFormFile FormFileFromStream(Stream stream, string fileName)
    {
        stream.Position = 0;
        return new FormFile(stream, 0, stream.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        };
    }
}
