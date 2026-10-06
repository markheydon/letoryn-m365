namespace Letoryn.Application.Me;

/// <summary>Response for GET /api/v1/me.</summary>
public sealed record MeProfileResponse(
    Guid UserId,
    string Email,
    bool IsPlatformOperator,
    Guid? LastUsedAgencyId,
    IReadOnlyList<MeMembershipSummary> Memberships,
    IReadOnlyList<MeOperatorAssignmentSummary> OperatorAssignments,
    IReadOnlyList<MePendingInviteSummary> PendingInvites);

/// <summary>Agency membership summary for the signed-in user.</summary>
public sealed record MeMembershipSummary(
    Guid AgencyId,
    string DisplayName,
    string Status,
    string Role,
    string AgencyLifecycleStatus);

/// <summary>Platform operator assignment summary.</summary>
public sealed record MeOperatorAssignmentSummary(
    Guid AgencyId,
    string DisplayName,
    string AgencyLifecycleStatus);

/// <summary>Pending invitation visible to the invitee.</summary>
public sealed record MePendingInviteSummary(Guid MembershipId, Guid AgencyId, string DisplayName, string Role);

/// <summary>Request body for PUT /api/v1/me/active-agency.</summary>
public sealed record SetActiveAgencyRequest(Guid AgencyId);
