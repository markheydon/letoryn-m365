namespace TenancyHub.Application.Abstractions.Audit;

/// <summary>
/// Immutable audit event payload for append-only persistence (FR-008, FR-012).
/// </summary>
/// <param name="AgencyId">Agency scope; <see langword="null"/> for global or pre-context events such as sign-in.</param>
/// <param name="ActorUserIdentityId">Acting user; <see langword="null"/> for system-generated rows.</param>
/// <param name="ActionType">Stable action code (for example <c>auth.sign_in_succeeded</c>).</param>
/// <param name="Summary">Human-readable English summary.</param>
/// <param name="TargetUserIdentityId">Optional subject user for membership or role changes.</param>
/// <param name="PayloadJson">Optional structured details without secrets.</param>
public sealed record AuditEventWrite(
    Guid? AgencyId,
    Guid? ActorUserIdentityId,
    string ActionType,
    string Summary,
    Guid? TargetUserIdentityId = null,
    string? PayloadJson = null);
