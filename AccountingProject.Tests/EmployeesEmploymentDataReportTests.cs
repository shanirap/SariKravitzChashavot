using AccountingProject.Models;
using AccountingProject.Services;
using AccountingProject.Tests.TestHelpers;
using ClosedXML.Excel;

namespace AccountingProject.Tests;

public sealed class EmployeesEmploymentDataReportTests
{
    private static readonly string[] ExpectedHeaders =
    [
        "שם העובדת", "סמל מוסד", "תפקיד", "ש\"ש", "בסיס משרה",
        "אחוז משרה", "אחוז תוספת אם", "שעות גיל", "מס' גמולים", "כפל תואר",
        "הפרשה לקרן השתלמות", "הכפלה כללית"
    ];

    private static readonly string[] RemovedHeaders =
    [
        "ת\"ז", "מעסיק", "שם הדירוג", "דרגה", "ותק", "תוספת מעונות"
    ];

    [Fact]
    public async Task EmployeesEmploymentDataReport_HasExpectedColumnsAndSlotRows()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = new Employer { Name = "דוח מעסיק" };
        db.Employers.Add(employer);
        await db.SaveChangesAsync();

        var employee = new Employee
        {
            EmployerId = employer.Id,
            IdNumber = "123456789",
            FirstName = "רחל",
            LastName = "כהן",
        };
        db.Employees.Add(employee);
        await db.SaveChangesAsync();

        var ed = new EmploymentData
        {
            EmployeeId = employee.Id,
            EmployerId = employer.Id,
            AcademicYear = "תשפ\"ו",
            Grade1Role = "גננת",
            Grade1JobPercent = 100m,
            Grade1MotherBenefitPercent = 5m,
            Grade1AgeHours = 2m,
            Grade1TrainingBenefits = 3m,
            Grade1DoubleDegree = 1m,
            Grade1TrainingFundPercent = 7.5m,
            Grade2Role = "סייעת",
            Grade2JobPercent = 50m,
            Grade2MotherBenefitPercent = 2m,
            Grade2AgeHours = 1m,
            Grade2TrainingBenefits = 4m,
            Grade2DoubleDegree = 0.5m,
            Grade2TrainingFundPercent = 6m,
            Slots =
            [
                new EmploymentDataSlot
                {
                    GradeBand = 1,
                    SlotIndex = 1,
                    InstitutionSymbol = "111",
                    WeeklyHours = 30m,
                    JobBase = 28m,
                },
                new EmploymentDataSlot
                {
                    GradeBand = 2,
                    SlotIndex = 1,
                    InstitutionSymbol = "222",
                    WeeklyHours = 20m,
                    JobBase = 18m,
                },
                new EmploymentDataSlot
                {
                    GradeBand = 1,
                    SlotIndex = 2,
                    InstitutionSymbol = null,
                    WeeklyHours = null,
                    JobBase = null,
                },
            ],
        };
        db.EmploymentData.Add(ed);
        await db.SaveChangesAsync();

        var sut = new ReportExportService(db);
        var bytes = await sut.EmployeesEmploymentDataAsync(employer.Id, "תשפ\"ו");

        using var wb = new XLWorkbook(new MemoryStream(bytes));
        Assert.Single(wb.Worksheets);
        var ws = wb.Worksheet("עובדים נתוני העסקה");

        for (var i = 0; i < ExpectedHeaders.Length; i++)
            Assert.Equal(ExpectedHeaders[i], ws.Cell(1, i + 1).GetString());

        foreach (var removed in RemovedHeaders)
            Assert.DoesNotContain(removed, ExpectedHeaders);

        Assert.DoesNotContain(wb.Worksheets, w => w.Name == "הערות");

        var dataRows = ws.RowsUsed().Skip(1).ToList();
        Assert.Equal(2, dataRows.Count);

        foreach (var row in dataRows)
        {
            Assert.Equal("רחל כהן", row.Cell(1).GetString());
            Assert.Equal(0m, row.Cell(12).GetValue<decimal>());
        }

        var slot111 = dataRows.Single(r => r.Cell(2).GetString() == "111");
        Assert.Equal("גננת", slot111.Cell(3).GetString());
        Assert.Equal(30m, slot111.Cell(4).GetValue<decimal>());
        Assert.Equal(28m, slot111.Cell(5).GetValue<decimal>());
        Assert.Equal(100m, slot111.Cell(6).GetValue<decimal>());
        Assert.Equal(5m, slot111.Cell(7).GetValue<decimal>());
        Assert.Equal(2m, slot111.Cell(8).GetValue<decimal>());
        Assert.Equal(3m, slot111.Cell(9).GetValue<decimal>());
        Assert.Equal(1m, slot111.Cell(10).GetValue<decimal>());
        Assert.Equal(7.5m, slot111.Cell(11).GetValue<decimal>());

        var slot222 = dataRows.Single(r => r.Cell(2).GetString() == "222");
        Assert.Equal("סייעת", slot222.Cell(3).GetString());
        Assert.Equal(50m, slot222.Cell(6).GetValue<decimal>());
        Assert.Equal(4m, slot222.Cell(9).GetValue<decimal>());
    }
}
