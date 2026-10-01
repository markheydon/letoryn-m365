using TenancyHub.Domain.Agencies;
using TenancyHub.Domain.Memberships;

namespace TenancyHub.Application.Abstractions.Tenancy;

/// <summary>
/// Active agency tenancy boundary for routine shell and agency-scoped operations (FR-002, FR-012).
/// </summary>
/// <remarks>
/// <para>
/// One authenticated session binds to at most one active agency at a time for routine agency-scoped work.
/// Platform operators without assignments may have no active agency while using operator-only global flows (FR-002).
/// </para>
/// <para>
/// Feature modules MUST treat <see cref="ActiveAgencyId"/> and membership fields as authoritative for isolation (FR-003)
/// and MUST NOT maintain parallel tenancy state.
/// </para>
/// </remarks>
public interface IAgencyContext
{
    /// <summary>
    /// Canonical agency tenant key when routine shell context is established; otherwise <see langword="null"/>.
    /// </summary>
    Guid? ActiveAgencyId { get; }

    /// <summary>
    /// Lifecycle status of <see cref="ActiveAgencyId"/> when present.
    /// </summary>
    AgencyLifecycleStatus? ActiveAgencyLifecycleStatus { get; }

    /// <summary>
    /// Active membership identifier for the user in <see cref="ActiveAgencyId"/> when resolved.
    /// </summary>
    Guid? ActiveMembershipId { get; }

    /// <summary>
    /// Membership status for <see cref="ActiveMembershipId"/> when resolved.
    /// </summary>
    MembershipStatus? ActiveMembershipStatus { get; }

    /// <summary>
    /// Agency role for <see cref="ActiveMembershipId"/> when resolved (FR-005).
    /// </summary>
    AgencyRole? ActiveAgencyRole { get; }

    /// <summary>
    /// Whether the current user is assigned as a platform operator to <see cref="ActiveAgencyId"/> (FR-006).
    /// </summary>
    bool IsOperatorAssignedToActiveAgency { get; }

    /// <summary>
    /// Whether routine agency-scoped shell work is permitted: active agency, active membership, and agency not blocking members (FR-002).
    /// </summary>
    bool HasRoutineShellAgencyContext { get; }
}
