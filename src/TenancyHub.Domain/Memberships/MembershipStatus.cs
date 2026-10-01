namespace TenancyHub.Domain.Memberships;

/// <summary>
/// Status of a user's membership in an agency (FR-007).
/// </summary>
public enum MembershipStatus
{
    Invited = 0,
    Active = 1,
    Suspended = 2,
    Removed = 3,
}
