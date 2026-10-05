using Microsoft.AspNetCore.Components;
using TenancyHub.Application.Me;

namespace TenancyHub.Web.Services;

/// <summary>Post-sign-in routing for agency shell, invitations, and access gates (US1).</summary>
public sealed class ShellNavigationService(NavigationManager navigation, AgencyContextState agencyContext)
{
    /// <summary>Loads profile into state and navigates to the correct shell entry point.</summary>
    public void ApplyProfileAndRoute(MeProfileResponse profile, string currentPath)
    {
        agencyContext.SetProfile(profile);

        if (agencyContext.RequiresInviteOnlyGate)
        {
            if (!currentPath.StartsWith("/invitations", StringComparison.OrdinalIgnoreCase))
            {
                navigation.NavigateTo("/invitations");
            }

            return;
        }

        var hasActiveMembership = agencyContext.HasActiveMembership;
        var isOperator = profile.IsPlatformOperator;
        var hasAssignments = agencyContext.HasOperatorAssignments;

        if (!hasActiveMembership && !isOperator)
        {
            if (!currentPath.StartsWith("/access-not-configured", StringComparison.OrdinalIgnoreCase))
            {
                navigation.NavigateTo("/access-not-configured");
            }

            return;
        }

        if (isOperator && !hasActiveMembership && !hasAssignments)
        {
            if (!currentPath.StartsWith("/operator", StringComparison.OrdinalIgnoreCase))
            {
                navigation.NavigateTo("/operator");
            }

            return;
        }

        if (currentPath is "/access-not-configured" or "/operator" or "/invitations")
        {
            navigation.NavigateTo("/");
        }
    }
}
