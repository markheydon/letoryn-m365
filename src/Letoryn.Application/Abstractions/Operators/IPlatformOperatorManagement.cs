using Letoryn.Application.Abstractions.Memberships;

namespace Letoryn.Application.Abstractions.Operators;

/// <summary>Platform operator grant, revoke, and assignment management.</summary>
public interface IPlatformOperatorManagement
{
    /// <summary>Lists platform operators.</summary>
    Task<IReadOnlyList<PlatformOperatorDto>> ListOperatorsAsync(CancellationToken cancellationToken = default);

    /// <summary>Grants platform operator status.</summary>
    Task<MembershipOperationResult> GrantOperatorAsync(
        Guid actorUserIdentityId,
        Guid targetUserIdentityId,
        CancellationToken cancellationToken = default);

    /// <summary>Revokes platform operator status.</summary>
    Task<MembershipOperationResult> RevokeOperatorAsync(
        Guid actorUserIdentityId,
        Guid targetUserIdentityId,
        CancellationToken cancellationToken = default);

    /// <summary>Replaces agency assignments for an operator.</summary>
    Task<MembershipOperationResult> SetAssignmentsAsync(
        Guid actorUserIdentityId,
        Guid targetUserIdentityId,
        IReadOnlyList<Guid> agencyIds,
        CancellationToken cancellationToken = default);
}

/// <summary>Platform operator summary row.</summary>
public sealed record PlatformOperatorDto(
    Guid UserId,
    string Email,
    IReadOnlyList<Guid> AssignedAgencyIds);
