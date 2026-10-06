using Letoryn.Application.Abstractions.Operators;
using Letoryn.Application.Abstractions.Tenancy;

namespace Letoryn.ApiService.Infrastructure;

/// <summary>
/// Verifies platform operator assignment for <c>/api/v1/operator/agencies/{agencyId}/...</c> routes (FR-006).
/// </summary>
public sealed class OperatorAgencyAuthorizationFilter : IEndpointFilter
{
    /// <inheritdoc />
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        if (!context.HttpContext.Request.RouteValues.TryGetValue("agencyId", out var agencyObj)
            || agencyObj is not string agencyText
            || !Guid.TryParse(agencyText, out var agencyId))
        {
            return await next(context);
        }

        var currentUser = context.HttpContext.RequestServices.GetRequiredService<ICurrentUser>();
        if (currentUser.UserIdentityId == Guid.Empty)
        {
            return TenantSafeResults.NotFoundOrForbidden();
        }

        var assignments = context.HttpContext.RequestServices.GetRequiredService<IOperatorAssignmentService>();
        var assigned = await assignments.IsAssignedAsync(
            currentUser.UserIdentityId,
            agencyId,
            context.HttpContext.RequestAborted);

        if (!assigned)
        {
            return TenantSafeResults.NotFoundOrForbidden();
        }

        return await next(context);
    }
}
