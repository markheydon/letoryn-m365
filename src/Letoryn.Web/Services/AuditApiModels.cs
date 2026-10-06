namespace Letoryn.Web.Services;

/// <summary>Single audit row from GET /api/v1/agencies/{agencyId}/audit.</summary>
public sealed record AgencyAuditItemDto(
    DateTimeOffset OccurredAt,
    string ActorEmail,
    string ActionType,
    string Summary);

/// <summary>Cursor-paginated audit list response.</summary>
public sealed record AgencyAuditListDto(
    IReadOnlyList<AgencyAuditItemDto> Items,
    string? NextCursor);

/// <summary>Outcome of loading agency audit history.</summary>
public enum AgencyAuditLoadAccess
{
    Success,
    Denied,
    Failed,
}

/// <summary>Result of GET agency audit.</summary>
public sealed record AgencyAuditLoadResult(
    AgencyAuditLoadAccess Access,
    AgencyAuditListDto? Page)
{
    /// <summary>Successful audit page load.</summary>
    public static AgencyAuditLoadResult Success(AgencyAuditListDto page) =>
        new(AgencyAuditLoadAccess.Success, page);

    /// <summary>Caller is not permitted to view audit history.</summary>
    public static AgencyAuditLoadResult Denied() =>
        new(AgencyAuditLoadAccess.Denied, null);

    /// <summary>Request failed for a non-authorization reason.</summary>
    public static AgencyAuditLoadResult Failed() =>
        new(AgencyAuditLoadAccess.Failed, null);
}
