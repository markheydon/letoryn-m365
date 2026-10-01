using TenancyHub.ApiService.Tenancy;
using TenancyHub.Application.Abstractions.Tenancy;

namespace TenancyHub.ApiService.UnitTests.Tenancy;

public sealed class HttpCurrentUserTests
{
    [Fact]
    public void Properties_BeforeCacheInitialized_ReturnAnonymousSnapshot()
    {
        var cache = new CurrentUserSnapshotCache();
        ICurrentUser user = new HttpCurrentUser(cache);

        Assert.Equal(Guid.Empty, user.UserIdentityId);
        Assert.False(user.IsPlatformOperator);
    }

    [Fact]
    public void Properties_AfterCacheInitialized_ReturnCachedValues()
    {
        var cache = new CurrentUserSnapshotCache();
        var identityId = Guid.NewGuid();
        cache.Set(new CurrentUserSnapshotCache.UserSnapshot(identityId, "oid-123", "user@contoso.com", true));
        ICurrentUser user = new HttpCurrentUser(cache);

        Assert.Equal(identityId, user.UserIdentityId);
        Assert.Equal("oid-123", user.EntraObjectId);
        Assert.Equal("user@contoso.com", user.Email);
        Assert.True(user.IsPlatformOperator);
    }
}
