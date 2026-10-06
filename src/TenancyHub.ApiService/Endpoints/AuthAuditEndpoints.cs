using Microsoft.EntityFrameworkCore;
using TenancyHub.ApiService.Auth;
using TenancyHub.Infrastructure.Persistence;

namespace TenancyHub.ApiService.Endpoints;

/// <summary>Internal audit hooks for Web-tier authentication failures.</summary>
public static class AuthAuditEndpoints
{
    /// <summary>Rate limiter policy for internal sign-in failure audit posts.</summary>
    public const string AuthAuditRateLimitPolicyName = "internal-auth-audit";

    /// <summary>Maps auth audit routes.</summary>
    public static IEndpointRouteBuilder MapAuthAuditEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/v1/auth/sign-in-failed", ReportSignInFailedAsync)
            .RequireRateLimiting(AuthAuditRateLimitPolicyName);

        return endpoints;
    }

    private static async Task<IResult> ReportSignInFailedAsync(
        HttpContext httpContext,
        IConfiguration configuration,
        ReportSignInFailedRequest? request,
        TenancyHubDbContext dbContext,
        SignInAuditService signInAudit,
        CancellationToken cancellationToken)
    {
        if (!WebInternalRequestValidation.IsTrustedWebRequest(httpContext, configuration))
        {
            return Results.NotFound();
        }

        Guid? userIdentityId = null;
        if (!string.IsNullOrWhiteSpace(request?.EntraObjectId))
        {
            userIdentityId = await dbContext.UserIdentities
                .AsNoTracking()
                .Where(u => u.EntraObjectId == request.EntraObjectId)
                .Select(u => (Guid?)u.Id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        await signInAudit.WriteSignInFailedAsync(
            userIdentityId,
            "Web session validation failed (directory account or token refresh).",
            cancellationToken);

        return Results.NoContent();
    }

    private sealed record ReportSignInFailedRequest(string? EntraObjectId);
}
