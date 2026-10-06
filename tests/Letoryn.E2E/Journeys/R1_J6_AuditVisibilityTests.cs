using Letoryn.E2E.Infrastructure;
using Microsoft.Playwright;

namespace Letoryn.E2E.Journeys;

/// <summary>R1-J6 — Audit visibility for administrators; standard members denied.</summary>
[Trait("Journey", "R1-J6")]
public sealed class R1_J6_AuditVisibilityTests
{
    [Fact]
    public async Task Agency_administrator_can_view_audit_history()
    {
        var baseUrl = E2eSkipGuards.RequireBaseUrl();
        var storageState = E2eSkipGuards.RequireAdministratorStorageState();
        var cancellationToken = TestContext.Current.CancellationToken;

        await using var session = await BrowserSession.CreateAsync(storageState, cancellationToken);
        var page = await session.NewPageAsync(cancellationToken);
        await page.GotoAsync($"{baseUrl}/audit", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Audit history" })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Standard_member_is_denied_audit_history()
    {
        var baseUrl = E2eSkipGuards.RequireBaseUrl();
        var storageState = E2eSkipGuards.RequireStandardMemberStorageState();
        var cancellationToken = TestContext.Current.CancellationToken;

        await using var session = await BrowserSession.CreateAsync(storageState, cancellationToken);
        var page = await session.NewPageAsync(cancellationToken);
        await page.GotoAsync($"{baseUrl}/audit", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        await Assertions.Expect(page.GetByText("do not have permission to view audit history", new() { Exact = false }))
            .ToBeVisibleAsync();
    }
}
