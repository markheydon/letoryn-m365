namespace Letoryn.Domain.Agencies;

/// <summary>
/// Lifecycle state of an agency tenant (FR-004).
/// </summary>
public enum AgencyLifecycleStatus
{
    /// <summary>Normal operation; members and operators may use the agency per access rules.</summary>
    Active = 0,

    /// <summary>Temporarily disabled; routine member access is blocked until reactivated.</summary>
    Suspended = 1,

    /// <summary>Permanently retired; mutations and member access are not permitted.</summary>
    Archived = 2,
}
