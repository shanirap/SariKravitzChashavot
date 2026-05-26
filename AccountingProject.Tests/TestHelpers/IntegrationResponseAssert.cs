using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace AccountingProject.Tests.TestHelpers;

internal static class IntegrationResponseAssert
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public static async Task AssertBadRequestMessageAsync(
        HttpResponseMessage response,
        string expectedMessageSubstring)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var message = await ReadMessageAsync(response);
        Assert.Contains(expectedMessageSubstring, message, StringComparison.Ordinal);
    }

    public static async Task<string> ReadMessageAsync(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(json))
            return string.Empty;

        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("message", out var message))
                return message.GetString() ?? string.Empty;
            if (doc.RootElement.TryGetProperty("title", out var title))
                return title.GetString() ?? string.Empty;
        }
        catch (System.Text.Json.JsonException)
        {
            return json;
        }

        return json;
    }

    private sealed class MessageJson
    {
        public string? Message { get; set; }
    }
}
