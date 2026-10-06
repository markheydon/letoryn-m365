using Microsoft.Playwright;
using TenancyHub.E2E.Infrastructure;

namespace TenancyHub.E2E.Journeys;

/// <summary>R1-J5.3 — Role guards: roster visibility and audit denial.</summary>
[Trait("Journey", "R1-J5.3")]
public sealed class R1_J5_3_PermissionGuardsTests
{
    [Fact]
    public async Task Read_only_member_is_denied_member_roster()
    {
        var baseUrl = E2eSkipGuards.RequireBaseUrl();
        var storageState = E2eSkipGuards.RequireReadOnlyMemberStorageState();
        var cancellationToken = TestContext.Current.CancellationToken;

        await using var session = await BrowserSession.CreateAsync(storageState, cancellationToken);
        var page = await session.NewPageAsync(cancellationToken);
        await page.GotoAsync($"{baseUrl}/members", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        await Assertions.Expect(page.GetByText("You do not have permission to view the member list for this agency."))
            .ToBeVisibleAsync();
    }

    [Fact]
    public async Task Standard_member_can_open_notification_panel()
    {
        var baseUrl = E2eSkipGuards.RequireBaseUrl();
        var storageState = E2eSkipGuards.RequireStandardMemberStorageState();
        var cancellationToken = TestContext.Current.CancellationToken;

        await using var session = await BrowserSession.CreateAsync(storageState, cancellationToken);
        var page = await session.NewPageAsync(cancellationToken);
        await E2ePlaywrightAssertions.GotoAppRootAsync(page, baseUrl, cancellationToken);

        var bell = page.GetByRole(AriaRole.Button, new() { Name = "Notifications" });
        await bell.ClickAsync();
        await Assertions.Expect(page.GetByText("Notifications", new() { Exact = true })).ToBeVisibleAsync();
    }
}
