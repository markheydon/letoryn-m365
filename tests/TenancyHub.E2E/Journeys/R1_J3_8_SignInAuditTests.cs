using Microsoft.Playwright;
using TenancyHub.E2E.Infrastructure;

namespace TenancyHub.E2E.Journeys;

/// <summary>R1-J3.8 — Successful sign-in appears in agency audit within contributor validation window.</summary>
[Trait("Journey", "R1-J3.8")]
public sealed class R1_J3_8_SignInAuditTests
{
    [Fact]
    public async Task Agency_administrator_audit_view_loads_after_member_sign_in_fixture()
    {
        var baseUrl = E2eSkipGuards.RequireBaseUrl();
        var storageState = E2eSkipGuards.RequireAdministratorStorageState();
        var cancellationToken = TestContext.Current.CancellationToken;

        await using var session = await BrowserSession.CreateAsync(storageState, cancellationToken);
        var page = await session.NewPageAsync(cancellationToken);
        await page.GotoAsync($"{baseUrl}/audit", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        var heading = page.GetByRole(AriaRole.Heading, new() { Name = "Audit history" });
        await heading.WaitForAsync(new LocatorWaitForOptions { Timeout = 30_000 });
        await Assertions.Expect(heading).ToBeVisibleAsync();
        await Assertions.Expect(page.GetByText("sign-in", new() { Exact = false })).ToBeVisibleAsync();
    }
}
