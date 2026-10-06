using Letoryn.Application.Abstractions.Authorization;
using Letoryn.Application.Abstractions.Tenancy;
using Letoryn.Application.Agencies;
using Letoryn.Application.Authorization;
using Letoryn.Domain.Agencies;
using Letoryn.Domain.Memberships;

namespace Letoryn.Application.UnitTests.Authorization;

/// <summary>
/// Isolation matrix: tenancy header rules plus scoped authorization (FR-003, FR-004, FR-007).
/// </summary>
public sealed class AgencyIsolationTests
{
    [Fact]
    public void AuthorizeAgencyScopedAccess_WhenAgencyIdDiffersFromContext_ReturnsNotFound()
    {
        var contextAgencyId = Guid.NewGuid();
        var requestedAgencyId = Guid.NewGuid();
        IAgencyContext context = new TestAgencyContext
        {
            ActiveAgencyId = contextAgencyId,
            ActiveAgencyLifecycleStatus = AgencyLifecycleStatus.Active,
            ActiveMembershipStatus = MembershipStatus.Active,
            ActiveMembershipId = Guid.NewGuid(),
            ActiveAgencyRole = AgencyRole.StandardMember,
        };
        IAgencyAuthorizationService service = new AgencyAuthorizationService(context);

        var result = service.AuthorizeAgencyScopedAccess(requestedAgencyId);

        Assert.False(result.IsAuthorized);
        Assert.Equal(AuthorizationFailureKind.NotFound, result.FailureKind);
        Assert.Null(result.UserMessage);
    }

    [Fact]
    public void AuthorizeAgencyScopedAccess_WhenMembershipSuspendedOnActiveAgency_DeniesWithMembershipMessage()
    {
        var agencyId = Guid.NewGuid();
        IAgencyContext context = new TestAgencyContext
        {
            ActiveAgencyId = agencyId,
            ActiveAgencyLifecycleStatus = AgencyLifecycleStatus.Active,
            ActiveMembershipStatus = MembershipStatus.Suspended,
            ActiveMembershipId = Guid.NewGuid(),
            ActiveAgencyRole = AgencyRole.StandardMember,
        };
        IAgencyAuthorizationService service = new AgencyAuthorizationService(context);

        var result = service.AuthorizeAgencyScopedAccess(agencyId);

        Assert.False(result.IsAuthorized);
        Assert.Equal(AgencyAccessRules.MembershipSuspendedMessage, result.UserMessage);
        Assert.Equal(AuthorizationFailureKind.MembershipAccessBlocked, result.FailureKind);
    }

    [Fact]
    public void AuthorizeAgencyScopedAccess_WhenAgencySuspendedWithActiveMembership_DeniesWithAgencySuspendedMessage()
    {
        var agencyId = Guid.NewGuid();
        IAgencyContext context = new TestAgencyContext
        {
            ActiveAgencyId = agencyId,
            ActiveAgencyLifecycleStatus = AgencyLifecycleStatus.Suspended,
            ActiveMembershipStatus = MembershipStatus.Active,
            ActiveMembershipId = Guid.NewGuid(),
            ActiveAgencyRole = AgencyRole.StandardMember,
        };
        IAgencyAuthorizationService service = new AgencyAuthorizationService(context);

        var result = service.AuthorizeAgencyScopedAccess(agencyId);

        Assert.False(result.IsAuthorized);
        Assert.Equal(AgencyAccessRules.AgencySuspendedMemberMessage, result.UserMessage);
        Assert.Equal(AuthorizationFailureKind.AgencyAccessBlocked, result.FailureKind);
    }

    [Fact]
    public void AuthorizeAgencyScopedAccess_WhenAgencyArchived_DeniesWithAgencyArchivedMessage()
    {
        var agencyId = Guid.NewGuid();
        IAgencyContext context = new TestAgencyContext
        {
            ActiveAgencyId = agencyId,
            ActiveAgencyLifecycleStatus = AgencyLifecycleStatus.Archived,
            ActiveMembershipStatus = MembershipStatus.Active,
            ActiveMembershipId = Guid.NewGuid(),
            ActiveAgencyRole = AgencyRole.StandardMember,
        };
        IAgencyAuthorizationService service = new AgencyAuthorizationService(context);

        var result = service.AuthorizeAgencyScopedAccess(agencyId);

        Assert.False(result.IsAuthorized);
        Assert.Equal(AgencyAccessRules.AgencyArchivedMemberMessage, result.UserMessage);
        Assert.Equal(AuthorizationFailureKind.AgencyAccessBlocked, result.FailureKind);
    }

    [Fact]
    public void EvaluateAgencyHeaderAccess_OperatorRouteWithoutAssignment_ReturnsNotFound()
    {
        var result = AgencyAccessRules.EvaluateAgencyHeaderAccess(
            AgencyLifecycleStatus.Active,
            membershipStatus: null,
            isOperatorAssigned: false,
            isOperatorRoute: true);

        Assert.False(result.IsAuthorized);
        Assert.Equal(AuthorizationFailureKind.NotFound, result.FailureKind);
        Assert.Null(result.UserMessage);
    }

    [Fact]
    public void EvaluateAgencyHeaderAccess_OperatorRouteWithAssignmentOnArchivedAgency_Succeeds()
    {
        var result = AgencyAccessRules.EvaluateAgencyHeaderAccess(
            AgencyLifecycleStatus.Archived,
            membershipStatus: null,
            isOperatorAssigned: true,
            isOperatorRoute: true);

        Assert.True(result.IsAuthorized);
    }

    [Fact]
    public void OperatorFlow_OnArchivedAgency_HeaderAccessThenOperatorAuthorization_Succeeds()
    {
        var agencyId = Guid.NewGuid();
        IAgencyContext context = new TestAgencyContext
        {
            ActiveAgencyId = agencyId,
            ActiveAgencyLifecycleStatus = AgencyLifecycleStatus.Archived,
            ActiveMembershipStatus = null,
            IsOperatorAssignedToActiveAgency = true,
        };

        var headerAccess = AgencyAccessRules.EvaluateAgencyHeaderAccess(
            context.ActiveAgencyLifecycleStatus!.Value,
            context.ActiveMembershipStatus,
            context.IsOperatorAssignedToActiveAgency,
            isOperatorRoute: true);

        IAgencyAuthorizationService service = new AgencyAuthorizationService(context);
        var operatorAccess = service.AuthorizeOperatorAgencyAccess(agencyId);

        Assert.True(headerAccess.IsAuthorized);
        Assert.True(operatorAccess.IsAuthorized);
    }

    [Fact]
    public void MemberFlow_OnActiveAgency_HeaderAccessThenScopedAuthorization_Succeeds()
    {
        var agencyId = Guid.NewGuid();
        IAgencyContext context = new TestAgencyContext
        {
            ActiveAgencyId = agencyId,
            ActiveAgencyLifecycleStatus = AgencyLifecycleStatus.Active,
            ActiveMembershipStatus = MembershipStatus.Active,
            ActiveMembershipId = Guid.NewGuid(),
            ActiveAgencyRole = AgencyRole.ReadOnlyMember,
        };

        var headerAccess = AgencyAccessRules.EvaluateAgencyHeaderAccess(
            context.ActiveAgencyLifecycleStatus!.Value,
            context.ActiveMembershipStatus,
            context.IsOperatorAssignedToActiveAgency,
            isOperatorRoute: false);

        IAgencyAuthorizationService service = new AgencyAuthorizationService(context);
        var scopedAccess = service.AuthorizeAgencyScopedAccess(agencyId);

        Assert.True(headerAccess.IsAuthorized);
        Assert.True(scopedAccess.IsAuthorized);
    }
}
