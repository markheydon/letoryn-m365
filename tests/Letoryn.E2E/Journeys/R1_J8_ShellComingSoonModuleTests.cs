using Letoryn.E2E.Infrastructure;
using Microsoft.Playwright;

namespace Letoryn.E2E.Journeys;

/// <summary>
/// R1-J8 — Shell home shows coming-soon module summary for administrator or standard member
/// (<see href="docs/platform-foundation-user-journeys.md"/>).
/// </summary>
[Trait("Journey", "R1-J8")]
public sealed class R1_J8_ShellComingSoonModuleTests
{
    [Fact]
    public async Task Agency_member_home_shows_coming_soon_module_summary()
    {
        var baseUrl = E2eSkipGuards.RequireBaseUrl();
        var storageState = E2eSkipGuards.RequireMemberStorageState();
        var cancellationToken = TestContext.Current.CancellationToken;

        await using var session = await BrowserSession.CreateAsync(storageState, cancellationToken);
        var page = await session.NewPageAsync(cancellationToken);

        await page.GotoAsync($"{baseUrl}/", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        var comingSoon = page.GetByRole(AriaRole.Heading, new() { Name = "Coming soon" });
        await comingSoon.WaitForAsync(new LocatorWaitForOptions { Timeout = 30_000 });

        await Assertions.Expect(page.GetByText("Welcome to your agency workspace")).ToBeVisibleAsync();
        await Assertions.Expect(page.GetByRole(AriaRole.Button, new() { Name = "Sign out" })).ToBeVisibleAsync();
    }
}
