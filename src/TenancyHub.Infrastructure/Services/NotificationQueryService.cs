using Microsoft.EntityFrameworkCore;
using TenancyHub.Application.Abstractions.Notifications;
using TenancyHub.Infrastructure.Persistence;

namespace TenancyHub.Infrastructure.Services;

/// <inheritdoc />
public sealed class NotificationQueryService(TenancyHubDbContext dbContext, TimeProvider timeProvider)
    : INotificationQueryService
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<NotificationListItem>> ListForActiveAgencyAsync(
        Guid agencyId,
        Guid membershipId,
        CancellationToken cancellationToken = default)
    {
        var membership = await dbContext.AgencyMemberships
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == membershipId && m.AgencyId == agencyId, cancellationToken);

        if (membership is null)
        {
            return [];
        }

        return await dbContext.Notifications
            .AsNoTracking()
            .Where(n => n.AgencyId == agencyId
                && n.UserIdentityId == membership.UserIdentityId
                && n.Category != "membership.invited")
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationListItem(
                n.Id,
                n.Title,
                n.Body,
                n.CreatedAt,
                n.IsRead))
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> MarkReadAsync(
        Guid agencyId,
        Guid notificationId,
        Guid membershipId,
        CancellationToken cancellationToken = default)
    {
        var membership = await dbContext.AgencyMemberships
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == membershipId && m.AgencyId == agencyId, cancellationToken);

        if (membership is null)
        {
            return false;
        }

        var notification = await dbContext.Notifications
            .FirstOrDefaultAsync(
                n => n.Id == notificationId
                    && n.AgencyId == agencyId
                    && n.UserIdentityId == membership.UserIdentityId,
                cancellationToken);

        if (notification is null)
        {
            return false;
        }

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = timeProvider.GetUtcNow();
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return true;
    }
}
