using System.Net.Http.Json;
using System.Text.Json;
using AccountingProject.Contracts;

namespace AccountingProject.Tests.Integration;

public sealed class PayrollMonthlyInputsApiIntegrationTests
{
    private const string Year = "תשפ\"ו";
    private static readonly int[] AcademicYearMonthOrder = [9, 10, 11, 12, 1, 2, 3, 4, 5, 6, 7, 8];

    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    [Fact]
    public async Task Status_NoBatches_ReturnsTwelveMonths()
    {
        var months = await GetStatusMonthsAsync();
        Assert.Equal(12, months.Count);
    }

    [Fact]
    public async Task Status_NoBatches_AllMonthsMissing()
    {
        var months = await GetStatusMonthsAsync();
        Assert.All(months, m => Assert.Equal("חסר", m.Status));
    }

    [Fact]
    public async Task Status_NoBatches_MonthsOrderedSepThroughAug()
    {
        var months = await GetStatusMonthsAsync();
        Assert.Equal(AcademicYearMonthOrder, months.Select(m => m.Month).ToArray());
    }

    private static async Task<IReadOnlyList<PayrollMonthStatusDto>> GetStatusMonthsAsync()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);

        var createResp = await client.PostAsJsonAsync("/api/employers", new EmployerDto
        {
            Name = "Payroll Monthly Status Employer",
        });
        createResp.EnsureSuccessStatusCode();
        var created = await createResp.Content.ReadFromJsonAsync<EmployerIdJson>(Json);
        Assert.NotNull(created);

        var resp = await client.GetAsync(
            $"/api/payroll-monthly-inputs/status?employerId={created!.Id}&academicYear={Uri.EscapeDataString(Year)}");
        resp.EnsureSuccessStatusCode();

        var months = await resp.Content.ReadFromJsonAsync<List<PayrollMonthStatusDto>>(Json);
        Assert.NotNull(months);
        return months!;
    }

    private sealed class EmployerIdJson
    {
        public int Id { get; set; }
    }
}
