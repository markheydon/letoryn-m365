namespace Letoryn.Application.Abstractions.Audit;

/// <summary>Read-only audit history queries (FR-009).</summary>
public interface IAuditQueryService
{
    /// <summary>Lists agency-scoped audit events for administrators.</summary>
    Task<AuditPageResult> GetAgencyAuditForAdministratorAsync(
        Guid agencyId,
        Guid actorUserIdentityId,
        string? cursor,
        int limit,
        CancellationToken cancellationToken = default);

    /// <summary>Lists agency audit for assigned operators (any lifecycle).</summary>
    Task<AuditPageResult> GetAgencyAuditForOperatorAsync(
        Guid agencyId,
        Guid operatorUserIdentityId,
        string? cursor,
        int limit,
        CancellationToken cancellationToken = default);
}

/// <summary>Cursor-paginated audit page.</summary>
public sealed record AuditPageResult(
    IReadOnlyList<AuditListItem> Items,
    string? NextCursor);

/// <summary>Audit list row.</summary>
public sealed record AuditListItem(
    DateTimeOffset OccurredAt,
    string ActorEmail,
    string ActionType,
    string Summary);
