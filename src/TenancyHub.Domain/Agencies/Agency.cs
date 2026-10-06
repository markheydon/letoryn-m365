namespace TenancyHub.Domain.Agencies;

/// <summary>
/// Agency tenant. <see cref="Id"/> is the canonical tenancy key.
/// </summary>
public class Agency
{
    /// <summary>Primary key and canonical tenant identity.</summary>
    public Guid Id { get; set; }

    /// <summary>Display name; duplicates are allowed across agencies.</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Primary contact email for the agency.</summary>
    public string PrimaryContactEmail { get; set; } = string.Empty;

    /// <summary>Primary contact phone for the agency.</summary>
    public string PrimaryContactPhone { get; set; } = string.Empty;

    /// <summary>Current lifecycle state.</summary>
    public AgencyLifecycleStatus LifecycleStatus { get; set; }

    /// <summary>When the agency record was created (UTC).</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>When the agency record was last updated (UTC).</summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>When lifecycle status last changed, for operator diagnostics.</summary>
    public DateTimeOffset LastLifecycleChangeAt { get; set; }
}
