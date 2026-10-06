using TenancyHub.Domain.Guards;
using TenancyHub.Domain.Memberships;

namespace TenancyHub.Application.UnitTests.Guards;

public sealed class MembershipGuardsTests
{
    [Fact]
    public void EnsureAtLeastOneActiveAdministrator_WhenZero_Throws()
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => MembershipGuards.EnsureAtLeastOneActiveAdministrator(0));

        Assert.Contains("administrator", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void EnsureAtLeastOneActiveAdministrator_WhenOne_DoesNotThrow()
    {
        MembershipGuards.EnsureAtLeastOneActiveAdministrator(1);
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
