using Letoryn.ApiService.Infrastructure;
using Letoryn.Application.Abstractions.Memberships;
using Letoryn.Application.Abstractions.Tenancy;

namespace Letoryn.ApiService.Endpoints;

/// <summary>Invitee invitation acceptance routes.</summary>
public static class InvitationEndpoints
{
    /// <summary>Maps invitation routes.</summary>
    public static IEndpointRouteBuilder MapInvitationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/invitations")
            .RequireAuthorization();

        group.MapGet("/pending", ListPendingAsync);
        group.MapPost("/{membershipId:guid}/accept", AcceptAsync);
        group.MapPost("/{membershipId:guid}/decline", DeclineAsync);

        return endpoints;
    }

    private static async Task<IResult> ListPendingAsync(
        ICurrentUser currentUser,
        IMembershipOperations operations,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserIdentityId == Guid.Empty)
        {
            return Results.Unauthorized();
        }

        var invites = await operations.ListPendingInvitationsAsync(
            currentUser.UserIdentityId,
            currentUser.Email,
            cancellationToken);

        return Results.Ok(invites);
    }

    private static async Task<IResult> AcceptAsync(
        Guid membershipId,
        ICurrentUser currentUser,
        IMembershipOperations operations,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserIdentityId == Guid.Empty)
        {
            return Results.Unauthorized();
        }

        var result = await operations.AcceptInvitationAsync(
            currentUser.UserIdentityId,
            currentUser.Email,
            membershipId,
            cancellationToken);

        return result.ToHttpResult();
    }

    private static async Task<IResult> DeclineAsync(
        Guid membershipId,
        ICurrentUser currentUser,
        IMembershipOperations operations,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserIdentityId == Guid.Empty)
        {
            return Results.Unauthorized();
        }

        var result = await operations.DeclineInvitationAsync(
            currentUser.UserIdentityId,
            currentUser.Email,
            membershipId,
            cancellationToken);

        return result.ToHttpResult();
    }
}
