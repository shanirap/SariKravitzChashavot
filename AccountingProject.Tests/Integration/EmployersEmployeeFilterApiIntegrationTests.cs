using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AccountingProject.Contracts;
using AccountingProject.Tests.TestHelpers;

namespace AccountingProject.Tests.Integration;

public sealed class EmployersEmployeeFilterApiIntegrationTests
{
    private const string Year = "תשפ\"ו";

    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    [Fact]
    public async Task GetEmployees_IsActiveTrue_ReturnsOnlyActiveEmployees()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);
        var seed = await SeedFilterScenarioAsync(client);

        var resp = await client.GetAsync(
            $"/api/employers/{seed.EmployerId}/employees?isActive=true");
        resp.EnsureSuccessStatusCode();
        var page = await resp.Content.ReadFromJsonAsync<EmployeePageJson>(Json);

        Assert.NotNull(page);
        Assert.Equal(1, page!.TotalCount);
        Assert.Equal(seed.ActiveEmployeeId, page.Items![0].Id);
        Assert.True(page.Items[0].IsActive);
    }

    [Fact]
    public async Task GetEmployees_IsActiveFalse_ReturnsInactiveEmployees()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);
        var seed = await SeedFilterScenarioAsync(client);

        var resp = await client.GetAsync(
            $"/api/employers/{seed.EmployerId}/employees?isActive=false");
        resp.EnsureSuccessStatusCode();
        var page = await resp.Content.ReadFromJsonAsync<EmployeePageJson>(Json);

        Assert.NotNull(page);
        Assert.Equal(2, page!.TotalCount);
        Assert.Contains(page.Items!, e => e.Id == seed.InactiveManualId);
        Assert.Contains(page.Items!, e => e.Id == seed.NoEmploymentId);
    }

    [Fact]
    public async Task GetEmployees_InstitutionSymbol_ReturnsMatchingEmployeesOnly()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);
        var seed = await SeedFilterScenarioAsync(client);

        var resp = await client.GetAsync(
            $"/api/employers/{seed.EmployerId}/employees?institutionSymbol=SYM-A");
        resp.EnsureSuccessStatusCode();
        var page = await resp.Content.ReadFromJsonAsync<EmployeePageJson>(Json);

        Assert.NotNull(page);
        Assert.Equal(2, page!.TotalCount);
        Assert.Contains(page.Items!, e => e.Id == seed.ActiveEmployeeId);
        Assert.Contains(page.Items!, e => e.Id == seed.InactiveManualId);
        Assert.DoesNotContain(page.Items!, e => e.Id == seed.NoEmploymentId);
    }

    [Fact]
    public async Task GetEmployees_ActiveAndInstitutionSymbol_CombinesFilters()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);
        var seed = await SeedFilterScenarioAsync(client);

        var resp = await client.GetAsync(
            $"/api/employers/{seed.EmployerId}/employees?isActive=true&institutionSymbol=SYM-A");
        resp.EnsureSuccessStatusCode();
        var page = await resp.Content.ReadFromJsonAsync<EmployeePageJson>(Json);

        Assert.NotNull(page);
        Assert.Equal(1, page!.TotalCount);
        Assert.Equal(seed.ActiveEmployeeId, page.Items![0].Id);
    }

    private static async Task<FilterSeed> SeedFilterScenarioAsync(HttpClient client)
    {
        var er = await client.PostAsJsonAsync("/api/employers", new EmployerDto { Name = "Filter API Employer" });
        er.EnsureSuccessStatusCode();
        var employer = await er.Content.ReadFromJsonAsync<IdJson>(Json);
        var employerId = employer!.Id;

        foreach (var sym in new[] { "SYM-A", "SYM-B" })
        {
            (await client.PostAsJsonAsync($"/api/employers/{employerId}/institution-symbols",
                new EmployerInstitutionSymbolDto { InstitutionSymbol = sym, InstitutionSymbolName = sym }))
                .EnsureSuccessStatusCode();
        }

        var activeEmp = await CreateEmployeeAsync(client, employerId, "100100100", "Active", "Worker");
        var inactiveEmp = await CreateEmployeeAsync(client, employerId, "200200200", "Inactive", "Worker");
        var noEdEmp = await CreateEmployeeAsync(client, employerId, "300300300", "No", "Employment");

        await client.PatchAsync(
            $"/api/employees/{inactiveEmp}/active-status",
            JsonContent.Create(new { isActive = false }));

        await CreateEmploymentAsync(client, employerId, activeEmp, "SYM-A", secondSymbol: "SYM-B");
        await CreateEmploymentAsync(client, employerId, inactiveEmp, "SYM-A");

        return new FilterSeed(employerId, activeEmp, inactiveEmp, noEdEmp);
    }

    private static async Task<int> CreateEmployeeAsync(
        HttpClient client, int employerId, string idNumber, string first, string last)
    {
        var resp = await client.PostAsJsonAsync("/api/employees", new EmployeeDto
        {
            EmployerId = employerId,
            IdNumber = idNumber,
            FirstName = first,
            LastName = last,
            Gender = "נקבה",
            BirthDate = "1990-01-01",
        });
        resp.EnsureSuccessStatusCode();
        var emp = await resp.Content.ReadFromJsonAsync<IdJson>(Json);
        return emp!.Id;
    }

    private static async Task CreateEmploymentAsync(
        HttpClient client,
        int employerId,
        int employeeId,
        string symbol,
        string? secondSymbol = null)
    {
        var slots = new List<EmploymentDataSlotDto>
        {
            new()
            {
                GradeBand = 1,
                SlotIndex = 1,
                InstitutionSymbol = symbol,
                WeeklyHours = 30m,
                JobBase = 30m,
            },
        };
        if (secondSymbol != null)
        {
            slots.Add(new EmploymentDataSlotDto
            {
                GradeBand = 2,
                SlotIndex = 1,
                InstitutionSymbol = secondSymbol,
                WeeklyHours = 10m,
                JobBase = 30m,
            });
        }

        var dto = new EmploymentDataDto
        {
            EmployeeId = employeeId,
            EmployerId = employerId,
            AcademicYear = Year,
            Grade1GradeName = "יסודי וגנים",
            Grade1Role = "גננת ראשית",
            Grade1Grade = "ב",
            Grade1Seniority = "1",
            Grade2GradeName = secondSymbol == null ? null : "יסודי וגנים",
            Grade2Role = secondSymbol == null ? null : "גננת ראשית",
            Grade2Grade = secondSymbol == null ? null : "ב",
            Grade2Seniority = secondSymbol == null ? null : "1",
            Slots = slots,
        };
        (await client.PostAsJsonAsync("/api/employment-data", dto)).EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GetEmployees_ReturnsChildBirthDates()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);

        var er = await client.PostAsJsonAsync("/api/employers", new { name = "Child Dates List Employer" });
        er.EnsureSuccessStatusCode();
        var employer = await er.Content.ReadFromJsonAsync<IdJson>(Json);
        Assert.NotNull(employer);

        var create = await client.PostAsJsonAsync("/api/employees", new
        {
            employerId = employer!.Id,
            idNumber = "123456789",
            firstName = "רחל",
            lastName = "כהן",
            gender = "נקבה",
            birthDate = "1985-01-01",
            childBirthDate1 = "2015-03-15",
        });
        create.EnsureSuccessStatusCode();
        var created = await create.Content.ReadFromJsonAsync<IdJson>(Json);
        Assert.NotNull(created);

        var resp = await client.GetAsync($"/api/employers/{employer.Id}/employees");
        resp.EnsureSuccessStatusCode();
        var page = await resp.Content.ReadFromJsonAsync<EmployeePageJson>(Json);

        Assert.NotNull(page);
        var item = Assert.Single(page!.Items!);
        Assert.Equal(created!.Id, item.Id);
        Assert.Equal("2015-03-15", item.ChildBirthDate1);
    }

    private sealed record FilterSeed(
        int EmployerId,
        int ActiveEmployeeId,
        int InactiveManualId,
        int NoEmploymentId);

    private sealed class IdJson
    {
        public int Id { get; set; }
    }

    private sealed class EmployeePageJson
    {
        public List<EmployeeListItemJson>? Items { get; set; }
        public int TotalCount { get; set; }
    }

    private sealed class EmployeeListItemJson
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public bool HasEmploymentData { get; set; }
        public string? ChildBirthDate1 { get; set; }
    }
}
