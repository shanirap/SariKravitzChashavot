using System.Net;
using System.Net.Http.Json;
using AccountingProject.Tests.TestHelpers;

namespace AccountingProject.Tests.Integration;

public sealed class PayrollMonthlyInputsApiValidationTests
{
    private const string Year = "תשפ\"ו";

    [Fact]
    public async Task Status_MissingEmployerId_Returns400()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);

        var resp = await client.GetAsync(
            $"/api/payroll-monthly-inputs/status?employerId=0&academicYear={Uri.EscapeDataString(Year)}");

        await IntegrationResponseAssert.AssertBadRequestMessageAsync(resp, "employerId");
    }

    [Fact]
    public async Task Rows_InvalidMonth_Returns400()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);

        var resp = await client.GetAsync(
            $"/api/payroll-monthly-inputs/rows?employerId=1&academicYear={Uri.EscapeDataString(Year)}&month=0");

        await IntegrationResponseAssert.AssertBadRequestMessageAsync(resp, "חודש");
    }

    [Fact]
    public async Task Rows_Update_UnknownRow_Returns404()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);

        var resp = await client.PutAsJsonAsync(
            "/api/payroll-monthly-inputs/rows/999999?employerId=1",
            new { role = "test" });

        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task PayrollMonthlyInputs_Unauthorized_Returns401()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = factory.CreateClient();

        var resp = await client.GetAsync(
            $"/api/payroll-monthly-inputs/status?employerId=1&academicYear={Uri.EscapeDataString(Year)}");

        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }
}
