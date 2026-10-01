namespace TenancyHub.Domain.Memberships;

/// <summary>
/// Exactly one agency role per membership (FR-005, FR-006).
/// </summary>
public enum AgencyRole
{
    Administrator = 0,
    StandardMember = 1,
    ReadOnlyMember = 2,
}
