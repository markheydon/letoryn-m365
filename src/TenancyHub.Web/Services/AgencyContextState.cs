using TenancyHub.Application.Agencies;
using TenancyHub.Application.Me;

namespace TenancyHub.Web.Services;

/// <summary>
/// Active agency context for the signed-in Blazor session (FR-002).
/// </summary>
public sealed class AgencyContextState
{
    /// <summary>Currently selected agency id for API calls.</summary>
    public Guid? ActiveAgencyId { get; private set; }

    /// <summary>Display name for chrome.</summary>
    public string? ActiveAgencyDisplayName { get; private set; }

    /// <summary>Latest profile from the API.</summary>
    public MeProfileResponse? Profile { get; private set; }

    /// <summary>Raised when the active agency or profile changes.</summary>
    public event Action? Changed;

    /// <summary>Stores the profile and resolves default agency selection.</summary>
    public void SetProfile(MeProfileResponse profile)
    {
        Profile = profile;
        if (ActiveAgencyId is null)
        {
            ApplyDefaultAgency(profile);
        }

        Changed?.Invoke();
    }

    /// <summary>Sets the active agency after server confirmation.</summary>
    public void SetActiveAgency(Guid agencyId, string? displayName)
    {
        ActiveAgencyId = agencyId;
        ActiveAgencyDisplayName = displayName;
        Changed?.Invoke();
    }

    /// <summary>Clears agency context on sign-out.</summary>
    public void Clear()
    {
        Profile = null;
        ActiveAgencyId = null;
        ActiveAgencyDisplayName = null;
        Changed?.Invoke();
    }

    /// <summary>Whether the user has at least one active agency membership.</summary>
    public bool HasActiveMembership =>
        Profile?.Memberships.Any(m => string.Equals(m.Status, "Active", StringComparison.Ordinal)) == true;

    /// <summary>Whether the user has at least one suspended agency membership.</summary>
    public bool HasSuspendedMembership =>
        Profile?.Memberships.Any(m => string.Equals(m.Status, "Suspended", StringComparison.Ordinal)) == true;

    /// <summary>Whether the user has pending invitations.</summary>
    public bool HasPendingInvites => Profile?.PendingInvites.Count > 0;

    /// <summary>Whether the user can use the operator shell (assignments exist).</summary>
    public bool HasOperatorAssignments => Profile?.OperatorAssignments.Count > 0;

    /// <summary>Whether invite-only routing applies (no active membership, no operator path).</summary>
    public bool RequiresInviteOnlyGate =>
        HasPendingInvites && !HasActiveMembership && Profile?.IsPlatformOperator != true;

    /// <summary>Whether routine shell work has a valid active agency (FR-002, FR-004).</summary>
    public bool HasValidRoutineShellContext =>
        ActiveAgencyId is not null && IsRoutineShellAgency(ActiveAgencyId.Value);

    /// <summary>Whether the agency notification bell may appear (FR-002, FR-010).</summary>
    public bool CanShowAgencyNotificationBell =>
        HasValidRoutineShellContext && ActiveAgencyMemberRole is not null;

    /// <summary>Whether the user may mark agency notifications read (FR-010).</summary>
    public bool CanMarkAgencyNotificationsRead =>
        ActiveAgencyMemberRole is "StandardMember" or "Administrator";

    /// <summary>Whether the user may enter routine shell via membership (active membership in an active agency).</summary>
    public bool HasSelectableRoutineMemberAgency =>
        Profile?.Memberships.Any(IsRoutineMemberAgency) == true;

    /// <summary>UK English message when active memberships exist but none qualify for routine shell.</summary>
    public string? RoutineShellBlockedMessage
    {
        get
        {
            if (Profile is null || !HasActiveMembership || HasSelectableRoutineMemberAgency)
            {
                return null;
            }

            var activeMemberships = Profile.Memberships
                .Where(m => string.Equals(m.Status, "Active", StringComparison.Ordinal))
                .ToList();

            if (activeMemberships.All(m =>
                    string.Equals(m.AgencyLifecycleStatus, "Archived", StringComparison.Ordinal)))
            {
                return AgencyAccessRules.AgencyArchivedMemberMessage;
            }

            if (activeMemberships.Any(m =>
                    string.Equals(m.AgencyLifecycleStatus, "Suspended", StringComparison.Ordinal)))
            {
                return AgencyAccessRules.AgencySuspendedMemberMessage;
            }

            return AgencyAccessRules.AgencySuspendedMemberMessage;
        }
    }

    /// <summary>Whether the platform operator is assigned to the active agency.</summary>
    public bool IsOperatorAssignedToActiveAgency =>
        ActiveAgencyId is Guid agencyId
        && Profile?.IsPlatformOperator == true
        && Profile.OperatorAssignments.Any(a =>
            a.AgencyId == agencyId && IsOperatorSelectableAgency(a));

    /// <summary>Agency role for the user's active membership on the selected agency, if any.</summary>
    public string? ActiveAgencyMemberRole
    {
        get
        {
            if (ActiveAgencyId is not Guid agencyId || Profile is null)
            {
                return null;
            }

            var membership = Profile.Memberships.FirstOrDefault(m => m.AgencyId == agencyId);
            if (membership is null
                || !string.Equals(membership.Status, "Active", StringComparison.Ordinal))
            {
                return null;
            }

            return membership.Role;
        }
    }

    /// <summary>Whether the user may view the agency member roster (US3).</summary>
    public bool CanViewMemberRoster =>
        IsOperatorAssignedToActiveAgency
        || ActiveAgencyMemberRole is "Administrator" or "StandardMember";

    /// <summary>Whether the user may invite, provision, or change memberships (US3).</summary>
    public bool CanManageMembers =>
        IsOperatorAssignedToActiveAgency
        || string.Equals(ActiveAgencyMemberRole, "Administrator", StringComparison.Ordinal);

    /// <summary>Lifecycle status of the selected agency from profile data.</summary>
    public string? ActiveAgencyLifecycleStatus
    {
        get
        {
            if (ActiveAgencyId is not Guid agencyId || Profile is null)
            {
                return null;
            }

            var membership = Profile.Memberships.FirstOrDefault(m => m.AgencyId == agencyId);
            if (membership is not null)
            {
                return membership.AgencyLifecycleStatus;
            }

            return Profile.OperatorAssignments.FirstOrDefault(a => a.AgencyId == agencyId)?.AgencyLifecycleStatus;
        }
    }

    /// <summary>Whether the user may view and update agency settings (FR-015).</summary>
    public bool CanManageAgencySettings =>
        IsOperatorAssignedToActiveAgency
        || (string.Equals(ActiveAgencyMemberRole, "Administrator", StringComparison.Ordinal)
            && string.Equals(ActiveAgencyLifecycleStatus, "Active", StringComparison.Ordinal));

    /// <summary>Whether the user may view agency-scoped audit history (FR-009, US4).</summary>
    public bool CanViewAgencyAudit =>
        HasValidRoutineShellContext
        && string.Equals(ActiveAgencyMemberRole, "Administrator", StringComparison.Ordinal)
        && string.Equals(ActiveAgencyLifecycleStatus, "Active", StringComparison.Ordinal);

    /// <summary>Whether the agency id is valid for routine member shell work.</summary>
    public bool IsRoutineShellAgency(Guid agencyId)
    {
        if (Profile is null)
        {
            return false;
        }

        var membership = Profile.Memberships.FirstOrDefault(m => m.AgencyId == agencyId);
        if (membership is not null && IsRoutineMemberAgency(membership))
        {
            return true;
        }

        if (Profile.IsPlatformOperator)
        {
            var assignment = Profile.OperatorAssignments.FirstOrDefault(a => a.AgencyId == agencyId);
            return assignment is not null && IsOperatorSelectableAgency(assignment);
        }

        return false;
    }

    private void ApplyDefaultAgency(MeProfileResponse profile)
    {
        var candidates = profile.Memberships.Where(IsRoutineMemberAgency).ToList();

        if (candidates.Count > 0)
        {
            var selected = profile.LastUsedAgencyId is Guid lastUsed
                && candidates.Any(c => c.AgencyId == lastUsed)
                ? candidates.First(c => c.AgencyId == lastUsed)
                : candidates[0];

            ActiveAgencyId = selected.AgencyId;
            ActiveAgencyDisplayName = selected.DisplayName;
            return;
        }

        var operatorAgencies = profile.OperatorAssignments.Where(IsOperatorSelectableAgency).ToList();
        if (operatorAgencies.Count == 0)
        {
            return;
        }

        var operatorAgency = profile.LastUsedAgencyId is Guid operatorLastUsed
            && operatorAgencies.Any(a => a.AgencyId == operatorLastUsed)
            ? operatorAgencies.First(a => a.AgencyId == operatorLastUsed)
            : operatorAgencies[0];

        ActiveAgencyId = operatorAgency.AgencyId;
        ActiveAgencyDisplayName = operatorAgency.DisplayName;
    }

    private static bool IsRoutineMemberAgency(MeMembershipSummary membership) =>
        string.Equals(membership.Status, "Active", StringComparison.Ordinal)
        && string.Equals(membership.AgencyLifecycleStatus, "Active", StringComparison.Ordinal);

    private static bool IsOperatorSelectableAgency(MeOperatorAssignmentSummary assignment) =>
        !string.Equals(assignment.AgencyLifecycleStatus, "Archived", StringComparison.Ordinal);
}
