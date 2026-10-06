namespace TenancyHub.ApiService.UnitTests;

/// <summary>Smoke tests for Development OpenAPI document exposure.</summary>
public sealed class OpenApiEndpointTests
{
    [Fact]
    public async Task OpenApi_v1_json_is_available_in_development()
    {
        await using var factory = new TenancyHubWebApplicationFactory();

        using var client = factory.CreateClient();
        using var response = await client.GetAsync("/openapi/v1.json", TestContext.Current.CancellationToken);

        Assert.True(response.IsSuccessStatusCode);

        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("/api/v1/me", json, StringComparison.Ordinal);
        Assert.Contains("/api/v1/agencies", json, StringComparison.Ordinal);
    }
}
