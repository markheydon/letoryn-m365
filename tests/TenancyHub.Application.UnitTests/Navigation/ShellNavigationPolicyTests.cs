using TenancyHub.Application.Me;
using TenancyHub.Application.Navigation;
using TenancyHub.Domain.Memberships;

namespace TenancyHub.Application.UnitTests.Navigation;

public sealed class ShellNavigationPolicyTests
{
    private static readonly Guid AgencyId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid OtherAgencyId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    [Fact]
    public void GetPermittedItems_InviteOnlyGate_ReturnsInvitationsOnly()
    {
        var profile = Profile(
            memberships: [],
            pendingInvites: [new MePendingInviteSummary(Guid.NewGuid(), AgencyId, "Agency A", "StandardMember")]);

        var context = ShellNavigationContext.FromMeProfile(profile, activeAgencyId: null);
        var items = ShellNavigationPolicy.GetPermittedItems(context);

        Assert.Single(items);
        Assert.Equal(ShellNavigationItemId.Invitations, items[0].Id);
        Assert.Equal(ShellNavigationRoutes.Invitations, items[0].Href);
    }

    [Fact]
    public void GetPermittedItems_OperatorGlobalOnly_ReturnsOperatorEntries()
    {
        var profile = Profile(isPlatformOperator: true);

        var context = ShellNavigationContext.FromMeProfile(profile, activeAgencyId: null);
        var items = ShellNavigationPolicy.GetPermittedItems(context);

        Assert.Equal(2, items.Count);
        Assert.All(items, i => Assert.Equal(ShellNavigationGroup.Operator, i.Group));
        Assert.Contains(items, i => i.Id == ShellNavigationItemId.OperatorAgencies);
        Assert.Contains(items, i => i.Id == ShellNavigationItemId.OperatorPlatformOperators);
    }

    [Fact]
    public void GetPermittedItems_ActiveAdministratorOnActiveAgency_IncludesRoutineAndAudit()
    {
        var profile = Profile(memberships: [ActiveMembership(AgencyId, AgencyRole.Administrator)]);
        var context = ShellNavigationContext.FromMeProfile(profile, AgencyId);

        var items = ShellNavigationPolicy.GetPermittedItems(context);
        var ids = items.Select(i => i.Id).ToHashSet();

        Assert.Contains(ShellNavigationItemId.Home, ids);
        Assert.Contains(ShellNavigationItemId.Members, ids);
        Assert.Contains(ShellNavigationItemId.AgencySettings, ids);
        Assert.Contains(ShellNavigationItemId.Audit, ids);
    }

    [Fact]
    public void GetPermittedItems_StandardMember_IncludesMembersButNotAuditOrSettings()
    {
        var profile = Profile(memberships: [ActiveMembership(AgencyId, AgencyRole.StandardMember)]);
        var context = ShellNavigationContext.FromMeProfile(profile, AgencyId);

        var items = ShellNavigationPolicy.GetPermittedItems(context);
        var ids = items.Select(i => i.Id).ToHashSet();

        Assert.Contains(ShellNavigationItemId.Home, ids);
        Assert.Contains(ShellNavigationItemId.Members, ids);
        Assert.DoesNotContain(ShellNavigationItemId.Audit, ids);
        Assert.DoesNotContain(ShellNavigationItemId.AgencySettings, ids);
    }

    [Fact]
    public void CanViewMemberRoster_ReadOnlyMember_ReturnsFalse()
    {
        var profile = Profile(memberships: [ActiveMembership(AgencyId, AgencyRole.ReadOnlyMember)]);
        var context = ShellNavigationContext.FromMeProfile(profile, AgencyId);

        Assert.False(ShellNavigationPolicy.CanViewMemberRoster(context));
    }

    [Fact]
    public void GetPermittedItems_ReadOnlyMember_ExcludesMembersAndAudit()
    {
        var profile = Profile(memberships: [ActiveMembership(AgencyId, AgencyRole.ReadOnlyMember)]);
        var context = ShellNavigationContext.FromMeProfile(profile, AgencyId);

        var items = ShellNavigationPolicy.GetPermittedItems(context);
        var ids = items.Select(i => i.Id).ToHashSet();

        Assert.Contains(ShellNavigationItemId.Home, ids);
        Assert.DoesNotContain(ShellNavigationItemId.Members, ids);
        Assert.DoesNotContain(ShellNavigationItemId.Audit, ids);
    }

    [Fact]
    public void GetPermittedItems_PendingInvitesWithActiveMembership_IncludesInvitationsLink()
    {
        var profile = Profile(
            memberships: [ActiveMembership(AgencyId, AgencyRole.Administrator)],
            pendingInvites: [new MePendingInviteSummary(Guid.NewGuid(), OtherAgencyId, "Agency B", "StandardMember")]);

        var context = ShellNavigationContext.FromMeProfile(profile, AgencyId);
        var items = ShellNavigationPolicy.GetPermittedItems(context);

        Assert.Contains(items, i => i.Id == ShellNavigationItemId.Invitations);
    }

    [Fact]
    public void GetPermittedItems_OperatorAssignedToSuspendedAgency_IncludesSettingsAndDiagnosticsNotMemberAudit()
    {
        var profile = Profile(
            isPlatformOperator: true,
            operatorAssignments: [new MeOperatorAssignmentSummary(AgencyId, "Agency A", "Suspended")]);

        var context = ShellNavigationContext.FromMeProfile(profile, AgencyId);
        var items = ShellNavigationPolicy.GetPermittedItems(context);
        var ids = items.Select(i => i.Id).ToHashSet();

        Assert.Contains(ShellNavigationItemId.AgencySettings, ids);
        Assert.Contains(ShellNavigationItemId.OperatorAgencyDiagnostics, ids);
        Assert.DoesNotContain(ShellNavigationItemId.Audit, ids);
        Assert.Contains(ShellNavigationItemId.OperatorAgencies, ids);
    }

    private static MeProfileResponse Profile(
        bool isPlatformOperator = false,
        IReadOnlyList<MeMembershipSummary>? memberships = null,
        IReadOnlyList<MeOperatorAssignmentSummary>? operatorAssignments = null,
        IReadOnlyList<MePendingInviteSummary>? pendingInvites = null) =>
        new(
            Guid.NewGuid(),
            "user@example.com",
            isPlatformOperator,
            LastUsedAgencyId: null,
            memberships ?? [],
            operatorAssignments ?? [],
            pendingInvites ?? []);

    private static MeMembershipSummary ActiveMembership(Guid agencyId, AgencyRole role) =>
        new(agencyId, "Agency A", MembershipStatus.Active.ToString(), role.ToString(), "Active");
}
