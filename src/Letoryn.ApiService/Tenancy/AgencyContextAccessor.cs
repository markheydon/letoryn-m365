using Letoryn.Application.Abstractions.Tenancy;
using Letoryn.Domain.Agencies;
using Letoryn.Domain.Memberships;

namespace Letoryn.ApiService.Tenancy;

/// <summary>Mutable agency context populated by middleware.</summary>
public sealed class AgencyContextAccessor : IAgencyContext
{
    /// <inheritdoc />
    public Guid? ActiveAgencyId { get; internal set; }

    /// <inheritdoc />
    public AgencyLifecycleStatus? ActiveAgencyLifecycleStatus { get; internal set; }

    /// <inheritdoc />
    public Guid? ActiveMembershipId { get; internal set; }

    /// <inheritdoc />
    public MembershipStatus? ActiveMembershipStatus { get; internal set; }

    /// <inheritdoc />
    public AgencyRole? ActiveAgencyRole { get; internal set; }

    /// <inheritdoc />
    public bool IsOperatorAssignedToActiveAgency { get; internal set; }

    /// <inheritdoc />
    public bool HasRoutineShellAgencyContext =>
        ActiveAgencyId is not null
        && ActiveAgencyLifecycleStatus == AgencyLifecycleStatus.Active
        && ActiveMembershipStatus == MembershipStatus.Active
        && ActiveMembershipId is not null;
}
