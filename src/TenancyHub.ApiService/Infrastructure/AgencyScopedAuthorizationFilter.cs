using TenancyHub.Application.Abstractions.Authorization;
using TenancyHub.Application.Abstractions.Tenancy;

namespace TenancyHub.ApiService.Infrastructure;

/// <summary>
/// Enforces agency-scoped authorization for minimal API routes that include a route <c>agencyId</c> (FR-003).
/// </summary>
public sealed class AgencyScopedAuthorizationFilter : IEndpointFilter
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

        var authorization = context.HttpContext.RequestServices.GetRequiredService<IAgencyAuthorizationService>();
        var agencyContext = context.HttpContext.RequestServices.GetRequiredService<IAgencyContext>();

        if (agencyContext.ActiveAgencyId is null)
        {
            return TenantSafeResults.NotFoundOrForbidden();
        }

        var result = authorization.AuthorizeAgencyScopedAccess(agencyId);
        if (!result.IsAuthorized)
        {
            return result.ToHttpResult();
        }

        return await next(context);
    }
}
