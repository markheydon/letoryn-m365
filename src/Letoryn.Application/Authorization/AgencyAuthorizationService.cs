using Letoryn.Application.Abstractions.Authorization;
using Letoryn.Application.Abstractions.Tenancy;
using Letoryn.Application.Agencies;
using Letoryn.Domain.Agencies;
using Letoryn.Domain.Memberships;

namespace Letoryn.Application.Authorization;

/// <summary>
/// Authorization checks derived from resolved tenancy context (FR-003, FR-006, FR-012).
/// </summary>
/// <remarks>
/// Role and membership fields are populated from the database on each API request via
/// <c>TenancyContextMiddleware</c> (no long-lived client cache), so role changes apply on the next request
/// without full sign-out (FR-005, quickstart §5).
/// </remarks>
public sealed class AgencyAuthorizationService(IAgencyContext agencyContext) : IAgencyAuthorizationService
{
    /// <inheritdoc />
    public AuthorizationResult AuthorizeRoutineShellAccess()
    {
        if (!agencyContext.HasRoutineShellAgencyContext)
        {
            return AuthorizationResult.Denied(
                AgencyAccessRules.MembershipNotActiveForShellMessage,
                AuthorizationFailureKind.NoActiveAgency);
        }

        return AuthorizationResult.Succeeded();
    }

    /// <inheritdoc />
    public AuthorizationResult AuthorizeAgencyScopedAccess(Guid agencyId)
    {
        if (agencyContext.ActiveAgencyId != agencyId)
        {
            return AuthorizationResult.NotFound();
        }

        if (agencyContext.ActiveAgencyLifecycleStatus is null)
        {
            return AuthorizationResult.NotFound();
        }

        return AgencyAccessRules.EvaluateAgencyHeaderAccess(
            agencyContext.ActiveAgencyLifecycleStatus.Value,
            agencyContext.ActiveMembershipStatus,
            agencyContext.IsOperatorAssignedToActiveAgency,
            isOperatorRoute: false);
    }

    /// <inheritdoc />
    public AuthorizationResult AuthorizeAgencyRole(IReadOnlyCollection<AgencyRole> allowedRoles)
    {
        if (agencyContext.ActiveAgencyRole is null || !allowedRoles.Contains(agencyContext.ActiveAgencyRole.Value))
        {
            return AuthorizationResult.Denied(
                "You do not have permission to perform this action.",
                AuthorizationFailureKind.Forbidden);
        }

        return AuthorizationResult.Succeeded();
    }

    /// <inheritdoc />
    public AuthorizationResult AuthorizeOperatorAgencyAccess(Guid agencyId)
    {
        if (agencyContext.ActiveAgencyId != agencyId || !agencyContext.IsOperatorAssignedToActiveAgency)
        {
            return AuthorizationResult.NotFound();
        }

        return AuthorizationResult.Succeeded();
    }

    /// <inheritdoc />
    public AuthorizationResult AuthorizeAgencyLifecycle(Guid agencyId, AgencyLifecycleStatus requiredStatus)
    {
        if (agencyContext.ActiveAgencyId != agencyId)
        {
            return AuthorizationResult.NotFound();
        }

        if (agencyContext.ActiveAgencyLifecycleStatus != requiredStatus)
        {
            return AuthorizationResult.Denied(
                "This action is not allowed for the agency in its current lifecycle state.",
                AuthorizationFailureKind.AgencyAccessBlocked);
        }

        return AuthorizationResult.Succeeded();
    }

    /// <inheritdoc />
    public AuthorizationResult AuthorizeMembershipStatus(Guid agencyId, MembershipStatus requiredStatus)
    {
        if (agencyContext.ActiveAgencyId != agencyId)
        {
            return AuthorizationResult.NotFound();
        }

        if (agencyContext.ActiveMembershipStatus != requiredStatus)
        {
            return AuthorizationResult.Denied(
                AgencyAccessRules.MembershipNotActiveForShellMessage,
                AuthorizationFailureKind.MembershipAccessBlocked);
        }

        return AuthorizationResult.Succeeded();
    }
}
