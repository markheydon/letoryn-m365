using Microsoft.Playwright;

namespace Letoryn.E2E.Infrastructure;

internal static class E2ePlaywrightAssertions
{
    public static async Task GotoAppRootAsync(IPage page, string baseUrl, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await page.GotoAsync($"{baseUrl}/", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
    }

    public static async Task AssertMicrosoftSignInUrlAsync(IPage page)
    {
        var url = page.Url;
        Assert.True(
            url.Contains("MicrosoftIdentity/Account/SignIn", StringComparison.OrdinalIgnoreCase)
            || url.Contains("login.microsoftonline.com", StringComparison.OrdinalIgnoreCase),
            $"Expected Microsoft sign-in redirect; actual URL was {url}.");
    }
}
