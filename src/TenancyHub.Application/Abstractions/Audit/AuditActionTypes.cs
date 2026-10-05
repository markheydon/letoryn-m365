namespace TenancyHub.Application.Abstractions.Audit;

/// <summary>Stable audit action codes for platform foundation (FR-008).</summary>
public static class AuditActionTypes
{
    /// <summary>Successful interactive or token-validated sign-in.</summary>
    public const string SignInSucceeded = "auth.sign_in_succeeded";

    /// <summary>Failed sign-in or token validation.</summary>
    public const string SignInFailed = "auth.sign_in_failed";

    /// <summary>Session ended due to idle, absolute cap, validation failure, or sign-out.</summary>
    public const string SessionTerminated = "auth.session_terminated";
}
