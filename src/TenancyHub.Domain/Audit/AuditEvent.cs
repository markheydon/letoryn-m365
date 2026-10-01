namespace TenancyHub.Domain.Audit;

/// <summary>
/// Append-only audit record for operator and agency activity.
/// </summary>
public class AuditEvent
{
    /// <summary>Primary key.</summary>
    public Guid Id { get; set; }

    /// <summary>Agency scope; null for global operator-only events.</summary>
    public Guid? AgencyId { get; set; }

    /// <summary>When the event occurred (UTC).</summary>
    public DateTimeOffset OccurredAt { get; set; }

    /// <summary>Acting user; null for system actions.</summary>
    public Guid? ActorUserIdentityId { get; set; }

    /// <summary>Stable action code (for example <c>auth.sign_in_succeeded</c>).</summary>
    public string ActionType { get; set; } = string.Empty;

    /// <summary>Human-readable English summary.</summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>User affected by the action, if any.</summary>
    public Guid? TargetUserIdentityId { get; set; }

    /// <summary>Structured details as JSON; must not contain secrets.</summary>
    public string? PayloadJson { get; set; }
}
