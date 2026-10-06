using Letoryn.Application.Abstractions.Agencies;
using Letoryn.Application.Abstractions.Audit;
using Letoryn.Application.Abstractions.Memberships;
using Letoryn.Application.Notifications;
using Letoryn.Domain.Agencies;
using Letoryn.Domain.Memberships;
using Letoryn.Domain.PlatformOperators;
using Letoryn.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Letoryn.Infrastructure.Services;

/// <inheritdoc />
public sealed class AgencyOperationsService(
    LetorynDbContext dbContext,
    IAuditWriter auditWriter,
    AgencyNotificationTriggerService agencyNotifications,
    TimeProvider timeProvider) : IAgencyOperations
{
    /// <inheritdoc />
    public async Task<AgencySettingsReadResult> GetSettingsAsync(
        Guid agencyId,
        bool actorIsAdministrator,
        bool actorIsOperatorOnAgency,
        CancellationToken cancellationToken = default)
    {
        var agency = await dbContext.Agencies.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == agencyId, cancellationToken);
        if (agency is null)
        {
            return new AgencySettingsReadResult(MembershipOperationStatus.NotFound);
        }

        var access = EvaluateSettingsAccess(agency, actorIsAdministrator, actorIsOperatorOnAgency);
        if (access is not null)
        {
            return access;
        }

        return new AgencySettingsReadResult(
            MembershipOperationStatus.Succeeded,
            Settings: new AgencySettingsSnapshot(
                agency.DisplayName,
                agency.PrimaryContactEmail,
                agency.PrimaryContactPhone,
                agency.LifecycleStatus));
    }

    /// <inheritdoc />
    public async Task<AgencyOperationResult> UpdateSettingsAsync(
        Guid actorUserIdentityId,
        Guid agencyId,
        string? displayName,
        string? primaryContactEmail,
        string? primaryContactPhone,
        bool actorIsAdministrator,
        bool actorIsOperatorOnAgency,
        CancellationToken cancellationToken = default)
    {
        var agency = await dbContext.Agencies.FirstOrDefaultAsync(a => a.Id == agencyId, cancellationToken);
        if (agency is null)
        {
            return new AgencyOperationResult(MembershipOperationStatus.NotFound);
        }

        var readDenied = EvaluateSettingsAccess(agency, actorIsAdministrator, actorIsOperatorOnAgency);
        if (readDenied is not null)
        {
            return new AgencyOperationResult(readDenied.Status, readDenied.UserMessage);
        }

        if (!string.IsNullOrWhiteSpace(displayName))
        {
            agency.DisplayName = displayName.Trim();
        }

        if (!string.IsNullOrWhiteSpace(primaryContactEmail))
        {
            agency.PrimaryContactEmail = primaryContactEmail.Trim();
        }

        if (!string.IsNullOrWhiteSpace(primaryContactPhone))
        {
            agency.PrimaryContactPhone = primaryContactPhone.Trim();
        }

        agency.UpdatedAt = timeProvider.GetUtcNow();
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditWriter.WriteAsync(
            new AuditEventWrite(
                agencyId,
                actorUserIdentityId,
                AuditActionTypes.AgencySettingsChanged,
                "Agency settings updated."),
            cancellationToken);

        await NotifyActiveMembersAsync(
            agencyId,
            "Agency settings were updated.",
            settingsChange: true,
            cancellationToken);

        return new AgencyOperationResult(MembershipOperationStatus.Succeeded, AgencyId: agencyId);
    }

    /// <inheritdoc />
    public async Task<AgencyOperationResult> CreateAgencyAsync(
        Guid operatorUserIdentityId,
        string displayName,
        string primaryContactEmail,
        string primaryContactPhone,
        CancellationToken cancellationToken = default)
    {
        var operatorUser = await dbContext.UserIdentities
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == operatorUserIdentityId, cancellationToken);

        if (operatorUser is null || !operatorUser.IsPlatformOperator)
        {
            return new AgencyOperationResult(MembershipOperationStatus.Forbidden);
        }

        var now = timeProvider.GetUtcNow();
        var agency = new Agency
        {
            Id = Guid.NewGuid(),
            DisplayName = displayName.Trim(),
            PrimaryContactEmail = primaryContactEmail.Trim(),
            PrimaryContactPhone = primaryContactPhone.Trim(),
            LifecycleStatus = AgencyLifecycleStatus.Active,
            CreatedAt = now,
            UpdatedAt = now,
            LastLifecycleChangeAt = now,
        };

        dbContext.Agencies.Add(agency);
        dbContext.PlatformOperatorAssignments.Add(new PlatformOperatorAssignment
        {
            Id = Guid.NewGuid(),
            AgencyId = agency.Id,
            UserIdentityId = operatorUserIdentityId,
            AssignedAt = now,
            AssignedByUserIdentityId = operatorUserIdentityId,
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        await auditWriter.WriteAsync(
            new AuditEventWrite(
                agency.Id,
                operatorUserIdentityId,
                AuditActionTypes.AgencyCreated,
                $"Agency created: {agency.DisplayName}."),
            cancellationToken);

        return new AgencyOperationResult(MembershipOperationStatus.Succeeded, AgencyId: agency.Id);
    }

    /// <inheritdoc />
    public async Task<AgencyOperationResult> ChangeLifecycleAsync(
        Guid operatorUserIdentityId,
        Guid agencyId,
        AgencyLifecycleStatus targetStatus,
        CancellationToken cancellationToken = default)
    {
        var operatorUser = await dbContext.UserIdentities
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == operatorUserIdentityId, cancellationToken);

        if (operatorUser is null || !operatorUser.IsPlatformOperator)
        {
            return new AgencyOperationResult(MembershipOperationStatus.Forbidden);
        }

        var assigned = await dbContext.PlatformOperatorAssignments
            .AnyAsync(a => a.AgencyId == agencyId && a.UserIdentityId == operatorUserIdentityId, cancellationToken);

        if (!assigned)
        {
            return new AgencyOperationResult(MembershipOperationStatus.NotFound);
        }

        var agency = await dbContext.Agencies.FirstOrDefaultAsync(a => a.Id == agencyId, cancellationToken);
        if (agency is null)
        {
            return new AgencyOperationResult(MembershipOperationStatus.NotFound);
        }

        if (!IsValidLifecycleTransition(agency.LifecycleStatus, targetStatus))
        {
            return new AgencyOperationResult(
                MembershipOperationStatus.ValidationFailed,
                "This lifecycle transition is not allowed.");
        }

        var previous = agency.LifecycleStatus;
        agency.LifecycleStatus = targetStatus;
        var now = timeProvider.GetUtcNow();
        agency.UpdatedAt = now;
        agency.LastLifecycleChangeAt = now;
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditWriter.WriteAsync(
            new AuditEventWrite(
                agencyId,
                operatorUserIdentityId,
                AuditActionTypes.AgencyLifecycleChanged,
                $"Agency lifecycle changed from {previous} to {targetStatus}."),
            cancellationToken);

        await NotifyActiveMembersAsync(
            agencyId,
            $"Agency status changed to {targetStatus}.",
            settingsChange: false,
            cancellationToken);

        return new AgencyOperationResult(MembershipOperationStatus.Succeeded, AgencyId: agencyId);
    }

    private async Task NotifyActiveMembersAsync(
        Guid agencyId,
        string summary,
        bool settingsChange,
        CancellationToken cancellationToken)
    {
        var members = await dbContext.AgencyMemberships
            .AsNoTracking()
            .Where(m => m.AgencyId == agencyId
                && (m.Status == MembershipStatus.Active || m.Status == MembershipStatus.Suspended))
            .Select(m => m.UserIdentityId)
            .ToListAsync(cancellationToken);

        foreach (var userId in members)
        {
            if (settingsChange)
            {
                await agencyNotifications.NotifySettingsChangedAsync(agencyId, userId, summary, cancellationToken);
            }
            else
            {
                await agencyNotifications.NotifyLifecycleChangeAsync(agencyId, userId, summary, cancellationToken);
            }
        }
    }

    private static bool IsValidLifecycleTransition(AgencyLifecycleStatus current, AgencyLifecycleStatus target)
    {
        if (current == target)
        {
            return true;
        }

        return current switch
        {
            AgencyLifecycleStatus.Active => target is AgencyLifecycleStatus.Suspended or AgencyLifecycleStatus.Archived,
            AgencyLifecycleStatus.Suspended => target is AgencyLifecycleStatus.Active or AgencyLifecycleStatus.Archived,
            AgencyLifecycleStatus.Archived => target is AgencyLifecycleStatus.Suspended or AgencyLifecycleStatus.Active,
            _ => false,
        };
    }

    private static AgencySettingsReadResult? EvaluateSettingsAccess(
        Agency agency,
        bool actorIsAdministrator,
        bool actorIsOperatorOnAgency)
    {
        if (agency.LifecycleStatus == AgencyLifecycleStatus.Archived)
        {
            return new AgencySettingsReadResult(
                MembershipOperationStatus.Forbidden,
                "Settings cannot be changed while the agency is archived.");
        }

        var allowed = actorIsAdministrator && agency.LifecycleStatus == AgencyLifecycleStatus.Active
            || actorIsOperatorOnAgency && agency.LifecycleStatus is AgencyLifecycleStatus.Active or AgencyLifecycleStatus.Suspended;

        return allowed
            ? null
            : new AgencySettingsReadResult(MembershipOperationStatus.Forbidden);
    }
}
