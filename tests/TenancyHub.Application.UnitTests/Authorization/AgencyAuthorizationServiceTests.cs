using TenancyHub.Application.Abstractions.Authorization;
using TenancyHub.Application.Abstractions.Tenancy;
using TenancyHub.Application.Agencies;
using TenancyHub.Application.Authorization;
using TenancyHub.Domain.Agencies;
using TenancyHub.Domain.Memberships;

namespace TenancyHub.Application.UnitTests.Authorization;

public sealed class AgencyAuthorizationServiceTests
{
    [Fact]
    public void AuthorizeRoutineShellAccess_WhenContextIncomplete_Denies()
    {
        IAgencyContext context = new TestAgencyContext
        {
            ActiveAgencyId = Guid.NewGuid(),
            ActiveAgencyLifecycleStatus = AgencyLifecycleStatus.Active,
            ActiveMembershipStatus = MembershipStatus.Invited,
            ActiveMembershipId = Guid.NewGuid(),
        };
        IAgencyAuthorizationService service = new AgencyAuthorizationService(context);

        var result = service.AuthorizeRoutineShellAccess();

        Assert.False(result.IsAuthorized);
    }

    [Fact]
    public void AuthorizeAgencyScopedAccess_WhenAgencySuspended_DeniesWithLifecycleMessage()
    {
        var agencyId = Guid.NewGuid();
        IAgencyContext context = new TestAgencyContext
        {
            ActiveAgencyId = agencyId,
            ActiveAgencyLifecycleStatus = AgencyLifecycleStatus.Suspended,
            ActiveMembershipStatus = MembershipStatus.Active,
            ActiveMembershipId = Guid.NewGuid(),
            ActiveAgencyRole = AgencyRole.StandardMember,
        };
        IAgencyAuthorizationService service = new AgencyAuthorizationService(context);

        var result = service.AuthorizeAgencyScopedAccess(agencyId);

        Assert.False(result.IsAuthorized);
        Assert.Equal(AgencyAccessRules.AgencySuspendedMemberMessage, result.UserMessage);
    }

    private sealed class TestAgencyContext : IAgencyContext
    {
        public Guid? ActiveAgencyId { get; init; }
        public AgencyLifecycleStatus? ActiveAgencyLifecycleStatus { get; init; }
        public Guid? ActiveMembershipId { get; init; }
        public MembershipStatus? ActiveMembershipStatus { get; init; }
        public AgencyRole? ActiveAgencyRole { get; init; }
        public bool IsOperatorAssignedToActiveAgency { get; init; }

        public bool HasRoutineShellAgencyContext =>
            ActiveAgencyId is not null
            && ActiveAgencyLifecycleStatus == AgencyLifecycleStatus.Active
            && ActiveMembershipStatus == MembershipStatus.Active
            && ActiveMembershipId is not null;
    }
}
