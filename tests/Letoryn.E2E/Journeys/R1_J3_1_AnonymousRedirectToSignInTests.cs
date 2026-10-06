using Letoryn.E2E.Infrastructure;
using Microsoft.Playwright;

namespace Letoryn.E2E.Journeys;

/// <summary>
/// R1-J3.1 — Anonymous hit → Microsoft sign-in (<see href="docs/platform-foundation-user-journeys.md"/>).
/// </summary>
[Trait("Journey", "R1-J3.1")]
public sealed class R1_J3_1_AnonymousRedirectToSignInTests
{
    [Fact]
    public async Task Unauthenticated_home_redirects_to_microsoft_sign_in()
    {
        var baseUrl = E2eSkipGuards.RequireBaseUrl();
        var cancellationToken = TestContext.Current.CancellationToken;

        await using var session = await BrowserSession.CreateAsync(storageStatePath: null, cancellationToken);
        var page = await session.NewPageAsync(cancellationToken);

        await page.GotoAsync($"{baseUrl}/", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        var url = page.Url;
        Assert.True(
            url.Contains("MicrosoftIdentity/Account/SignIn", StringComparison.OrdinalIgnoreCase)
            || url.Contains("login.microsoftonline.com", StringComparison.OrdinalIgnoreCase),
            $"Expected Microsoft sign-in redirect; actual URL was {url}.");
    }
}
