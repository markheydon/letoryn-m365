using Microsoft.EntityFrameworkCore;
using TenancyHub.Application.Abstractions.Audit;
using TenancyHub.Application.Memberships;
using TenancyHub.Domain.Memberships;
using TenancyHub.Infrastructure.Persistence;

namespace TenancyHub.ApiService.Services;

/// <summary>Periodic sweep to expire stale invitations (optional complement to lazy evaluation).</summary>
public sealed class InvitationExpiryHostedService(
    IServiceProvider serviceProvider,
    TimeProvider timeProvider,
    ILogger<InvitationExpiryHostedService> logger) : BackgroundService
{
    private static readonly TimeSpan SweepInterval = TimeSpan.FromHours(6);

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SweepAsync(stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogWarning(ex, "Invitation expiry sweep failed.");
            }

            await Task.Delay(SweepInterval, stoppingToken);
        }
    }

    private async Task SweepAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TenancyHubDbContext>();
        var auditWriter = scope.ServiceProvider.GetRequiredService<IAuditWriter>();
        var now = timeProvider.GetUtcNow();

        var invited = await db.AgencyMemberships
            .Where(m => m.Status == MembershipStatus.Invited)
            .ToListAsync(cancellationToken);

        var changed = false;
        foreach (var membership in invited)
        {
            if (!InvitationRules.IsExpired(membership, now))
            {
                continue;
            }

            membership.Status = MembershipStatus.Removed;
            membership.UpdatedAt = now;
            changed = true;

            await auditWriter.WriteAsync(
                new AuditEventWrite(
                    membership.AgencyId,
                    null,
                    AuditActionTypes.MembershipInviteExpired,
                    "Invitation expired during background sweep.",
                    membership.UserIdentityId),
                cancellationToken);
        }

        if (changed)
        {
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
