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
        var path = NormalizePath(currentPath);

        if (agencyContext.RequiresInviteOnlyGate)
        {
            if (!path.StartsWith("/invitations", StringComparison.OrdinalIgnoreCase))
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
            if (agencyContext.HasSuspendedMembership)
            {
                if (!path.StartsWith("/membership-suspended", StringComparison.OrdinalIgnoreCase))
                {
                    navigation.NavigateTo("/membership-suspended");
                }
            }
            else if (!path.StartsWith("/access-not-configured", StringComparison.OrdinalIgnoreCase))
            {
                navigation.NavigateTo("/access-not-configured");
            }

            return;
        }

        if (isOperator && !hasActiveMembership && !hasAssignments)
        {
            if (!path.StartsWith("/operator", StringComparison.OrdinalIgnoreCase))
            {
                navigation.NavigateTo("/operator");
            }

            return;
        }

        if (hasActiveMembership
            && !agencyContext.HasSelectableRoutineMemberAgency
            && !(isOperator && hasAssignments))
        {
            if (!path.StartsWith("/agency-access-blocked", StringComparison.OrdinalIgnoreCase))
            {
                navigation.NavigateTo("/agency-access-blocked");
            }

            return;
        }

        if (hasActiveMembership
            || (isOperator && hasAssignments))
        {
            if (!agencyContext.HasValidRoutineShellContext
                && !(isOperator && !hasActiveMembership && hasAssignments))
            {
                if (!path.StartsWith("/agency-access-blocked", StringComparison.OrdinalIgnoreCase))
                {
                    navigation.NavigateTo("/agency-access-blocked");
                }

                return;
            }
        }

        if (path is "/access-not-configured"
            or "/operator"
            or "/invitations"
            or "/membership-suspended"
            or "/agency-access-blocked")
        {
            navigation.NavigateTo("/");
        }
    }

    private static string NormalizePath(string currentPath)
    {
        if (string.IsNullOrWhiteSpace(currentPath) || currentPath == "/")
        {
            return "/";
        }

        return currentPath.StartsWith('/') ? currentPath : $"/{currentPath}";
    }
}
