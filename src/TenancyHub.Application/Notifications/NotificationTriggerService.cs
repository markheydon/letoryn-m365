using TenancyHub.Application.Abstractions.Notifications;

namespace TenancyHub.Application.Notifications;

/// <summary>Membership-related notification triggers (FR-010).</summary>
public sealed class NotificationTriggerService(INotificationWriter writer)
{
    /// <summary>Notifies on membership activation (not invite-pending bell delivery).</summary>
    public Task NotifyMembershipActivatedAsync(
        Guid agencyId,
        Guid userIdentityId,
        string summary,
        CancellationToken cancellationToken = default) =>
        writer.WriteAsync(
            new NotificationWrite(
                agencyId,
                userIdentityId,
                "membership.activated",
                "Membership activated",
                summary,
                NotificationDeliverySurface.AgencyNotificationList),
            cancellationToken);

    /// <summary>May persist invite notification for history but not agency bell.</summary>
    public Task NotifyInviteCreatedAsync(
        Guid agencyId,
        Guid userIdentityId,
        string summary,
        CancellationToken cancellationToken = default) =>
        writer.WriteAsync(
            new NotificationWrite(
                agencyId,
                userIdentityId,
                "membership.invited",
                "Agency invitation",
                summary,
                NotificationDeliverySurface.InvitationAcceptance),
            cancellationToken);

    /// <summary>Role change bell notification.</summary>
    public Task NotifyRoleChangedAsync(
        Guid agencyId,
        Guid userIdentityId,
        string summary,
        CancellationToken cancellationToken = default) =>
        writer.WriteAsync(
            new NotificationWrite(
                agencyId,
                userIdentityId,
                "membership.role_changed",
                "Role changed",
                summary,
                NotificationDeliverySurface.AgencyNotificationList),
            cancellationToken);

    /// <summary>Membership suspended bell notification.</summary>
    public Task NotifyMembershipSuspendedAsync(
        Guid agencyId,
        Guid userIdentityId,
        string summary,
        CancellationToken cancellationToken = default) =>
        writer.WriteAsync(
            new NotificationWrite(
                agencyId,
                userIdentityId,
                "membership.suspended",
                "Membership suspended",
                summary,
                NotificationDeliverySurface.AgencyNotificationList),
            cancellationToken);

    /// <summary>Membership reactivated bell notification.</summary>
    public Task NotifyMembershipReactivatedAsync(
        Guid agencyId,
        Guid userIdentityId,
        string summary,
        CancellationToken cancellationToken = default) =>
        writer.WriteAsync(
            new NotificationWrite(
                agencyId,
                userIdentityId,
                "membership.reactivated",
                "Membership reactivated",
                summary,
                NotificationDeliverySurface.AgencyNotificationList),
            cancellationToken);

    /// <summary>Membership removed bell notification.</summary>
    public Task NotifyMembershipRemovedAsync(
        Guid agencyId,
        Guid userIdentityId,
        string summary,
        CancellationToken cancellationToken = default) =>
        writer.WriteAsync(
            new NotificationWrite(
                agencyId,
                userIdentityId,
                "membership.removed",
                "Membership removed",
                summary,
                NotificationDeliverySurface.AgencyNotificationList),
            cancellationToken);
}
