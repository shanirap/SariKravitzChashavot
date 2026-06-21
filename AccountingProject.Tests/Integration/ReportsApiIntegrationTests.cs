using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AccountingProject.Tests.TestHelpers;
using ClosedXML.Excel;

namespace AccountingProject.Tests.Integration;

public sealed class ReportsApiIntegrationTests
{
    private const string Year = "תשפ\"ו";

    [Fact]
    public async Task Reports_AllIssuanceEndpoints_ReturnXlsxWhenSeeded()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);

        var employerId = await SeedEmployerWithDataViaApiAsync(client);

        await AssertXlsxGetAsync(client,
            $"/api/reports/kindergarten-annual?employerId={employerId}&academicYear={Uri.EscapeDataString(Year)}");
        await AssertXlsxGetAsync(client,
            $"/api/reports/school-annual?employerId={employerId}&academicYear={Uri.EscapeDataString(Year)}");
        await AssertXlsxGetAsync(client, $"/api/reports/employees-personal?employerId={employerId}");
        await AssertXlsxGetAsync(client,
            $"/api/reports/employees-employment-data?employerId={employerId}&academicYear={Uri.EscapeDataString(Year)}");
        await AssertXlsxGetAsync(client,
            $"/api/reports/institution-hours?employerId={employerId}&academicYear={Uri.EscapeDataString(Year)}&institutionSymbol=G-API");

        await using var upload = MonthlyComparisonUploadWorkbook.Create(
            "999888777", null, "Report Worker", 9, 2025, b => b.Band1());
        using var mp = new MultipartFormDataContent();
        var fileContent = new StreamContent(upload);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        mp.Add(fileContent, "file", "payroll.xlsx");

        var monthlyResp = await client.PostAsync(
            $"/api/reports/monthly-comparison?employerId={employerId}&academicYear={Uri.EscapeDataString(Year)}&month=9",
            mp);
        Assert.Equal(HttpStatusCode.OK, monthlyResp.StatusCode);
        Assert.Equal(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            monthlyResp.Content.Headers.ContentType?.MediaType);
        Assert.True((await monthlyResp.Content.ReadAsByteArrayAsync()).Length > 64);

        upload.Position = 0;
        using var mp2 = new MultipartFormDataContent();
        var fileContent2 = new StreamContent(upload);
        fileContent2.Headers.ContentType = new MediaTypeHeaderValue(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        mp2.Add(fileContent2, "file", "payroll.xlsx");

        var annualResp = await client.PostAsync(
            $"/api/reports/annual-comparison?employerId={employerId}&academicYear={Uri.EscapeDataString(Year)}",
            mp2);
        Assert.Equal(HttpStatusCode.OK, annualResp.StatusCode);
        Assert.True((await annualResp.Content.ReadAsByteArrayAsync()).Length > 64);

        await AssertXlsxGetAsync(client,
            $"/api/reports/annual-comparison-saved?employerId={employerId}&academicYear={Uri.EscapeDataString(Year)}");
    }

    [Fact]
    public async Task Employers_ComparisonMonthlyPayroll_WithValidUpload_ReturnsXlsx()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);
        var employerId = await SeedEmployerWithDataViaApiAsync(client);

        await using var upload = ExcelTestWorkbook.CreatePayrollComparisonUpload("999888777", 9, 2025);
        using var mp = new MultipartFormDataContent();
        var fileContent = new StreamContent(upload);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        mp.Add(fileContent, "file", "payroll.xlsx");

        var resp = await client.PostAsync(
            $"/api/employers/{employerId}/comparison/monthly-payroll",
            mp);

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var bytes = await resp.Content.ReadAsByteArrayAsync();
        Assert.True(bytes.Length > 64);
        using var wb = new XLWorkbook(new MemoryStream(bytes));
        Assert.NotNull(wb.Worksheet("Comparison"));
    }

    private static async Task AssertXlsxGetAsync(HttpClient client, string url)
    {
        var resp = await client.GetAsync(url);
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        Assert.Equal(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            resp.Content.Headers.ContentType?.MediaType);
        Assert.True((await resp.Content.ReadAsByteArrayAsync()).Length > 64);
    }

    internal static async Task<int> SeedEmployerWithDataViaApiAsync(HttpClient client)
    {
        var er = await client.PostAsJsonAsync("/api/employers", new Contracts.EmployerDto
        {
            Name = "Reports API Employer",
            BusinessNumber = "514000001",
        });
        er.EnsureSuccessStatusCode();
        var employer = await er.Content.ReadFromJsonAsync<IdJson>();
        Assert.NotNull(employer);

        await client.PostAsJsonAsync($"/api/employers/{employer!.Id}/institution-symbols",
            new Contracts.EmployerInstitutionSymbolDto
            {
                InstitutionSymbol = "G-API",
                InstitutionSymbolName = "Garden",
                InstitutionType = "גן",
            });

        var empResp = await client.PostAsJsonAsync("/api/employees", new Contracts.EmployeeDto
        {
            EmployerId = employer.Id,
            IdNumber = "999888777",
            FirstName = "Report",
            LastName = "Worker",
            Gender = "נקבה",
            BirthDate = "1990-01-01",
        });
        empResp.EnsureSuccessStatusCode();
        var employee = await empResp.Content.ReadFromJsonAsync<IdJson>();
        Assert.NotNull(employee);

        var edResp = await client.PostAsJsonAsync("/api/employment-data", new Contracts.EmploymentDataDto
        {
            EmployeeId = employee!.Id,
            EmployerId = employer.Id,
            AcademicYear = Year,
            Grade1GradeName = "יסודי וגנים",
            Grade1Role = "מורה מקצועי",
            Grade1Grade = "ב",
            Grade1Seniority = "5",
            Slots =
            [
                new Contracts.EmploymentDataSlotDto
                {
                    GradeBand = 1,
                    SlotIndex = 1,
                    InstitutionSymbol = "G-API",
                    WeeklyHours = 30m,
                    JobBase = 30m,
                },
            ],
        });
        edResp.EnsureSuccessStatusCode();

        return employer.Id;
    }

    private sealed class IdJson
    {
        public int Id { get; set; }
    }
}
