using Microsoft.EntityFrameworkCore;
using TenancyHub.Application.Abstractions.Authorization;
using TenancyHub.Application.Abstractions.Me;
using TenancyHub.Application.Agencies;
using TenancyHub.Application.Me;
using TenancyHub.Domain.Agencies;
using TenancyHub.Domain.Memberships;
using TenancyHub.Infrastructure.Persistence;

namespace TenancyHub.Infrastructure.Services;

/// <inheritdoc />
public sealed class MeProfileService(TenancyHubDbContext dbContext) : IMeProfileService
{
    /// <inheritdoc />
    public async Task<MeProfileResponse?> GetProfileAsync(
        Guid userIdentityId,
        CancellationToken cancellationToken = default)
    {
        var identity = await dbContext.UserIdentities
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userIdentityId, cancellationToken);

        if (identity is null)
        {
            return null;
        }

        var memberships = await dbContext.AgencyMemberships
            .AsNoTracking()
            .Where(m => m.UserIdentityId == userIdentityId)
            .Join(
                dbContext.Agencies.AsNoTracking(),
                m => m.AgencyId,
                a => a.Id,
                (m, a) => new { Membership = m, Agency = a })
            .ToListAsync(cancellationToken);

        var membershipSummaries = memberships
            .Where(x => x.Membership.Status is MembershipStatus.Active or MembershipStatus.Suspended)
            .Select(x => new MeMembershipSummary(
                x.Agency.Id,
                x.Agency.DisplayName,
                x.Membership.Status.ToString(),
                x.Membership.AgencyRole.ToString(),
                x.Agency.LifecycleStatus.ToString()))
            .ToList();

        var pendingInvites = memberships
            .Where(x => x.Membership.Status == MembershipStatus.Invited)
            .Select(x => new MePendingInviteSummary(
                x.Membership.Id,
                x.Agency.Id,
                x.Agency.DisplayName,
                (x.Membership.InvitedRoleSnapshot ?? x.Membership.AgencyRole).ToString()))
            .ToList();

        var operatorAssignments = await dbContext.PlatformOperatorAssignments
            .AsNoTracking()
            .Where(a => a.UserIdentityId == userIdentityId)
            .Join(
                dbContext.Agencies.AsNoTracking(),
                a => a.AgencyId,
                ag => ag.Id,
                (a, ag) => new MeOperatorAssignmentSummary(
                    ag.Id,
                    ag.DisplayName,
                    ag.LifecycleStatus.ToString()))
            .ToListAsync(cancellationToken);

        return new MeProfileResponse(
            identity.Id,
            identity.Email,
            identity.IsPlatformOperator,
            identity.LastUsedAgencyId,
            membershipSummaries,
            operatorAssignments,
            pendingInvites);
    }

    /// <inheritdoc />
    public async Task<SetActiveAgencyOutcome> SetActiveAgencyAsync(
        Guid userIdentityId,
        Guid agencyId,
        CancellationToken cancellationToken = default)
    {
        var agency = await dbContext.Agencies
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == agencyId, cancellationToken);

        if (agency is null)
        {
            return new SetActiveAgencyOutcome(SetActiveAgencyResult.NotFound);
        }

        var membership = await dbContext.AgencyMemberships
            .AsNoTracking()
            .FirstOrDefaultAsync(
                m => m.AgencyId == agencyId && m.UserIdentityId == userIdentityId,
                cancellationToken);

        var operatorAssigned = await dbContext.PlatformOperatorAssignments
            .AsNoTracking()
            .AnyAsync(
                a => a.AgencyId == agencyId && a.UserIdentityId == userIdentityId,
                cancellationToken);

        if (membership is null && !operatorAssigned)
        {
            return new SetActiveAgencyOutcome(SetActiveAgencyResult.NotFound);
        }

        var access = AgencyAccessRules.EvaluateMemberActiveAgencySelection(
            agency.LifecycleStatus,
            membership?.Status,
            operatorAssigned);

        if (!access.IsAuthorized)
        {
            return access.FailureKind == AuthorizationFailureKind.NotFound
                ? new SetActiveAgencyOutcome(SetActiveAgencyResult.NotFound)
                : new SetActiveAgencyOutcome(SetActiveAgencyResult.Forbidden, access.UserMessage);
        }

        if (membership?.Status == MembershipStatus.Active && agency.LifecycleStatus == AgencyLifecycleStatus.Active)
        {
            await UpdateLastUsedAsync(userIdentityId, agencyId, cancellationToken);
            return new SetActiveAgencyOutcome(SetActiveAgencyResult.Succeeded);
        }

        if (operatorAssigned && agency.LifecycleStatus != AgencyLifecycleStatus.Archived)
        {
            await UpdateLastUsedAsync(userIdentityId, agencyId, cancellationToken);
            return new SetActiveAgencyOutcome(SetActiveAgencyResult.Succeeded);
        }

        return new SetActiveAgencyOutcome(SetActiveAgencyResult.Forbidden, access.UserMessage);
    }

    private async Task UpdateLastUsedAsync(
        Guid userIdentityId,
        Guid agencyId,
        CancellationToken cancellationToken)
    {
        var identity = await dbContext.UserIdentities
            .FirstOrDefaultAsync(u => u.Id == userIdentityId, cancellationToken);

        if (identity is null)
        {
            return;
        }

        identity.LastUsedAgencyId = agencyId;
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
