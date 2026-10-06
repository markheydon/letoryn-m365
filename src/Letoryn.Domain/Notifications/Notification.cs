namespace Letoryn.Domain.Notifications;

/// <summary>
/// Agency-scoped in-app notification for a user.
/// </summary>
public class Notification
{
    /// <summary>Primary key.</summary>
    public Guid Id { get; set; }

    /// <summary>Agency tenant foreign key.</summary>
    public Guid AgencyId { get; set; }

    /// <summary>Recipient user identity.</summary>
    public Guid UserIdentityId { get; set; }

    /// <summary>When the notification was created (UTC).</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Category mapping to FR-010 trigger types.</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>Notification title in English.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Notification body in English.</summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>Whether the user has marked the notification read.</summary>
    public bool IsRead { get; set; }

    /// <summary>When the notification was marked read, if applicable.</summary>
    public DateTimeOffset? ReadAt { get; set; }
}
