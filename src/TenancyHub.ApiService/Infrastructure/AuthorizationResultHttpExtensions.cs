using TenancyHub.Application.Abstractions.Authorization;

namespace TenancyHub.ApiService.Infrastructure;

/// <summary>Maps authorization outcomes to tenant-safe HTTP responses (FR-013).</summary>
public static class AuthorizationResultHttpExtensions
{
    /// <summary>Executes the appropriate HTTP result for a failed authorization check.</summary>
    public static Task ExecuteFailureAsync(this AuthorizationResult result, HttpContext context)
    {
        if (result.IsAuthorized)
        {
            throw new InvalidOperationException("Cannot execute failure for a successful authorization result.");
        }

        return result.FailureKind switch
        {
            AuthorizationFailureKind.NotFound => TenantSafeResults.NotFoundOrForbidden().ExecuteAsync(context),
            AuthorizationFailureKind.Forbidden
                or AuthorizationFailureKind.NoActiveAgency
                or AuthorizationFailureKind.AgencyAccessBlocked
                or AuthorizationFailureKind.MembershipAccessBlocked when !string.IsNullOrWhiteSpace(result.UserMessage)
                => Results.Problem(
                    title: "Forbidden",
                    detail: result.UserMessage,
                    statusCode: StatusCodes.Status403Forbidden,
                    type: "https://tools.ietf.org/html/rfc9110#section-15.5.4").ExecuteAsync(context),
            _ => TenantSafeResults.Forbidden().ExecuteAsync(context),
        };
    }
}
