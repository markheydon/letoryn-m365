using Microsoft.Playwright;
using TenancyHub.E2E.Infrastructure;

namespace TenancyHub.E2E.Journeys;

/// <summary>R1-J3.2 — Agency member sign-in → agency shell.</summary>
[Trait("Journey", "R1-J3.2")]
public sealed class R1_J3_2_MemberAgencyShellTests
{
    [Fact]
    public async Task Active_member_sees_agency_shell_with_agency_name()
    {
        var baseUrl = E2eSkipGuards.RequireBaseUrl();
        var storageState = E2eSkipGuards.RequireMemberStorageState();
        var agencyName = E2eSkipGuards.RequireMemberAgencyName();
        var cancellationToken = TestContext.Current.CancellationToken;

        await using var session = await BrowserSession.CreateAsync(storageState, cancellationToken);
        var page = await session.NewPageAsync(cancellationToken);
        await E2ePlaywrightAssertions.GotoAppRootAsync(page, baseUrl, cancellationToken);

        await Assertions.Expect(page.GetByText(agencyName)).ToBeVisibleAsync();
        await Assertions.Expect(page.GetByRole(AriaRole.Button, new() { Name = "Sign out" })).ToBeVisibleAsync();
        await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Coming soon" })).ToBeVisibleAsync();
    }
}
