using AccountingProject.Contracts;
using AccountingProject.Data;
using AccountingProject.Models;
using AccountingProject.Services;
using AccountingProject.Tests.TestHelpers;

namespace AccountingProject.Tests;

public sealed class EmploymentDataFieldsTests
{
    [Fact]
    public async Task EmploymentDataCreate_PersistsGrade1TrainingBenefitsAndDoubleDegree()
    {
        await using var db = DbTestFactory.CreateContext();
        var (employer, employee) = SeedEmployerAndEmployee(db);

        var sut = ServiceTestFactory.CreateEmploymentDataService(db);
        var dto = new EmploymentDataDto
        {
            EmployeeId = employee.Id,
            EmployerId = employer.Id,
            AcademicYear = "תשפ\"ו",
            Grade1TrainingBenefits = 12.5m,
            Grade1DoubleDegree = 3m,
            Grade2TrainingBenefits = 8m,
            Grade2DoubleDegree = 1.5m,
            Slots = [],
        };

        var (record, message) = await sut.CreateAsync(dto);

        Assert.Null(message);
        Assert.NotNull(record);
        Assert.Equal(12.5m, record!.Grade1TrainingBenefits);
        Assert.Equal(3m, record.Grade1DoubleDegree);
        Assert.Equal(8m, record.Grade2TrainingBenefits);
        Assert.Equal(1.5m, record.Grade2DoubleDegree);

        var saved = await db.EmploymentData.FindAsync(record.Id);
        Assert.Equal(12.5m, saved!.Grade1TrainingBenefits);
        Assert.Equal(8m, saved.Grade2TrainingBenefits);
    }

    [Fact]
    public async Task EmploymentDataUpdate_DoesNotClearPerGradeTrainingFields()
    {
        await using var db = DbTestFactory.CreateContext();
        var (employer, employee) = SeedEmployerAndEmployee(db, "987654321");
        db.EmploymentData.Add(new EmploymentData
        {
            EmployeeId = employee.Id,
            EmployerId = employer.Id,
            AcademicYear = "תשפ\"ו",
            Grade1TrainingBenefits = 7m,
            Grade1DoubleDegree = 2m,
            Grade2TrainingBenefits = 4m,
            Grade2DoubleDegree = 1m,
        });
        db.SaveChanges();
        var existing = db.EmploymentData.First();

        var sut = ServiceTestFactory.CreateEmploymentDataService(db);
        var dto = new EmploymentDataDto
        {
            EmployeeId = employee.Id,
            EmployerId = employer.Id,
            AcademicYear = "תשפ\"ו",
            Grade1TrainingBenefits = 7m,
            Grade1DoubleDegree = 2m,
            Grade2TrainingBenefits = 4m,
            Grade2DoubleDegree = 1m,
            Slots = [],
        };

        var (record, message) = await sut.UpdateAsync(existing.Id, dto);

        Assert.Null(message);
        Assert.NotNull(record);
        Assert.Equal(7m, record!.Grade1TrainingBenefits);
        Assert.Equal(2m, record.Grade1DoubleDegree);
        Assert.Equal(4m, record.Grade2TrainingBenefits);
        Assert.Equal(1m, record.Grade2DoubleDegree);
    }

    [Fact]
    public async Task EmployeesPersonalReport_IncludesChildrenBirthDates()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = new Employer { Name = "Report Employer" };
        db.Employers.Add(employer);
        await db.SaveChangesAsync();
        var withChildren = new Employee
        {
            EmployerId = employer.Id,
            IdNumber = "111111111",
            FirstName = "With",
            LastName = "Children",
            ChildBirthDate1 = new DateOnly(2018, 3, 12),
            ChildBirthDate2 = new DateOnly(2020, 9, 4),
        };
        var noChildren = new Employee
        {
            EmployerId = employer.Id,
            IdNumber = "222222222",
            FirstName = "No",
            LastName = "Children",
        };
        db.Employees.AddRange(withChildren, noChildren);
        await db.SaveChangesAsync();

        var sut = new ReportExportService(db);
        var bytes = await sut.EmployeesPersonalAsync(employer.Id);

        using var wb = new ClosedXML.Excel.XLWorkbook(new MemoryStream(bytes));
        var ws = wb.Worksheet("עובדים אישיים");
        Assert.Equal("ילד 1", ws.Cell(1, 9).GetString());
        Assert.Equal("ילד 10", ws.Cell(1, 18).GetString());

        var rows = ws.RowsUsed().Skip(1).ToList();
        Assert.Equal(2, rows.Count);

        var withChildrenRow = rows.First(r => r.Cell(3).GetString() == "111111111");
        Assert.Equal("12/03/2018", withChildrenRow.Cell(9).GetString());
        Assert.Equal("04/09/2020", withChildrenRow.Cell(10).GetString());

        var noChildrenRow = rows.First(r => r.Cell(3).GetString() == "222222222");
        for (var col = 9; col <= 18; col++)
            Assert.Equal(string.Empty, noChildrenRow.Cell(col).GetString());
    }

    private static (Employer Employer, Employee Employee) SeedEmployerAndEmployee(
        PayrollDbContext db, string idNumber = "123456789")
    {
        var employer = new Employer { Name = "Test Employer" };
        db.Employers.Add(employer);
        db.SaveChanges();
        var employee = new Employee
        {
            EmployerId = employer.Id,
            IdNumber = idNumber,
            FirstName = "Test",
            LastName = "User",
            Gender = "זכר",
        };
        db.Employees.Add(employee);
        db.SaveChanges();
        return (employer, employee);
    }
}
