using Microsoft.Playwright;
using TenancyHub.E2E.Infrastructure;

namespace TenancyHub.E2E.Journeys;

/// <summary>R1-J5.2 — Invitations acceptance UI and agency settings entry.</summary>
[Trait("Journey", "R1-J5.2")]
public sealed class R1_J5_2_InvitesProvisionSettingsTests
{
    [Fact]
    public async Task Administrator_can_open_agency_settings()
    {
        var baseUrl = E2eSkipGuards.RequireBaseUrl();
        var storageState = E2eSkipGuards.RequireAdministratorStorageState();
        var cancellationToken = TestContext.Current.CancellationToken;

        await using var session = await BrowserSession.CreateAsync(storageState, cancellationToken);
        var page = await session.NewPageAsync(cancellationToken);
        await page.GotoAsync($"{baseUrl}/agency-settings", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Agency settings" })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Invitee_can_open_pending_invitations_list()
    {
        var baseUrl = E2eSkipGuards.RequireBaseUrl();
        var storageState = E2eSkipGuards.RequireInviteOnlyStorageState();
        var cancellationToken = TestContext.Current.CancellationToken;

        await using var session = await BrowserSession.CreateAsync(storageState, cancellationToken);
        var page = await session.NewPageAsync(cancellationToken);
        await page.GotoAsync($"{baseUrl}/invitations", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Pending invitations" })).ToBeVisibleAsync();
    }
}
