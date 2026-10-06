namespace TenancyHub.Domain.Memberships;

/// <summary>
/// Exactly one agency role per membership (FR-005, FR-006).
/// </summary>
public enum AgencyRole
{
    /// <summary>Full agency administration, including membership and settings.</summary>
    Administrator = 0,

    /// <summary>Day-to-day agency work without administrative privileges.</summary>
    StandardMember = 1,

    /// <summary>View-only access within the agency.</summary>
    ReadOnlyMember = 2,
}
