using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AccountingProject.Contracts;
namespace AccountingProject.Tests.Integration;

public sealed class EmployeesApiIntegrationTests
{
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    [Fact]
    public async Task Employees_ByIdNumberOnly_Returns400_WithHebrewMessage()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);

        var resp = await client.GetAsync("/api/employees/by-id-number/123456789");

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        var body = await resp.Content.ReadAsStringAsync();
        Assert.Contains("מעסיק", body);
    }

    [Fact]
    public async Task Employees_Create_Update_Delete_RoundTrip()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);

        var er = await client.PostAsJsonAsync("/api/employers", new EmployerDto { Name = "Emp For Employees" });
        er.EnsureSuccessStatusCode();
        var employer = await er.Content.ReadFromJsonAsync<EmployerIdJson>(Json);
        var employerId = employer!.Id;

        var hintResp = await client.GetAsync($"/api/employees/precreate-hint?employerId={employerId}&idNumber=999001");
        hintResp.EnsureSuccessStatusCode();

        var createResp = await client.PostAsJsonAsync("/api/employees", new EmployeeDto
        {
            EmployerId = employerId,
            IdNumber = "999001",
            FirstName = "Test",
            LastName = "Worker",
            Gender = "זכר",
            BirthDate = "1991-06-15",
        });
        Assert.True(
            createResp.StatusCode is HttpStatusCode.OK or HttpStatusCode.Created,
            "expected 200 (existing) or 201 (created)");
        var emp = await createResp.Content.ReadFromJsonAsync<EmployeeIdJson>(Json);
        Assert.NotNull(emp?.Id);

        var updateResp = await client.PutAsJsonAsync($"/api/employees/{emp!.Id}", new EmployeeDto
        {
            EmployerId = employerId,
            IdNumber = "999001",
            FirstName = "Test",
            LastName = "Updated",
            Gender = "זכר",
            BirthDate = "1991-06-15",
            Phone = "0500000000",
        });
        Assert.Equal(HttpStatusCode.NoContent, updateResp.StatusCode);

        var patchResp = await client.PatchAsync(
            $"/api/employees/{emp.Id}/active-status",
            JsonContent.Create(new { isActive = false }));
        Assert.Equal(HttpStatusCode.NoContent, patchResp.StatusCode);

        var delResp = await client.DeleteAsync($"/api/employees/{emp.Id}");
        Assert.Equal(HttpStatusCode.NoContent, delResp.StatusCode);
    }

    private sealed class EmployerIdJson
    {
        public int Id { get; set; }
    }

    private sealed class EmployeeIdJson
    {
        public int Id { get; set; }
    }
}
