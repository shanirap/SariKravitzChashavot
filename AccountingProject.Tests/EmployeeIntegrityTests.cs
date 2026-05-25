using AccountingProject.Contracts;
using AccountingProject.Models;
using AccountingProject.Services;
using AccountingProject.Tests.TestHelpers;

namespace AccountingProject.Tests;

public sealed class EmployeeIntegrityTests
{
    [Fact]
    public async Task EmploymentDataCreate_RejectsEmployeeFromDifferentEmployer()
    {
        await using var db = DbTestFactory.CreateContext();
        var (emp1, emp2) = SeedTwoEmployersAndEmployees(db);

        var sut = new EmploymentDataService(db);
        var dto = BasicEmploymentDto(emp1.Id, emp2.EmployerId, "תשפ\"ו");

        var (record, message) = await sut.CreateAsync(dto);

        Assert.Null(record);
        Assert.NotNull(message);
        Assert.Contains("שייך למעסיק אחר", message);
    }

    [Fact]
    public async Task EmploymentDataUpdate_RejectsEmployeeFromDifferentEmployer()
    {
        await using var db = DbTestFactory.CreateContext();
        var (emp1, emp2) = SeedTwoEmployersAndEmployees(db);
        db.EmploymentData.Add(new EmploymentData
        {
            EmployeeId = emp1.Id,
            EmployerId = emp1.EmployerId,
            AcademicYear = "תשפ\"ו",
        });
        await db.SaveChangesAsync();
        var existing = await db.EmploymentData.FirstAsync();

        var sut = new EmploymentDataService(db);
        var dto = BasicEmploymentDto(emp2.Id, emp1.EmployerId, "תשפ\"ז");

        var (record, message) = await sut.UpdateAsync(existing.Id, dto);

        Assert.Null(record);
        Assert.NotNull(message);
        Assert.Contains("שייך למעסיק אחר", message);
    }

    [Fact]
    public async Task CreateEmployee_AllowsSameIdNumberAcrossDifferentEmployers()
    {
        await using var db = DbTestFactory.CreateContext();
        var employerA = new Employer { Name = "A Ltd" };
        var employerB = new Employer { Name = "B Ltd" };
        db.Employers.AddRange(employerA, employerB);
        await db.SaveChangesAsync();

        var sut = new EmployeeService(db);
        var dtoA = BasicEmployeeDto(employerA.Id, "123456789");
        var dtoB = BasicEmployeeDto(employerB.Id, "123456789");

        var resultA = await sut.CreateOrGetAsync(dtoA);
        var resultB = await sut.CreateOrGetAsync(dtoB);

        Assert.True(resultA.CreatedNew);
        Assert.True(resultB.CreatedNew);
        Assert.NotEqual(resultA.Employee.Id, resultB.Employee.Id);
        Assert.Equal(2, await db.Employees.CountAsync(e => e.IdNumber == "123456789"));
    }

    [Fact]
    public async Task EmployeeCreation_DoesNotRequireEmploymentDataOrAcademicYear()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer = new Employer { Name = "A Ltd" };
        db.Employers.Add(employer);
        await db.SaveChangesAsync();

        var sut = new EmployeeService(db);
        var dto = BasicEmployeeDto(employer.Id, "000111222");

        var created = await sut.CreateOrGetAsync(dto);

        Assert.True(created.CreatedNew);
        Assert.Equal(0, await db.EmploymentData.CountAsync());
    }

    private static (Employee Emp1, Employee Emp2) SeedTwoEmployersAndEmployees(PayrollDbContext db)
    {
        var employer1 = new Employer { Name = "Employer 1" };
        var employer2 = new Employer { Name = "Employer 2" };
        db.Employers.AddRange(employer1, employer2);
        db.SaveChanges();

        var emp1 = new Employee
        {
            EmployerId = employer1.Id,
            IdNumber = "111111111",
            FirstName = "A",
            LastName = "One",
            Gender = "זכר",
            BirthDate = new DateOnly(1990, 1, 1),
        };
        var emp2 = new Employee
        {
            EmployerId = employer2.Id,
            IdNumber = "222222222",
            FirstName = "B",
            LastName = "Two",
            Gender = "נקבה",
            BirthDate = new DateOnly(1991, 2, 2),
        };
        db.Employees.AddRange(emp1, emp2);
        db.SaveChanges();
        return (emp1, emp2);
    }

    private static EmploymentDataDto BasicEmploymentDto(int employeeId, int employerId, string academicYear) =>
        new()
        {
            EmployeeId = employeeId,
            EmployerId = employerId,
            AcademicYear = academicYear,
            Slots = [],
        };

    private static EmployeeDto BasicEmployeeDto(int employerId, string idNumber) =>
        new()
        {
            EmployerId = employerId,
            IdNumber = idNumber,
            FirstName = "Test",
            LastName = "User",
            Gender = "זכר",
            BirthDate = "1990-01-01",
        };
}
