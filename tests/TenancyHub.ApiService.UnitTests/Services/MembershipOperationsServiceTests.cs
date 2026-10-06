using Microsoft.EntityFrameworkCore;
using TenancyHub.Application.Abstractions.Audit;
using TenancyHub.Application.Abstractions.Memberships;
using TenancyHub.Application.Abstractions.Notifications;
using TenancyHub.Application.Memberships;
using TenancyHub.Application.Notifications;
using TenancyHub.Domain.Agencies;
using TenancyHub.Domain.Identities;
using TenancyHub.Domain.Memberships;
using TenancyHub.Infrastructure.Persistence;
using TenancyHub.Infrastructure.Services;

namespace TenancyHub.ApiService.UnitTests.Services;

public sealed class MembershipOperationsServiceTests
{
    [Fact]
    public async Task AcceptInvitationAsync_ExpiredInvite_WritesExpiryAudit()
    {
        await using var db = CreateDbContext();
        var agencyId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var membershipId = Guid.NewGuid();
        var invitedAt = DateTimeOffset.UtcNow.AddDays(-40);

        var now = DateTimeOffset.UtcNow;
        db.Agencies.Add(new Agency
        {
            Id = agencyId,
            DisplayName = "Test Agency",
            PrimaryContactEmail = "ops@example.com",
            PrimaryContactPhone = "000",
            LifecycleStatus = AgencyLifecycleStatus.Active,
            CreatedAt = now,
            UpdatedAt = now,
            LastLifecycleChangeAt = now,
        });
        db.UserIdentities.Add(new UserIdentity
        {
            Id = userId,
            Email = "invitee@example.com",
            EntraObjectId = "entra-1",
            CreatedAt = DateTimeOffset.UtcNow,
        });
        db.AgencyMemberships.Add(new AgencyMembership
        {
            Id = membershipId,
            AgencyId = agencyId,
            UserIdentityId = userId,
            Status = MembershipStatus.Invited,
            AgencyRole = AgencyRole.StandardMember,
            InvitedAt = invitedAt,
            ExpiresAt = InvitationRules.ComputeExpiresAt(invitedAt),
            UpdatedAt = invitedAt,
        });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var auditWriter = Substitute.For<IAuditWriter>();
        var notifications = new NotificationTriggerService(Substitute.For<INotificationWriter>());
        var time = TimeProvider.System;
        var service = new MembershipOperationsService(db, auditWriter, notifications, time);

        var result = await service.AcceptInvitationAsync(
            userId,
            "invitee@example.com",
            membershipId,
            TestContext.Current.CancellationToken);

        Assert.Equal(MembershipOperationStatus.ValidationFailed, result.Status);
        await auditWriter.Received(1).WriteAsync(
            Arg.Is<AuditEventWrite>(e =>
                e.ActionType == AuditActionTypes.MembershipInviteExpired
                && e.AgencyId == agencyId
                && e.TargetUserIdentityId == userId),
            Arg.Any<CancellationToken>());
    }

    private static TenancyHubDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TenancyHubDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new TenancyHubDbContext(options);
    }
}
