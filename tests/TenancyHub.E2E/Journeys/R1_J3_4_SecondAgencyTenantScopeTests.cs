using Microsoft.Playwright;
using TenancyHub.E2E.Infrastructure;

namespace TenancyHub.E2E.Journeys;

/// <summary>R1-J3.4 — Second agency user — no cross-agency UI data.</summary>
[Trait("Journey", "R1-J3.4")]
public sealed class R1_J3_4_SecondAgencyTenantScopeTests
{
    [Fact]
    public async Task Second_agency_member_sees_only_their_agency_context()
    {
        var baseUrl = E2eSkipGuards.RequireBaseUrl();
        var storageState = E2eSkipGuards.RequireSecondMemberStorageState();
        var agencyName = E2eSkipGuards.RequireSecondMemberAgencyName();
        var otherAgencyName = E2eEnvironment.MemberAgencyName;
        var cancellationToken = TestContext.Current.CancellationToken;

        await using var session = await BrowserSession.CreateAsync(storageState, cancellationToken);
        var page = await session.NewPageAsync(cancellationToken);
        await E2ePlaywrightAssertions.GotoAppRootAsync(page, baseUrl, cancellationToken);

        await Assertions.Expect(page.GetByText(agencyName)).ToBeVisibleAsync();
        if (otherAgencyName is not null)
        {
            await Assertions.Expect(page.GetByText(otherAgencyName)).ToHaveCountAsync(0);
        }
    }
}
