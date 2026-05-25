using AccountingProject.Controllers;
using AccountingProject.Models;
using AccountingProject.Services;
using AccountingProject.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;

namespace AccountingProject.Tests;

public sealed class EmployeeLookupTests
{
    [Fact]
    public async Task LookupByEmployerAndIdNumber_ReturnsEmployerScopedEmployee()
    {
        await using var db = DbTestFactory.CreateContext();
        var employer1 = new Employer { Name = "Employer 1" };
        var employer2 = new Employer { Name = "Employer 2" };
        db.Employers.AddRange(employer1, employer2);
        await db.SaveChangesAsync();

        db.Employees.AddRange(
            new Employee
            {
                EmployerId = employer1.Id,
                IdNumber = "777777777",
                FirstName = "Scoped",
                LastName = "One",
                Gender = "זכר",
                BirthDate = new DateOnly(1990, 1, 1),
            },
            new Employee
            {
                EmployerId = employer2.Id,
                IdNumber = "777777777",
                FirstName = "Scoped",
                LastName = "Two",
                Gender = "נקבה",
                BirthDate = new DateOnly(1991, 1, 1),
            });
        await db.SaveChangesAsync();

        var sut = new EmployeeService(db);
        var found = await sut.GetByEmployerAndIdNumberAsync(employer2.Id, "777777777");

        Assert.NotNull(found);
        Assert.Equal(employer2.Id, found!.EmployerId);
        Assert.Equal("Two", found.LastName);
    }

    [Fact]
    public void LookupByIdNumberOnlyEndpoint_ReturnsBadRequest()
    {
        var controller = new EmployeesController(new NoopEmployeeService());

        var action = controller.GetByIdNumberDeprecated("777777777");

        var badRequest = Assert.IsType<BadRequestObjectResult>(action);
        var payload = badRequest.Value?.ToString() ?? string.Empty;
        Assert.Contains("לא ניתן לחפש עובד לפי מספר תעודת זהות בלבד", payload);
    }
}
