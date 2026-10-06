using Letoryn.E2E.Infrastructure;
using Microsoft.Playwright;

namespace Letoryn.E2E.Journeys;

/// <summary>R1-J4.1 — Agency/membership lifecycle gates (suspended agency member).</summary>
[Trait("Journey", "R1-J4.1")]
public sealed class R1_J4_1_LifecycleMembershipGatesTests
{
    [Fact]
    public async Task Member_of_suspended_agency_sees_agency_suspended_message()
    {
        var baseUrl = E2eSkipGuards.RequireBaseUrl();
        var storageState = E2eSkipGuards.RequireSuspendedAgencyMemberStorageState();
        var cancellationToken = TestContext.Current.CancellationToken;

        await using var session = await BrowserSession.CreateAsync(storageState, cancellationToken);
        var page = await session.NewPageAsync(cancellationToken);
        await E2ePlaywrightAssertions.GotoAppRootAsync(page, baseUrl, cancellationToken);

        await Assertions.Expect(page.GetByText("This agency is suspended", new() { Exact = false })).ToBeVisibleAsync();
    }
}
