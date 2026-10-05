using TenancyHub.Application.Me;

namespace TenancyHub.Web.Services;

/// <summary>
/// Active agency context for the signed-in Blazor session (FR-002).
/// </summary>
public sealed class AgencyContextState
{
    /// <summary>Currently selected agency id for API calls.</summary>
    public Guid? ActiveAgencyId { get; private set; }

    /// <summary>Display name for chrome.</summary>
    public string? ActiveAgencyDisplayName { get; private set; }

    /// <summary>Latest profile from the API.</summary>
    public MeProfileResponse? Profile { get; private set; }

    /// <summary>Raised when the active agency or profile changes.</summary>
    public event Action? Changed;

    /// <summary>Stores the profile and resolves default agency selection.</summary>
    public void SetProfile(MeProfileResponse profile)
    {
        Profile = profile;
        if (ActiveAgencyId is null)
        {
            ApplyDefaultAgency(profile);
        }

        Changed?.Invoke();
    }

    /// <summary>Sets the active agency after server confirmation.</summary>
    public void SetActiveAgency(Guid agencyId, string? displayName)
    {
        ActiveAgencyId = agencyId;
        ActiveAgencyDisplayName = displayName;
        Changed?.Invoke();
    }

    /// <summary>Clears agency context on sign-out.</summary>
    public void Clear()
    {
        Profile = null;
        ActiveAgencyId = null;
        ActiveAgencyDisplayName = null;
        Changed?.Invoke();
    }

    /// <summary>Whether the user has at least one active agency membership.</summary>
    public bool HasActiveMembership =>
        Profile?.Memberships.Any(m => string.Equals(m.Status, "Active", StringComparison.Ordinal)) == true;

    /// <summary>Whether the user has pending invitations.</summary>
    public bool HasPendingInvites => Profile?.PendingInvites.Count > 0;

    /// <summary>Whether the user can use the operator shell (assignments exist).</summary>
    public bool HasOperatorAssignments => Profile?.OperatorAssignments.Count > 0;

    /// <summary>Whether invite-only routing applies (no active membership, no operator path).</summary>
    public bool RequiresInviteOnlyGate =>
        HasPendingInvites && !HasActiveMembership && Profile?.IsPlatformOperator != true;

    private void ApplyDefaultAgency(MeProfileResponse profile)
    {
        var candidates = profile.Memberships
            .Where(m => string.Equals(m.Status, "Active", StringComparison.Ordinal))
            .ToList();

        if (candidates.Count > 0)
        {
            var selected = profile.LastUsedAgencyId is Guid lastUsed
                && candidates.Any(c => c.AgencyId == lastUsed)
                ? candidates.First(c => c.AgencyId == lastUsed)
                : candidates[0];

            ActiveAgencyId = selected.AgencyId;
            ActiveAgencyDisplayName = selected.DisplayName;
            return;
        }

        var operatorAgencies = profile.OperatorAssignments;
        if (operatorAgencies.Count == 0)
        {
            return;
        }

        var operatorAgency = profile.LastUsedAgencyId is Guid operatorLastUsed
            && operatorAgencies.Any(a => a.AgencyId == operatorLastUsed)
            ? operatorAgencies.First(a => a.AgencyId == operatorLastUsed)
            : operatorAgencies[0];

        ActiveAgencyId = operatorAgency.AgencyId;
        ActiveAgencyDisplayName = operatorAgency.DisplayName;
    }
}
