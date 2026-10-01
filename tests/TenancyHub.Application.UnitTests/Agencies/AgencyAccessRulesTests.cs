using TenancyHub.Application.Agencies;
using TenancyHub.Domain.Agencies;
using TenancyHub.Domain.Memberships;

namespace TenancyHub.Application.UnitTests.Agencies;

public sealed class AgencyAccessRulesTests
{
    [Fact]
    public void EvaluateAgencyHeaderAccess_ActiveMembershipOnActiveAgency_Succeeds()
    {
        var result = AgencyAccessRules.EvaluateAgencyHeaderAccess(
            AgencyLifecycleStatus.Active,
            MembershipStatus.Active,
            isOperatorAssigned: false,
            isOperatorRoute: false);

        Assert.True(result.IsAuthorized);
    }

    [Theory]
    [InlineData(MembershipStatus.Invited)]
    [InlineData(MembershipStatus.Removed)]
    [InlineData(MembershipStatus.Suspended)]
    public void EvaluateAgencyHeaderAccess_NonActiveMembershipOnMemberRoute_Denies(MembershipStatus status)
    {
        var result = AgencyAccessRules.EvaluateAgencyHeaderAccess(
            AgencyLifecycleStatus.Active,
            status,
            isOperatorAssigned: false,
            isOperatorRoute: false);

        Assert.False(result.IsAuthorized);
    }

    [Fact]
    public void EvaluateAgencyHeaderAccess_SuspendedAgencyOnMemberRoute_DeniesWithAgencyMessage()
    {
        var result = AgencyAccessRules.EvaluateAgencyHeaderAccess(
            AgencyLifecycleStatus.Suspended,
            MembershipStatus.Active,
            isOperatorAssigned: false,
            isOperatorRoute: false);

        Assert.False(result.IsAuthorized);
        Assert.Equal(AgencyAccessRules.AgencySuspendedMemberMessage, result.UserMessage);
    }

    [Fact]
    public void EvaluateAgencyHeaderAccess_ArchivedAgencyOnMemberRoute_Denies()
    {
        var result = AgencyAccessRules.EvaluateAgencyHeaderAccess(
            AgencyLifecycleStatus.Archived,
            MembershipStatus.Active,
            isOperatorAssigned: false,
            isOperatorRoute: false);

        Assert.False(result.IsAuthorized);
    }

    [Fact]
    public void EvaluateAgencyHeaderAccess_OperatorRouteRequiresAssignment()
    {
        var withoutAssignment = AgencyAccessRules.EvaluateAgencyHeaderAccess(
            AgencyLifecycleStatus.Archived,
            membershipStatus: null,
            isOperatorAssigned: false,
            isOperatorRoute: true);
        Assert.False(withoutAssignment.IsAuthorized);

        var withAssignment = AgencyAccessRules.EvaluateAgencyHeaderAccess(
            AgencyLifecycleStatus.Archived,
            membershipStatus: null,
            isOperatorAssigned: true,
            isOperatorRoute: true);
        Assert.True(withAssignment.IsAuthorized);
    }
}
