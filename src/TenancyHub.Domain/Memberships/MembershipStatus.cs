namespace TenancyHub.Domain.Memberships;

/// <summary>
/// Status of a user's membership in an agency (FR-007).
/// </summary>
public enum MembershipStatus
{
    /// <summary>Invitation sent; not yet accepted by the invitee.</summary>
    Invited = 0,

    /// <summary>Member may use the agency according to role and agency lifecycle.</summary>
    Active = 1,

    /// <summary>Membership paused; access is blocked until reactivated.</summary>
    Suspended = 2,

    /// <summary>Membership ended; user no longer belongs to the agency.</summary>
    Removed = 3,
}
