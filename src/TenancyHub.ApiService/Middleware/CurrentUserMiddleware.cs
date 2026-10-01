using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using TenancyHub.ApiService.Tenancy;
using TenancyHub.Infrastructure.Persistence;

namespace TenancyHub.ApiService.Middleware;

/// <summary>
/// Loads the product user identity asynchronously once per authenticated request.
/// </summary>
public sealed class CurrentUserMiddleware(RequestDelegate next)
{
    /// <inheritdoc />
    public async Task InvokeAsync(
        HttpContext context,
        TenancyHubDbContext dbContext,
        CurrentUserSnapshotCache cache)
    {
        var principal = context.User;
        if (principal?.Identity?.IsAuthenticated == true)
        {
            var oid = principal.FindFirstValue("oid")
                ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrWhiteSpace(oid))
            {
                var identity = await dbContext.UserIdentities
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.EntraObjectId == oid, context.RequestAborted);

                cache.Set(identity is null
                    ? new CurrentUserSnapshotCache.UserSnapshot(Guid.Empty, oid, string.Empty, false)
                    : new CurrentUserSnapshotCache.UserSnapshot(
                        identity.Id,
                        identity.EntraObjectId,
                        identity.Email,
                        identity.IsPlatformOperator));
            }
            else
            {
                cache.Set(CurrentUserSnapshotCache.UserSnapshot.Anonymous);
            }
        }

        await next(context);
    }
}
