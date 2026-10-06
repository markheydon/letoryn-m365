using Microsoft.Playwright;
using TenancyHub.E2E.Infrastructure;

namespace TenancyHub.E2E.Journeys;

/// <summary>R1-J3.6 — Active agency + pending invitation banner.</summary>
[Trait("Journey", "R1-J3.6")]
public sealed class R1_J3_6_ActivePlusPendingInviteTests
{
    [Fact]
    public async Task User_with_active_and_invited_memberships_sees_invitation_prompt_in_shell()
    {
        var baseUrl = E2eSkipGuards.RequireBaseUrl();
        var storageState = E2eSkipGuards.RequireActiveAndInvitedStorageState();
        var cancellationToken = TestContext.Current.CancellationToken;

        await using var session = await BrowserSession.CreateAsync(storageState, cancellationToken);
        var page = await session.NewPageAsync(cancellationToken);
        await E2ePlaywrightAssertions.GotoAppRootAsync(page, baseUrl, cancellationToken);

        await Assertions.Expect(page.GetByText("You have pending invitations.")).ToBeVisibleAsync();
        await Assertions.Expect(page.GetByRole(AriaRole.Link, new() { Name = "Review invitations" })).ToBeVisibleAsync();
        await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Coming soon" })).ToBeVisibleAsync();
    }
}
