using TenancyHub.Application.Abstractions.Notifications;
using TenancyHub.Domain.Notifications;
using TenancyHub.Infrastructure.Persistence;

namespace TenancyHub.Infrastructure.Services;

/// <summary>EF-backed notification writer.</summary>
public sealed class NotificationWriter(TenancyHubDbContext dbContext) : INotificationWriter
{
    /// <inheritdoc />
    public async Task WriteAsync(NotificationWrite notification, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notification);

        if (notification.DeliverySurface == NotificationDeliverySurface.InvitationAcceptance)
        {
            // Persist row but callers control surfacing (FR-010 invite path).
        }

        var entity = new Notification
        {
            Id = Guid.NewGuid(),
            AgencyId = notification.AgencyId,
            UserIdentityId = notification.UserIdentityId,
            CreatedAt = DateTimeOffset.UtcNow,
            Category = notification.Category,
            Title = notification.Title,
            Body = notification.Body,
            IsRead = false,
        };

        dbContext.Notifications.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
