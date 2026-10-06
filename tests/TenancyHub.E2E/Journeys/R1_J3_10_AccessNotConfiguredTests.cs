using Microsoft.Playwright;
using TenancyHub.E2E.Infrastructure;

namespace TenancyHub.E2E.Journeys;

/// <summary>
/// R1-J3.10 — Valid Entra user with no membership → access not configured
/// (<see href="docs/platform-foundation-user-journeys.md"/>).
/// </summary>
[Trait("Journey", "R1-J3.10")]
public sealed class R1_J3_10_AccessNotConfiguredTests
{
    [Fact]
    public async Task User_without_membership_sees_access_not_configured()
    {
        var baseUrl = E2eSkipGuards.RequireBaseUrl();
        var storageState = E2eSkipGuards.RequireNoAccessStorageState();
        var cancellationToken = TestContext.Current.CancellationToken;

        await using var session = await BrowserSession.CreateAsync(storageState, cancellationToken);
        var page = await session.NewPageAsync(cancellationToken);

        await page.GotoAsync($"{baseUrl}/", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        var title = page.GetByRole(AriaRole.Heading, new() { Name = "Access not configured" });
        await title.WaitForAsync(new LocatorWaitForOptions { Timeout = 30_000 });

        await Assertions.Expect(title).ToBeVisibleAsync();
        await Assertions.Expect(page.GetByText("does not have an active agency membership")).ToBeVisibleAsync();
    }
}
