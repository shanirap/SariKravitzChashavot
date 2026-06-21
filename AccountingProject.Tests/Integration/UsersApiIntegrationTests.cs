using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AccountingProject.Contracts;
using AccountingProject.Models;

namespace AccountingProject.Tests.Integration;

public sealed class UsersApiIntegrationTests
{
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    [Fact]
    public async Task Users_AsViewer_Returns403()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateViewerClientAsync(factory);

        var resp = await client.GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
    }

    [Fact]
    public async Task Users_AsAdmin_List_Create_SetPassword()
    {
        await using var factory = new AccountingWebApplicationFactory();
        var client = await IntegrationAuth.CreateAdminClientAsync(factory);

        var listResp = await client.GetAsync("/api/users");
        listResp.EnsureSuccessStatusCode();
        var users = await listResp.Content.ReadFromJsonAsync<List<UserSummaryJson>>(Json);
        Assert.NotNull(users);
        Assert.True(users!.Count >= 2);

        var createResp = await client.PostAsJsonAsync("/api/users", new AdminCreateUserRequestDto
        {
            Username = "created-by-integration",
            Password = "Aa1!cccccccc",
            Role = UserRoles.Viewer,
        });
        Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
        var created = await createResp.Content.ReadFromJsonAsync<UserSummaryJson>(Json);
        Assert.NotNull(created?.Id);
        Assert.Equal(UserRoles.Admin, created!.Role);

        var pwResp = await client.PutAsJsonAsync(
            $"/api/users/{created!.Id}/password",
            new SetPasswordRequestDto { Password = "Bb2@dddddddd" });
        Assert.Equal(HttpStatusCode.NoContent, pwResp.StatusCode);
    }

    private sealed class UserSummaryJson
    {
        public int Id { get; set; }
        public string Username { get; set; } = "";
        public string Role { get; set; } = "";
    }
}
