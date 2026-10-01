using TenancyHub.Domain.Agencies;

namespace TenancyHub.Domain.Guards;

/// <summary>
/// Domain rules for agency lifecycle mutations (FR-004, FR-007).
/// </summary>
public static class AgencyLifecycleGuards
{
    /// <summary>
    /// Throws when mutations are not permitted on an archived agency.
    /// </summary>
    /// <param name="agency">Agency being mutated.</param>
    /// <exception cref="InvalidOperationException">Agency is archived.</exception>
    public static void EnsureNotArchived(Agency agency)
    {
        ArgumentNullException.ThrowIfNull(agency);
        if (agency.LifecycleStatus == AgencyLifecycleStatus.Archived)
        {
            throw new InvalidOperationException("Archived agencies cannot be modified.");
        }
    }
}
