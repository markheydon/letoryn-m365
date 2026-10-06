using TenancyHub.Application.Abstractions.Memberships;
using TenancyHub.Domain.Agencies;

namespace TenancyHub.Application.Abstractions.Agencies;

/// <summary>Agency settings and operator lifecycle commands.</summary>
public interface IAgencyOperations
{
    /// <summary>Reads agency settings when permitted.</summary>
    Task<AgencySettingsReadResult> GetSettingsAsync(
        Guid agencyId,
        bool actorIsAdministrator,
        bool actorIsOperatorOnAgency,
        CancellationToken cancellationToken = default);

    /// <summary>Updates agency settings when permitted.</summary>
    Task<AgencyOperationResult> UpdateSettingsAsync(
        Guid actorUserIdentityId,
        Guid agencyId,
        string? displayName,
        string? primaryContactEmail,
        string? primaryContactPhone,
        bool actorIsAdministrator,
        bool actorIsOperatorOnAgency,
        CancellationToken cancellationToken = default);

    /// <summary>Creates a new agency and assigns the creating operator.</summary>
    Task<AgencyOperationResult> CreateAgencyAsync(
        Guid operatorUserIdentityId,
        string displayName,
        string primaryContactEmail,
        string primaryContactPhone,
        CancellationToken cancellationToken = default);

    /// <summary>Transitions agency lifecycle status.</summary>
    Task<AgencyOperationResult> ChangeLifecycleAsync(
        Guid operatorUserIdentityId,
        Guid agencyId,
        AgencyLifecycleStatus targetStatus,
        CancellationToken cancellationToken = default);
}

/// <summary>Agency mutation outcome.</summary>
public sealed record AgencyOperationResult(
    MembershipOperationStatus Status,
    string? UserMessage = null,
    Guid? AgencyId = null);
