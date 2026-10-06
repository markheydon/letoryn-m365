using TenancyHub.ApiService.Infrastructure;
using TenancyHub.Application.Abstractions.Operators;
using TenancyHub.Application.Abstractions.Tenancy;

namespace TenancyHub.ApiService.Endpoints;

/// <summary>Platform operator grant, revoke, and assignment routes.</summary>
public static class OperatorPlatformEndpoints
{
    /// <summary>Maps platform operator management routes.</summary>
    public static IEndpointRouteBuilder MapOperatorPlatformEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/operator/platform")
            .RequireAuthorization()
            .AddEndpointFilter<PlatformOperatorAuthorizationFilter>();

        group.MapGet("/operators", ListOperatorsAsync);
        group.MapPost("/operators/{userId:guid}/grant", GrantAsync);
        group.MapPost("/operators/{userId:guid}/revoke", RevokeAsync);
        group.MapPut("/operators/{userId:guid}/assignments", SetAssignmentsAsync);

        return endpoints;
    }

    private static async Task<IResult> ListOperatorsAsync(
        IPlatformOperatorManagement management,
        CancellationToken cancellationToken)
    {
        var operators = await management.ListOperatorsAsync(cancellationToken);
        return Results.Ok(operators);
    }

    private static async Task<IResult> GrantAsync(
        Guid userId,
        ICurrentUser currentUser,
        IPlatformOperatorManagement management,
        CancellationToken cancellationToken)
    {
        var result = await management.GrantOperatorAsync(currentUser.UserIdentityId, userId, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> RevokeAsync(
        Guid userId,
        ICurrentUser currentUser,
        IPlatformOperatorManagement management,
        CancellationToken cancellationToken)
    {
        var result = await management.RevokeOperatorAsync(currentUser.UserIdentityId, userId, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> SetAssignmentsAsync(
        Guid userId,
        SetAssignmentsRequest request,
        ICurrentUser currentUser,
        IPlatformOperatorManagement management,
        CancellationToken cancellationToken)
    {
        var result = await management.SetAssignmentsAsync(
            currentUser.UserIdentityId,
            userId,
            request.AgencyIds,
            cancellationToken);

        return result.ToHttpResult();
    }

    private sealed record SetAssignmentsRequest(IReadOnlyList<Guid> AgencyIds);
}
