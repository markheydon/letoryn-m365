using Microsoft.EntityFrameworkCore;
using TenancyHub.Application.Identities;
using TenancyHub.Domain.Identities;
using TenancyHub.Infrastructure.Persistence;
using TenancyHub.Infrastructure.Services;

namespace TenancyHub.ApiService.UnitTests.Services;

public sealed class EnsureUserIdentityServiceTests
{
    [Fact]
    public async Task EnsureAsync_UnlinkedEmailMatch_RebindsEntraObjectId()
    {
        await using var db = CreateDbContext();
        var userId = Guid.NewGuid();
        const string email = "invited@example.com";
        db.UserIdentities.Add(new UserIdentity
        {
            Id = userId,
            Email = email,
            EntraObjectId = $"{UserIdentityLinkConstants.UnlinkedEntraObjectIdPrefix}abc",
            CreatedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var service = new EnsureUserIdentityService(db);
        var result = await service.EnsureAsync(
            "entra-oid-1",
            email,
            TestContext.Current.CancellationToken);

        Assert.Equal(userId, result.UserIdentityId);
        Assert.Equal("entra-oid-1", result.EntraObjectId);
        var row = await db.UserIdentities.SingleAsync(u => u.Id == userId, TestContext.Current.CancellationToken);
        Assert.Equal("entra-oid-1", row.EntraObjectId);
    }

    [Fact]
    public async Task EnsureAsync_LinkedEmailMismatch_ThrowsBindingConflict()
    {
        await using var db = CreateDbContext();
        const string email = "member@example.com";
        db.UserIdentities.Add(new UserIdentity
        {
            Id = Guid.NewGuid(),
            Email = email,
            EntraObjectId = "entra-original",
            CreatedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var service = new EnsureUserIdentityService(db);
        await Assert.ThrowsAsync<UserIdentityBindingConflictException>(() =>
            service.EnsureAsync("entra-other", email, TestContext.Current.CancellationToken));
    }

    private static TenancyHubDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TenancyHubDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new TenancyHubDbContext(options);
    }
}
