using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AccountingProject.Contracts;

namespace AccountingProject.Tests.Integration;

public sealed class EmployersApiIntegrationTests
{
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    [Fact]
    public async Task Employers_CRUD_and_EmployeeSubresources_HappyPath()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);

        var createResp = await client.PostAsJsonAsync("/api/employers", new EmployerDto
        {
            Name = "Integration Employer",
            BusinessNumber = "555",
        });
        Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
        var created = await createResp.Content.ReadFromJsonAsync<EmployerDtoResponse>(Json);
        Assert.NotNull(created);
        var id = created!.Id;

        var getResp = await client.GetAsync($"/api/employers/{id}");
        getResp.EnsureSuccessStatusCode();
        var fetched = await getResp.Content.ReadFromJsonAsync<EmployerDtoResponse>(Json);
        Assert.Equal("Integration Employer", fetched!.Name);

        var listEmpResp = await client.GetAsync($"/api/employers/{id}/employees");
        listEmpResp.EnsureSuccessStatusCode();
        var empPage = await listEmpResp.Content.ReadFromJsonAsync<PagedEmployeesJson>(Json);
        Assert.NotNull(empPage?.Items);
        Assert.Empty(empPage!.Items);

        var symResp = await client.PostAsJsonAsync($"/api/employers/{id}/institution-symbols",
            new EmployerInstitutionSymbolDto { InstitutionSymbol = "SYM1", InstitutionSymbolName = "School A" });
        symResp.EnsureSuccessStatusCode();
        var createdSymbol = await symResp.Content.ReadFromJsonAsync<SymbolJson>(Json);
        Assert.NotNull(createdSymbol);

        var symbolsGet = await client.GetAsync($"/api/employers/{id}/institution-symbols");
        symbolsGet.EnsureSuccessStatusCode();
        var symbols = await symbolsGet.Content.ReadFromJsonAsync<List<SymbolJson>>(Json);
        Assert.Single(symbols!);
        Assert.Equal("אחר", symbols![0].InstitutionType);

        var updateResp = await client.PutAsJsonAsync(
            $"/api/employers/{id}/institution-symbols/{createdSymbol!.Id}",
            new EmployerInstitutionSymbolUpdateDto { InstitutionType = "גן", InstitutionSymbolName = "School A" });
        updateResp.EnsureSuccessStatusCode();
        var updated = await updateResp.Content.ReadFromJsonAsync<SymbolJson>(Json);
        Assert.Equal("גן", updated!.InstitutionType);

        var exportResp = await client.GetAsync($"/api/employers/{id}/export/excel");
        exportResp.EnsureSuccessStatusCode();
        var xlsx = await exportResp.Content.ReadAsByteArrayAsync();
        Assert.True(xlsx.Length > 64);

        var deleteResp = await client.DeleteAsync($"/api/employers/{id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResp.StatusCode);

        var gone = await client.GetAsync($"/api/employers/{id}");
        Assert.Equal(HttpStatusCode.NotFound, gone.StatusCode);
    }

    [Fact]
    public async Task Employers_Create_EmptyName_Returns400()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);

        var resp = await client.PostAsJsonAsync("/api/employers", new EmployerDto { Name = "   " });

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task Employers_ComparisonMonthlyPayroll_EmptyBodyRejected()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);

        var createResp = await client.PostAsJsonAsync("/api/employers", new EmployerDto { Name = "Cmp Employer" });
        createResp.EnsureSuccessStatusCode();
        var created = await createResp.Content.ReadFromJsonAsync<EmployerDtoResponse>(Json);
        Assert.NotNull(created);

        using var mp = new MultipartFormDataContent();
        var file = new ByteArrayContent([]);
        file.Headers.ContentType = new MediaTypeHeaderValue(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        mp.Add(file, "file", "empty.xlsx");

        var resp = await client.PostAsync(
            $"/api/employers/{created!.Id}/comparison/monthly-payroll",
            mp);

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    private sealed class EmployerDtoResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    private sealed class PagedEmployeesJson
    {
        public List<object>? Items { get; set; }
    }

    private sealed class SymbolJson
    {
        public int Id { get; set; }
        public string InstitutionSymbol { get; set; } = "";
        public string? InstitutionType { get; set; }
    }
}
