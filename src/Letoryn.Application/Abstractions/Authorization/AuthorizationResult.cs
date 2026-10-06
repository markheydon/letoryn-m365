namespace Letoryn.Application.Abstractions.Authorization;

/// <summary>
/// Outcome of a tenancy or role authorization check (FR-012 seam; FR-013 safe failures).
/// </summary>
/// <remarks>
/// Callers MUST NOT expose internal tenancy details in <see cref="UserMessage"/> beyond what policy allows.
/// Use <see cref="NotFound"/> when existence of another agency's data must be hidden.
/// </remarks>
public sealed class AuthorizationResult
{
    private AuthorizationResult(bool isAuthorized, AuthorizationFailureKind? failureKind, string? userMessage)
    {
        IsAuthorized = isAuthorized;
        FailureKind = failureKind;
        UserMessage = userMessage;
    }

    /// <summary>
    /// Whether the requested operation is authorized.
    /// </summary>
    public bool IsAuthorized { get; }

    /// <summary>
    /// Failure classification when <see cref="IsAuthorized"/> is <see langword="false"/>.
    /// </summary>
    public AuthorizationFailureKind? FailureKind { get; }

    /// <summary>
    /// UK English message safe to return to the end user when authorization fails, when applicable.
    /// </summary>
    public string? UserMessage { get; }

    /// <summary>
    /// Creates a successful authorization outcome.
    /// </summary>
    public static AuthorizationResult Succeeded() => new(true, null, null);

    /// <summary>
    /// Creates a denied outcome with a safe user-facing message.
    /// </summary>
    /// <param name="userMessage">UK English explanation for the signed-in user.</param>
    /// <param name="failureKind">How to map the failure to HTTP semantics (FR-013).</param>
    public static AuthorizationResult Denied(string userMessage, AuthorizationFailureKind failureKind = AuthorizationFailureKind.Forbidden)
        => new(false, failureKind, userMessage);

    /// <summary>
    /// Creates a denied outcome that must map to a generic not-found response (FR-013).
    /// </summary>
    public static AuthorizationResult NotFound()
        => new(false, AuthorizationFailureKind.NotFound, null);
}
