using Microsoft.Playwright;
using TenancyHub.E2E.Infrastructure;

namespace TenancyHub.E2E.Journeys;

/// <summary>R1-J5.1 — Operator agency create and lifecycle UI entry.</summary>
[Trait("Journey", "R1-J5.1")]
public sealed class R1_J5_1_OperatorProvisioningTests
{
    [Fact]
    public async Task Platform_operator_can_open_agencies_management()
    {
        var baseUrl = E2eSkipGuards.RequireBaseUrl();
        var storageState = E2eSkipGuards.RequireOperatorStorageState();
        var cancellationToken = TestContext.Current.CancellationToken;

        await using var session = await BrowserSession.CreateAsync(storageState, cancellationToken);
        var page = await session.NewPageAsync(cancellationToken);
        await page.GotoAsync($"{baseUrl}/operator/agencies", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Agencies" })).ToBeVisibleAsync();
        await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Create agency" })).ToBeVisibleAsync();
    }
}
