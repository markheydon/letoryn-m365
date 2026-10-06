using TenancyHub.ApiService.Infrastructure;
using TenancyHub.Application.Abstractions.Tenancy;
using TenancyHub.Domain.Agencies;
using TenancyHub.Domain.Memberships;

namespace TenancyHub.ApiService.UnitTests.Infrastructure;

public sealed class MembershipAccessHelperTests
{
    [Fact]
    public void CanViewRoster_ReadOnlyMember_ReturnsFalse()
    {
        IAgencyContext context = new TestAgencyContext
        {
            ActiveAgencyRole = AgencyRole.ReadOnlyMember,
            ActiveMembershipStatus = MembershipStatus.Active,
        };

        Assert.False(MembershipAccessHelper.CanViewRoster(context));
    }

    [Fact]
    public void CanViewRoster_StandardMember_ReturnsTrue()
    {
        IAgencyContext context = new TestAgencyContext
        {
            ActiveAgencyRole = AgencyRole.StandardMember,
            ActiveMembershipStatus = MembershipStatus.Active,
        };

        Assert.True(MembershipAccessHelper.CanViewRoster(context));
    }

    [Fact]
    public void CanManageMemberships_ReadOnlyMember_ReturnsFalse()
    {
        IAgencyContext context = new TestAgencyContext
        {
            ActiveAgencyRole = AgencyRole.ReadOnlyMember,
            ActiveMembershipStatus = MembershipStatus.Active,
        };

        Assert.False(MembershipAccessHelper.CanManageMemberships(context));
    }

    [Fact]
    public void CanManageMemberships_OperatorAssigned_ReturnsTrue()
    {
        IAgencyContext context = new TestAgencyContext
        {
            ActiveAgencyRole = AgencyRole.ReadOnlyMember,
            IsOperatorAssignedToActiveAgency = true,
        };

        Assert.True(MembershipAccessHelper.CanManageMemberships(context));
    }

    private sealed class TestAgencyContext : IAgencyContext
    {
        public Guid? ActiveAgencyId { get; init; }

        public AgencyLifecycleStatus? ActiveAgencyLifecycleStatus { get; init; }

        public Guid? ActiveMembershipId { get; init; }

        public MembershipStatus? ActiveMembershipStatus { get; init; }

        public AgencyRole? ActiveAgencyRole { get; init; }

        public bool IsOperatorAssignedToActiveAgency { get; init; }

        public bool HasRoutineShellAgencyContext => false;
    }
}
