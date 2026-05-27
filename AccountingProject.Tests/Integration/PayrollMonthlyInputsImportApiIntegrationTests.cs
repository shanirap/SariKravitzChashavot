using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AccountingProject.Contracts;
using AccountingProject.Tests.TestHelpers;

namespace AccountingProject.Tests.Integration;

public sealed class PayrollMonthlyInputsImportApiIntegrationTests
{
    private const string Year = "תשפ\"ו";
    private const int Month = 9;

    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    [Fact]
    public async Task Import_ValidOketsFile_ReturnsOkAndStatusCaptured()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);
        var employerId = await CreateEmployerAsync(client);

        var importResp = await PostImportAsync(client, employerId, Month, "okets.xlsx");
        if (!importResp.IsSuccessStatusCode)
        {
            var body = await IntegrationResponseAssert.ReadMessageAsync(importResp);
            Assert.Fail($"Import failed {(int)importResp.StatusCode}: {body}");
        }

        var result = await importResp.Content.ReadFromJsonAsync<PayrollImportResultDto>(Json);
        Assert.NotNull(result);
        Assert.True(result!.RowsCount > 0);

        var statusResp = await client.GetAsync(
            $"/api/payroll-monthly-inputs/status?employerId={employerId}&academicYear={Uri.EscapeDataString(Year)}");
        statusResp.EnsureSuccessStatusCode();
        var months = await statusResp.Content.ReadFromJsonAsync<List<PayrollMonthStatusDto>>(Json);
        var september = months!.Single(m => m.Month == Month);
        Assert.Equal("נקלט", september.Status);
        Assert.Equal(result.RowsCount, september.RowsCount);
        Assert.Equal("okets.xlsx", september.OriginalFileName);
    }

    [Fact]
    public async Task Import_MissingFile_Returns400()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);
        var employerId = await CreateEmployerAsync(client);

        var resp = await client.PostAsync(
            $"/api/payroll-monthly-inputs/import?employerId={employerId}&academicYear={Uri.EscapeDataString(Year)}&month={Month}",
            null);

        await IntegrationResponseAssert.AssertBadRequestMessageAsync(resp, "לא הועלה קובץ תקין");
    }

    [Fact]
    public async Task Import_InvalidMonth_Returns400()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);
        var employerId = await CreateEmployerAsync(client);

        var resp = await PostImportAsync(client, employerId, 13, "bad-month.xlsx");

        await IntegrationResponseAssert.AssertBadRequestMessageAsync(resp, "חודש");
    }

    [Fact]
    public async Task Import_XlsmExtension_Returns400WithStrictXlsxMessage()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);
        var employerId = await CreateEmployerAsync(client);

        using var mp = new MultipartFormDataContent();
        var bytes = new byte[] { 0x50, 0x4B, 0x03, 0x04 };
        var fileContent = new ByteArrayContent(bytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(
            "application/vnd.ms-excel.sheet.macroEnabled.12");
        mp.Add(fileContent, "file", "macro.xlsm");

        var resp = await client.PostAsync(
            $"/api/payroll-monthly-inputs/import?employerId={employerId}&academicYear={Uri.EscapeDataString(Year)}&month={Month}",
            mp);

        await IntegrationResponseAssert.AssertBadRequestMessageAsync(resp, ".xlsm");
    }

    [Fact]
    public async Task Import_ReimportSameMonth_UpdatesStatusToLatestFile()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);
        var employerId = await CreateEmployerAsync(client);

        (await PostImportAsync(client, employerId, Month, "first.xlsx")).EnsureSuccessStatusCode();
        (await PostImportAsync(client, employerId, Month, "second.xlsx")).EnsureSuccessStatusCode();

        var statusResp = await client.GetAsync(
            $"/api/payroll-monthly-inputs/status?employerId={employerId}&academicYear={Uri.EscapeDataString(Year)}");
        statusResp.EnsureSuccessStatusCode();
        var months = await statusResp.Content.ReadFromJsonAsync<List<PayrollMonthStatusDto>>(Json);
        var september = months!.Single(m => m.Month == Month);
        Assert.Equal("second.xlsx", september.OriginalFileName);
    }

    [Fact]
    public async Task Import_UnknownEmployer_Returns400EmployerNotFound()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);

        var resp = await PostImportAsync(client, 99999, Month, "orphan.xlsx");

        await IntegrationResponseAssert.AssertBadRequestMessageAsync(resp, "המעסיק לא נמצא");
    }

    private static async Task<int> CreateEmployerAsync(HttpClient client)
    {
        var resp = await client.PostAsJsonAsync("/api/employers", new EmployerDto
        {
            Name = "Payroll Import API Employer",
        });
        resp.EnsureSuccessStatusCode();
        var created = await resp.Content.ReadFromJsonAsync<EmployerIdJson>(Json);
        return created!.Id;
    }

    private static async Task<HttpResponseMessage> PostImportAsync(
        HttpClient client,
        int employerId,
        int month,
        string fileName)
    {
        await using var upload = MonthlyComparisonUploadWorkbook.Create(
            "123456789",
            1001,
            "Import Worker",
            Month,
            2025,
            b => b.Band1());

        using var mp = new MultipartFormDataContent();
        var fileContent = new StreamContent(upload);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        mp.Add(fileContent, "file", fileName);

        return await client.PostAsync(
            $"/api/payroll-monthly-inputs/import?employerId={employerId}&academicYear={Uri.EscapeDataString(Year)}&month={month}",
            mp);
    }

    private sealed class EmployerIdJson
    {
        public int Id { get; set; }
    }
}
