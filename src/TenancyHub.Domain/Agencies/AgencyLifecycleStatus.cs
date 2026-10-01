namespace TenancyHub.Domain.Agencies;

/// <summary>
/// Lifecycle state of an agency tenant (FR-004).
/// </summary>
public enum AgencyLifecycleStatus
{
    Active = 0,
    Suspended = 1,
    Archived = 2,
}
