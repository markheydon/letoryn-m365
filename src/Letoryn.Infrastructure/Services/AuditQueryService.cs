using Letoryn.Application.Abstractions.Audit;
using Letoryn.Domain.Agencies;
using Letoryn.Domain.Memberships;
using Letoryn.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Letoryn.Infrastructure.Services;

/// <inheritdoc />
public sealed class AuditQueryService(LetorynDbContext dbContext) : IAuditQueryService
{
    /// <inheritdoc />
    public async Task<AuditPageResult> GetAgencyAuditForAdministratorAsync(
        Guid agencyId,
        Guid actorUserIdentityId,
        string? cursor,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var agency = await dbContext.Agencies
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == agencyId, cancellationToken);

        if (agency is null || agency.LifecycleStatus != AgencyLifecycleStatus.Active)
        {
            return new AuditPageResult([], null);
        }

        var membership = await dbContext.AgencyMemberships
            .AsNoTracking()
            .FirstOrDefaultAsync(
                m => m.AgencyId == agencyId
                    && m.UserIdentityId == actorUserIdentityId
                    && m.Status == MembershipStatus.Active
                    && m.AgencyRole == AgencyRole.Administrator,
                cancellationToken);

        if (membership is null)
        {
            return new AuditPageResult([], null);
        }

        return await QueryPageAsync(agencyId, cursor, limit, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<AuditPageResult> GetAgencyAuditForOperatorAsync(
        Guid agencyId,
        Guid operatorUserIdentityId,
        string? cursor,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var assigned = await dbContext.PlatformOperatorAssignments
            .AsNoTracking()
            .AnyAsync(
                a => a.AgencyId == agencyId && a.UserIdentityId == operatorUserIdentityId,
                cancellationToken);

        if (!assigned)
        {
            return new AuditPageResult([], null);
        }

        return await QueryPageAsync(agencyId, cursor, limit, cancellationToken);
    }

    private async Task<AuditPageResult> QueryPageAsync(
        Guid agencyId,
        string? cursor,
        int limit,
        CancellationToken cancellationToken)
    {
        limit = Math.Clamp(limit, 1, 100);
        DateTimeOffset? cursorTime = null;
        if (!string.IsNullOrWhiteSpace(cursor)
            && DateTimeOffset.TryParse(cursor, out var parsed))
        {
            cursorTime = parsed;
        }

        var query = dbContext.AuditEvents
            .AsNoTracking()
            .Where(e => e.AgencyId == agencyId);

        if (cursorTime is DateTimeOffset before)
        {
            query = query.Where(e => e.OccurredAt < before);
        }

        var events = await query
            .OrderByDescending(e => e.OccurredAt)
            .Take(limit + 1)
            .ToListAsync(cancellationToken);

        string? nextCursor = null;
        if (events.Count > limit)
        {
            events = events.Take(limit).ToList();
            nextCursor = events[^1].OccurredAt.ToString("O");
        }

        var actorIds = events
            .Where(e => e.ActorUserIdentityId is not null)
            .Select(e => e.ActorUserIdentityId!.Value)
            .Distinct()
            .ToList();

        var actors = await dbContext.UserIdentities
            .AsNoTracking()
            .Where(u => actorIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Email, cancellationToken);

        var items = events.Select(e => new AuditListItem(
            e.OccurredAt,
            e.ActorUserIdentityId is Guid id && actors.TryGetValue(id, out var email) ? email : "System",
            e.ActionType,
            e.Summary)).ToList();

        return new AuditPageResult(items, nextCursor);
    }
}
