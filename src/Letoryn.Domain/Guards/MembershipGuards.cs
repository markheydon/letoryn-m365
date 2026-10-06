using Letoryn.Domain.Memberships;

namespace Letoryn.Domain.Guards;

/// <summary>
/// Membership invariants (FR-007).
/// </summary>
public static class MembershipGuards
{
    /// <summary>
    /// Ensures removing or demoting an administrator would not leave the agency without an active administrator.
    /// </summary>
    /// <param name="activeAdministratorCount">Count of active administrators after the proposed change.</param>
    /// <exception cref="InvalidOperationException">Would remove the last active administrator.</exception>
    public static void EnsureAtLeastOneActiveAdministrator(int activeAdministratorCount)
    {
        if (activeAdministratorCount < 1)
        {
            throw new InvalidOperationException("An agency must retain at least one active administrator.");
        }
    }

    /// <summary>
    /// Returns whether a membership counts as an active administrator.
    /// </summary>
    public static bool IsActiveAdministrator(AgencyMembership membership) =>
        membership.Status == MembershipStatus.Active
        && membership.AgencyRole == AgencyRole.Administrator;
}
