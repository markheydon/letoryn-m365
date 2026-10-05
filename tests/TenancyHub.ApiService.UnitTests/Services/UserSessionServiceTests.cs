using Microsoft.EntityFrameworkCore;
using TenancyHub.Application.Abstractions.Sessions;
using TenancyHub.Domain.Sessions;
using TenancyHub.Infrastructure.Persistence;
using TenancyHub.Infrastructure.Services;

namespace TenancyHub.ApiService.UnitTests.Services;

public sealed class UserSessionServiceTests
{
    [Fact]
    public async Task ValidateAndTouchAsync_EndedSession_ReturnsNotFound()
    {
        await using var db = CreateDbContext();
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        db.UserSessions.Add(new UserSession
        {
            Id = sessionId,
            UserIdentityId = userId,
            StartedAt = DateTimeOffset.UtcNow,
            LastActivityAt = DateTimeOffset.UtcNow,
            EndedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var service = new UserSessionService(db);
        var result = await service.ValidateAndTouchAsync(
            sessionId,
            userId,
            TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);
        Assert.Equal(UserSessionTerminationReason.NotFound, result.Reason);
    }

    [Fact]
    public async Task ValidateAndTouchAsync_IdleTimeout_EndsSession()
    {
        await using var db = CreateDbContext();
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var staleActivity = DateTimeOffset.UtcNow.AddMinutes(-36);
        db.UserSessions.Add(new UserSession
        {
            Id = sessionId,
            UserIdentityId = userId,
            StartedAt = staleActivity,
            LastActivityAt = staleActivity,
        });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var service = new UserSessionService(db);
        var result = await service.ValidateAndTouchAsync(
            sessionId,
            userId,
            TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);
        Assert.Equal(UserSessionTerminationReason.IdleTimeout, result.Reason);
        var row = await db.UserSessions.SingleAsync(s => s.Id == sessionId, TestContext.Current.CancellationToken);
        Assert.NotNull(row.EndedAt);
    }

    [Fact]
    public async Task EndSessionAsync_ActiveSession_SetsEndedAt()
    {
        await using var db = CreateDbContext();
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        db.UserSessions.Add(new UserSession
        {
            Id = sessionId,
            UserIdentityId = userId,
            StartedAt = DateTimeOffset.UtcNow,
            LastActivityAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var service = new UserSessionService(db);
        var ended = await service.EndSessionAsync(sessionId, userId, TestContext.Current.CancellationToken);

        Assert.True(ended);
        var row = await db.UserSessions.SingleAsync(s => s.Id == sessionId, TestContext.Current.CancellationToken);
        Assert.NotNull(row.EndedAt);
    }

    private static TenancyHubDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TenancyHubDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new TenancyHubDbContext(options);
    }
}
