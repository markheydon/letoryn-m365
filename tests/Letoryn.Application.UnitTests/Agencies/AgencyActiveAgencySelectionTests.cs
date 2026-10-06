using Letoryn.Application.Agencies;
using Letoryn.Domain.Agencies;
using Letoryn.Domain.Memberships;

namespace Letoryn.Application.UnitTests.Agencies;

public sealed class AgencyActiveAgencySelectionTests
{
    [Fact]
    public void EvaluateMemberActiveAgencySelection_OperatorAssignedToSuspendedAgency_Succeeds()
    {
        var result = AgencyAccessRules.EvaluateMemberActiveAgencySelection(
            AgencyLifecycleStatus.Suspended,
            membershipStatus: null,
            isOperatorAssigned: true);

        Assert.True(result.IsAuthorized);
    }

    [Fact]
    public void EvaluateMemberActiveAgencySelection_ActiveMemberOnActiveAgency_Succeeds()
    {
        var result = AgencyAccessRules.EvaluateMemberActiveAgencySelection(
            AgencyLifecycleStatus.Active,
            MembershipStatus.Active,
            isOperatorAssigned: false);

        Assert.True(result.IsAuthorized);
    }

    [Fact]
    public void EvaluateMemberActiveAgencySelection_MemberOnSuspendedAgency_DeniesWithAgencyMessage()
    {
        var result = AgencyAccessRules.EvaluateMemberActiveAgencySelection(
            AgencyLifecycleStatus.Suspended,
            MembershipStatus.Active,
            isOperatorAssigned: false);

        Assert.False(result.IsAuthorized);
        Assert.Equal(AgencyAccessRules.AgencySuspendedMemberMessage, result.UserMessage);
    }

    [Fact]
    public void EvaluateMemberActiveAgencySelection_InvitedMemberOnActiveAgency_DeniesWithShellMessage()
    {
        var result = AgencyAccessRules.EvaluateMemberActiveAgencySelection(
            AgencyLifecycleStatus.Active,
            MembershipStatus.Invited,
            isOperatorAssigned: false);

        Assert.False(result.IsAuthorized);
        Assert.Equal(AgencyAccessRules.MembershipNotActiveForShellMessage, result.UserMessage);
    }

    [Fact]
    public void EvaluateMemberActiveAgencySelection_NoMembershipOrAssignment_ReturnsNotFound()
    {
        var result = AgencyAccessRules.EvaluateMemberActiveAgencySelection(
            AgencyLifecycleStatus.Active,
            membershipStatus: null,
            isOperatorAssigned: false);

        Assert.False(result.IsAuthorized);
        Assert.Null(result.UserMessage);
    }
}
