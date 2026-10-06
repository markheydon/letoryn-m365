using Letoryn.ApiService.Infrastructure;
using Letoryn.Application.Abstractions.Agencies;
using Letoryn.Application.Abstractions.Memberships;
using Letoryn.Application.Abstractions.Tenancy;

namespace Letoryn.ApiService.Endpoints;

/// <summary>Canonical agency settings update route.</summary>
public static class AgencySettingsEndpoints
{
    /// <summary>Maps agency settings routes.</summary>
    public static IEndpointRouteBuilder MapAgencySettingsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapAgencyScopedApiGroup();
        group.MapGet("/{agencyId:guid}/settings", GetSettingsAsync);
        group.MapPatch("/{agencyId:guid}/settings", UpdateSettingsAsync);
        return endpoints;
    }

    private static async Task<IResult> GetSettingsAsync(
        Guid agencyId,
        IAgencyContext agencyContext,
        IAgencyOperations operations,
        CancellationToken cancellationToken)
    {
        var result = await operations.GetSettingsAsync(
            agencyId,
            MembershipAccessHelper.IsActiveAgencyAdministrator(agencyContext),
            agencyContext.IsOperatorAssignedToActiveAgency,
            cancellationToken);

        return result.Status switch
        {
            MembershipOperationStatus.Succeeded when result.Settings is not null => Results.Ok(new AgencySettingsResponse(
                result.Settings.DisplayName,
                result.Settings.PrimaryContactEmail,
                result.Settings.PrimaryContactPhone,
                result.Settings.LifecycleStatus.ToString())),
            MembershipOperationStatus.Forbidden => Results.Problem(
                title: "Forbidden",
                detail: result.UserMessage ?? "You do not have permission to view settings.",
                statusCode: StatusCodes.Status403Forbidden),
            _ => TenantSafeResults.NotFoundOrForbidden(),
        };
    }

    private static async Task<IResult> UpdateSettingsAsync(
        Guid agencyId,
        UpdateAgencySettingsRequest request,
        ICurrentUser currentUser,
        IAgencyContext agencyContext,
        IAgencyOperations operations,
        CancellationToken cancellationToken)
    {
        var result = await operations.UpdateSettingsAsync(
            currentUser.UserIdentityId,
            agencyId,
            request.DisplayName,
            request.PrimaryContactEmail,
            request.PrimaryContactPhone,
            MembershipAccessHelper.IsActiveAgencyAdministrator(agencyContext),
            agencyContext.IsOperatorAssignedToActiveAgency,
            cancellationToken);

        return result.Status switch
        {
            MembershipOperationStatus.Succeeded => Results.NoContent(),
            MembershipOperationStatus.ValidationFailed => Results.Problem(
                title: "Validation failed",
                detail: result.UserMessage,
                statusCode: StatusCodes.Status400BadRequest),
            MembershipOperationStatus.Forbidden => Results.Problem(
                title: "Forbidden",
                detail: result.UserMessage ?? "You do not have permission to update settings.",
                statusCode: StatusCodes.Status403Forbidden),
            _ => TenantSafeResults.NotFoundOrForbidden(),
        };
    }

    private sealed record AgencySettingsResponse(
        string DisplayName,
        string PrimaryContactEmail,
        string PrimaryContactPhone,
        string LifecycleStatus);

    private sealed record UpdateAgencySettingsRequest(
        string? DisplayName,
        string? PrimaryContactEmail,
        string? PrimaryContactPhone);
}
