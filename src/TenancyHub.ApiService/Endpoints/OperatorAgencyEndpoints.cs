using TenancyHub.ApiService.Infrastructure;
using TenancyHub.ApiService.Validation;
using TenancyHub.ApiService.Validation.Operator;
using TenancyHub.Application.Abstractions.Agencies;
using TenancyHub.Application.Abstractions.Memberships;
using TenancyHub.Application.Abstractions.Tenancy;
using TenancyHub.Domain.Agencies;

namespace TenancyHub.ApiService.Endpoints;

/// <summary>Platform operator agency create and lifecycle routes.</summary>
public static class OperatorAgencyEndpoints
{
    /// <summary>Maps operator agency routes.</summary>
    public static IEndpointRouteBuilder MapOperatorAgencyEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var platformGroup = endpoints.MapGroup("/api/v1/operator")
            .RequireAuthorization()
            .AddEndpointFilter<PlatformOperatorAuthorizationFilter>();

        platformGroup.MapPost("/agencies", CreateAgencyAsync);

        var agencyGroup = endpoints.MapOperatorAgencyApiGroup();
        agencyGroup.MapPost("/{agencyId:guid}/lifecycle", ChangeLifecycleAsync);

        return endpoints;
    }

    private static async Task<IResult> CreateAgencyAsync(
        CreateOperatorAgencyRequest request,
        ICurrentUser currentUser,
        IAgencyOperations operations,
        IRequestValidator<CreateOperatorAgencyRequest> validator,
        CancellationToken cancellationToken)
    {
        var validation = validator.Validate(request);
        if (!validation.IsValid)
        {
            return ValidationProblemResults.FromFailures(validation.Failures);
        }

        var result = await operations.CreateAgencyAsync(
            currentUser.UserIdentityId,
            request.DisplayName,
            request.PrimaryContactEmail,
            request.PrimaryContactPhone ?? string.Empty,
            cancellationToken);

        return result.Status switch
        {
            MembershipOperationStatus.Succeeded => Results.Created(
                $"/api/v1/operator/agencies/{result.AgencyId}",
                new { agencyId = result.AgencyId }),
            MembershipOperationStatus.Forbidden => TenantSafeResults.NotFoundOrForbidden(),
            _ => Results.Problem(title: "Validation failed", detail: result.UserMessage, statusCode: 400),
        };
    }

    private static async Task<IResult> ChangeLifecycleAsync(
        Guid agencyId,
        ChangeLifecycleRequest request,
        ICurrentUser currentUser,
        IAgencyOperations operations,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<AgencyLifecycleStatus>(request.TargetStatus, ignoreCase: true, out var target))
        {
            return Results.Problem(title: "Validation failed", detail: "Target status is invalid.", statusCode: 400);
        }

        var result = await operations.ChangeLifecycleAsync(
            currentUser.UserIdentityId,
            agencyId,
            target,
            cancellationToken);

        return result.Status switch
        {
            MembershipOperationStatus.Succeeded => Results.NoContent(),
            MembershipOperationStatus.ValidationFailed => Results.Problem(
                title: "Validation failed",
                detail: result.UserMessage,
                statusCode: StatusCodes.Status400BadRequest),
            _ => TenantSafeResults.NotFoundOrForbidden(),
        };
    }

    private sealed record ChangeLifecycleRequest(string TargetStatus);
}
