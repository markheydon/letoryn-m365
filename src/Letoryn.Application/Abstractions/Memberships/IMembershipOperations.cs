using Letoryn.Domain.Memberships;

namespace Letoryn.Application.Abstractions.Memberships;

/// <summary>Agency membership invite, provision, and lifecycle commands.</summary>
public interface IMembershipOperations
{
    /// <summary>Lists pending invitations for the caller's email.</summary>
    Task<IReadOnlyList<PendingInvitationDto>> ListPendingInvitationsAsync(
        Guid userIdentityId,
        string userEmail,
        CancellationToken cancellationToken = default);

    /// <summary>Accepts an invitation when caller email matches.</summary>
    Task<MembershipOperationResult> AcceptInvitationAsync(
        Guid userIdentityId,
        string userEmail,
        Guid membershipId,
        CancellationToken cancellationToken = default);

    /// <summary>Declines an invitation when caller email matches.</summary>
    Task<MembershipOperationResult> DeclineInvitationAsync(
        Guid userIdentityId,
        string userEmail,
        Guid membershipId,
        CancellationToken cancellationToken = default);

    /// <summary>Invites a member by email (idempotent pending invite).</summary>
    Task<MembershipOperationResult> InviteMemberAsync(
        Guid actorUserIdentityId,
        Guid agencyId,
        string email,
        AgencyRole role,
        bool actorIsAdministrator,
        bool actorIsOperatorOnAgency,
        CancellationToken cancellationToken = default);

    /// <summary>Provisions an active member by email.</summary>
    Task<MembershipOperationResult> ProvisionMemberAsync(
        Guid actorUserIdentityId,
        Guid agencyId,
        string email,
        AgencyRole role,
        bool actorIsAdministrator,
        bool actorIsOperatorOnAgency,
        CancellationToken cancellationToken = default);

    /// <summary>Lists agency roster entries.</summary>
    Task<IReadOnlyList<MembershipRosterDto>?> ListRosterAsync(
        Guid agencyId,
        bool canViewRoster,
        CancellationToken cancellationToken = default);

    /// <summary>Changes role on invited or active membership.</summary>
    Task<MembershipOperationResult> ChangeRoleAsync(
        Guid actorUserIdentityId,
        Guid agencyId,
        Guid membershipId,
        AgencyRole newRole,
        bool actorIsAdministrator,
        bool actorIsOperatorOnAgency,
        CancellationToken cancellationToken = default);

    /// <summary>Suspends an active membership.</summary>
    Task<MembershipOperationResult> SuspendMemberAsync(
        Guid actorUserIdentityId,
        Guid agencyId,
        Guid membershipId,
        bool actorIsAdministrator,
        bool actorIsOperatorOnAgency,
        CancellationToken cancellationToken = default);

    /// <summary>Reactivates a suspended membership.</summary>
    Task<MembershipOperationResult> ReactivateMemberAsync(
        Guid actorUserIdentityId,
        Guid agencyId,
        Guid membershipId,
        bool actorIsAdministrator,
        bool actorIsOperatorOnAgency,
        CancellationToken cancellationToken = default);

    /// <summary>Removes membership access.</summary>
    Task<MembershipOperationResult> RemoveMemberAsync(
        Guid actorUserIdentityId,
        Guid agencyId,
        Guid membershipId,
        bool actorIsAdministrator,
        bool actorIsOperatorOnAgency,
        CancellationToken cancellationToken = default);

    /// <summary>Revokes a pending invitation.</summary>
    Task<MembershipOperationResult> RevokeInvitationAsync(
        Guid actorUserIdentityId,
        Guid agencyId,
        Guid membershipId,
        bool actorIsAdministrator,
        bool actorIsOperatorOnAgency,
        CancellationToken cancellationToken = default);
}

/// <summary>Pending invitation row for invitee APIs.</summary>
public sealed record PendingInvitationDto(
    Guid MembershipId,
    Guid AgencyId,
    string AgencyDisplayName,
    string Role,
    DateTimeOffset ExpiresAt);

/// <summary>Agency membership roster row.</summary>
public sealed record MembershipRosterDto(
    Guid MembershipId,
    string Email,
    string Status,
    string Role);
