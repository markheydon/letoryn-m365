using TenancyHub.ApiService.Infrastructure;
using TenancyHub.Application.Abstractions.Authorization;
using TenancyHub.Domain.Memberships;

namespace TenancyHub.ApiService.Endpoints;

/// <summary>
/// Agency and operator audit routes (US2 isolation surface; US4 expands query behaviour).
/// </summary>
public static class AuditEndpoints
{
    /// <summary>Maps audit routes used for tenant isolation validation and later US4 history.</summary>
    public static IEndpointRouteBuilder MapAuditEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var agencyGroup = endpoints.MapAgencyScopedApiGroup();
        agencyGroup.MapGet("/{agencyId:guid}/audit", GetAgencyAuditAsync);

        var operatorGroup = endpoints.MapOperatorAgencyApiGroup();
        operatorGroup.MapGet("/{agencyId:guid}/audit", GetOperatorAgencyAuditAsync);

        return endpoints;
    }

    private static IResult GetAgencyAuditAsync(
        Guid agencyId,
        IAgencyAuthorizationService authorization)
    {
        var roleCheck = authorization.AuthorizeAgencyRole([AgencyRole.Administrator]);
        if (!roleCheck.IsAuthorized)
        {
            return roleCheck.ToHttpResult();
        }

        return Results.Ok(new AuditListResponse([], null));
    }

    private static IResult GetOperatorAgencyAuditAsync(Guid agencyId) =>
        Results.Ok(new AuditListResponse([], null));

    private sealed record AuditListResponse(
        IReadOnlyList<AuditItemResponse> Items,
        string? NextCursor);

    private sealed record AuditItemResponse(
        DateTimeOffset OccurredAt,
        string ActorEmail,
        string ActionType,
        string Summary);
}
