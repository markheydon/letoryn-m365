using TenancyHub.Application.Me;

namespace TenancyHub.Application.Abstractions.Me;

/// <summary>Loads signed-in user profile for /api/v1/me.</summary>
public interface IMeProfileService
{
    /// <summary>Builds the caller profile including memberships and operator assignments.</summary>
    Task<MeProfileResponse?> GetProfileAsync(Guid userIdentityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets last-used agency when the caller may use that agency context (membership or operator assignment).
    /// </summary>
    Task<SetActiveAgencyOutcome> SetActiveAgencyAsync(
        Guid userIdentityId,
        Guid agencyId,
        CancellationToken cancellationToken = default);
}

/// <summary>Outcome of active agency selection.</summary>
public enum SetActiveAgencyResult
{
    /// <summary>Agency context updated.</summary>
    Succeeded,

    /// <summary>Agency or membership not permitted.</summary>
    Forbidden,

    /// <summary>Agency not found or not visible.</summary>
    NotFound,
}
