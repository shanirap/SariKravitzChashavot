using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AccountingProject.Contracts;
using AccountingProject.Domain;
using AccountingProject.Tests.TestHelpers;

namespace AccountingProject.Tests.Integration;

public sealed class InvalidAcademicYearApiIntegrationTests
{
    private const string ValidHebrewYear = "תשפ\"ו";
    private const string InvalidYear = "xyz123";

    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    [Fact]
    public async Task EmploymentData_Create_InvalidAcademicYear_Returns400()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);
        var seed = await SeedEmployeeScenarioAsync(client);

        var resp = await client.PostAsJsonAsync("/api/employment-data", BuildEmploymentDto(
            seed.EmployeeId, seed.EmployerId, InvalidYear));

        await IntegrationResponseAssert.AssertBadRequestMessageAsync(resp, HebrewAcademicYear.InvalidMessage);
    }

    [Fact]
    public async Task EmploymentData_Update_InvalidAcademicYear_Returns400()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);
        var seed = await SeedEmployeeScenarioAsync(client);

        var createResp = await client.PostAsJsonAsync("/api/employment-data", BuildEmploymentDto(
            seed.EmployeeId, seed.EmployerId, ValidHebrewYear));
        createResp.EnsureSuccessStatusCode();
        var created = await createResp.Content.ReadFromJsonAsync<EmploymentIdJson>(Json);
        Assert.NotNull(created);

        var updateResp = await client.PutAsJsonAsync(
            $"/api/employment-data/{created!.Id}",
            BuildEmploymentDto(seed.EmployeeId, seed.EmployerId, InvalidYear));

        await IntegrationResponseAssert.AssertBadRequestMessageAsync(updateResp, HebrewAcademicYear.InvalidMessage);
    }

    [Theory]
    [InlineData("/api/reports/kindergarten-annual?employerId=1&academicYear=")]
    [InlineData("/api/reports/school-annual?employerId=1&academicYear=")]
    [InlineData("/api/reports/employees-employment-data?employerId=1&academicYear=")]
    [InlineData("/api/reports/annual-comparison-saved?employerId=1&academicYear=")]
    [InlineData("/api/reports/annual-comparison-saved/preview?employerId=1&academicYear=")]
    [InlineData("/api/payroll-monthly-inputs/status?employerId=1&academicYear=")]
    public async Task ReportGetEndpoints_InvalidAcademicYear_Returns400(string urlPrefix)
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);

        var resp = await client.GetAsync($"{urlPrefix}{Uri.EscapeDataString(InvalidYear)}");

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        var message = await IntegrationResponseAssert.ReadMessageAsync(resp);
        Assert.Contains(HebrewAcademicYear.InvalidMessage, message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(ValidHebrewYear)]
    [InlineData("5786")]
    [InlineData("2026")]
    public async Task EmploymentData_Create_ValidAcademicYearFormats_ReturnSuccess(string academicYear)
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);
        var seed = await SeedEmployeeScenarioAsync(client);

        var resp = await client.PostAsJsonAsync("/api/employment-data", BuildEmploymentDto(
            seed.EmployeeId, seed.EmployerId, academicYear));

        resp.EnsureSuccessStatusCode();
        var created = await resp.Content.ReadFromJsonAsync<JsonElement>(Json);
        Assert.Equal(ValidHebrewYear, created.GetProperty("academicYear").GetString());
    }

    private static EmploymentDataDto BuildEmploymentDto(int employeeId, int employerId, string academicYear) =>
        new()
        {
            EmployeeId = employeeId,
            EmployerId = employerId,
            AcademicYear = academicYear,
            Grade1GradeName = "יסודי וגנים",
            Grade1Role = "גננת ראשית",
            Grade1Grade = "ב",
            Grade1Seniority = "1",
            Slots =
            [
                new EmploymentDataSlotDto
                {
                    GradeBand = 1,
                    SlotIndex = 1,
                    InstitutionSymbol = "SYM-Y",
                    WeeklyHours = 30m,
                    JobBase = 30m,
                },
            ],
        };

    private static async Task<EmployeeScenarioSeed> SeedEmployeeScenarioAsync(HttpClient client)
    {
        var er = await client.PostAsJsonAsync("/api/employers", new EmployerDto { Name = "Invalid Year Test" });
        er.EnsureSuccessStatusCode();
        var employer = await er.Content.ReadFromJsonAsync<IdJson>(Json);

        (await client.PostAsJsonAsync($"/api/employers/{employer!.Id}/institution-symbols",
            new EmployerInstitutionSymbolDto { InstitutionSymbol = "SYM-Y", InstitutionSymbolName = "Y" }))
            .EnsureSuccessStatusCode();

        var empResp = await client.PostAsJsonAsync("/api/employees", new EmployeeDto
        {
            EmployerId = employer.Id,
            IdNumber = "909090909",
            FirstName = "Year",
            LastName = "Test",
            Gender = "נקבה",
            BirthDate = "1990-01-01",
        });
        empResp.EnsureSuccessStatusCode();
        var employee = await empResp.Content.ReadFromJsonAsync<IdJson>(Json);

        return new EmployeeScenarioSeed(employer.Id, employee!.Id);
    }

    private sealed record EmployeeScenarioSeed(int EmployerId, int EmployeeId);

    private sealed class IdJson
    {
        public int Id { get; set; }
    }

    private sealed class EmploymentIdJson
    {
        public int Id { get; set; }
    }
}
