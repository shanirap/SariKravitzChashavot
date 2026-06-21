using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AccountingProject.Contracts;
using AccountingProject.Tests.TestHelpers;
using ClosedXML.Excel;

namespace AccountingProject.Tests.Integration;

public sealed class AnnualComparisonSavedOverridesApiIntegrationTests
{
    private const string Year = "תשפ\"ו";
    private const int Month = 9;

    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    [Fact]
    public async Task Preview_ReturnsRowsAfterSavedImport()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);
        var seed = await SeedScenarioAsync(client);

        var resp = await client.GetAsync(
            $"/api/reports/annual-comparison-saved/preview?employerId={seed.EmployerId}&academicYear={Uri.EscapeDataString(Year)}");
        resp.EnsureSuccessStatusCode();
        var preview = await resp.Content.ReadFromJsonAsync<AnnualComparisonPreviewDto>(Json);

        Assert.NotNull(preview);
        Assert.Equal(Year, preview!.AcademicYear);
        Assert.Single(preview.Rows);
        Assert.Equal(seed.SlotId, preview.Rows[0].SlotId);
        Assert.Equal("Worker Report", preview.Rows[0].FullName.Display);
    }

    [Fact]
    public async Task SaveAndClearOverrides_RoundTripViaApi()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);
        var seed = await SeedScenarioAsync(client);

        var saveResp = await client.PutAsJsonAsync("/api/reports/annual-comparison-saved/overrides",
            new AnnualComparisonOverrideSaveRequest
            {
                EmployerId = seed.EmployerId,
                AcademicYear = Year,
                Rows =
                [
                    new AnnualComparisonOverrideRowSaveDto
                    {
                        SlotId = seed.SlotId,
                        FullName = "שם מ-API",
                        MonthCells = new Dictionary<string, string> { ["9.2025"] = "V-API" },
                    },
                ],
            });
        saveResp.EnsureSuccessStatusCode();

        var previewResp = await client.GetAsync(
            $"/api/reports/annual-comparison-saved/preview?employerId={seed.EmployerId}&academicYear={Uri.EscapeDataString(Year)}");
        previewResp.EnsureSuccessStatusCode();
        var preview = await previewResp.Content.ReadFromJsonAsync<AnnualComparisonPreviewDto>(Json);
        Assert.Equal("שם מ-API", preview!.Rows[0].FullName.Display);
        Assert.True(preview.Rows[0].FullName.IsOverridden);

        var exportResp = await client.GetAsync(
            $"/api/reports/annual-comparison-saved?employerId={seed.EmployerId}&academicYear={Uri.EscapeDataString(Year)}");
        exportResp.EnsureSuccessStatusCode();
        using var wb = new XLWorkbook(new MemoryStream(await exportResp.Content.ReadAsByteArrayAsync()));
        Assert.Equal("שם מ-API", wb.Worksheet("השוואה שנתית").Cell(2, 2).GetString());

        var clearResp = await client.DeleteAsync(
            $"/api/reports/annual-comparison-saved/overrides?employerId={seed.EmployerId}&academicYear={Uri.EscapeDataString(Year)}&slotId={seed.SlotId}");
        clearResp.EnsureSuccessStatusCode();

        var afterClear = await client.GetAsync(
            $"/api/reports/annual-comparison-saved/preview?employerId={seed.EmployerId}&academicYear={Uri.EscapeDataString(Year)}");
        afterClear.EnsureSuccessStatusCode();
        var restored = await afterClear.Content.ReadFromJsonAsync<AnnualComparisonPreviewDto>(Json);
        Assert.False(restored!.Rows[0].FullName.IsOverridden);
        Assert.Equal("Worker Report", restored.Rows[0].FullName.Display);
    }

    [Fact]
    public async Task Preview_InvalidEmployer_Returns400()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);

        var resp = await client.GetAsync(
            $"/api/reports/annual-comparison-saved/preview?employerId=99999&academicYear={Uri.EscapeDataString(Year)}");

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    private static async Task<ScenarioSeed> SeedScenarioAsync(HttpClient client)
    {
        var employerId = await ReportsApiIntegrationTests.SeedEmployerWithDataViaApiAsync(client);

        await using var upload = MonthlyComparisonUploadWorkbook.Create(
            "999888777",
            1001,
            "Report Worker",
            Month,
            2025,
            b => b.Band1(misra1Hours: 30m, misra1Base: 30m, jobPercent: 100m));

        using var mp = new MultipartFormDataContent();
        var fileContent = new StreamContent(upload);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        mp.Add(fileContent, "file", "sept.xlsx");
        (await client.PostAsync(
            $"/api/payroll-monthly-inputs/import?employerId={employerId}&academicYear={Uri.EscapeDataString(Year)}&month={Month}",
            mp)).EnsureSuccessStatusCode();

        var previewResp = await client.GetAsync(
            $"/api/reports/annual-comparison-saved/preview?employerId={employerId}&academicYear={Uri.EscapeDataString(Year)}");
        previewResp.EnsureSuccessStatusCode();
        var preview = await previewResp.Content.ReadFromJsonAsync<AnnualComparisonPreviewDto>(Json);
        var slotId = preview!.Rows[0].SlotId;

        return new ScenarioSeed(employerId, slotId);
    }

    private sealed record ScenarioSeed(int EmployerId, int SlotId);
}
