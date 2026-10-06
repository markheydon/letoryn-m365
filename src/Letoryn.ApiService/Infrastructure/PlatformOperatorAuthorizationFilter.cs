using Letoryn.Application.Abstractions.Tenancy;

namespace Letoryn.ApiService.Infrastructure;

/// <summary>Requires the caller to be a platform operator.</summary>
public sealed class PlatformOperatorAuthorizationFilter : IEndpointFilter
{
    /// <inheritdoc />
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var currentUser = context.HttpContext.RequestServices.GetRequiredService<ICurrentUser>();
        if (currentUser.UserIdentityId == Guid.Empty || !currentUser.IsPlatformOperator)
        {
            return TenantSafeResults.NotFoundOrForbidden();
        }

        return await next(context);
    }
}
