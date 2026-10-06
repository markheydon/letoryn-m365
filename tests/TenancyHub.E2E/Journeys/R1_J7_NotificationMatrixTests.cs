using Microsoft.Playwright;
using TenancyHub.E2E.Infrastructure;

namespace TenancyHub.E2E.Journeys;

/// <summary>R1-J7 — In-app notification bell for routine shell members.</summary>
[Trait("Journey", "R1-J7")]
public sealed class R1_J7_NotificationMatrixTests
{
    [Fact]
    public async Task Active_member_sees_notification_bell_in_shell()
    {
        var baseUrl = E2eSkipGuards.RequireBaseUrl();
        var storageState = E2eSkipGuards.RequireStandardMemberStorageState();
        var cancellationToken = TestContext.Current.CancellationToken;

        await using var session = await BrowserSession.CreateAsync(storageState, cancellationToken);
        var page = await session.NewPageAsync(cancellationToken);
        await E2ePlaywrightAssertions.GotoAppRootAsync(page, baseUrl, cancellationToken);

        await Assertions.Expect(page.GetByRole(AriaRole.Button, new() { Name = "Notifications" })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Read_only_member_cannot_mark_notifications_read_from_panel()
    {
        var baseUrl = E2eSkipGuards.RequireBaseUrl();
        var storageState = E2eSkipGuards.RequireReadOnlyMemberStorageState();
        var cancellationToken = TestContext.Current.CancellationToken;

        await using var session = await BrowserSession.CreateAsync(storageState, cancellationToken);
        var page = await session.NewPageAsync(cancellationToken);
        await E2ePlaywrightAssertions.GotoAppRootAsync(page, baseUrl, cancellationToken);

        var bell = page.GetByRole(AriaRole.Button, new() { Name = "Notifications" });
        await bell.ClickAsync();
        await Assertions.Expect(page.GetByRole(AriaRole.Button, new() { Name = "Mark as read" })).ToHaveCountAsync(0);
    }
}
