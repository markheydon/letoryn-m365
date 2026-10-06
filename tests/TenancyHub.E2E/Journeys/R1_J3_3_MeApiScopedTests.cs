using Microsoft.Playwright;
using TenancyHub.E2E.Infrastructure;

namespace TenancyHub.E2E.Journeys;

/// <summary>R1-J3.3 — <c>GET /api/v1/me</c> scoped to active agency.</summary>
[Trait("Journey", "R1-J3.3")]
public sealed class R1_J3_3_MeApiScopedTests
{
    [Fact]
    public async Task Me_endpoint_returns_success_for_signed_in_member()
    {
        var apiBaseUrl = E2eSkipGuards.RequireApiBaseUrl();
        var storageState = E2eSkipGuards.RequireMemberStorageState();
        var cancellationToken = TestContext.Current.CancellationToken;

        using var playwright = await Playwright.CreateAsync();
        await using var request = await playwright.APIRequest.NewContextAsync(new APIRequestNewContextOptions
        {
            StorageStatePath = storageState,
        });

        var response = await request.GetAsync($"{apiBaseUrl}/api/v1/me");
        Assert.True(response.Ok, $"Expected 2xx from /api/v1/me; status {(int)response.Status}.");

        var body = await response.TextAsync();
        Assert.Contains("memberships", body, StringComparison.OrdinalIgnoreCase);
    }
}
