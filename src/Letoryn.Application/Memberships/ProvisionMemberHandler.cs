using Letoryn.Application.Abstractions.Memberships;
using Letoryn.Domain.Memberships;

namespace Letoryn.Application.Memberships;

/// <summary>Provision member command handler (immediate active membership).</summary>
public sealed class ProvisionMemberHandler(IMembershipOperations operations)
{
    /// <summary>Provisions an active member by email.</summary>
    public Task<MembershipOperationResult> HandleAsync(
        Guid actorUserIdentityId,
        Guid agencyId,
        string email,
        AgencyRole role,
        bool actorIsAdministrator,
        bool actorIsOperatorOnAgency,
        CancellationToken cancellationToken = default) =>
        operations.ProvisionMemberAsync(
            actorUserIdentityId,
            agencyId,
            email,
            role,
            actorIsAdministrator,
            actorIsOperatorOnAgency,
            cancellationToken);
}
