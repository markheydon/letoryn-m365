namespace TenancyHub.Application.Abstractions.Operators;

/// <summary>Operator cross-agency diagnostics (FR-009).</summary>
public interface IOperatorDiagnosticsService
{
    /// <summary>Read-only agency summary for assigned operators.</summary>
    Task<OperatorAgencySummary?> GetSummaryAsync(
        Guid operatorUserIdentityId,
        Guid agencyId,
        CancellationToken cancellationToken = default);
}

/// <summary>Operator diagnostics summary.</summary>
public sealed record OperatorAgencySummary(
    Guid AgencyId,
    string DisplayName,
    string LifecycleStatus,
    string PrimaryContactEmail,
    string PrimaryContactPhone,
    DateTimeOffset LastLifecycleChangeAt,
    OperatorRoleCounts RoleCounts);

/// <summary>Membership counts by status for diagnostics.</summary>
public sealed record OperatorRoleCounts(
    int Invited,
    int Active,
    int Suspended,
    int Removed);
