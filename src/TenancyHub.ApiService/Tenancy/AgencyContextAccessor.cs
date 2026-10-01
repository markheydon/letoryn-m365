using TenancyHub.Application.Abstractions.Tenancy;
using TenancyHub.Domain.Agencies;
using TenancyHub.Domain.Memberships;

namespace TenancyHub.ApiService.Tenancy;

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
