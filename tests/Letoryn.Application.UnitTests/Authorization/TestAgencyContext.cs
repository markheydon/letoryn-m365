using Letoryn.Application.Abstractions.Tenancy;
using Letoryn.Domain.Agencies;
using Letoryn.Domain.Memberships;

namespace Letoryn.Application.UnitTests.Authorization;

internal sealed class TestAgencyContext : IAgencyContext
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
