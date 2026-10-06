namespace TenancyHub.Web.Services;

/// <summary>Agency settings returned from GET /api/v1/agencies/{agencyId}/settings.</summary>
public sealed record AgencySettingsApiDto(
    string DisplayName,
    string PrimaryContactEmail,
    string PrimaryContactPhone,
    string LifecycleStatus);

/// <summary>Request body for PATCH agency settings.</summary>
public sealed record UpdateAgencySettingsApiRequest(
    string DisplayName,
    string PrimaryContactEmail,
    string? PrimaryContactPhone);

/// <summary>Outcome of loading agency settings.</summary>
public enum AgencySettingsLoadAccess
{
    Success,
    Denied,
    Failed,
}

/// <summary>Result of GET agency settings.</summary>
public sealed record AgencySettingsLoadResult(
    AgencySettingsLoadAccess Access,
    AgencySettingsApiDto? Settings)
{
    /// <summary>Successful settings load.</summary>
    public static AgencySettingsLoadResult Success(AgencySettingsApiDto settings) =>
        new(AgencySettingsLoadAccess.Success, settings);

    /// <summary>Caller is not permitted to view settings.</summary>
    public static AgencySettingsLoadResult Denied() =>
        new(AgencySettingsLoadAccess.Denied, null);

    /// <summary>Request failed for a non-authorization reason.</summary>
    public static AgencySettingsLoadResult Failed() =>
        new(AgencySettingsLoadAccess.Failed, null);
}
