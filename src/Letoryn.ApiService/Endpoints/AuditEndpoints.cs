using Letoryn.ApiService.Infrastructure;
using Letoryn.Application.Abstractions.Audit;
using Letoryn.Application.Abstractions.Authorization;
using Letoryn.Application.Abstractions.Tenancy;
using Letoryn.Domain.Memberships;

namespace Letoryn.ApiService.Endpoints;

/// <summary>Agency and operator audit history routes (FR-009).</summary>
public static class AuditEndpoints
{
    /// <summary>Maps audit routes.</summary>
    public static IEndpointRouteBuilder MapAuditEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var agencyGroup = endpoints.MapAgencyScopedApiGroup();
        agencyGroup.MapGet("/{agencyId:guid}/audit", GetAgencyAuditAsync);

        var operatorGroup = endpoints.MapOperatorAgencyApiGroup();
        operatorGroup.MapGet("/{agencyId:guid}/audit", GetOperatorAgencyAuditAsync);

        return endpoints;
    }

    private static async Task<IResult> GetAgencyAuditAsync(
        Guid agencyId,
        HttpRequest request,
        ICurrentUser currentUser,
        IAgencyAuthorizationService authorization,
        IAuditQueryService auditQuery,
        CancellationToken cancellationToken)
    {
        var roleCheck = authorization.AuthorizeAgencyRole([AgencyRole.Administrator]);
        if (!roleCheck.IsAuthorized)
        {
            return roleCheck.ToHttpResult();
        }

        var limit = ParseLimit(request);
        var membershipCheck = authorization.AuthorizeMembershipStatus(agencyId, MembershipStatus.Active);
        if (!membershipCheck.IsAuthorized)
        {
            return membershipCheck.ToHttpResult();
        }

        var page = await auditQuery.GetAgencyAuditForAdministratorAsync(
            agencyId,
            currentUser.UserIdentityId,
            request.Query["cursor"].FirstOrDefault(),
            limit,
            cancellationToken);

        return Results.Ok(new AuditListResponse(page.Items, page.NextCursor));
    }

    private static async Task<IResult> GetOperatorAgencyAuditAsync(
        Guid agencyId,
        HttpRequest request,
        ICurrentUser currentUser,
        IAuditQueryService auditQuery,
        CancellationToken cancellationToken)
    {
        var limit = ParseLimit(request);
        var page = await auditQuery.GetAgencyAuditForOperatorAsync(
            agencyId,
            currentUser.UserIdentityId,
            request.Query["cursor"].FirstOrDefault(),
            limit,
            cancellationToken);

        return Results.Ok(new AuditListResponse(page.Items, page.NextCursor));
    }

    private static int ParseLimit(HttpRequest request)
    {
        return int.TryParse(request.Query["limit"].FirstOrDefault(), out var limit) ? limit : 50;
    }

    private sealed record AuditListResponse(
        IReadOnlyList<AuditListItem> Items,
        string? NextCursor);
}
