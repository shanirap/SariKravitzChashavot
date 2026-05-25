using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AccountingProject.Contracts;
using AccountingProject.Data;

namespace AccountingProject.Tests.Integration;

internal static class IntegrationAuth
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public static async Task<HttpClient> CreateAuthenticatedClientAsync(
        AccountingWebApplicationFactory factory,
        string username,
        string password)
    {
        using var scope = factory.Services.CreateScope();
        IntegrationSeed.EnsureUsers(scope.ServiceProvider.GetRequiredService<PayrollDbContext>());

        var client = factory.CreateClient();
        var resp = await client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto
        {
            Username = username,
            Password = password,
        });
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadFromJsonAsync<LoginResponseDto>(JsonOptions);
        if (string.IsNullOrEmpty(body?.Token))
            throw new InvalidOperationException("Login response missing token.");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body.Token);
        return client;
    }

    public static Task<HttpClient> CreateAdminClientAsync(AccountingWebApplicationFactory factory) =>
        CreateAuthenticatedClientAsync(factory, IntegrationSeed.AdminUsername, IntegrationSeed.AdminPassword);

    public static Task<HttpClient> CreateViewerClientAsync(AccountingWebApplicationFactory factory) =>
        CreateAuthenticatedClientAsync(factory, IntegrationSeed.ViewerUsername, IntegrationSeed.ViewerPassword);
}
