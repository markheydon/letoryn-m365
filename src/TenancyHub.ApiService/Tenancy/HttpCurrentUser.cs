using TenancyHub.Application.Abstractions.Tenancy;

namespace TenancyHub.ApiService.Tenancy;

/// <summary>Resolves <see cref="ICurrentUser"/> from the per-request user snapshot cache.</summary>
public sealed class HttpCurrentUser(CurrentUserSnapshotCache cache) : ICurrentUser
{
    /// <inheritdoc />
    public Guid UserIdentityId => Snapshot.UserIdentityId;

    /// <inheritdoc />
    public string EntraObjectId => Snapshot.EntraObjectId;

    /// <inheritdoc />
    public string Email => Snapshot.Email;

    /// <inheritdoc />
    public bool IsPlatformOperator => Snapshot.IsPlatformOperator;

    private CurrentUserSnapshotCache.UserSnapshot Snapshot =>
        cache.IsInitialized ? cache.Snapshot : CurrentUserSnapshotCache.UserSnapshot.Anonymous;
}
