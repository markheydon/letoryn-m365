using System.Security.Claims;
using TenancyHub.ApiService.Tenancy;
using TenancyHub.Application.Abstractions.Identities;

namespace TenancyHub.ApiService.Middleware;

/// <summary>
/// Loads the product user identity asynchronously once per authenticated request.
/// </summary>
public sealed class CurrentUserMiddleware(RequestDelegate next)
{
    /// <inheritdoc />
    public async Task InvokeAsync(
        HttpContext context,
        IEnsureUserIdentityService ensureUserIdentity,
        CurrentUserSnapshotCache cache)
    {
        var principal = context.User;
        if (principal?.Identity?.IsAuthenticated == true)
        {
            var oid = principal.FindFirstValue("oid")
                ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = principal.FindFirstValue("preferred_username")
                ?? principal.FindFirstValue(ClaimTypes.Email)
                ?? principal.FindFirstValue(ClaimTypes.Upn)
                ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(oid))
            {
                var ensured = await ensureUserIdentity.EnsureAsync(
                    oid,
                    string.IsNullOrWhiteSpace(email) ? $"{oid}@unknown.local" : email,
                    context.RequestAborted);

                cache.Set(new CurrentUserSnapshotCache.UserSnapshot(
                    ensured.UserIdentityId,
                    ensured.EntraObjectId,
                    ensured.Email,
                    ensured.IsPlatformOperator));
            }
            else
            {
                cache.Set(CurrentUserSnapshotCache.UserSnapshot.Anonymous);
            }
        }

        await next(context);
    }
}
