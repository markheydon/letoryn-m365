using Microsoft.Playwright;
using TenancyHub.E2E.Infrastructure;

namespace TenancyHub.E2E.Journeys;

/// <summary>R1-J3.7 — Invite-only sign-in → invitation-acceptance experience.</summary>
[Trait("Journey", "R1-J3.7")]
public sealed class R1_J3_7_InviteOnlySignInTests
{
    [Fact]
    public async Task Invite_only_user_lands_on_invitations_without_agency_bell()
    {
        var baseUrl = E2eSkipGuards.RequireBaseUrl();
        var storageState = E2eSkipGuards.RequireInviteOnlyStorageState();
        var cancellationToken = TestContext.Current.CancellationToken;

        await using var session = await BrowserSession.CreateAsync(storageState, cancellationToken);
        var page = await session.NewPageAsync(cancellationToken);
        await E2ePlaywrightAssertions.GotoAppRootAsync(page, baseUrl, cancellationToken);

        var invitationsHeading = page.GetByRole(AriaRole.Heading, new() { Name = "Pending invitations" });
        await invitationsHeading.WaitForAsync(new LocatorWaitForOptions { Timeout = 30_000 });
        await Assertions.Expect(invitationsHeading).ToBeVisibleAsync();
        await Assertions.Expect(page.GetByRole(AriaRole.Button, new() { Name = "Notifications" })).ToHaveCountAsync(0);
    }
}
