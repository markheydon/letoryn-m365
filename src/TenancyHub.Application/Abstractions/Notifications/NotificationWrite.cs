namespace TenancyHub.Application.Abstractions.Notifications;

/// <summary>
/// Agency-scoped in-app notification payload (FR-010, FR-012).
/// </summary>
/// <param name="AgencyId">Agency tenant key.</param>
/// <param name="UserIdentityId">Recipient identity.</param>
/// <param name="Category">Stable category aligned with FR-010 triggers.</param>
/// <param name="Title">English title.</param>
/// <param name="Body">English body.</param>
/// <param name="DeliverySurface">Where the notice is shown in R1.</param>
public sealed record NotificationWrite(
    Guid AgencyId,
    Guid UserIdentityId,
    string Category,
    string Title,
    string Body,
    NotificationDeliverySurface DeliverySurface = NotificationDeliverySurface.AgencyNotificationList);
