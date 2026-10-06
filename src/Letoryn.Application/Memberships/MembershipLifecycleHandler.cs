using Letoryn.Application.Abstractions.Memberships;
using Letoryn.Domain.Memberships;

namespace Letoryn.Application.Memberships;

/// <summary>Membership suspend, reactivate, remove, revoke, and role change commands.</summary>
public sealed class MembershipLifecycleHandler(IMembershipOperations operations)
{
    /// <summary>Changes membership role.</summary>
    public Task<MembershipOperationResult> ChangeRoleAsync(
        Guid actorUserIdentityId,
        Guid agencyId,
        Guid membershipId,
        AgencyRole role,
        bool actorIsAdministrator,
        bool actorIsOperatorOnAgency,
        CancellationToken cancellationToken = default) =>
        operations.ChangeRoleAsync(
            actorUserIdentityId,
            agencyId,
            membershipId,
            role,
            actorIsAdministrator,
            actorIsOperatorOnAgency,
            cancellationToken);

    /// <summary>Suspends a member.</summary>
    public Task<MembershipOperationResult> SuspendAsync(
        Guid actorUserIdentityId,
        Guid agencyId,
        Guid membershipId,
        bool actorIsAdministrator,
        bool actorIsOperatorOnAgency,
        CancellationToken cancellationToken = default) =>
        operations.SuspendMemberAsync(
            actorUserIdentityId,
            agencyId,
            membershipId,
            actorIsAdministrator,
            actorIsOperatorOnAgency,
            cancellationToken);

    /// <summary>Reactivates a suspended member.</summary>
    public Task<MembershipOperationResult> ReactivateAsync(
        Guid actorUserIdentityId,
        Guid agencyId,
        Guid membershipId,
        bool actorIsAdministrator,
        bool actorIsOperatorOnAgency,
        CancellationToken cancellationToken = default) =>
        operations.ReactivateMemberAsync(
            actorUserIdentityId,
            agencyId,
            membershipId,
            actorIsAdministrator,
            actorIsOperatorOnAgency,
            cancellationToken);

    /// <summary>Removes membership access.</summary>
    public Task<MembershipOperationResult> RemoveAsync(
        Guid actorUserIdentityId,
        Guid agencyId,
        Guid membershipId,
        bool actorIsAdministrator,
        bool actorIsOperatorOnAgency,
        CancellationToken cancellationToken = default) =>
        operations.RemoveMemberAsync(
            actorUserIdentityId,
            agencyId,
            membershipId,
            actorIsAdministrator,
            actorIsOperatorOnAgency,
            cancellationToken);

    /// <summary>Revokes a pending invitation.</summary>
    public Task<MembershipOperationResult> RevokeInvitationAsync(
        Guid actorUserIdentityId,
        Guid agencyId,
        Guid membershipId,
        bool actorIsAdministrator,
        bool actorIsOperatorOnAgency,
        CancellationToken cancellationToken = default) =>
        operations.RevokeInvitationAsync(
            actorUserIdentityId,
            agencyId,
            membershipId,
            actorIsAdministrator,
            actorIsOperatorOnAgency,
            cancellationToken);
}
