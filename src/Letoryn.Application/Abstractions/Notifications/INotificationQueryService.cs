namespace Letoryn.Application.Abstractions.Notifications;

/// <summary>Reads in-app notifications for the active agency context.</summary>
public interface INotificationQueryService
{
    /// <summary>Lists notifications for the active membership in the agency.</summary>
    Task<IReadOnlyList<NotificationListItem>> ListForActiveAgencyAsync(
        Guid agencyId,
        Guid membershipId,
        CancellationToken cancellationToken = default);

    /// <summary>Marks a notification as read for standard members.</summary>
    Task<bool> MarkReadAsync(
        Guid agencyId,
        Guid notificationId,
        Guid membershipId,
        CancellationToken cancellationToken = default);
}

/// <summary>Notification list row.</summary>
public sealed record NotificationListItem(
    Guid Id,
    string Title,
    string Body,
    DateTimeOffset CreatedAt,
    bool IsRead);
