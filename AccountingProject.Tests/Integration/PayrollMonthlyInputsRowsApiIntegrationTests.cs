using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AccountingProject.Contracts;
using AccountingProject.Data;
using AccountingProject.Models;
using AccountingProject.Tests.TestHelpers;
using Microsoft.Extensions.DependencyInjection;

namespace AccountingProject.Tests.Integration;

public sealed class PayrollMonthlyInputsRowsApiIntegrationTests
{
    private const string Year = "תשפ\"ו";
    private const int Month = 9;
    private const int GregorianYear = 2025;

    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    [Fact]
    public async Task Rows_ActiveBatch_ReturnsRows()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);
        var seed = await SeedActiveBatchWithRowAsync(factory);

        var rows = await GetRowsAsync(client, seed.EmployerId, Month);

        Assert.Single(rows);
        Assert.Equal(seed.RowId, rows[0].Id);
        Assert.Equal("111222333", rows[0].IdNumber);
        Assert.Equal("Original Name", rows[0].FullName);
    }

    [Fact]
    public async Task Rows_NoActiveBatch_ReturnsEmptyList()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);
        var employerId = await SeedEmployerOnlyAsync(factory);

        var rows = await GetRowsAsync(client, employerId, Month);

        Assert.Empty(rows);
    }

    [Fact]
    public async Task Rows_Update_UpdatesEditableFields()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);
        var seed = await SeedActiveBatchWithRowAsync(factory);

        var updateResp = await client.PutAsJsonAsync(
            $"/api/payroll-monthly-inputs/rows/{seed.RowId}?employerId={seed.EmployerId}",
            new PayrollMonthlyInputRowEditDto
            {
                FullName = "Updated Name",
                WeeklyHours = 12.5m,
                ManualEditNote = "test note",
            });
        updateResp.EnsureSuccessStatusCode();
        var updated = await updateResp.Content.ReadFromJsonAsync<PayrollMonthlyInputRowDto>(Json);
        Assert.NotNull(updated);
        Assert.Equal("Updated Name", updated!.FullName);
        Assert.Equal(12.5m, updated.WeeklyHours);
    }

    [Fact]
    public async Task Rows_Update_SetsIsManualEdited()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);
        var seed = await SeedActiveBatchWithRowAsync(factory);

        var updateResp = await client.PutAsJsonAsync(
            $"/api/payroll-monthly-inputs/rows/{seed.RowId}?employerId={seed.EmployerId}",
            new PayrollMonthlyInputRowEditDto { Role = "Teacher" });
        updateResp.EnsureSuccessStatusCode();
        var updated = await updateResp.Content.ReadFromJsonAsync<PayrollMonthlyInputRowDto>(Json);

        Assert.True(updated!.IsManualEdited);
    }

    [Fact]
    public async Task Rows_Delete_UnknownRow_Returns404WithMessage()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);

        var resp = await client.DeleteAsync("/api/payroll-monthly-inputs/rows/424242?employerId=1");

        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        var message = await IntegrationResponseAssert.ReadMessageAsync(resp);
        Assert.Contains("שורת קלט עוקץ חודשי לא נמצאה", message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Rows_Delete_SoftDeletesAndExcludesFromGet()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);
        var seed = await SeedActiveBatchWithRowAsync(factory);

        var deleteResp = await client.DeleteAsync(
            $"/api/payroll-monthly-inputs/rows/{seed.RowId}?employerId={seed.EmployerId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResp.StatusCode);

        var rows = await GetRowsAsync(client, seed.EmployerId, Month);
        Assert.DoesNotContain(rows, r => r.Id == seed.RowId);
    }

    [Fact]
    public async Task Rows_UpdateWithWrongEmployerId_Returns404AndDoesNotChangeRow()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);
        var seed = await SeedActiveBatchWithRowAsync(factory);
        var otherEmployerId = await SeedEmployerOnlyAsync(factory);

        var updateResp = await client.PutAsJsonAsync(
            $"/api/payroll-monthly-inputs/rows/{seed.RowId}?employerId={otherEmployerId}",
            new PayrollMonthlyInputRowEditDto { FullName = "Hacked Name" });

        Assert.Equal(HttpStatusCode.NotFound, updateResp.StatusCode);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PayrollDbContext>();
        var row = await db.PayrollMonthlyInputRows.FindAsync(seed.RowId);
        Assert.Equal("Original Name", row!.FullName);
    }

    [Fact]
    public async Task Rows_DeleteWithWrongEmployerId_Returns404AndDoesNotDeleteRow()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);
        var seed = await SeedActiveBatchWithRowAsync(factory);
        var otherEmployerId = await SeedEmployerOnlyAsync(factory);

        var deleteResp = await client.DeleteAsync(
            $"/api/payroll-monthly-inputs/rows/{seed.RowId}?employerId={otherEmployerId}");

        Assert.Equal(HttpStatusCode.NotFound, deleteResp.StatusCode);

        var rows = await GetRowsAsync(client, seed.EmployerId, Month);
        Assert.Contains(rows, r => r.Id == seed.RowId);
        Assert.Equal("Original Name", rows.Single(r => r.Id == seed.RowId).FullName);
    }

    private static async Task<List<PayrollMonthlyInputRowDto>> GetRowsAsync(
        HttpClient client,
        int employerId,
        int month)
    {
        var resp = await client.GetAsync(
            $"/api/payroll-monthly-inputs/rows?employerId={employerId}&academicYear={Uri.EscapeDataString(Year)}&month={month}");
        resp.EnsureSuccessStatusCode();
        var rows = await resp.Content.ReadFromJsonAsync<List<PayrollMonthlyInputRowDto>>(Json);
        Assert.NotNull(rows);
        return rows!;
    }

    private static async Task<int> SeedEmployerOnlyAsync(AccountingWebApplicationFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PayrollDbContext>();
        var employer = new Employer { Name = "Okets Rows Empty Employer" };
        db.Employers.Add(employer);
        await db.SaveChangesAsync();
        return employer.Id;
    }

    private static async Task<RowsSeed> SeedActiveBatchWithRowAsync(AccountingWebApplicationFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PayrollDbContext>();
        var now = DateTime.UtcNow;

        var employer = new Employer { Name = "Okets Rows Batch Employer" };
        db.Employers.Add(employer);
        await db.SaveChangesAsync();

        var batch = new PayrollMonthlyInputBatch
        {
            EmployerId = employer.Id,
            AcademicYear = Year,
            Month = Month,
            GregorianYear = GregorianYear,
            OriginalFileName = "seed.xlsx",
            UploadedAtUtc = now,
            RowsCount = 1,
            IsActive = true,
            CreatedAtUtc = now,
        };
        db.PayrollMonthlyInputBatches.Add(batch);
        await db.SaveChangesAsync();

        var row = new PayrollMonthlyInputRow
        {
            BatchId = batch.Id,
            EmployerId = employer.Id,
            AcademicYear = Year,
            Month = Month,
            GregorianYear = GregorianYear,
            IdNumber = "111222333",
            FullName = "Original Name",
            CreatedAtUtc = now,
        };
        db.PayrollMonthlyInputRows.Add(row);
        await db.SaveChangesAsync();

        return new RowsSeed(employer.Id, row.Id);
    }

    private sealed record RowsSeed(int EmployerId, int RowId);
}
