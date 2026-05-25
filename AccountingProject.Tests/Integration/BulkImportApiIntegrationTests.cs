using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AccountingProject.Contracts;

namespace AccountingProject.Tests.Integration;

public sealed class BulkImportApiIntegrationTests
{
    [Fact]
    public async Task TemplateEmployers_ReturnsXlsx()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);

        var resp = await client.GetAsync("/api/bulk-import/template/employers");

        resp.EnsureSuccessStatusCode();
        Assert.Equal(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            resp.Content.Headers.ContentType?.MediaType);
        Assert.True((await resp.Content.ReadAsByteArrayAsync()).Length > 100);
    }

    [Fact]
    public async Task TemplateEmployees_WithEmployerId_ReturnsXlsx()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);

        var er = await client.PostAsJsonAsync("/api/employers", new EmployerDto { Name = "Tpl Employer" });
        er.EnsureSuccessStatusCode();
        var created = await er.Content.ReadFromJsonAsync<EmployerIdOnly>(RelaxedJson);
        Assert.NotNull(created);
        var id = created!.Id;

        var resp = await client.GetAsync($"/api/bulk-import/template/employees?includeEmployerName=false&employerId={id}");

        resp.EnsureSuccessStatusCode();
        Assert.True((await resp.Content.ReadAsByteArrayAsync()).Length > 100);
    }

    [Fact]
    public async Task ImportEmployees_EmptyFile_ReturnsBadRequest()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);

        using var mp = new MultipartFormDataContent();
        var bytes = Array.Empty<byte>();
        var file = new ByteArrayContent(bytes);
        file.Headers.ContentType = new MediaTypeHeaderValue(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        mp.Add(file, "file", "empty.xlsx");

        var resp = await client.PostAsync("/api/bulk-import/employees", mp);

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    private static readonly JsonSerializerOptions RelaxedJson = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private sealed class EmployerIdOnly
    {
        public int Id { get; set; }
    }
}
