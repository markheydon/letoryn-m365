using Letoryn.ApiService.Infrastructure;
using Letoryn.Application.Abstractions.Notifications;
using Letoryn.Application.Abstractions.Tenancy;
using Letoryn.Domain.Memberships;

namespace Letoryn.ApiService.Endpoints;

/// <summary>Agency-scoped in-app notification routes.</summary>
public static class NotificationEndpoints
{
    /// <summary>Maps notification routes.</summary>
    public static IEndpointRouteBuilder MapNotificationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapAgencyScopedApiGroup();
        group.MapGet("/{agencyId:guid}/notifications", ListAsync);
        group.MapPost("/{agencyId:guid}/notifications/{notificationId:guid}/read", MarkReadAsync);
        return endpoints;
    }

    private static async Task<IResult> ListAsync(
        Guid agencyId,
        IAgencyContext agencyContext,
        INotificationQueryService query,
        CancellationToken cancellationToken)
    {
        if (!agencyContext.HasRoutineShellAgencyContext || agencyContext.ActiveAgencyId != agencyId)
        {
            return TenantSafeResults.NotFoundOrForbidden();
        }

        var items = await query.ListForActiveAgencyAsync(
            agencyId,
            agencyContext.ActiveMembershipId!.Value,
            cancellationToken);

        return Results.Ok(items);
    }

    private static async Task<IResult> MarkReadAsync(
        Guid agencyId,
        Guid notificationId,
        IAgencyContext agencyContext,
        INotificationQueryService query,
        CancellationToken cancellationToken)
    {
        if (!agencyContext.HasRoutineShellAgencyContext
            || agencyContext.ActiveAgencyId != agencyId
            || agencyContext.ActiveAgencyRole is not (AgencyRole.StandardMember or AgencyRole.Administrator))
        {
            return TenantSafeResults.NotFoundOrForbidden();
        }

        var updated = await query.MarkReadAsync(
            agencyId,
            notificationId,
            agencyContext.ActiveMembershipId!.Value,
            cancellationToken);

        return updated ? Results.NoContent() : TenantSafeResults.NotFoundOrForbidden();
    }
}
