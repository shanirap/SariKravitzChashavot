using System.Net.Http.Json;
using System.Text.Json;
using AccountingProject.Contracts;
using AccountingProject.Domain;

namespace AccountingProject.Tests.Integration;

public sealed class EmploymentDataMotherBenefitApiIntegrationTests
{
    private const string Year = "תשפ\"ו";

    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    [Fact]
    public async Task CreateEmploymentData_FemaleWithEligibleChild_ComputesMotherBenefitViaApi()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);

        var er = await client.PostAsJsonAsync("/api/employers", new EmployerDto { Name = "Mother Benefit API" });
        er.EnsureSuccessStatusCode();
        var employer = await er.Content.ReadFromJsonAsync<IdJson>(Json);

        (await client.PostAsJsonAsync($"/api/employers/{employer!.Id}/institution-symbols",
            new EmployerInstitutionSymbolDto { InstitutionSymbol = "MB-1", InstitutionSymbolName = "Garden" }))
            .EnsureSuccessStatusCode();

        var empResp = await client.PostAsJsonAsync("/api/employees", new EmployeeDto
        {
            EmployerId = employer.Id,
            IdNumber = "121212121",
            FirstName = "Mother",
            LastName = "Benefit",
            Gender = "נקבה",
            BirthDate = "1988-05-05",
            ChildBirthDate1 = "2012-03-15",
        });
        empResp.EnsureSuccessStatusCode();
        var employee = await empResp.Content.ReadFromJsonAsync<IdJson>(Json);

        var edResp = await client.PostAsJsonAsync("/api/employment-data", new EmploymentDataDto
        {
            EmployeeId = employee!.Id,
            EmployerId = employer.Id,
            AcademicYear = Year,
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
                    InstitutionSymbol = "MB-1",
                    WeeklyHours = 30m,
                    JobBase = 30m,
                },
            ],
        });
        edResp.EnsureSuccessStatusCode();
        var created = await edResp.Content.ReadFromJsonAsync<EmploymentDataJson>(Json);

        Assert.Equal(10m, created!.Grade1MotherBenefitPercent);
    }

    [Fact]
    public async Task CreateEmploymentData_FemaleAhid_ComputesZeroMotherBenefitViaApi()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);

        var er = await client.PostAsJsonAsync("/api/employers", new EmployerDto { Name = "Ahid Mother Benefit" });
        er.EnsureSuccessStatusCode();
        var employer = await er.Content.ReadFromJsonAsync<IdJson>(Json);

        (await client.PostAsJsonAsync($"/api/employers/{employer!.Id}/institution-symbols",
            new EmployerInstitutionSymbolDto { InstitutionSymbol = "AH-1" }))
            .EnsureSuccessStatusCode();

        var empResp = await client.PostAsJsonAsync("/api/employees", new EmployeeDto
        {
            EmployerId = employer.Id,
            IdNumber = "131313131",
            FirstName = "Ahid",
            LastName = "Worker",
            Gender = "נקבה",
            BirthDate = "1988-05-05",
            ChildBirthDate1 = "2012-03-15",
        });
        empResp.EnsureSuccessStatusCode();
        var employee = await empResp.Content.ReadFromJsonAsync<IdJson>(Json);

        var edResp = await client.PostAsJsonAsync("/api/employment-data", new EmploymentDataDto
        {
            EmployeeId = employee!.Id,
            EmployerId = employer.Id,
            AcademicYear = Year,
            Grade1GradeName = GradeOptions.UnifiedEducationSupportGradeName,
            Grade1Role = "סייעת ראשית",
            Grade1Grade = "תומכת חינוך",
            Grade1Seniority = "1",
            Slots =
            [
                new EmploymentDataSlotDto
                {
                    GradeBand = 1,
                    SlotIndex = 1,
                    InstitutionSymbol = "AH-1",
                    WeeklyHours = 30m,
                    JobBase = 30m,
                },
            ],
        });
        edResp.EnsureSuccessStatusCode();
        var created = await edResp.Content.ReadFromJsonAsync<EmploymentDataJson>(Json);

        Assert.Equal(0m, created!.Grade1MotherBenefitPercent);
        Assert.Equal(GradeOptions.UnifiedEducationSupportGradeName, created.Grade1GradeName);
    }

    [Fact]
    public async Task CreateEmploymentData_AcceptsDecimalSeniorityViaApi()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);

        var er = await client.PostAsJsonAsync("/api/employers", new EmployerDto { Name = "Decimal Seniority API" });
        er.EnsureSuccessStatusCode();
        var employer = await er.Content.ReadFromJsonAsync<IdJson>(Json);

        (await client.PostAsJsonAsync($"/api/employers/{employer!.Id}/institution-symbols",
            new EmployerInstitutionSymbolDto { InstitutionSymbol = "DS-1" }))
            .EnsureSuccessStatusCode();

        var empResp = await client.PostAsJsonAsync("/api/employees", new EmployeeDto
        {
            EmployerId = employer.Id,
            IdNumber = "141414141",
            FirstName = "Decimal",
            LastName = "Seniority",
            Gender = "זכר",
            BirthDate = "1988-05-05",
        });
        empResp.EnsureSuccessStatusCode();
        var employee = await empResp.Content.ReadFromJsonAsync<IdJson>(Json);

        var edResp = await client.PostAsJsonAsync("/api/employment-data", new EmploymentDataDto
        {
            EmployeeId = employee!.Id,
            EmployerId = employer.Id,
            AcademicYear = Year,
            Grade1GradeName = GradeOptions.LegacyUnifiedGradeName,
            Grade1Role = "סייעת ראשית",
            Grade1Grade = "תומכת חינוך",
            Grade1Seniority = "5.5",
            Slots =
            [
                new EmploymentDataSlotDto
                {
                    GradeBand = 1,
                    SlotIndex = 1,
                    InstitutionSymbol = "DS-1",
                    WeeklyHours = 30m,
                    JobBase = 40m,
                },
            ],
        });
        edResp.EnsureSuccessStatusCode();
        var created = await edResp.Content.ReadFromJsonAsync<EmploymentDataJson>(Json);

        Assert.Equal("5.5", created!.Grade1Seniority);
        Assert.Equal(GradeOptions.UnifiedEducationSupportGradeName, created.Grade1GradeName);
    }

    private sealed class IdJson
    {
        public int Id { get; set; }
    }

    private sealed class EmploymentDataJson
    {
        public decimal? Grade1MotherBenefitPercent { get; set; }
        public string? Grade1Seniority { get; set; }
        public string? Grade1GradeName { get; set; }
    }
}
