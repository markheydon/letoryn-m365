using TenancyHub.Application.Abstractions.Notifications;

namespace TenancyHub.Application.Notifications;

/// <summary>Agency settings and lifecycle notification triggers (FR-010).</summary>
public sealed class AgencyNotificationTriggerService(INotificationWriter writer)
{
    /// <summary>Notifies members of agency lifecycle changes.</summary>
    public Task NotifyLifecycleChangeAsync(
        Guid agencyId,
        Guid userIdentityId,
        string summary,
        CancellationToken cancellationToken = default) =>
        writer.WriteAsync(
            new NotificationWrite(
                agencyId,
                userIdentityId,
                "agency.lifecycle",
                "Agency status changed",
                summary,
                NotificationDeliverySurface.AgencyNotificationList),
            cancellationToken);

    /// <summary>Notifies members of agency settings changes.</summary>
    public Task NotifySettingsChangedAsync(
        Guid agencyId,
        Guid userIdentityId,
        string summary,
        CancellationToken cancellationToken = default) =>
        writer.WriteAsync(
            new NotificationWrite(
                agencyId,
                userIdentityId,
                "agency.settings",
                "Agency settings updated",
                summary,
                NotificationDeliverySurface.AgencyNotificationList),
            cancellationToken);
}
