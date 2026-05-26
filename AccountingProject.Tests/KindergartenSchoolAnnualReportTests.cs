using AccountingProject.Data;
using AccountingProject.Domain;
using AccountingProject.Models;
using AccountingProject.Services;
using AccountingProject.Tests.TestHelpers;
using ClosedXML.Excel;

namespace AccountingProject.Tests;

public sealed class KindergartenSchoolAnnualReportTests
{
    private static readonly string[] ExpectedHeaders =
    [
        "סמל מוסד", "שם", "ת.ז.", "טלפון", "תפקיד", "דרגה", "ותק",
        "שעות גיל", "השתל'", "כפל תואר", "בסיס משרה", "שעות שבועיות", "חינוך", "סה\"כ"
    ];

    private const string Year = "תשפ\"ו";

    [Fact]
    public async Task KindergartenAnnual_IncludesOnlyKindergartenInstitutionTypeSlots()
    {
        await using var db = DbTestFactory.CreateContext();
        var (employerId, _) = await SeedRosterDataAsync(db);

        var sut = new ReportExportService(db);
        var bytes = await sut.KindergartenAnnualAsync(employerId, Year);

        using var wb = new XLWorkbook(new MemoryStream(bytes));
        var ws = wb.Worksheet("מצבת גנים");
        AssertHeaders(ws);

        var dataRows = GetDataRows(ws).ToList();
        Assert.Equal(2, dataRows.Count);
        Assert.All(dataRows, r => Assert.Equal("G-1", r.Cell(1).GetString()));
        Assert.DoesNotContain(dataRows, r => r.Cell(1).GetString() is "S-1" or "O-1");
    }

    [Fact]
    public async Task SchoolAnnual_IncludesOnlySchoolInstitutionTypeSlots()
    {
        await using var db = DbTestFactory.CreateContext();
        var (employerId, _) = await SeedRosterDataAsync(db);

        var sut = new ReportExportService(db);
        var bytes = await sut.SchoolAnnualAsync(employerId, Year);

        using var wb = new XLWorkbook(new MemoryStream(bytes));
        var ws = wb.Worksheet("מצבת בית ספר");
        AssertHeaders(ws);

        var dataRows = GetDataRows(ws).ToList();
        Assert.Single(dataRows);
        Assert.Equal("S-1", dataRows[0].Cell(1).GetString());
        Assert.Equal("מורה", dataRows[0].Cell(5).GetString());
    }

    [Fact]
    public async Task AnnualRosters_EducationColumnEmpty_TotalEqualsWeeklyHours()
    {
        await using var db = DbTestFactory.CreateContext();
        var (employerId, _) = await SeedRosterDataAsync(db);
        var sut = new ReportExportService(db);

        foreach (var bytes in new[]
        {
            await sut.KindergartenAnnualAsync(employerId, Year),
            await sut.SchoolAnnualAsync(employerId, Year),
        })
        {
            using var wb = new XLWorkbook(new MemoryStream(bytes));
            var ws = wb.Worksheets.First();
            foreach (var row in GetDataRows(ws))
            {
                Assert.Equal(string.Empty, row.Cell(13).GetString());
                Assert.Equal(row.Cell(12).GetValue<decimal>(), row.Cell(14).GetValue<decimal>());
            }
        }
    }

    [Fact]
    public async Task KindergartenAnnual_InsertsBlankRowBetweenDifferentInstitutionSymbols()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = new Employer { Name = "Roster Employer" };
        db.Employers.Add(employer);
        await db.SaveChangesAsync();

        db.EmployerInstitutionSymbols.AddRange(
            new EmployerInstitutionSymbol { EmployerId = employer.Id, InstitutionSymbol = "G-A", InstitutionType = InstitutionTypes.Kindergarten },
            new EmployerInstitutionSymbol { EmployerId = employer.Id, InstitutionSymbol = "G-B", InstitutionType = InstitutionTypes.Kindergarten });
        var emp = new Employee { EmployerId = employer.Id, IdNumber = "111", FirstName = "A", LastName = "Worker" };
        db.Employees.Add(emp);
        await db.SaveChangesAsync();

        db.EmploymentData.Add(new EmploymentData
        {
            EmployeeId = emp.Id,
            EmployerId = employer.Id,
            AcademicYear = Year,
            Grade1Role = "גננת",
            Slots =
            [
                new EmploymentDataSlot { GradeBand = 1, SlotIndex = 1, InstitutionSymbol = "G-A", WeeklyHours = 10m },
                new EmploymentDataSlot { GradeBand = 1, SlotIndex = 2, InstitutionSymbol = "G-B", WeeklyHours = 12m },
            ],
        });
        await db.SaveChangesAsync();

        var bytes = await new ReportExportService(db).KindergartenAnnualAsync(employer.Id, Year);
        using var wb = new XLWorkbook(new MemoryStream(bytes));
        var ws = wb.Worksheet("מצבת גנים");

        Assert.Equal("G-A", ws.Cell(2, 1).GetString());
        Assert.True(string.IsNullOrWhiteSpace(ws.Cell(3, 1).GetString()));
        Assert.True(string.IsNullOrWhiteSpace(ws.Cell(3, 2).GetString()));
        Assert.Equal("G-B", ws.Cell(4, 1).GetString());
    }

    [Fact]
    public async Task SameEmployee_AppearsInCorrectReportPerSlotInstitutionType()
    {
        await using var db = DbTestFactory.CreateContext();
        var (employerId, _) = await SeedRosterDataAsync(db);
        var sut = new ReportExportService(db);

        var kgBytes = await sut.KindergartenAnnualAsync(employerId, Year);
        var schoolBytes = await sut.SchoolAnnualAsync(employerId, Year);

        using (var kgWb = new XLWorkbook(new MemoryStream(kgBytes)))
        {
            var kgRows = GetDataRows(kgWb.Worksheet("מצבת גנים")).ToList();
            Assert.Equal(2, kgRows.Count);
            Assert.All(kgRows, r => Assert.Equal("123456789", r.Cell(3).GetString()));
        }

        using (var schoolWb = new XLWorkbook(new MemoryStream(schoolBytes)))
        {
            var schoolRows = GetDataRows(schoolWb.Worksheet("מצבת בית ספר")).ToList();
            Assert.Single(schoolRows);
            Assert.Equal("123456789", schoolRows[0].Cell(3).GetString());
            Assert.Equal("S-1", schoolRows[0].Cell(1).GetString());
        }
    }

    private static void AssertHeaders(IXLWorksheet ws)
    {
        for (var i = 0; i < ExpectedHeaders.Length; i++)
            Assert.Equal(ExpectedHeaders[i], ws.Cell(1, i + 1).GetString());
    }

    private static IEnumerable<IXLRow> GetDataRows(IXLWorksheet ws) =>
        ws.RowsUsed()
            .Skip(1)
            .Where(r => r.CellsUsed().Any(c => !string.IsNullOrWhiteSpace(c.GetString()) || c.TryGetValue<decimal>(out var d) && d != 0));

    private static async Task<(int EmployerId, int EmployeeId)> SeedRosterDataAsync(PayrollDbContext db)
    {
        var employer = new Employer { Name = "Annual Roster Employer" };
        db.Employers.Add(employer);
        await db.SaveChangesAsync();

        db.EmployerInstitutionSymbols.AddRange(
            new EmployerInstitutionSymbol { EmployerId = employer.Id, InstitutionSymbol = "G-1", InstitutionType = InstitutionTypes.Kindergarten },
            new EmployerInstitutionSymbol { EmployerId = employer.Id, InstitutionSymbol = "S-1", InstitutionType = InstitutionTypes.School },
            new EmployerInstitutionSymbol { EmployerId = employer.Id, InstitutionSymbol = "O-1", InstitutionType = InstitutionTypes.Other });

        var employee = new Employee
        {
            EmployerId = employer.Id,
            IdNumber = "123456789",
            FirstName = "רחל",
            LastName = "כהן",
            Phone = "050-1111111",
        };
        db.Employees.Add(employee);
        await db.SaveChangesAsync();

        db.EmploymentData.Add(new EmploymentData
        {
            EmployeeId = employee.Id,
            EmployerId = employer.Id,
            AcademicYear = Year,
            Grade1Role = "גננת",
            Grade1Grade = "ב",
            Grade1Seniority = "5",
            Grade1AgeHours = 2m,
            Grade1TrainingBenefits = 3m,
            Grade1DoubleDegree = 1m,
            Grade2Role = "מורה",
            Grade2Grade = "ג",
            Grade2Seniority = "3",
            Grade2AgeHours = 1m,
            Grade2TrainingBenefits = 4m,
            Grade2DoubleDegree = 0.5m,
            Slots =
            [
                new EmploymentDataSlot { GradeBand = 1, SlotIndex = 1, InstitutionSymbol = "G-1", WeeklyHours = 30m, JobBase = 28m },
                new EmploymentDataSlot { GradeBand = 1, SlotIndex = 2, InstitutionSymbol = "G-1", WeeklyHours = 20m, JobBase = 18m },
                new EmploymentDataSlot { GradeBand = 2, SlotIndex = 1, InstitutionSymbol = "S-1", WeeklyHours = 25m, JobBase = 22m },
                new EmploymentDataSlot { GradeBand = 1, SlotIndex = 3, InstitutionSymbol = "O-1", WeeklyHours = 15m, JobBase = 14m },
            ],
        });
        await db.SaveChangesAsync();

        return (employer.Id, employee.Id);
    }
}
