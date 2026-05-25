using System.Net.Http.Json;
using System.Text.Json;

namespace AccountingProject.Tests.Integration;

public sealed class EmploymentDataApiIntegrationTests
{
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    [Fact]
    public async Task EmploymentData_GetByEmployee_ReturnsEmptyArrayInitially()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);

        var er = await client.PostAsJsonAsync(
            "/api/employers",
            new Contracts.EmployerDto { Name = "Emp Employment Data" });
        er.EnsureSuccessStatusCode();
        var employer = await er.Content.ReadFromJsonAsync<EmployerIdJson>(Json);
        Assert.NotNull(employer);

        var empResp = await client.PostAsJsonAsync("/api/employees", new Contracts.EmployeeDto
        {
            EmployerId = employer!.Id,
            IdNumber = "888777",
            FirstName = "Ed",
            LastName = "Test",
            Gender = "נקבה",
            BirthDate = "1993-03-03",
        });
        empResp.EnsureSuccessStatusCode();
        var employee = await empResp.Content.ReadFromJsonAsync<EmployeeIdJson>(Json);
        Assert.NotNull(employee?.Id);

        var getResp = await client.GetAsync(
            $"/api/employment-data/employee/{employee!.Id}/employer/{employer.Id}");
        getResp.EnsureSuccessStatusCode();
        var arr = await getResp.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(JsonValueKind.Array, arr.ValueKind);
        Assert.Equal(0, arr.GetArrayLength());
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
