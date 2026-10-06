namespace Letoryn.Application.Abstractions.Audit;

/// <summary>Stable audit action codes for platform foundation (FR-008).</summary>
public static class AuditActionTypes
{
    /// <summary>Successful interactive or token-validated sign-in.</summary>
    public const string SignInSucceeded = "auth.sign_in_succeeded";

    /// <summary>Failed sign-in or token validation.</summary>
    public const string SignInFailed = "auth.sign_in_failed";

    /// <summary>Session ended due to idle, absolute cap, validation failure, or sign-out.</summary>
    public const string SessionTerminated = "auth.session_terminated";

    /// <summary>Agency created by a platform operator.</summary>
    public const string AgencyCreated = "agency.created";

    /// <summary>Agency lifecycle status changed.</summary>
    public const string AgencyLifecycleChanged = "agency.lifecycle_changed";

    /// <summary>Agency settings updated.</summary>
    public const string AgencySettingsChanged = "agency.settings_changed";

    /// <summary>Membership invited.</summary>
    public const string MembershipInvited = "membership.invited";

    /// <summary>Membership provisioned as active.</summary>
    public const string MembershipProvisioned = "membership.provisioned";

    /// <summary>Invitation accepted.</summary>
    public const string MembershipInviteAccepted = "membership.invite_accepted";

    /// <summary>Invitation declined.</summary>
    public const string MembershipInviteDeclined = "membership.invite_declined";

    /// <summary>Invitation revoked.</summary>
    public const string MembershipInviteRevoked = "membership.invite_revoked";

    /// <summary>Invitation accept or decline failed (identity mismatch).</summary>
    public const string MembershipInviteActionFailed = "membership.invite_action_failed";

    /// <summary>Invitation expired (lazy evaluation or background sweep).</summary>
    public const string MembershipInviteExpired = "membership.invite_expired";

    /// <summary>Membership role changed.</summary>
    public const string MembershipRoleChanged = "membership.role_changed";

    /// <summary>Membership suspended.</summary>
    public const string MembershipSuspended = "membership.suspended";

    /// <summary>Membership reactivated.</summary>
    public const string MembershipReactivated = "membership.reactivated";

    /// <summary>Membership removed.</summary>
    public const string MembershipRemoved = "membership.removed";

    /// <summary>Platform operator status granted.</summary>
    public const string PlatformOperatorGranted = "platform_operator.granted";

    /// <summary>Platform operator status revoked.</summary>
    public const string PlatformOperatorRevoked = "platform_operator.revoked";

    /// <summary>Platform operator agency assignments changed.</summary>
    public const string PlatformOperatorAssignmentsChanged = "platform_operator.assignments_changed";

    /// <summary>Operator viewed cross-agency diagnostics or audit.</summary>
    public const string OperatorCrossAgencyView = "operator.cross_agency_view";
}
