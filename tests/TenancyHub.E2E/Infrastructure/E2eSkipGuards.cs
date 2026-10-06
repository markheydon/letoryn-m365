namespace TenancyHub.E2E.Infrastructure;

internal static class E2eSkipGuards
{
    public static string RequireBaseUrl()
    {
        var baseUrl = E2eEnvironment.BaseUrl;
        if (baseUrl is null)
        {
            Assert.Skip(
                $"Set {E2eEnvironment.BaseUrlVariable} to the webfrontend URL "
                + "(for example from `aspire describe webfrontend --format Json`).");
        }

        return baseUrl;
    }

    public static string RequireApiBaseUrl()
    {
        var apiBaseUrl = E2eEnvironment.ApiBaseUrl;
        if (apiBaseUrl is null)
        {
            Assert.Skip(
                $"Set {E2eEnvironment.ApiBaseUrlVariable} to the apiservice URL "
                + "(for example from `aspire describe apiservice --format Json`).");
        }

        return apiBaseUrl;
    }

    public static string RequireMemberStorageState()
    {
        var path = E2eEnvironment.MemberStorageStatePath;
        if (path is null)
        {
            Assert.Skip(
                $"Set {E2eEnvironment.MemberStorageStateVariable} to a Playwright storage state file "
                + "for an active agency member (record after Entra sign-in).");
        }

        return path;
    }

    public static string RequireNoAccessStorageState()
    {
        var path = E2eEnvironment.NoAccessStorageStatePath;
        if (path is null)
        {
            Assert.Skip(
                $"Set {E2eEnvironment.NoAccessStorageStateVariable} to a Playwright storage state file "
                + "for a valid Entra user with no Tenancy Hub membership.");
        }

        return path;
    }

    public static string RequireSecondMemberStorageState()
    {
        var path = E2eEnvironment.SecondMemberStorageStatePath;
        if (path is null)
        {
            Assert.Skip(
                $"Set {E2eEnvironment.SecondMemberStorageStateVariable} to a Playwright storage state file "
                + "for a user in a second agency (tenant isolation).");
        }

        return path;
    }

    public static string RequireMemberAgencyName()
    {
        var name = E2eEnvironment.MemberAgencyName;
        if (name is null)
        {
            Assert.Skip(
                $"Set {E2eEnvironment.MemberAgencyNameVariable} to the agency display name shown in the shell.");
        }

        return name;
    }

    public static string RequireSecondMemberAgencyName()
    {
        var name = E2eEnvironment.SecondMemberAgencyName;
        if (name is null)
        {
            Assert.Skip(
                $"Set {E2eEnvironment.SecondMemberAgencyNameVariable} to the second agency display name.");
        }

        return name;
    }

    public static string RequireOperatorStorageState()
    {
        var path = E2eEnvironment.OperatorStorageStatePath;
        if (path is null)
        {
            Assert.Skip(
                $"Set {E2eEnvironment.OperatorStorageStateVariable} to a Playwright storage state file "
                + "for a platform operator account.");
        }

        return path;
    }

    public static string RequireInviteOnlyStorageState()
    {
        var path = E2eEnvironment.InviteOnlyStorageStatePath;
        if (path is null)
        {
            Assert.Skip(
                $"Set {E2eEnvironment.InviteOnlyStorageStateVariable} to a Playwright storage state file "
                + "for a user with only invited memberships.");
        }

        return path;
    }

    public static string RequireActiveAndInvitedStorageState()
    {
        var path = E2eEnvironment.ActiveAndInvitedStorageStatePath;
        if (path is null)
        {
            Assert.Skip(
                $"Set {E2eEnvironment.ActiveAndInvitedStorageStateVariable} to a Playwright storage state file "
                + "for a user with active membership and a pending invite elsewhere.");
        }

        return path;
    }

    public static string RequireAdministratorStorageState()
    {
        var path = E2eEnvironment.AdministratorStorageStatePath;
        if (path is null)
        {
            Assert.Skip(
                $"Set {E2eEnvironment.AdministratorStorageStateVariable} to a Playwright storage state file "
                + "for an agency administrator.");
        }

        return path;
    }

    public static string RequireStandardMemberStorageState()
    {
        var path = E2eEnvironment.StandardMemberStorageStatePath;
        if (path is null)
        {
            Assert.Skip(
                $"Set {E2eEnvironment.StandardMemberStorageStateVariable} to a Playwright storage state file "
                + "for a standard agency member.");
        }

        return path;
    }

    public static string RequireReadOnlyMemberStorageState()
    {
        var path = E2eEnvironment.ReadOnlyMemberStorageStatePath;
        if (path is null)
        {
            Assert.Skip(
                $"Set {E2eEnvironment.ReadOnlyMemberStorageStateVariable} to a Playwright storage state file "
                + "for a read-only agency member.");
        }

        return path;
    }

    public static string RequireSuspendedAgencyMemberStorageState()
    {
        var path = E2eEnvironment.SuspendedAgencyMemberStorageStatePath;
        if (path is null)
        {
            Assert.Skip(
                $"Set {E2eEnvironment.SuspendedAgencyMemberStorageStateVariable} to a Playwright storage state file "
                + "for a member of a suspended agency.");
        }

        return path;
    }
}
