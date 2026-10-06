using Letoryn.Application.Memberships;
using Letoryn.Domain.Memberships;

namespace Letoryn.Application.UnitTests.Memberships;

public sealed class InvitationRulesTests
{
    private static readonly DateTimeOffset InvitedAt = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void ComputeExpiresAt_AddsThirtyDayLifetime()
    {
        var expires = InvitationRules.ComputeExpiresAt(InvitedAt);

        Assert.Equal(InvitedAt.Add(InvitationRules.InviteLifetime), expires);
    }

    [Fact]
    public void IsExpired_WhenActiveMembership_ReturnsFalse()
    {
        var membership = new AgencyMembership { Status = MembershipStatus.Active };

        Assert.False(InvitationRules.IsExpired(membership, InvitedAt.AddDays(31)));
    }

    [Fact]
    public void IsExpired_WhenExpiresAtInFuture_ReturnsFalse()
    {
        var membership = new AgencyMembership
        {
            Status = MembershipStatus.Invited,
            ExpiresAt = InvitedAt.AddDays(30),
        };

        Assert.False(InvitationRules.IsExpired(membership, InvitedAt.AddDays(29)));
    }

    [Fact]
    public void IsExpired_WhenExpiresAtPassed_ReturnsTrue()
    {
        var membership = new AgencyMembership
        {
            Status = MembershipStatus.Invited,
            ExpiresAt = InvitedAt.AddDays(30),
        };

        Assert.True(InvitationRules.IsExpired(membership, InvitedAt.AddDays(30).AddSeconds(1)));
    }

    [Fact]
    public void IsExpired_WhenInvitedAtOnlyAndPastWindow_ReturnsTrue()
    {
        var membership = new AgencyMembership
        {
            Status = MembershipStatus.Invited,
            InvitedAt = InvitedAt,
        };

        Assert.True(InvitationRules.IsExpired(membership, InvitationRules.ComputeExpiresAt(InvitedAt).AddSeconds(1)));
    }

    [Fact]
    public void IsExpired_WhenInvitedWithoutTimestamps_ReturnsFalse()
    {
        var membership = new AgencyMembership { Status = MembershipStatus.Invited };

        Assert.False(InvitationRules.IsExpired(membership, InvitedAt));
    }
}
