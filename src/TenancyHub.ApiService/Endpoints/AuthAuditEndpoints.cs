using Microsoft.EntityFrameworkCore;
using TenancyHub.ApiService.Auth;
using TenancyHub.Application.Abstractions.Tenancy;
using TenancyHub.Infrastructure.Persistence;

namespace TenancyHub.ApiService.Endpoints;

/// <summary>Internal audit hooks for Web-tier authentication failures.</summary>
public static class AuthAuditEndpoints
{
    /// <summary>Maps auth audit routes.</summary>
    public static IEndpointRouteBuilder MapAuthAuditEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/v1/auth/sign-in-failed", ReportSignInFailedAsync);

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
        var expectedKey = configuration["TenancyHub:InternalSignInAuditKey"];
        if (string.IsNullOrWhiteSpace(expectedKey)
            || !httpContext.Request.Headers.TryGetValue(TenancyHttpHeaders.InternalAuditKey, out var providedKey)
            || !string.Equals(providedKey.FirstOrDefault(), expectedKey, StringComparison.Ordinal))
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
