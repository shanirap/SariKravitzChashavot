using System.Net;
using AccountingProject.Services;
using AccountingProject.Tests.TestHelpers;
using ClosedXML.Excel;

namespace AccountingProject.Tests.Integration;

public sealed class AnnualComparisonSavedApiIntegrationTests
{
    private const string SheetName = "השוואה שנתית";
    private const string Year = "תשפ\"ו";

    [Fact]
    public async Task AnnualComparisonSaved_Get_ReturnsXlsxWithoutUploadedFile()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);
        var employerId = await ReportsApiIntegrationTests.SeedEmployerWithDataViaApiAsync(client);

        var resp = await client.GetAsync(
            $"/api/reports/annual-comparison-saved?employerId={employerId}&academicYear={Uri.EscapeDataString(Year)}");

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        Assert.Equal(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            resp.Content.Headers.ContentType?.MediaType);

        var bytes = await resp.Content.ReadAsByteArrayAsync();
        Assert.True(bytes.Length > 64);

        using var wb = new XLWorkbook(new MemoryStream(bytes));
        var ws = wb.Worksheet(SheetName);
        Assert.True(ws.LastRowUsed()?.RowNumber() >= 2);
        Assert.Equal(AnnualComparisonReportBuilder.NotCapturedInInput, ws.Cell(2, 10).GetString());
    }

    [Fact]
    public async Task AnnualComparisonSaved_UnknownEmployer_Returns400()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);

        var resp = await client.GetAsync(
            $"/api/reports/annual-comparison-saved?employerId=99999&academicYear={Uri.EscapeDataString(Year)}");

        await IntegrationResponseAssert.AssertBadRequestMessageAsync(resp, "המעסיק לא נמצא");
    }
}
