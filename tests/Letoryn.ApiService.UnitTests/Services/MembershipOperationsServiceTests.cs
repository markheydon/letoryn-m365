using Letoryn.Application.Abstractions.Audit;
using Letoryn.Application.Abstractions.Memberships;
using Letoryn.Application.Abstractions.Notifications;
using Letoryn.Application.Memberships;
using Letoryn.Application.Notifications;
using Letoryn.Domain.Agencies;
using Letoryn.Domain.Identities;
using Letoryn.Domain.Memberships;
using Letoryn.Infrastructure.Persistence;
using Letoryn.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace Letoryn.ApiService.UnitTests.Services;

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
        var service = CreateService(db, auditWriter);

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

    [Fact]
    public async Task InviteMemberAsync_ExistingExpiredInvite_RefreshesExpiryWindow()
    {
        await using var db = CreateDbContext();
        var agencyId = Guid.NewGuid();
        var actorId = Guid.NewGuid();
        var inviteeId = Guid.NewGuid();
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
            Id = inviteeId,
            Email = "invitee@example.com",
            EntraObjectId = "entra-invitee",
            CreatedAt = now,
        });
        db.AgencyMemberships.Add(new AgencyMembership
        {
            Id = membershipId,
            AgencyId = agencyId,
            UserIdentityId = inviteeId,
            Status = MembershipStatus.Invited,
            AgencyRole = AgencyRole.StandardMember,
            InvitedAt = invitedAt,
            ExpiresAt = InvitationRules.ComputeExpiresAt(invitedAt),
            UpdatedAt = invitedAt,
        });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var auditWriter = Substitute.For<IAuditWriter>();
        var service = CreateService(db, auditWriter);
        var result = await service.InviteMemberAsync(
            actorId,
            agencyId,
            "invitee@example.com",
            AgencyRole.ReadOnlyMember,
            actorIsAdministrator: true,
            actorIsOperatorOnAgency: false,
            TestContext.Current.CancellationToken);

        Assert.Equal(MembershipOperationStatus.Succeeded, result.Status);
        var row = await db.AgencyMemberships.SingleAsync(m => m.Id == membershipId, TestContext.Current.CancellationToken);
        Assert.Equal(AgencyRole.ReadOnlyMember, row.AgencyRole);
        Assert.True(row.ExpiresAt > DateTimeOffset.UtcNow);
        await auditWriter.Received(1).WriteAsync(
            Arg.Is<AuditEventWrite>(e => e.ActionType == AuditActionTypes.MembershipInvited),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProvisionMemberAsync_ExistingInvitedMembership_ActivatesMember()
    {
        await using var db = CreateDbContext();
        var agencyId = Guid.NewGuid();
        var actorId = Guid.NewGuid();
        var inviteeId = Guid.NewGuid();
        var membershipId = Guid.NewGuid();
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
            Id = inviteeId,
            Email = "invitee@example.com",
            EntraObjectId = "entra-invitee",
            CreatedAt = now,
        });
        db.AgencyMemberships.Add(new AgencyMembership
        {
            Id = membershipId,
            AgencyId = agencyId,
            UserIdentityId = inviteeId,
            Status = MembershipStatus.Invited,
            AgencyRole = AgencyRole.StandardMember,
            InvitedAt = now,
            ExpiresAt = InvitationRules.ComputeExpiresAt(now),
            UpdatedAt = now,
        });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var auditWriter = Substitute.For<IAuditWriter>();
        var service = CreateService(db, auditWriter);
        var result = await service.ProvisionMemberAsync(
            actorId,
            agencyId,
            "invitee@example.com",
            AgencyRole.Administrator,
            actorIsAdministrator: true,
            actorIsOperatorOnAgency: false,
            TestContext.Current.CancellationToken);

        Assert.Equal(MembershipOperationStatus.Succeeded, result.Status);
        var row = await db.AgencyMemberships.SingleAsync(m => m.Id == membershipId, TestContext.Current.CancellationToken);
        Assert.Equal(MembershipStatus.Active, row.Status);
        Assert.Equal(AgencyRole.Administrator, row.AgencyRole);
        Assert.NotNull(row.ActivatedAt);
        await auditWriter.Received(1).WriteAsync(
            Arg.Is<AuditEventWrite>(e => e.ActionType == AuditActionTypes.MembershipProvisioned),
            Arg.Any<CancellationToken>());
    }

    private static MembershipOperationsService CreateService(
        LetorynDbContext db,
        IAuditWriter? auditWriter = null)
    {
        auditWriter ??= Substitute.For<IAuditWriter>();
        var notifications = new NotificationTriggerService(Substitute.For<INotificationWriter>());
        return new MembershipOperationsService(db, auditWriter, notifications, TimeProvider.System);
    }

    private static LetorynDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<LetorynDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new LetorynDbContext(options);
    }
}
