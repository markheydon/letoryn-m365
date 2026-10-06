using Letoryn.Application.Abstractions.Audit;
using Letoryn.Application.Abstractions.Memberships;
using Letoryn.Application.Abstractions.Operators;
using Letoryn.Domain.Guards;
using Letoryn.Domain.PlatformOperators;
using Letoryn.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Letoryn.Infrastructure.Services;

/// <inheritdoc />
public sealed class PlatformOperatorManagementService(
    LetorynDbContext dbContext,
    IAuditWriter auditWriter,
    TimeProvider timeProvider) : IPlatformOperatorManagement
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<PlatformOperatorDto>> ListOperatorsAsync(CancellationToken cancellationToken = default)
    {
        var operators = await dbContext.UserIdentities
            .AsNoTracking()
            .Where(u => u.IsPlatformOperator)
            .ToListAsync(cancellationToken);

        var assignments = await dbContext.PlatformOperatorAssignments
            .AsNoTracking()
            .GroupBy(a => a.UserIdentityId)
            .Select(g => new { UserId = g.Key, AgencyIds = g.Select(a => a.AgencyId).ToList() })
            .ToListAsync(cancellationToken);

        var assignmentLookup = assignments.ToDictionary(a => a.UserId, a => (IReadOnlyList<Guid>)a.AgencyIds);

        return operators
            .Select(o => new PlatformOperatorDto(
                o.Id,
                o.Email,
                assignmentLookup.TryGetValue(o.Id, out var ids) ? ids : []))
            .ToList();
    }

    /// <inheritdoc />
    public async Task<MembershipOperationResult> GrantOperatorAsync(
        Guid actorUserIdentityId,
        Guid targetUserIdentityId,
        CancellationToken cancellationToken = default)
    {
        if (!await ActorIsPlatformOperatorAsync(actorUserIdentityId, cancellationToken))
        {
            return new MembershipOperationResult(MembershipOperationStatus.Forbidden);
        }

        var target = await dbContext.UserIdentities
            .FirstOrDefaultAsync(u => u.Id == targetUserIdentityId, cancellationToken);

        if (target is null)
        {
            return new MembershipOperationResult(MembershipOperationStatus.NotFound);
        }

        if (target.IsPlatformOperator)
        {
            return new MembershipOperationResult(MembershipOperationStatus.Succeeded);
        }

        target.IsPlatformOperator = true;
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditWriter.WriteAsync(
            new AuditEventWrite(
                null,
                actorUserIdentityId,
                AuditActionTypes.PlatformOperatorGranted,
                "Platform operator status granted.",
                targetUserIdentityId),
            cancellationToken);

        return new MembershipOperationResult(MembershipOperationStatus.Succeeded);
    }

    /// <inheritdoc />
    public async Task<MembershipOperationResult> RevokeOperatorAsync(
        Guid actorUserIdentityId,
        Guid targetUserIdentityId,
        CancellationToken cancellationToken = default)
    {
        if (!await ActorIsPlatformOperatorAsync(actorUserIdentityId, cancellationToken))
        {
            return new MembershipOperationResult(MembershipOperationStatus.Forbidden);
        }

        var target = await dbContext.UserIdentities
            .FirstOrDefaultAsync(u => u.Id == targetUserIdentityId, cancellationToken);

        if (target is null || !target.IsPlatformOperator)
        {
            return new MembershipOperationResult(MembershipOperationStatus.NotFound);
        }

        var remaining = await dbContext.UserIdentities
            .CountAsync(u => u.IsPlatformOperator && u.Id != targetUserIdentityId, cancellationToken);

        try
        {
            PlatformOperatorGuards.EnsureAtLeastOnePlatformOperator(remaining);
        }
        catch (InvalidOperationException ex)
        {
            await auditWriter.WriteAsync(
                new AuditEventWrite(
                    null,
                    actorUserIdentityId,
                    AuditActionTypes.PlatformOperatorRevoked,
                    "Blocked revoke of last platform operator.",
                    targetUserIdentityId),
                cancellationToken);
            return new MembershipOperationResult(MembershipOperationStatus.ValidationFailed, ex.Message);
        }

        target.IsPlatformOperator = false;
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditWriter.WriteAsync(
            new AuditEventWrite(
                null,
                actorUserIdentityId,
                AuditActionTypes.PlatformOperatorRevoked,
                "Platform operator status revoked.",
                targetUserIdentityId),
            cancellationToken);

        return new MembershipOperationResult(MembershipOperationStatus.Succeeded);
    }

    /// <inheritdoc />
    public async Task<MembershipOperationResult> SetAssignmentsAsync(
        Guid actorUserIdentityId,
        Guid targetUserIdentityId,
        IReadOnlyList<Guid> agencyIds,
        CancellationToken cancellationToken = default)
    {
        if (!await ActorIsPlatformOperatorAsync(actorUserIdentityId, cancellationToken))
        {
            return new MembershipOperationResult(MembershipOperationStatus.Forbidden);
        }

        var target = await dbContext.UserIdentities
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == targetUserIdentityId && u.IsPlatformOperator, cancellationToken);

        if (target is null)
        {
            return new MembershipOperationResult(MembershipOperationStatus.NotFound);
        }

        var distinctAgencyIds = agencyIds.Distinct().ToList();
        var existingAgencies = await dbContext.Agencies
            .AsNoTracking()
            .Where(a => distinctAgencyIds.Contains(a.Id))
            .Select(a => a.Id)
            .ToListAsync(cancellationToken);

        if (existingAgencies.Count != distinctAgencyIds.Count)
        {
            return new MembershipOperationResult(MembershipOperationStatus.ValidationFailed, "One or more agencies were not found.");
        }

        var current = await dbContext.PlatformOperatorAssignments
            .Where(a => a.UserIdentityId == targetUserIdentityId)
            .ToListAsync(cancellationToken);

        dbContext.PlatformOperatorAssignments.RemoveRange(current);

        var now = timeProvider.GetUtcNow();
        foreach (var agencyId in distinctAgencyIds)
        {
            dbContext.PlatformOperatorAssignments.Add(new PlatformOperatorAssignment
            {
                Id = Guid.NewGuid(),
                AgencyId = agencyId,
                UserIdentityId = targetUserIdentityId,
                AssignedAt = now,
                AssignedByUserIdentityId = actorUserIdentityId,
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        await auditWriter.WriteAsync(
            new AuditEventWrite(
                null,
                actorUserIdentityId,
                AuditActionTypes.PlatformOperatorAssignmentsChanged,
                $"Platform operator assignments updated ({distinctAgencyIds.Count} agencies).",
                targetUserIdentityId),
            cancellationToken);

        return new MembershipOperationResult(MembershipOperationStatus.Succeeded);
    }

    private async Task<bool> ActorIsPlatformOperatorAsync(Guid actorUserIdentityId, CancellationToken cancellationToken)
    {
        return await dbContext.UserIdentities
            .AsNoTracking()
            .AnyAsync(u => u.Id == actorUserIdentityId && u.IsPlatformOperator, cancellationToken);
    }
}
