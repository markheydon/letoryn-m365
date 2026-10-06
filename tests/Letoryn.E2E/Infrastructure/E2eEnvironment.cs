namespace Letoryn.E2E.Infrastructure;

/// <summary>
/// Environment-driven configuration for Playwright journeys against a running Letoryn stack.
/// Discover the web URL with <c>aspire describe webfrontend --format Json</c> (see playwright-handoff in Aspire skills).
/// </summary>
internal static class E2eEnvironment
{
    /// <summary>Web frontend base URL (no trailing slash required).</summary>
    public const string BaseUrlVariable = "LETORYN_E2E_BASE_URL";

    /// <summary>Playwright storage state JSON for an agency member (R1-J3.2 / R1-J8).</summary>
    public const string MemberStorageStateVariable = "LETORYN_E2E_MEMBER_STORAGE_STATE";

    /// <summary>Playwright storage state JSON for a signed-in user with no membership (R1-J3.10).</summary>
    public const string NoAccessStorageStateVariable = "LETORYN_E2E_NO_ACCESS_STORAGE_STATE";

    /// <summary>apiservice base URL for API journey checks (R1-J3.3, R1-J4.0).</summary>
    public const string ApiBaseUrlVariable = "LETORYN_E2E_API_BASE_URL";

    /// <summary>Expected active agency display name in the shell (R1-J3.2 / R1-J3.4).</summary>
    public const string MemberAgencyNameVariable = "LETORYN_E2E_MEMBER_AGENCY_NAME";

    /// <summary>Second tenant member storage state (R1-J3.4).</summary>
    public const string SecondMemberStorageStateVariable = "LETORYN_E2E_SECOND_MEMBER_STORAGE_STATE";

    /// <summary>Second tenant agency display name (R1-J3.4).</summary>
    public const string SecondMemberAgencyNameVariable = "LETORYN_E2E_SECOND_MEMBER_AGENCY_NAME";

    /// <summary>Platform operator with no agency assignments (R1-J3.11, R1-J5.1).</summary>
    public const string OperatorStorageStateVariable = "LETORYN_E2E_OPERATOR_STORAGE_STATE";

    /// <summary>User with only invited memberships (R1-J3.7).</summary>
    public const string InviteOnlyStorageStateVariable = "LETORYN_E2E_INVITE_ONLY_STORAGE_STATE";

    /// <summary>User with active membership plus pending invite elsewhere (R1-J3.6).</summary>
    public const string ActiveAndInvitedStorageStateVariable = "LETORYN_E2E_ACTIVE_AND_INVITED_STORAGE_STATE";

    /// <summary>Agency administrator storage state (R1-J3.8, R1-J6).</summary>
    public const string AdministratorStorageStateVariable = "LETORYN_E2E_ADMINISTRATOR_STORAGE_STATE";

    /// <summary>Standard member storage state (R1-J7, R1-J5.3).</summary>
    public const string StandardMemberStorageStateVariable = "LETORYN_E2E_STANDARD_MEMBER_STORAGE_STATE";

    /// <summary>Read-only member storage state (R1-J5.3, R1-J6).</summary>
    public const string ReadOnlyMemberStorageStateVariable = "LETORYN_E2E_READ_ONLY_MEMBER_STORAGE_STATE";

    /// <summary>Member of a suspended agency (R1-J4.1).</summary>
    public const string SuspendedAgencyMemberStorageStateVariable = "LETORYN_E2E_SUSPENDED_AGENCY_MEMBER_STORAGE_STATE";

    public static string? BaseUrl =>
        NormalizeBaseUrl(Environment.GetEnvironmentVariable(BaseUrlVariable));

    public static string? ApiBaseUrl =>
        NormalizeBaseUrl(Environment.GetEnvironmentVariable(ApiBaseUrlVariable));

    public static string? MemberAgencyName =>
        NormalizeOptionalText(Environment.GetEnvironmentVariable(MemberAgencyNameVariable));

    public static string? SecondMemberAgencyName =>
        NormalizeOptionalText(Environment.GetEnvironmentVariable(SecondMemberAgencyNameVariable));

    public static string? MemberStorageStatePath =>
        NormalizeExistingFile(Environment.GetEnvironmentVariable(MemberStorageStateVariable));

    public static string? NoAccessStorageStatePath =>
        NormalizeExistingFile(Environment.GetEnvironmentVariable(NoAccessStorageStateVariable));

    public static string? SecondMemberStorageStatePath =>
        NormalizeExistingFile(Environment.GetEnvironmentVariable(SecondMemberStorageStateVariable));

    public static string? OperatorStorageStatePath =>
        NormalizeExistingFile(Environment.GetEnvironmentVariable(OperatorStorageStateVariable));

    public static string? InviteOnlyStorageStatePath =>
        NormalizeExistingFile(Environment.GetEnvironmentVariable(InviteOnlyStorageStateVariable));

    public static string? ActiveAndInvitedStorageStatePath =>
        NormalizeExistingFile(Environment.GetEnvironmentVariable(ActiveAndInvitedStorageStateVariable));

    public static string? AdministratorStorageStatePath =>
        NormalizeExistingFile(Environment.GetEnvironmentVariable(AdministratorStorageStateVariable));

    public static string? StandardMemberStorageStatePath =>
        NormalizeExistingFile(Environment.GetEnvironmentVariable(StandardMemberStorageStateVariable));

    public static string? ReadOnlyMemberStorageStatePath =>
        NormalizeExistingFile(Environment.GetEnvironmentVariable(ReadOnlyMemberStorageStateVariable));

    public static string? SuspendedAgencyMemberStorageStatePath =>
        NormalizeExistingFile(Environment.GetEnvironmentVariable(SuspendedAgencyMemberStorageStateVariable));

    private static string? NormalizeBaseUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim().TrimEnd('/');
    }

    private static string? NormalizeExistingFile(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        var fullPath = Path.GetFullPath(path.Trim());
        return File.Exists(fullPath) ? fullPath : null;
    }

    private static string? NormalizeOptionalText(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
