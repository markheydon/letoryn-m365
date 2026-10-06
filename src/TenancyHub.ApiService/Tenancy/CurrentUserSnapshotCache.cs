namespace TenancyHub.ApiService.Tenancy;

/// <summary>Per-request cache for the resolved authenticated user snapshot.</summary>
public sealed class CurrentUserSnapshotCache
{
    private bool _initialized;
    private UserSnapshot _snapshot = UserSnapshot.Anonymous;

    /// <summary>Whether a snapshot has been loaded for this request.</summary>
    public bool IsInitialized => _initialized;

    /// <summary>Gets the cached snapshot.</summary>
    public UserSnapshot Snapshot => _snapshot;

    /// <summary>Stores the snapshot for the remainder of the request.</summary>
    public void Set(UserSnapshot snapshot)
    {
        _snapshot = snapshot;
        _initialized = true;
    }

    /// <summary>Resolved user identity for the current HTTP request.</summary>
    public readonly record struct UserSnapshot(
        Guid UserIdentityId,
        string EntraObjectId,
        string Email,
        bool IsPlatformOperator)
    {
        /// <summary>Anonymous / unauthenticated snapshot.</summary>
        public static UserSnapshot Anonymous => new(Guid.Empty, string.Empty, string.Empty, false);
    }
}
