using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace TenancyHub.ApiService.UnitTests;

public sealed class HealthEndpointTests
{
    [Fact]
    public async Task Health_returns_success_in_development()
    {
        await using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
                builder.UseSetting(WebHostDefaults.EnvironmentKey, Environments.Development));

        using var client = factory.CreateClient();
        using var response = await client.GetAsync("/health", TestContext.Current.CancellationToken);

        Assert.True(response.IsSuccessStatusCode);
    }
}
