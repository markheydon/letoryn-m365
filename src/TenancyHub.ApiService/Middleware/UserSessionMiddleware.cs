using TenancyHub.ApiService.Auth;
using TenancyHub.Application.Abstractions.Sessions;
using TenancyHub.Application.Abstractions.Tenancy;
using TenancyHttpHeaders = TenancyHub.Application.Abstractions.Tenancy.TenancyHttpHeaders;

namespace TenancyHub.ApiService.Middleware;

/// <summary>
/// Validates server-side session metadata on authenticated API requests (FR-001).
/// </summary>
public sealed class UserSessionMiddleware(RequestDelegate next)
{
    /// <summary>Session correlation header from the web client.</summary>
    public const string SessionHeaderName = TenancyHttpHeaders.SessionId;

    private static readonly PathString ApiPrefix = new("/api/v1");

    /// <inheritdoc />
    public async Task InvokeAsync(
        HttpContext context,
        IUserSessionService sessionService,
        ICurrentUser currentUser,
        SignInAuditService signInAudit)
    {
        if (!context.Request.Path.StartsWithSegments(ApiPrefix, StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        if (context.User.Identity?.IsAuthenticated != true)
        {
            await next(context);
            return;
        }

        var isMeBootstrap = context.Request.Path.Equals("/api/v1/me", StringComparison.OrdinalIgnoreCase)
            && HttpMethods.IsGet(context.Request.Method);
        var isMeSignOut = context.Request.Path.Equals("/api/v1/me/sign-out", StringComparison.OrdinalIgnoreCase)
            && HttpMethods.IsPost(context.Request.Method);

        if (!context.Request.Headers.TryGetValue(SessionHeaderName, out var headerValues)
            || !Guid.TryParse(headerValues.FirstOrDefault(), out var sessionId))
        {
            if (isMeBootstrap || isMeSignOut)
            {
                await next(context);
                return;
            }

            await Results.Problem(
                title: "Unauthorized",
                detail: "A valid session is required.",
                statusCode: StatusCodes.Status401Unauthorized,
                type: "https://tools.ietf.org/html/rfc9110#section-15.5.2").ExecuteAsync(context);
            return;
        }

        if (currentUser.UserIdentityId == Guid.Empty)
        {
            await signInAudit.WriteSignInFailedAsync(
                null,
                "Authentication token was valid but the product identity is not provisioned.",
                context.RequestAborted);
            await Results.Problem(
                title: "Unauthorized",
                detail: "Authentication is required.",
                statusCode: StatusCodes.Status401Unauthorized,
                type: "https://tools.ietf.org/html/rfc9110#section-15.5.2").ExecuteAsync(context);
            return;
        }

        var validation = await sessionService.ValidateAndTouchAsync(
            sessionId,
            currentUser.UserIdentityId,
            context.RequestAborted);

        if (!validation.IsValid)
        {
            if (validation.Reason is not null)
            {
                await signInAudit.WriteSessionTerminatedAsync(
                    currentUser.UserIdentityId,
                    validation.Reason.Value,
                    context.RequestAborted);
            }

            if (isMeBootstrap || isMeSignOut)
            {
                await next(context);
                return;
            }

            await Results.Problem(
                title: "Unauthorized",
                detail: "Your session has ended. Sign in again.",
                statusCode: StatusCodes.Status401Unauthorized,
                type: "https://tools.ietf.org/html/rfc9110#section-15.5.2").ExecuteAsync(context);
            return;
        }

        context.Items[SessionHeaderName] = sessionId;
        await next(context);
    }
}
