using TenancyHub.Application.Abstractions.Memberships;
using TenancyHub.Domain.Memberships;

namespace TenancyHub.Application.Memberships;

/// <summary>Invite member command handler (idempotent pending invite).</summary>
public sealed class InviteMemberHandler(IMembershipOperations operations)
{
    /// <summary>Invites a member by email.</summary>
    public Task<MembershipOperationResult> HandleAsync(
        Guid actorUserIdentityId,
        Guid agencyId,
        string email,
        AgencyRole role,
        bool actorIsAdministrator,
        bool actorIsOperatorOnAgency,
        CancellationToken cancellationToken = default) =>
        operations.InviteMemberAsync(
            actorUserIdentityId,
            agencyId,
            email,
            role,
            actorIsAdministrator,
            actorIsOperatorOnAgency,
            cancellationToken);
}
