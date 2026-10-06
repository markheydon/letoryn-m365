using Letoryn.E2E.Infrastructure;
using Microsoft.Playwright;

namespace Letoryn.E2E.Journeys;

/// <summary>R1-J3.5 — Sign-out ends the current browser session only.</summary>
[Trait("Journey", "R1-J3.5")]
public sealed class R1_J3_5_PerBrowserSignOutTests
{
    [Fact]
    public async Task Sign_out_in_one_browser_requires_sign_in_again()
    {
        var baseUrl = E2eSkipGuards.RequireBaseUrl();
        var storageState = E2eSkipGuards.RequireMemberStorageState();
        var cancellationToken = TestContext.Current.CancellationToken;

        await using var session = await BrowserSession.CreateAsync(storageState, cancellationToken);
        var page = await session.NewPageAsync(cancellationToken);
        await E2ePlaywrightAssertions.GotoAppRootAsync(page, baseUrl, cancellationToken);

        await page.GetByRole(AriaRole.Button, new() { Name = "Sign out" }).ClickAsync();
        await page.WaitForURLAsync(
            url => url.Contains("MicrosoftIdentity", StringComparison.OrdinalIgnoreCase)
                || url.Contains("login.microsoftonline.com", StringComparison.OrdinalIgnoreCase),
            new PageWaitForURLOptions { Timeout = 30_000 });

        await E2ePlaywrightAssertions.AssertMicrosoftSignInUrlAsync(page);
    }
}
