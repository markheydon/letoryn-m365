namespace TenancyHub.Application.Abstractions.Notifications;

/// <summary>
/// Application seam for creating user-visible in-app notices (FR-010, FR-012).
/// </summary>
/// <remarks>
/// R1 delivery is in-app only. Implementations enforce <see cref="NotificationWrite.DeliverySurface"/> rules
/// (for example invite-pending must not appear in the agency bell until membership is active).
/// </remarks>
public interface INotificationWriter
{
    /// <summary>
    /// Persists a notification for the recipient according to FR-010 delivery rules.
    /// </summary>
    /// <param name="notification">Notification payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task WriteAsync(NotificationWrite notification, CancellationToken cancellationToken = default);
}
