using Microsoft.EntityFrameworkCore;
using TenancyHub.Application.Abstractions.Sessions;
using TenancyHub.Domain.Sessions;
using TenancyHub.Infrastructure.Persistence;

namespace TenancyHub.Infrastructure.Services;

/// <inheritdoc />
public sealed class UserSessionService(TenancyHubDbContext dbContext) : IUserSessionService
{
    private static readonly TimeSpan IdleTimeout = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan AbsoluteTimeout = TimeSpan.FromHours(12);
    private static readonly TimeSpan ClockSkew = TimeSpan.FromMinutes(5);

    /// <inheritdoc />
    public async Task<UserSessionInfo> CreateSessionAsync(
        Guid userIdentityId,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var session = new UserSession
        {
            Id = Guid.NewGuid(),
            UserIdentityId = userIdentityId,
            StartedAt = now,
            LastActivityAt = now,
        };

        dbContext.UserSessions.Add(session);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new UserSessionInfo(session.Id, session.StartedAt, session.LastActivityAt);
    }

    /// <inheritdoc />
    public async Task<UserSessionValidationResult> ValidateAndTouchAsync(
        Guid sessionId,
        Guid userIdentityId,
        CancellationToken cancellationToken = default)
    {
        var session = await dbContext.UserSessions
            .FirstOrDefaultAsync(
                s => s.Id == sessionId && s.UserIdentityId == userIdentityId,
                cancellationToken);

        if (session is null || session.EndedAt is not null)
        {
            return new UserSessionValidationResult(false, UserSessionTerminationReason.NotFound);
        }

        var now = DateTimeOffset.UtcNow;

        if (now > session.StartedAt.Add(AbsoluteTimeout).Add(ClockSkew))
        {
            session.EndedAt = now;
            await dbContext.SaveChangesAsync(cancellationToken);
            return new UserSessionValidationResult(false, UserSessionTerminationReason.AbsoluteTimeout);
        }

        if (now > session.LastActivityAt.Add(IdleTimeout).Add(ClockSkew))
        {
            session.EndedAt = now;
            await dbContext.SaveChangesAsync(cancellationToken);
            return new UserSessionValidationResult(false, UserSessionTerminationReason.IdleTimeout);
        }

        session.LastActivityAt = now;
        await dbContext.SaveChangesAsync(cancellationToken);

        return new UserSessionValidationResult(true, null);
    }

    /// <inheritdoc />
    public async Task<bool> EndSessionAsync(
        Guid sessionId,
        Guid userIdentityId,
        CancellationToken cancellationToken = default)
    {
        var session = await dbContext.UserSessions
            .FirstOrDefaultAsync(
                s => s.Id == sessionId && s.UserIdentityId == userIdentityId,
                cancellationToken);

        if (session is null || session.EndedAt is not null)
        {
            return false;
        }

        session.EndedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
