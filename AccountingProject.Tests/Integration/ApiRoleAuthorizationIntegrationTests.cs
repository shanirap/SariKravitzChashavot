using System.Net;
using AccountingProject.Tests.TestHelpers;

namespace AccountingProject.Tests.Integration;

/// <summary>אימות — בקשות ללא JWT ומשתמש Viewer על Users (AdminOnly).</summary>
public sealed class ApiRoleAuthorizationIntegrationTests
{
    private const string Year = "תשפ\"ו";

    [Fact]
    public async Task PayrollMonthlyInputs_Status_WithoutToken_Returns401()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = factory.CreateClient();

        var resp = await client.GetAsync(
            $"/api/payroll-monthly-inputs/status?employerId=1&academicYear={Uri.EscapeDataString(Year)}");

        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task Reports_AnnualComparisonSaved_WithoutToken_Returns401()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = factory.CreateClient();

        var resp = await client.GetAsync(
            $"/api/reports/annual-comparison-saved?employerId=1&academicYear={Uri.EscapeDataString(Year)}");

        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task BulkImport_TemplateEmployers_WithoutToken_Returns401()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = factory.CreateClient();

        var resp = await client.GetAsync("/api/bulk-import/template/employers");

        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task Users_List_AsViewer_Returns403()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateViewerClientAsync(factory);

        var resp = await client.GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
    }

    [Fact]
    public async Task PayrollMonthlyInputs_Import_WithoutToken_Returns401()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = factory.CreateClient();

        var resp = await client.PostAsync(
            $"/api/payroll-monthly-inputs/import?employerId=1&academicYear={Uri.EscapeDataString(Year)}&month=9",
            null);

        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }
}
