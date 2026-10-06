using Letoryn.Application.Abstractions.Tenancy;
using Letoryn.Domain.Memberships;

namespace Letoryn.ApiService.Infrastructure;

/// <summary>Membership management permission helpers for agency routes.</summary>
public static class MembershipAccessHelper
{
    /// <summary>Whether the caller may invite, provision, or change memberships.</summary>
    public static bool CanManageMemberships(IAgencyContext context) =>
        context.ActiveAgencyRole == AgencyRole.Administrator
        || context.IsOperatorAssignedToActiveAgency;

    /// <summary>Whether the caller may view the member roster.</summary>
    public static bool CanViewRoster(IAgencyContext context) =>
        context.ActiveAgencyRole is AgencyRole.Administrator or AgencyRole.StandardMember
        || context.IsOperatorAssignedToActiveAgency;

    /// <summary>Whether the caller is an active agency administrator on an active agency.</summary>
    public static bool IsActiveAgencyAdministrator(IAgencyContext context) =>
        context.ActiveAgencyRole == AgencyRole.Administrator
        && context.ActiveMembershipStatus == MembershipStatus.Active;
}
