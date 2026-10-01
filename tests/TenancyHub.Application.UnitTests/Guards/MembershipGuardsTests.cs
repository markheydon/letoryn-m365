using TenancyHub.Domain.Guards;
using TenancyHub.Domain.Memberships;

namespace TenancyHub.Application.UnitTests.Guards;

public sealed class MembershipGuardsTests
{
    [Fact]
    public void EnsureAtLeastOneActiveAdministrator_WhenZero_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => MembershipGuards.EnsureAtLeastOneActiveAdministrator(0));
    }

    [Fact]
    public void IsActiveAdministrator_WhenActiveAdmin_ReturnsTrue()
    {
        var membership = new AgencyMembership
        {
            Status = MembershipStatus.Active,
            AgencyRole = AgencyRole.Administrator,
        };

        Assert.True(MembershipGuards.IsActiveAdministrator(membership));
    }
}
