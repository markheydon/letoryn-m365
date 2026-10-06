using Letoryn.Application.Memberships;
using Letoryn.Domain.Agencies;
using Letoryn.Domain.Identities;
using Letoryn.Domain.Memberships;
using Letoryn.Infrastructure.Persistence;
using Letoryn.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace Letoryn.ApiService.UnitTests.Services;

public sealed class MeProfileServiceTests
{
    [Fact]
    public async Task GetProfileAsync_ExcludesExpiredPendingInvites()
    {
        await using var db = CreateDbContext();
        var userId = Guid.NewGuid();
        var agencyId = Guid.NewGuid();
        var invitedAt = DateTimeOffset.UtcNow.AddDays(-40);
        var now = DateTimeOffset.UtcNow;

        db.UserIdentities.Add(new UserIdentity
        {
            Id = userId,
            Email = "user@example.com",
            EntraObjectId = "entra-user",
            CreatedAt = now,
        });
        db.Agencies.Add(new Agency
        {
            Id = agencyId,
            DisplayName = "Expired Invite Agency",
            PrimaryContactEmail = "ops@example.com",
            PrimaryContactPhone = "000",
            LifecycleStatus = AgencyLifecycleStatus.Active,
            CreatedAt = now,
            UpdatedAt = now,
            LastLifecycleChangeAt = now,
        });
        db.AgencyMemberships.Add(new AgencyMembership
        {
            Id = Guid.NewGuid(),
            AgencyId = agencyId,
            UserIdentityId = userId,
            Status = MembershipStatus.Invited,
            AgencyRole = AgencyRole.StandardMember,
            InvitedAt = invitedAt,
            ExpiresAt = InvitationRules.ComputeExpiresAt(invitedAt),
            UpdatedAt = invitedAt,
        });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var time = new FakeTimeProvider(now);
        var service = new MeProfileService(db, time);
        var profile = await service.GetProfileAsync(userId, TestContext.Current.CancellationToken);

        Assert.NotNull(profile);
        Assert.Empty(profile.PendingInvites);
    }

    private static LetorynDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<LetorynDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new LetorynDbContext(options);
    }

    private sealed class FakeTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
