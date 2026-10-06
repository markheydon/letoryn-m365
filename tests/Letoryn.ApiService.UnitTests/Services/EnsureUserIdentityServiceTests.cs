using Letoryn.Application.Identities;
using Letoryn.Domain.Identities;
using Letoryn.Infrastructure.Persistence;
using Letoryn.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace Letoryn.ApiService.UnitTests.Services;

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
    public async Task EnsureAsync_EntraEmailChangeToTakenAddress_ThrowsBindingConflict()
    {
        await using var db = CreateDbContext();
        var userId = Guid.NewGuid();
        db.UserIdentities.Add(new UserIdentity
        {
            Id = userId,
            Email = "current@example.com",
            EntraObjectId = "entra-1",
            CreatedAt = DateTimeOffset.UtcNow,
        });
        db.UserIdentities.Add(new UserIdentity
        {
            Id = Guid.NewGuid(),
            Email = "other@example.com",
            EntraObjectId = "entra-2",
            CreatedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var service = new EnsureUserIdentityService(db);
        await Assert.ThrowsAsync<UserIdentityBindingConflictException>(() =>
            service.EnsureAsync("entra-1", "other@example.com", TestContext.Current.CancellationToken));
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

    private static LetorynDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<LetorynDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new LetorynDbContext(options);
    }
}
