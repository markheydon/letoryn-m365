namespace Letoryn.Application.Abstractions.Identities;

/// <summary>Provisions product identity from Entra token claims (US1).</summary>
public interface IEnsureUserIdentityService
{
    /// <summary>
    /// Ensures a user identity exists for the Entra object id and normalised email.
    /// </summary>
    /// <returns>Internal user identity id and profile fields.</returns>
    Task<EnsuredUserIdentity> EnsureAsync(
        string entraObjectId,
        string email,
        CancellationToken cancellationToken = default);
}

/// <summary>Result of identity provisioning.</summary>
public sealed record EnsuredUserIdentity(
    Guid UserIdentityId,
    string EntraObjectId,
    string Email,
    bool IsPlatformOperator,
    Guid? LastUsedAgencyId);
