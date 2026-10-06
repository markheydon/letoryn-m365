namespace Letoryn.ApiService.UnitTests;

public sealed class HealthEndpointTests
{
    [Fact]
    public async Task Health_returns_success_in_development()
    {
        await using var factory = new LetorynWebApplicationFactory();

        using var client = factory.CreateClient();
        using var response = await client.GetAsync("/health", TestContext.Current.CancellationToken);

        Assert.True(response.IsSuccessStatusCode);
    }
}
