using TenancyHub.Domain.Agencies;
using TenancyHub.Domain.Memberships;

namespace TenancyHub.Application.Abstractions.Authorization;

/// <summary>
/// Central authorization entry points for agency isolation and role rules (FR-003, FR-006, FR-012).
/// </summary>
/// <remarks>
/// Implementations enforce server-side checks before persistence and return <see cref="AuthorizationResult"/>
/// for safe mapping to HTTP responses (FR-013). UI hiding alone is never sufficient (FR-011).
/// </remarks>
public interface IAgencyAuthorizationService
{
    /// <summary>
    /// Ensures the caller may perform routine agency-scoped work in the active shell context (FR-002).
    /// </summary>
    AuthorizationResult AuthorizeRoutineShellAccess();

    /// <summary>
    /// Ensures the caller may read or mutate data scoped to <paramref name="agencyId"/> (FR-003).
    /// </summary>
    /// <param name="agencyId">Target agency tenant key.</param>
    AuthorizationResult AuthorizeAgencyScopedAccess(Guid agencyId);

    /// <summary>
    /// Ensures the caller holds at least one of <paramref name="allowedRoles"/> for the active agency membership (FR-005, FR-006).
    /// </summary>
    /// <param name="allowedRoles">Permitted agency roles for the operation.</param>
    AuthorizationResult AuthorizeAgencyRole(IReadOnlyCollection<AgencyRole> allowedRoles);

    /// <summary>
    /// Ensures a platform operator may target <paramref name="agencyId"/> via operator flows (FR-006, FR-009).
    /// </summary>
    /// <param name="agencyId">Agency being accessed outside routine shell impersonation.</param>
    AuthorizationResult AuthorizeOperatorAgencyAccess(Guid agencyId);

    /// <summary>
    /// Ensures agency lifecycle permits the attempted mutation (FR-004, FR-007).
    /// </summary>
    /// <param name="agencyId">Agency whose lifecycle is evaluated.</param>
    /// <param name="requiredStatus">Lifecycle state required for the operation.</param>
    AuthorizationResult AuthorizeAgencyLifecycle(Guid agencyId, AgencyLifecycleStatus requiredStatus);

    /// <summary>
    /// Ensures membership status permits agency access (FR-007).
    /// </summary>
    /// <param name="agencyId">Agency whose membership is evaluated.</param>
    /// <param name="requiredStatus">Membership status required for the operation.</param>
    AuthorizationResult AuthorizeMembershipStatus(Guid agencyId, MembershipStatus requiredStatus);
}
