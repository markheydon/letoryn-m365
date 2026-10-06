using Microsoft.EntityFrameworkCore;
using TenancyHub.Application.Abstractions.Audit;
using TenancyHub.Application.Abstractions.Operators;
using TenancyHub.Domain.Memberships;
using TenancyHub.Infrastructure.Persistence;

namespace TenancyHub.Infrastructure.Services;

/// <inheritdoc />
public sealed class OperatorDiagnosticsService(
    TenancyHubDbContext dbContext,
    IAuditWriter auditWriter) : IOperatorDiagnosticsService
{
    /// <inheritdoc />
    public async Task<OperatorAgencySummary?> GetSummaryAsync(
        Guid operatorUserIdentityId,
        Guid agencyId,
        CancellationToken cancellationToken = default)
    {
        var assigned = await dbContext.PlatformOperatorAssignments
            .AsNoTracking()
            .AnyAsync(
                a => a.AgencyId == agencyId && a.UserIdentityId == operatorUserIdentityId,
                cancellationToken);

        if (!assigned)
        {
            return null;
        }

        var agency = await dbContext.Agencies
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == agencyId, cancellationToken);

        if (agency is null)
        {
            return null;
        }

        var memberships = await dbContext.AgencyMemberships
            .AsNoTracking()
            .Where(m => m.AgencyId == agencyId)
            .ToListAsync(cancellationToken);

        var counts = new OperatorRoleCounts(
            memberships.Count(m => m.Status == MembershipStatus.Invited),
            memberships.Count(m => m.Status == MembershipStatus.Active),
            memberships.Count(m => m.Status == MembershipStatus.Suspended),
            memberships.Count(m => m.Status == MembershipStatus.Removed));

        await auditWriter.WriteAsync(
            new AuditEventWrite(
                agencyId,
                operatorUserIdentityId,
                AuditActionTypes.OperatorCrossAgencyView,
                "Operator viewed agency diagnostics summary."),
            cancellationToken);

        return new OperatorAgencySummary(
            agency.Id,
            agency.DisplayName,
            agency.LifecycleStatus.ToString(),
            agency.PrimaryContactEmail,
            agency.PrimaryContactPhone,
            agency.LastLifecycleChangeAt,
            counts);
    }
}
