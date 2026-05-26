using AccountingProject.Data;
using AccountingProject.Domain;
using AccountingProject.Models;

namespace AccountingProject.Tests.TestHelpers;

internal static class ReportTestData
{
    public const string DefaultAcademicYear = "תשפ\"ו";

    public static async Task<Employer> SeedEmployerAsync(PayrollDbContext db, string name = "Test Employer")
    {
        var employer = new Employer { Name = name };
        db.Employers.Add(employer);
        await db.SaveChangesAsync();
        return employer;
    }

    public static async Task<Employee> SeedEmployeeAsync(
        PayrollDbContext db, int employerId, string idNumber = "123456789", string firstName = "Test", string lastName = "User")
    {
        var employee = new Employee
        {
            EmployerId = employerId,
            IdNumber = idNumber,
            FirstName = firstName,
            LastName = lastName,
            Gender = "נקבה",
            Phone = "050-1234567",
        };
        db.Employees.Add(employee);
        await db.SaveChangesAsync();
        return employee;
    }

    public static async Task<EmployerInstitutionSymbol> SeedSymbolAsync(
        PayrollDbContext db, int employerId, string symbol, string institutionType = InstitutionTypes.Kindergarten)
    {
        var row = new EmployerInstitutionSymbol
        {
            EmployerId = employerId,
            InstitutionSymbol = symbol,
            InstitutionSymbolName = symbol,
            InstitutionType = institutionType,
        };
        db.EmployerInstitutionSymbols.Add(row);
        await db.SaveChangesAsync();
        return row;
    }

    public static async Task<EmploymentData> SeedEmploymentWithSlotAsync(
        PayrollDbContext db,
        int employerId,
        int employeeId,
        string institutionSymbol,
        string academicYear = DefaultAcademicYear,
        string? grade1Role = "גננת",
        decimal weeklyHours = 30m,
        byte gradeBand = 1)
    {
        var ed = new EmploymentData
        {
            EmployeeId = employeeId,
            EmployerId = employerId,
            AcademicYear = academicYear,
            Grade1Role = grade1Role,
            Grade1Grade = "ב",
            Grade1Seniority = "5",
            Grade1AgeHours = 2m,
            Grade1TrainingBenefits = 3m,
            Grade1DoubleDegree = 1m,
            Grade2Role = "סייעת",
            Slots =
            [
                new EmploymentDataSlot
                {
                    GradeBand = gradeBand,
                    SlotIndex = 1,
                    InstitutionSymbol = institutionSymbol,
                    WeeklyHours = weeklyHours,
                    JobBase = 28m,
                },
            ],
        };
        db.EmploymentData.Add(ed);
        await db.SaveChangesAsync();
        return ed;
    }
}
