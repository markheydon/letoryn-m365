using TenancyHub.Application.Abstractions.Notifications;
using TenancyHub.Application.Notifications;

namespace TenancyHub.Application.UnitTests.Notifications;

public sealed class NotificationTriggerServiceTests
{
    private readonly INotificationWriter _writer = Substitute.For<INotificationWriter>();
    private readonly NotificationTriggerService _service;

    public NotificationTriggerServiceTests()
    {
        _service = new NotificationTriggerService(_writer);
    }

    [Fact]
    public async Task NotifyInviteCreatedAsync_UsesInvitationAcceptanceSurface_NotAgencyBell()
    {
        var agencyId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await _service.NotifyInviteCreatedAsync(agencyId, userId, "Invite summary", TestContext.Current.CancellationToken);

        await _writer.Received(1).WriteAsync(
            Arg.Is<NotificationWrite>(n =>
                n.Category == "membership.invited"
                && n.DeliverySurface == NotificationDeliverySurface.InvitationAcceptance),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task NotifyMembershipActivatedAsync_UsesAgencyNotificationList()
    {
        var agencyId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await _service.NotifyMembershipActivatedAsync(
            agencyId,
            userId,
            "Activated",
            TestContext.Current.CancellationToken);

        await _writer.Received(1).WriteAsync(
            Arg.Is<NotificationWrite>(n =>
                n.Category == "membership.activated"
                && n.DeliverySurface == NotificationDeliverySurface.AgencyNotificationList),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task NotifyRoleChangedAsync_UsesAgencyNotificationList()
    {
        var agencyId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await _service.NotifyRoleChangedAsync(
            agencyId,
            userId,
            "Role updated",
            TestContext.Current.CancellationToken);

        await _writer.Received(1).WriteAsync(
            Arg.Is<NotificationWrite>(n =>
                n.Category == "membership.role_changed"
                && n.DeliverySurface == NotificationDeliverySurface.AgencyNotificationList),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task NotifyMembershipSuspendedAsync_UsesAgencyNotificationList()
    {
        var agencyId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await _service.NotifyMembershipSuspendedAsync(
            agencyId,
            userId,
            "Suspended",
            TestContext.Current.CancellationToken);

        await _writer.Received(1).WriteAsync(
            Arg.Is<NotificationWrite>(n =>
                n.Category == "membership.suspended"
                && n.DeliverySurface == NotificationDeliverySurface.AgencyNotificationList),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task NotifyMembershipReactivatedAsync_UsesAgencyNotificationList()
    {
        var agencyId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await _service.NotifyMembershipReactivatedAsync(
            agencyId,
            userId,
            "Reactivated",
            TestContext.Current.CancellationToken);

        await _writer.Received(1).WriteAsync(
            Arg.Is<NotificationWrite>(n =>
                n.Category == "membership.reactivated"
                && n.DeliverySurface == NotificationDeliverySurface.AgencyNotificationList),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task NotifyMembershipRemovedAsync_UsesAgencyNotificationList()
    {
        var agencyId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await _service.NotifyMembershipRemovedAsync(
            agencyId,
            userId,
            "Removed",
            TestContext.Current.CancellationToken);

        await _writer.Received(1).WriteAsync(
            Arg.Is<NotificationWrite>(n =>
                n.Category == "membership.removed"
                && n.DeliverySurface == NotificationDeliverySurface.AgencyNotificationList),
            Arg.Any<CancellationToken>());
    }
}
