using Letoryn.Application.Me;
using Letoryn.Domain.Agencies;
using Letoryn.Domain.Memberships;

namespace Letoryn.Application.Navigation;

/// <summary>Stable identifier for a shell navigation entry (FR-011).</summary>
public enum ShellNavigationItemId
{
    Home = 0,
    Members = 1,
    AgencySettings = 2,
    Audit = 3,
    Invitations = 4,
    OperatorAgencies = 5,
    OperatorPlatformOperators = 6,
    OperatorAgencyDiagnostics = 7,
}

/// <summary>Logical grouping for shell navigation rendering.</summary>
public enum ShellNavigationGroup
{
    AgencyRoutine = 0,
    Invitations = 1,
    Operator = 2,
}

/// <summary>A permitted shell navigation target (hidden entirely when not returned).</summary>
public sealed record ShellNavigationItem(
    ShellNavigationItemId Id,
    string Href,
    string Label,
    ShellNavigationGroup Group);

/// <summary>Canonical relative routes for shell navigation (R1).</summary>
public static class ShellNavigationRoutes
{
    /// <summary>Agency home / dashboard.</summary>
    public const string Home = "/";

    /// <summary>Agency member roster.</summary>
    public const string Members = "/members";

    /// <summary>Agency settings for administrators and assigned operators.</summary>
    public const string AgencySettings = "/agency-settings";

    /// <summary>Agency administrator audit history.</summary>
    public const string Audit = "/audit";

    /// <summary>Pending invitation acceptance.</summary>
    public const string Invitations = "/invitations";

    /// <summary>Platform operator agency create and lifecycle.</summary>
    public const string OperatorAgencies = "/operator/agencies";

    /// <summary>Platform operator grant, revoke, and assignments.</summary>
    public const string OperatorPlatformOperators = "/operator/platform-operators";

    /// <summary>Operator diagnostics for an assigned agency (US4).</summary>
    public static string OperatorAgencyDiagnostics(Guid agencyId) =>
        $"/operator/agencies/{agencyId:D}/diagnostics";
}

/// <summary>
/// Signed-in shell inputs derived from <see cref="MeProfileResponse"/> and active agency selection (FR-011).
/// </summary>
public sealed record ShellNavigationContext
{
    /// <summary>Profile snapshot used to evaluate navigation.</summary>
    public required MeProfileResponse Profile { get; init; }

    /// <summary>Active agency id for routine shell work, if any.</summary>
    public Guid? ActiveAgencyId { get; init; }

    /// <summary>Whether invite-only routing applies (pending invites, no active membership, not an operator).</summary>
    public bool RequiresInviteOnlyGate { get; init; }

    /// <summary>Whether routine agency shell context is valid for the active agency.</summary>
    public bool HasValidRoutineShellContext { get; init; }

    /// <summary>Whether the user has at least one active agency membership.</summary>
    public bool HasActiveMembership { get; init; }

    /// <summary>Whether the user has pending invitations.</summary>
    public bool HasPendingInvites { get; init; }

    /// <summary>Whether the user has platform operator agency assignments.</summary>
    public bool HasOperatorAssignments { get; init; }

    /// <summary>Whether the user is a platform operator assigned to the active agency.</summary>
    public bool IsOperatorAssignedToActiveAgency { get; init; }

    /// <summary>Active membership role on the selected agency, when membership is active.</summary>
    public AgencyRole? ActiveAgencyRole { get; init; }

    /// <summary>Lifecycle status of the selected agency.</summary>
    public AgencyLifecycleStatus? ActiveAgencyLifecycleStatus { get; init; }

    /// <summary>Builds navigation context from GET /api/v1/me data and the current active agency.</summary>
    public static ShellNavigationContext FromMeProfile(MeProfileResponse profile, Guid? activeAgencyId)
    {
        var hasActiveMembership = profile.Memberships.Any(m =>
            ParseMembershipStatus(m.Status) == MembershipStatus.Active);

        var hasPendingInvites = profile.PendingInvites.Count > 0;
        var hasOperatorAssignments = profile.OperatorAssignments.Count > 0;

        var requiresInviteOnlyGate = hasPendingInvites
            && !hasActiveMembership
            && !profile.IsPlatformOperator;

        var isOperatorGlobalOnly = profile.IsPlatformOperator
            && !hasActiveMembership
            && !hasOperatorAssignments;

        var hasValidRoutineShellContext = activeAgencyId is Guid agencyId
            && IsRoutineShellAgency(profile, agencyId);

        AgencyRole? activeRole = null;
        AgencyLifecycleStatus? activeLifecycle = null;
        var isOperatorAssigned = false;

        if (activeAgencyId is Guid activeId)
        {
            activeLifecycle = ResolveAgencyLifecycleStatus(profile, activeId);
            isOperatorAssigned = profile.IsPlatformOperator
                && profile.OperatorAssignments.Any(a =>
                    a.AgencyId == activeId && IsOperatorSelectableAgency(a));

            var membership = profile.Memberships.FirstOrDefault(m => m.AgencyId == activeId);
            if (membership is not null
                && ParseMembershipStatus(membership.Status) == MembershipStatus.Active
                && Enum.TryParse<AgencyRole>(membership.Role, ignoreCase: true, out var role))
            {
                activeRole = role;
            }
        }

        return new ShellNavigationContext
        {
            Profile = profile,
            ActiveAgencyId = activeAgencyId,
            RequiresInviteOnlyGate = requiresInviteOnlyGate,
            HasValidRoutineShellContext = hasValidRoutineShellContext,
            HasActiveMembership = hasActiveMembership,
            HasPendingInvites = hasPendingInvites,
            HasOperatorAssignments = hasOperatorAssignments,
            IsOperatorAssignedToActiveAgency = isOperatorAssigned,
            ActiveAgencyRole = activeRole,
            ActiveAgencyLifecycleStatus = activeLifecycle,
            IsOperatorGlobalOnly = isOperatorGlobalOnly,
        };
    }

    /// <summary>Platform operator signed in with no memberships and no assignments (operator home flows).</summary>
    public bool IsOperatorGlobalOnly { get; init; }

    private static AgencyLifecycleStatus? ResolveAgencyLifecycleStatus(MeProfileResponse profile, Guid agencyId)
    {
        var membership = profile.Memberships.FirstOrDefault(m => m.AgencyId == agencyId);
        if (membership is not null
            && Enum.TryParse<AgencyLifecycleStatus>(membership.AgencyLifecycleStatus, ignoreCase: true, out var fromMembership))
        {
            return fromMembership;
        }

        var assignment = profile.OperatorAssignments.FirstOrDefault(a => a.AgencyId == agencyId);
        if (assignment is not null
            && Enum.TryParse<AgencyLifecycleStatus>(assignment.AgencyLifecycleStatus, ignoreCase: true, out var fromAssignment))
        {
            return fromAssignment;
        }

        return null;
    }

    private static bool IsRoutineShellAgency(MeProfileResponse profile, Guid agencyId)
    {
        var membership = profile.Memberships.FirstOrDefault(m => m.AgencyId == agencyId);
        if (membership is not null && IsRoutineMemberAgency(membership))
        {
            return true;
        }

        if (profile.IsPlatformOperator)
        {
            var assignment = profile.OperatorAssignments.FirstOrDefault(a => a.AgencyId == agencyId);
            return assignment is not null && IsOperatorSelectableAgency(assignment);
        }

        return false;
    }

    private static bool IsRoutineMemberAgency(MeMembershipSummary membership) =>
        ParseMembershipStatus(membership.Status) == MembershipStatus.Active
        && ParseAgencyLifecycle(membership.AgencyLifecycleStatus) == AgencyLifecycleStatus.Active;

    private static bool IsOperatorSelectableAgency(MeOperatorAssignmentSummary assignment) =>
        ParseAgencyLifecycle(assignment.AgencyLifecycleStatus) != AgencyLifecycleStatus.Archived;

    private static MembershipStatus? ParseMembershipStatus(string status) =>
        Enum.TryParse<MembershipStatus>(status, ignoreCase: true, out var parsed) ? parsed : null;

    private static AgencyLifecycleStatus? ParseAgencyLifecycle(string status) =>
        Enum.TryParse<AgencyLifecycleStatus>(status, ignoreCase: true, out var parsed) ? parsed : null;
}

/// <summary>Permission-to-navigation map for the signed-in shell (FR-011).</summary>
public static class ShellNavigationPolicy
{
    /// <summary>UK English labels for navigation entries.</summary>
    public static class Labels
    {
        /// <summary>Home navigation label.</summary>
        public const string Home = "Home";

        /// <summary>Members navigation label.</summary>
        public const string Members = "Members";

        /// <summary>Agency settings navigation label.</summary>
        public const string AgencySettings = "Agency settings";

        /// <summary>Audit history navigation label.</summary>
        public const string Audit = "Audit history";

        /// <summary>Invitations navigation label.</summary>
        public const string Invitations = "Invitations";

        /// <summary>Operator agencies navigation label.</summary>
        public const string OperatorAgencies = "Operator agencies";

        /// <summary>Platform operators navigation label.</summary>
        public const string OperatorPlatformOperators = "Platform operators";

        /// <summary>Operator diagnostics navigation label.</summary>
        public const string OperatorAgencyDiagnostics = "Agency diagnostics";
    }

    /// <summary>Returns navigation entries the caller may see; omits forbidden routes (no disabled tease).</summary>
    public static IReadOnlyList<ShellNavigationItem> GetPermittedItems(ShellNavigationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.RequiresInviteOnlyGate)
        {
            return [Create(ShellNavigationItemId.Invitations, ShellNavigationRoutes.Invitations, Labels.Invitations, ShellNavigationGroup.Invitations)];
        }

        if (context.IsOperatorGlobalOnly)
        {
            return GetOperatorGlobalItems();
        }

        var items = new List<ShellNavigationItem>();

        if (context.HasValidRoutineShellContext)
        {
            items.Add(Create(ShellNavigationItemId.Home, ShellNavigationRoutes.Home, Labels.Home, ShellNavigationGroup.AgencyRoutine));

            if (CanViewMemberRoster(context))
            {
                items.Add(Create(ShellNavigationItemId.Members, ShellNavigationRoutes.Members, Labels.Members, ShellNavigationGroup.AgencyRoutine));
            }

            if (CanManageAgencySettings(context))
            {
                items.Add(Create(
                    ShellNavigationItemId.AgencySettings,
                    ShellNavigationRoutes.AgencySettings,
                    Labels.AgencySettings,
                    ShellNavigationGroup.AgencyRoutine));
            }

            if (CanViewAgencyAudit(context))
            {
                items.Add(Create(ShellNavigationItemId.Audit, ShellNavigationRoutes.Audit, Labels.Audit, ShellNavigationGroup.AgencyRoutine));
            }
        }

        if (ShouldShowInvitationsInRoutineShell(context))
        {
            items.Add(Create(
                ShellNavigationItemId.Invitations,
                ShellNavigationRoutes.Invitations,
                Labels.Invitations,
                ShellNavigationGroup.Invitations));
        }

        if (context.Profile.IsPlatformOperator)
        {
            items.AddRange(GetOperatorItems(context));
        }

        return items;
    }

    /// <summary>Whether the member roster nav entry is permitted (US3).</summary>
    public static bool CanViewMemberRoster(ShellNavigationContext context) =>
        context.HasValidRoutineShellContext
        && (context.IsOperatorAssignedToActiveAgency
            || context.ActiveAgencyRole is AgencyRole.Administrator or AgencyRole.StandardMember);

    /// <summary>Whether agency settings nav is permitted (FR-015).</summary>
    public static bool CanManageAgencySettings(ShellNavigationContext context) =>
        context.HasValidRoutineShellContext
        && (context.IsOperatorAssignedToActiveAgency
            || (context.ActiveAgencyRole == AgencyRole.Administrator
                && context.ActiveAgencyLifecycleStatus == AgencyLifecycleStatus.Active));

    /// <summary>Whether agency administrator audit nav is permitted (FR-009).</summary>
    public static bool CanViewAgencyAudit(ShellNavigationContext context) =>
        context.HasValidRoutineShellContext
        && context.ActiveAgencyRole == AgencyRole.Administrator
        && context.ActiveAgencyLifecycleStatus == AgencyLifecycleStatus.Active;

    /// <summary>Whether operator diagnostics nav is permitted (FR-009 operator audit path).</summary>
    public static bool CanViewOperatorAgencyDiagnostics(ShellNavigationContext context) =>
        context.HasValidRoutineShellContext
        && context.IsOperatorAssignedToActiveAgency
        && context.ActiveAgencyId is not null;

    private static bool ShouldShowInvitationsInRoutineShell(ShellNavigationContext context) =>
        context.HasPendingInvites
        && context.HasValidRoutineShellContext
        && (context.HasActiveMembership || context.Profile.IsPlatformOperator);

    private static IReadOnlyList<ShellNavigationItem> GetOperatorGlobalItems() =>
    [
        Create(ShellNavigationItemId.OperatorAgencies, ShellNavigationRoutes.OperatorAgencies, Labels.OperatorAgencies, ShellNavigationGroup.Operator),
        Create(
            ShellNavigationItemId.OperatorPlatformOperators,
            ShellNavigationRoutes.OperatorPlatformOperators,
            Labels.OperatorPlatformOperators,
            ShellNavigationGroup.Operator),
    ];

    private static IEnumerable<ShellNavigationItem> GetOperatorItems(ShellNavigationContext context)
    {
        yield return Create(
            ShellNavigationItemId.OperatorAgencies,
            ShellNavigationRoutes.OperatorAgencies,
            Labels.OperatorAgencies,
            ShellNavigationGroup.Operator);
        yield return Create(
            ShellNavigationItemId.OperatorPlatformOperators,
            ShellNavigationRoutes.OperatorPlatformOperators,
            Labels.OperatorPlatformOperators,
            ShellNavigationGroup.Operator);

        if (CanViewOperatorAgencyDiagnostics(context) && context.ActiveAgencyId is Guid agencyId)
        {
            yield return Create(
                ShellNavigationItemId.OperatorAgencyDiagnostics,
                ShellNavigationRoutes.OperatorAgencyDiagnostics(agencyId),
                Labels.OperatorAgencyDiagnostics,
                ShellNavigationGroup.Operator);
        }
    }

    private static ShellNavigationItem Create(
        ShellNavigationItemId id,
        string href,
        string label,
        ShellNavigationGroup group) =>
        new(id, href, label, group);
}
