namespace TenancyHub.Application.Abstractions.Sessions;

/// <summary>
/// Server-side session metadata for idle and absolute expiry (FR-001).
/// </summary>
public interface IUserSessionService
{
    /// <summary>Creates a new session row for the authenticated user.</summary>
    Task<UserSessionInfo> CreateSessionAsync(Guid userIdentityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates the session, updates idle activity, and returns failure when expired or ended.
    /// </summary>
    Task<UserSessionValidationResult> ValidateAndTouchAsync(
        Guid sessionId,
        Guid userIdentityId,
        CancellationToken cancellationToken = default);

    /// <summary>Ends a single session (per-session sign-out).</summary>
    Task<bool> EndSessionAsync(
        Guid sessionId,
        Guid userIdentityId,
        CancellationToken cancellationToken = default);
}

/// <summary>Validated session metadata exposed to HTTP layers.</summary>
public sealed record UserSessionInfo(Guid SessionId, DateTimeOffset StartedAt, DateTimeOffset LastActivityAt);

/// <summary>Outcome of session validation.</summary>
public sealed record UserSessionValidationResult(bool IsValid, UserSessionTerminationReason? Reason);

/// <summary>Why a session is no longer valid.</summary>
public enum UserSessionTerminationReason
{
    /// <summary>Session row not found or already ended.</summary>
    NotFound,

    /// <summary>30-minute idle window exceeded.</summary>
    IdleTimeout,

    /// <summary>12-hour absolute cap exceeded.</summary>
    AbsoluteTimeout,

    /// <summary>User signed out of this session explicitly.</summary>
    SignOut,
}
