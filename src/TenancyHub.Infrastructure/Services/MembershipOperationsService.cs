using Microsoft.EntityFrameworkCore;
using TenancyHub.Application.Abstractions.Audit;
using TenancyHub.Application.Abstractions.Memberships;
using TenancyHub.Application.Memberships;
using TenancyHub.Application.Notifications;
using TenancyHub.Domain.Agencies;
using TenancyHub.Domain.Guards;
using TenancyHub.Domain.Identities;
using TenancyHub.Domain.Memberships;
using TenancyHub.Infrastructure.Persistence;

namespace TenancyHub.Infrastructure.Services;

/// <inheritdoc />
public sealed class MembershipOperationsService(
    TenancyHubDbContext dbContext,
    IAuditWriter auditWriter,
    NotificationTriggerService notificationTriggers,
    TimeProvider timeProvider) : IMembershipOperations
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<PendingInvitationDto>> ListPendingInvitationsAsync(
        Guid userIdentityId,
        string userEmail,
        CancellationToken cancellationToken = default)
    {
        var normalized = MembershipEmailNormalizer.Normalize(userEmail);
        var now = timeProvider.GetUtcNow();

        var rows = await dbContext.AgencyMemberships
            .AsNoTracking()
            .Where(m => m.UserIdentityId == userIdentityId && m.Status == MembershipStatus.Invited)
            .Join(
                dbContext.Agencies.AsNoTracking(),
                m => m.AgencyId,
                a => a.Id,
                (m, a) => new { Membership = m, Agency = a })
            .Join(
                dbContext.UserIdentities.AsNoTracking(),
                x => x.Membership.UserIdentityId,
                u => u.Id,
                (x, u) => new { x.Membership, x.Agency, User = u })
            .Where(x => x.User.Email == normalized)
            .ToListAsync(cancellationToken);

        var result = new List<PendingInvitationDto>();
        foreach (var row in rows)
        {
            if (InvitationRules.IsExpired(row.Membership, now))
            {
                continue;
            }

            var expiresAt = row.Membership.ExpiresAt
                ?? (row.Membership.InvitedAt is DateTimeOffset invited
                    ? InvitationRules.ComputeExpiresAt(invited)
                    : now.Add(InvitationRules.InviteLifetime));

            result.Add(new PendingInvitationDto(
                row.Membership.Id,
                row.Agency.Id,
                row.Agency.DisplayName,
                (row.Membership.InvitedRoleSnapshot ?? row.Membership.AgencyRole).ToString(),
                expiresAt));
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<MembershipOperationResult> AcceptInvitationAsync(
        Guid userIdentityId,
        string userEmail,
        Guid membershipId,
        CancellationToken cancellationToken = default)
    {
        var membership = await dbContext.AgencyMemberships
            .FirstOrDefaultAsync(m => m.Id == membershipId, cancellationToken);

        if (membership is null || membership.Status != MembershipStatus.Invited)
        {
            return new MembershipOperationResult(MembershipOperationStatus.NotFound);
        }

        var identity = await dbContext.UserIdentities
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userIdentityId, cancellationToken);

        if (identity is null
            || !string.Equals(
                identity.Email,
                MembershipEmailNormalizer.Normalize(userEmail),
                StringComparison.Ordinal))
        {
            await auditWriter.WriteAsync(
                new AuditEventWrite(
                    membership.AgencyId,
                    userIdentityId,
                    AuditActionTypes.MembershipInviteActionFailed,
                    "Invitation accept denied: identity does not match invited email.",
                    membership.UserIdentityId),
                cancellationToken);
            return new MembershipOperationResult(
                MembershipOperationStatus.Forbidden,
                "You must sign in with the invited work account to accept this invitation.");
        }

        if (membership.UserIdentityId != userIdentityId)
        {
            await auditWriter.WriteAsync(
                new AuditEventWrite(
                    membership.AgencyId,
                    userIdentityId,
                    AuditActionTypes.MembershipInviteActionFailed,
                    "Invitation accept denied: identity does not match invited email.",
                    membership.UserIdentityId),
                cancellationToken);
            return new MembershipOperationResult(
                MembershipOperationStatus.Forbidden,
                "You must sign in with the invited work account to accept this invitation.");
        }

        var now = timeProvider.GetUtcNow();
        if (InvitationRules.IsExpired(membership, now))
        {
            return new MembershipOperationResult(
                MembershipOperationStatus.ValidationFailed,
                "This invitation has expired. Ask for a new invitation.");
        }

        var agency = await dbContext.Agencies
            .AsNoTracking()
            .FirstAsync(a => a.Id == membership.AgencyId, cancellationToken);

        if (agency.LifecycleStatus == AgencyLifecycleStatus.Archived)
        {
            return new MembershipOperationResult(MembershipOperationStatus.Forbidden);
        }

        membership.Status = MembershipStatus.Active;
        membership.ActivatedAt = now;
        membership.UpdatedAt = now;
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditWriter.WriteAsync(
            new AuditEventWrite(
                membership.AgencyId,
                userIdentityId,
                AuditActionTypes.MembershipInviteAccepted,
                $"Invitation accepted with role {membership.AgencyRole}.",
                userIdentityId),
            cancellationToken);

        await notificationTriggers.NotifyMembershipActivatedAsync(
            membership.AgencyId,
            userIdentityId,
            "Your agency membership is now active.",
            cancellationToken);

        return new MembershipOperationResult(MembershipOperationStatus.Succeeded, MembershipId: membership.Id);
    }

    /// <inheritdoc />
    public async Task<MembershipOperationResult> DeclineInvitationAsync(
        Guid userIdentityId,
        string userEmail,
        Guid membershipId,
        CancellationToken cancellationToken = default)
    {
        var membership = await dbContext.AgencyMemberships
            .FirstOrDefaultAsync(m => m.Id == membershipId, cancellationToken);

        if (membership is null || membership.Status != MembershipStatus.Invited)
        {
            return new MembershipOperationResult(MembershipOperationStatus.NotFound);
        }

        var identity = await dbContext.UserIdentities
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userIdentityId, cancellationToken);

        if (identity is null
            || membership.UserIdentityId != userIdentityId
            || !string.Equals(
                identity.Email,
                MembershipEmailNormalizer.Normalize(userEmail),
                StringComparison.Ordinal))
        {
            await auditWriter.WriteAsync(
                new AuditEventWrite(
                    membership.AgencyId,
                    userIdentityId,
                    AuditActionTypes.MembershipInviteActionFailed,
                    "Invitation decline denied: identity does not match invited email.",
                    membership.UserIdentityId),
                cancellationToken);
            return new MembershipOperationResult(
                MembershipOperationStatus.Forbidden,
                "You must sign in with the invited work account to decline this invitation.");
        }

        var now = timeProvider.GetUtcNow();
        if (InvitationRules.IsExpired(membership, now))
        {
            return new MembershipOperationResult(
                MembershipOperationStatus.ValidationFailed,
                "This invitation has expired.");
        }

        membership.Status = MembershipStatus.Removed;
        membership.UpdatedAt = now;
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditWriter.WriteAsync(
            new AuditEventWrite(
                membership.AgencyId,
                userIdentityId,
                AuditActionTypes.MembershipInviteDeclined,
                "Invitation declined.",
                userIdentityId),
            cancellationToken);

        return new MembershipOperationResult(MembershipOperationStatus.Succeeded, MembershipId: membership.Id);
    }

    /// <inheritdoc />
    public Task<MembershipOperationResult> InviteMemberAsync(
        Guid actorUserIdentityId,
        Guid agencyId,
        string email,
        AgencyRole role,
        bool actorIsAdministrator,
        bool actorIsOperatorOnAgency,
        CancellationToken cancellationToken = default) =>
        CreateMembershipAsync(
            actorUserIdentityId,
            agencyId,
            email,
            role,
            invited: true,
            actorIsAdministrator,
            actorIsOperatorOnAgency,
            cancellationToken);

    /// <inheritdoc />
    public Task<MembershipOperationResult> ProvisionMemberAsync(
        Guid actorUserIdentityId,
        Guid agencyId,
        string email,
        AgencyRole role,
        bool actorIsAdministrator,
        bool actorIsOperatorOnAgency,
        CancellationToken cancellationToken = default) =>
        CreateMembershipAsync(
            actorUserIdentityId,
            agencyId,
            email,
            role,
            invited: false,
            actorIsAdministrator,
            actorIsOperatorOnAgency,
            cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<MembershipRosterDto>?> ListRosterAsync(
        Guid agencyId,
        bool canViewRoster,
        CancellationToken cancellationToken = default)
    {
        if (!canViewRoster)
        {
            return null;
        }

        var rows = await dbContext.AgencyMemberships
            .AsNoTracking()
            .Where(m => m.AgencyId == agencyId && m.Status != MembershipStatus.Removed)
            .Join(
                dbContext.UserIdentities.AsNoTracking(),
                m => m.UserIdentityId,
                u => u.Id,
                (m, u) => new { m.Id, u.Email, m.Status, m.AgencyRole })
            .ToListAsync(cancellationToken);

        return rows
            .Select(r => new MembershipRosterDto(
                r.Id,
                r.Email,
                r.Status.ToString(),
                r.AgencyRole.ToString()))
            .ToList();
    }

    /// <inheritdoc />
    public async Task<MembershipOperationResult> ChangeRoleAsync(
        Guid actorUserIdentityId,
        Guid agencyId,
        Guid membershipId,
        AgencyRole newRole,
        bool actorIsAdministrator,
        bool actorIsOperatorOnAgency,
        CancellationToken cancellationToken = default)
    {
        if (!actorIsAdministrator && !actorIsOperatorOnAgency)
        {
            return new MembershipOperationResult(MembershipOperationStatus.Forbidden);
        }

        if (!await CanMutateMembershipAsync(agencyId, cancellationToken))
        {
            return new MembershipOperationResult(MembershipOperationStatus.Forbidden);
        }

        var membership = await dbContext.AgencyMemberships
            .FirstOrDefaultAsync(m => m.Id == membershipId && m.AgencyId == agencyId, cancellationToken);

        if (membership is null
            || membership.Status is MembershipStatus.Removed)
        {
            return new MembershipOperationResult(MembershipOperationStatus.NotFound);
        }

        if (membership.Status == MembershipStatus.Invited)
        {
            membership.AgencyRole = newRole;
            membership.InvitedRoleSnapshot = newRole;
            membership.UpdatedAt = timeProvider.GetUtcNow();
            await dbContext.SaveChangesAsync(cancellationToken);
            await auditWriter.WriteAsync(
                new AuditEventWrite(
                    agencyId,
                    actorUserIdentityId,
                    AuditActionTypes.MembershipRoleChanged,
                    $"Invited membership role changed to {newRole}.",
                    membership.UserIdentityId),
                cancellationToken);
            return new MembershipOperationResult(MembershipOperationStatus.Succeeded, MembershipId: membership.Id);
        }

        if (membership.Status != MembershipStatus.Active)
        {
            return new MembershipOperationResult(MembershipOperationStatus.ValidationFailed);
        }

        if (membership.AgencyRole == AgencyRole.Administrator && newRole != AgencyRole.Administrator)
        {
            var remainingAdmins = await CountActiveAdministratorsAsync(agencyId, excludeMembershipId: membership.Id, cancellationToken);
            try
            {
                MembershipGuards.EnsureAtLeastOneActiveAdministrator(remainingAdmins);
            }
            catch (InvalidOperationException ex)
            {
                return new MembershipOperationResult(MembershipOperationStatus.ValidationFailed, ex.Message);
            }
        }

        membership.AgencyRole = newRole;
        membership.UpdatedAt = timeProvider.GetUtcNow();
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditWriter.WriteAsync(
            new AuditEventWrite(
                agencyId,
                actorUserIdentityId,
                AuditActionTypes.MembershipRoleChanged,
                $"Membership role changed to {newRole}.",
                membership.UserIdentityId),
            cancellationToken);

        await notificationTriggers.NotifyRoleChangedAsync(
            agencyId,
            membership.UserIdentityId,
            $"Your role is now {newRole}.",
            cancellationToken);

        return new MembershipOperationResult(MembershipOperationStatus.Succeeded, MembershipId: membership.Id);
    }

    /// <inheritdoc />
    public async Task<MembershipOperationResult> SuspendMemberAsync(
        Guid actorUserIdentityId,
        Guid agencyId,
        Guid membershipId,
        bool actorIsAdministrator,
        bool actorIsOperatorOnAgency,
        CancellationToken cancellationToken = default)
    {
        var membership = await dbContext.AgencyMemberships
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == membershipId && m.AgencyId == agencyId, cancellationToken);

        if (membership?.Status != MembershipStatus.Active)
        {
            return new MembershipOperationResult(MembershipOperationStatus.NotFound);
        }

        return await ChangeActiveMembershipStatusAsync(
            actorUserIdentityId,
            agencyId,
            membershipId,
            MembershipStatus.Suspended,
            AuditActionTypes.MembershipSuspended,
            "Membership suspended.",
            actorIsAdministrator,
            actorIsOperatorOnAgency,
            checkLastAdmin: true,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<MembershipOperationResult> ReactivateMemberAsync(
        Guid actorUserIdentityId,
        Guid agencyId,
        Guid membershipId,
        bool actorIsAdministrator,
        bool actorIsOperatorOnAgency,
        CancellationToken cancellationToken = default)
    {
        var membership = await dbContext.AgencyMemberships
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == membershipId && m.AgencyId == agencyId, cancellationToken);

        if (membership?.Status != MembershipStatus.Suspended)
        {
            return new MembershipOperationResult(MembershipOperationStatus.NotFound);
        }

        return await ChangeActiveMembershipStatusAsync(
            actorUserIdentityId,
            agencyId,
            membershipId,
            MembershipStatus.Active,
            AuditActionTypes.MembershipReactivated,
            "Membership reactivated.",
            actorIsAdministrator,
            actorIsOperatorOnAgency,
            checkLastAdmin: false,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<MembershipOperationResult> RemoveMemberAsync(
        Guid actorUserIdentityId,
        Guid agencyId,
        Guid membershipId,
        bool actorIsAdministrator,
        bool actorIsOperatorOnAgency,
        CancellationToken cancellationToken = default)
    {
        var membership = await dbContext.AgencyMemberships
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == membershipId && m.AgencyId == agencyId, cancellationToken);

        if (membership?.Status is not MembershipStatus.Active and not MembershipStatus.Suspended)
        {
            return new MembershipOperationResult(MembershipOperationStatus.NotFound);
        }

        return await ChangeActiveMembershipStatusAsync(
            actorUserIdentityId,
            agencyId,
            membershipId,
            MembershipStatus.Removed,
            AuditActionTypes.MembershipRemoved,
            "Membership removed.",
            actorIsAdministrator,
            actorIsOperatorOnAgency,
            checkLastAdmin: true,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<MembershipOperationResult> RevokeInvitationAsync(
        Guid actorUserIdentityId,
        Guid agencyId,
        Guid membershipId,
        bool actorIsAdministrator,
        bool actorIsOperatorOnAgency,
        CancellationToken cancellationToken = default)
    {
        if (!actorIsAdministrator && !actorIsOperatorOnAgency)
        {
            return new MembershipOperationResult(MembershipOperationStatus.Forbidden);
        }

        if (!await CanMutateMembershipAsync(agencyId, cancellationToken))
        {
            return new MembershipOperationResult(MembershipOperationStatus.Forbidden);
        }

        var membership = await dbContext.AgencyMemberships
            .FirstOrDefaultAsync(m => m.Id == membershipId && m.AgencyId == agencyId, cancellationToken);

        if (membership is null || membership.Status != MembershipStatus.Invited)
        {
            return new MembershipOperationResult(MembershipOperationStatus.NotFound);
        }

        membership.Status = MembershipStatus.Removed;
        membership.UpdatedAt = timeProvider.GetUtcNow();
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditWriter.WriteAsync(
            new AuditEventWrite(
                agencyId,
                actorUserIdentityId,
                AuditActionTypes.MembershipInviteRevoked,
                "Pending invitation revoked.",
                membership.UserIdentityId),
            cancellationToken);

        return new MembershipOperationResult(MembershipOperationStatus.Succeeded, MembershipId: membership.Id);
    }

    private async Task<MembershipOperationResult> CreateMembershipAsync(
        Guid actorUserIdentityId,
        Guid agencyId,
        string email,
        AgencyRole role,
        bool invited,
        bool actorIsAdministrator,
        bool actorIsOperatorOnAgency,
        CancellationToken cancellationToken)
    {
        if (!actorIsAdministrator && !actorIsOperatorOnAgency)
        {
            return new MembershipOperationResult(MembershipOperationStatus.Forbidden);
        }

        if (!await CanMutateMembershipAsync(agencyId, cancellationToken))
        {
            return new MembershipOperationResult(
                MembershipOperationStatus.Forbidden,
                "Membership changes are not allowed while the agency is archived.");
        }

        var normalizedEmail = MembershipEmailNormalizer.Normalize(email);
        var targetUser = await GetOrCreateUserByEmailAsync(normalizedEmail, cancellationToken);

        var existing = await dbContext.AgencyMemberships
            .FirstOrDefaultAsync(
                m => m.AgencyId == agencyId && m.UserIdentityId == targetUser.Id,
                cancellationToken);

        var now = timeProvider.GetUtcNow();

        if (existing is not null)
        {
            if (existing.Status == MembershipStatus.Invited && invited)
            {
                existing.AgencyRole = role;
                existing.InvitedRoleSnapshot = role;
                existing.UpdatedAt = now;
                await dbContext.SaveChangesAsync(cancellationToken);
                return new MembershipOperationResult(
                    MembershipOperationStatus.Succeeded,
                    MembershipId: existing.Id);
            }

            if (existing.Status is MembershipStatus.Active or MembershipStatus.Suspended)
            {
                return new MembershipOperationResult(
                    MembershipOperationStatus.Conflict,
                    "This person already has membership in this agency.");
            }

            if (existing.Status == MembershipStatus.Removed)
            {
                existing.Status = invited ? MembershipStatus.Invited : MembershipStatus.Active;
                existing.AgencyRole = role;
                existing.InvitedRoleSnapshot = invited ? role : null;
                existing.InvitedAt = invited ? now : null;
                existing.ExpiresAt = invited ? InvitationRules.ComputeExpiresAt(now) : null;
                existing.ActivatedAt = invited ? null : now;
                existing.UpdatedAt = now;
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }
        else
        {
            existing = new AgencyMembership
            {
                Id = Guid.NewGuid(),
                AgencyId = agencyId,
                UserIdentityId = targetUser.Id,
                AgencyRole = role,
                Status = invited ? MembershipStatus.Invited : MembershipStatus.Active,
                InvitedAt = invited ? now : null,
                InvitedRoleSnapshot = invited ? role : null,
                ExpiresAt = invited ? InvitationRules.ComputeExpiresAt(now) : null,
                ActivatedAt = invited ? null : now,
                UpdatedAt = now,
            };
            dbContext.AgencyMemberships.Add(existing);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var action = invited ? AuditActionTypes.MembershipInvited : AuditActionTypes.MembershipProvisioned;
        var summary = invited
            ? $"Member invited with role {role}."
            : $"Member provisioned with role {role}.";

        await auditWriter.WriteAsync(
            new AuditEventWrite(agencyId, actorUserIdentityId, action, summary, targetUser.Id),
            cancellationToken);

        if (invited)
        {
            await notificationTriggers.NotifyInviteCreatedAsync(
                agencyId,
                targetUser.Id,
                summary,
                cancellationToken);
        }
        else
        {
            await notificationTriggers.NotifyMembershipActivatedAsync(
                agencyId,
                targetUser.Id,
                summary,
                cancellationToken);
        }

        return new MembershipOperationResult(MembershipOperationStatus.Succeeded, MembershipId: existing.Id);
    }

    private async Task<MembershipOperationResult> ChangeActiveMembershipStatusAsync(
        Guid actorUserIdentityId,
        Guid agencyId,
        Guid membershipId,
        MembershipStatus targetStatus,
        string auditAction,
        string auditSummary,
        bool actorIsAdministrator,
        bool actorIsOperatorOnAgency,
        bool checkLastAdmin,
        CancellationToken cancellationToken)
    {
        if (!actorIsAdministrator && !actorIsOperatorOnAgency)
        {
            return new MembershipOperationResult(MembershipOperationStatus.Forbidden);
        }

        if (!await CanMutateMembershipAsync(agencyId, cancellationToken))
        {
            return new MembershipOperationResult(MembershipOperationStatus.Forbidden);
        }

        var membership = await dbContext.AgencyMemberships
            .FirstOrDefaultAsync(m => m.Id == membershipId && m.AgencyId == agencyId, cancellationToken);

        if (membership is null || membership.Status == MembershipStatus.Removed)
        {
            return new MembershipOperationResult(MembershipOperationStatus.NotFound);
        }

        if (checkLastAdmin && MembershipGuards.IsActiveAdministrator(membership))
        {
            var remaining = await CountActiveAdministratorsAsync(agencyId, excludeMembershipId: membership.Id, cancellationToken);
            try
            {
                MembershipGuards.EnsureAtLeastOneActiveAdministrator(remaining);
            }
            catch (InvalidOperationException ex)
            {
                return new MembershipOperationResult(MembershipOperationStatus.ValidationFailed, ex.Message);
            }
        }

        membership.Status = targetStatus;
        membership.UpdatedAt = timeProvider.GetUtcNow();
        if (targetStatus == MembershipStatus.Active && membership.ActivatedAt is null)
        {
            membership.ActivatedAt = timeProvider.GetUtcNow();
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        await auditWriter.WriteAsync(
            new AuditEventWrite(agencyId, actorUserIdentityId, auditAction, auditSummary, membership.UserIdentityId),
            cancellationToken);

        if (targetStatus == MembershipStatus.Suspended)
        {
            await notificationTriggers.NotifyMembershipSuspendedAsync(
                agencyId,
                membership.UserIdentityId,
                auditSummary,
                cancellationToken);
        }
        else if (targetStatus == MembershipStatus.Active
            && auditAction == AuditActionTypes.MembershipReactivated)
        {
            await notificationTriggers.NotifyMembershipReactivatedAsync(
                agencyId,
                membership.UserIdentityId,
                auditSummary,
                cancellationToken);
        }
        else if (targetStatus == MembershipStatus.Removed)
        {
            await notificationTriggers.NotifyMembershipRemovedAsync(
                agencyId,
                membership.UserIdentityId,
                auditSummary,
                cancellationToken);
        }

        return new MembershipOperationResult(MembershipOperationStatus.Succeeded, MembershipId: membership.Id);
    }

    private async Task<bool> CanMutateMembershipAsync(Guid agencyId, CancellationToken cancellationToken)
    {
        var agency = await dbContext.Agencies
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == agencyId, cancellationToken);

        return agency is not null && agency.LifecycleStatus != AgencyLifecycleStatus.Archived;
    }

    private async Task<int> CountActiveAdministratorsAsync(
        Guid agencyId,
        Guid? excludeMembershipId,
        CancellationToken cancellationToken)
    {
        var query = dbContext.AgencyMemberships.AsNoTracking()
            .Where(m => m.AgencyId == agencyId
                && m.Status == MembershipStatus.Active
                && m.AgencyRole == AgencyRole.Administrator);

        if (excludeMembershipId is Guid exclude)
        {
            query = query.Where(m => m.Id != exclude);
        }

        return await query.CountAsync(cancellationToken);
    }

    private async Task<UserIdentity> GetOrCreateUserByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        var existing = await dbContext.UserIdentities
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (existing is not null)
        {
            return existing;
        }

        var identity = new UserIdentity
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            EntraObjectId = $"unlinked:{Guid.NewGuid():N}",
            CreatedAt = timeProvider.GetUtcNow(),
        };
        dbContext.UserIdentities.Add(identity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return identity;
    }
}
