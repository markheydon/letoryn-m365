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
    public void AuthorizeAgencyRole_ReadOnlyMemberWhenAdministratorRequired_DeniesForbidden()
    {
        var agencyId = Guid.NewGuid();
        IAgencyContext context = new TestAgencyContext
        {
            ActiveAgencyId = agencyId,
            ActiveAgencyLifecycleStatus = AgencyLifecycleStatus.Active,
            ActiveMembershipStatus = MembershipStatus.Active,
            ActiveMembershipId = Guid.NewGuid(),
            ActiveAgencyRole = AgencyRole.ReadOnlyMember,
        };
        IAgencyAuthorizationService service = new AgencyAuthorizationService(context);

        var result = service.AuthorizeAgencyRole([AgencyRole.Administrator]);

        Assert.False(result.IsAuthorized);
        Assert.Equal(AuthorizationFailureKind.Forbidden, result.FailureKind);
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
}
