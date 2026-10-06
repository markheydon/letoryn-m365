namespace Letoryn.Web.Services;

/// <summary>Membership counts by agency role for operator diagnostics (FR-009).</summary>
public sealed record OperatorAgencyRoleCountsDto(
    int Administrator,
    int StandardMember,
    int ReadOnlyMember,
    int Invited);

/// <summary>Read-only agency summary from GET /api/v1/operator/agencies/{agencyId}/summary.</summary>
public sealed record OperatorAgencySummaryDto(
    Guid AgencyId,
    string DisplayName,
    string LifecycleStatus,
    string PrimaryContactEmail,
    string? PrimaryContactPhone,
    OperatorAgencyRoleCountsDto RoleCounts,
    DateTimeOffset LastLifecycleChangeAt);

/// <summary>Outcome of loading operator agency summary.</summary>
public enum OperatorAgencySummaryLoadAccess
{
    Success,
    Denied,
    Failed,
}

/// <summary>Result of GET operator agency summary.</summary>
public sealed record OperatorAgencySummaryLoadResult(
    OperatorAgencySummaryLoadAccess Access,
    OperatorAgencySummaryDto? Summary)
{
    /// <summary>Successful summary load.</summary>
    public static OperatorAgencySummaryLoadResult Success(OperatorAgencySummaryDto summary) =>
        new(OperatorAgencySummaryLoadAccess.Success, summary);

    /// <summary>Caller is not permitted or agency is not assigned.</summary>
    public static OperatorAgencySummaryLoadResult Denied() =>
        new(OperatorAgencySummaryLoadAccess.Denied, null);

    /// <summary>Request failed for a non-authorisation reason.</summary>
    public static OperatorAgencySummaryLoadResult Failed() =>
        new(OperatorAgencySummaryLoadAccess.Failed, null);
}
