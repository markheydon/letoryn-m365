using TenancyHub.Application.Abstractions.Authorization;
using TenancyHub.Domain.Agencies;
using TenancyHub.Domain.Memberships;

namespace TenancyHub.Application.Agencies;

/// <summary>
/// Agency and membership access gates for routine vs operator flows (FR-004, FR-007).
/// </summary>
public static class AgencyAccessRules
{
    /// <summary>UK English message when an agency is suspended (FR-004).</summary>
    public const string AgencySuspendedMemberMessage =
        "This agency is suspended. Member access is not available until a platform operator reactivates the agency.";

    /// <summary>UK English message when an agency is archived (FR-004).</summary>
    public const string AgencyArchivedMemberMessage =
        "This agency is archived. Operational access is not available until a platform operator reactivates the agency.";

    /// <summary>UK English message when membership is suspended (FR-007).</summary>
    public const string MembershipSuspendedMessage =
        "Your membership for this agency is suspended. You cannot access this agency until it is reactivated.";

    /// <summary>UK English message when membership is not active for routine shell work (FR-002, FR-007).</summary>
    public const string MembershipNotActiveForShellMessage =
        "Active agency membership is required for this action. Accept a pending invitation or contact your administrator.";

    /// <summary>
    /// Evaluates whether the caller may proceed with the request given agency header context (middleware).
    /// </summary>
    /// <param name="agencyStatus">Lifecycle status of the header agency.</param>
    /// <param name="membershipStatus">Caller membership status when a row exists.</param>
    /// <param name="isOperatorAssigned">Whether the caller is assigned to the agency as a platform operator.</param>
    /// <param name="isOperatorRoute">Whether the request targets operator-only API routes.</param>
    public static AuthorizationResult EvaluateAgencyHeaderAccess(
        AgencyLifecycleStatus agencyStatus,
        MembershipStatus? membershipStatus,
        bool isOperatorAssigned,
        bool isOperatorRoute)
    {
        if (isOperatorRoute)
        {
            if (!isOperatorAssigned)
            {
                return AuthorizationResult.NotFound();
            }

            return AuthorizationResult.Succeeded();
        }

        if (agencyStatus == AgencyLifecycleStatus.Archived)
        {
            return AuthorizationResult.Denied(
                AgencyArchivedMemberMessage,
                AuthorizationFailureKind.AgencyAccessBlocked);
        }

        if (agencyStatus == AgencyLifecycleStatus.Suspended)
        {
            return AuthorizationResult.Denied(
                AgencySuspendedMemberMessage,
                AuthorizationFailureKind.AgencyAccessBlocked);
        }

        if (membershipStatus is null)
        {
            return AuthorizationResult.NotFound();
        }

        return membershipStatus switch
        {
            MembershipStatus.Active => AuthorizationResult.Succeeded(),
            MembershipStatus.Suspended => AuthorizationResult.Denied(
                MembershipSuspendedMessage,
                AuthorizationFailureKind.MembershipAccessBlocked),
            MembershipStatus.Invited or MembershipStatus.Removed => AuthorizationResult.Denied(
                MembershipNotActiveForShellMessage,
                AuthorizationFailureKind.MembershipAccessBlocked),
            _ => AuthorizationResult.NotFound(),
        };
    }
}
