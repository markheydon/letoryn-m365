namespace TenancyHub.Domain.Sessions;

/// <summary>
/// Per-session metadata for idle/absolute expiry and audit correlation. Cookie auth remains primary.
/// </summary>
public class UserSession
{
    /// <summary>Primary key.</summary>
    public Guid Id { get; set; }

    /// <summary>Authenticated user identity.</summary>
    public Guid UserIdentityId { get; set; }

    /// <summary>Initial sign-in time for the 12-hour absolute cap (UTC).</summary>
    public DateTimeOffset StartedAt { get; set; }

    /// <summary>Last activity time for idle timeout reset (UTC).</summary>
    public DateTimeOffset LastActivityAt { get; set; }

    /// <summary>Sign-out or expiry time, if the session has ended.</summary>
    public DateTimeOffset? EndedAt { get; set; }
}
