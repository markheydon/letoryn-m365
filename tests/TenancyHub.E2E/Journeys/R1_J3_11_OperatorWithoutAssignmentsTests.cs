using Microsoft.Playwright;
using TenancyHub.E2E.Infrastructure;

namespace TenancyHub.E2E.Journeys;

/// <summary>R1-J3.11 — Platform operator without agency assignments → operator home.</summary>
[Trait("Journey", "R1-J3.11")]
public sealed class R1_J3_11_OperatorWithoutAssignmentsTests
{
    [Fact]
    public async Task Operator_without_assignments_sees_operator_home()
    {
        var baseUrl = E2eSkipGuards.RequireBaseUrl();
        var storageState = E2eSkipGuards.RequireOperatorStorageState();
        var cancellationToken = TestContext.Current.CancellationToken;

        await using var session = await BrowserSession.CreateAsync(storageState, cancellationToken);
        var page = await session.NewPageAsync(cancellationToken);
        await E2ePlaywrightAssertions.GotoAppRootAsync(page, baseUrl, cancellationToken);

        await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Platform operator" })).ToBeVisibleAsync();
        await Assertions.Expect(page.GetByRole(AriaRole.Link, new() { Name = "Open agencies" })).ToBeVisibleAsync();
    }
}
