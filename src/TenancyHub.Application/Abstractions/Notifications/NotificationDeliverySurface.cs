namespace TenancyHub.Application.Abstractions.Notifications;

/// <summary>
/// Where an in-app notice is surfaced to the user (FR-010).
/// </summary>
public enum NotificationDeliverySurface
{
    /// <summary>
    /// Agency notification bell and list for users with routine shell context (FR-010).
    /// </summary>
    AgencyNotificationList = 0,

    /// <summary>
    /// Invitation-acceptance experience for pending invites (not the agency bell) (FR-010).
    /// </summary>
    InvitationAcceptance = 1,
}
