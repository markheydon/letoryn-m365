namespace TenancyHub.Application.Abstractions.Authorization;

/// <summary>
/// Classification of a failed authorization check for mapping to safe HTTP responses (FR-013).
/// </summary>
public enum AuthorizationFailureKind
{
    /// <summary>
    /// Caller is authenticated but not permitted; respond with a generic forbidden result.
    /// </summary>
    Forbidden = 0,

    /// <summary>
    /// Resource must not be disclosed; respond with a generic not-found result (FR-013).
    /// </summary>
    NotFound = 1,

    /// <summary>
    /// No active agency context where one is required (FR-002).
    /// </summary>
    NoActiveAgency = 2,

    /// <summary>
    /// Agency lifecycle blocks member access (FR-004).
    /// </summary>
    AgencyAccessBlocked = 3,

    /// <summary>
    /// Membership status blocks agency access (FR-007).
    /// </summary>
    MembershipAccessBlocked = 4,
}
