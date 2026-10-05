using TenancyHub.ApiService.Auth;
using TenancyHub.ApiService.Middleware;
using TenancyHub.Application.Abstractions.Me;
using TenancyHub.Application.Abstractions.Sessions;
using UserSessionTerminationReason = TenancyHub.Application.Abstractions.Sessions.UserSessionTerminationReason;
using TenancyHub.Application.Abstractions.Tenancy;
using TenancyHub.Application.Me;
namespace TenancyHub.ApiService.Endpoints;

/// <summary>Session and identity endpoints for the signed-in user.</summary>
public static class MeEndpoints
{
    /// <summary>Maps /api/v1/me routes.</summary>
    public static IEndpointRouteBuilder MapMeEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/me")
            .RequireAuthorization();

        group.MapGet("/", GetMeAsync);
        group.MapPut("/active-agency", SetActiveAgencyAsync);
        group.MapPost("/sign-out", SignOutAsync);

        return endpoints;
    }

    private static async Task<IResult> GetMeAsync(
        HttpContext httpContext,
        ICurrentUser currentUser,
        IMeProfileService meProfile,
        IUserSessionService sessionService,
        SignInAuditService signInAudit,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserIdentityId == Guid.Empty)
        {
            return Results.Problem(
                title: "Unauthorized",
                detail: "Authentication is required.",
                statusCode: StatusCodes.Status401Unauthorized);
        }

        Guid sessionId;
        if (httpContext.Items.TryGetValue(UserSessionMiddleware.SessionHeaderName, out var validatedSession)
            && validatedSession is Guid validatedId)
        {
            sessionId = validatedId;
        }
        else if (httpContext.Request.Headers.TryGetValue(UserSessionMiddleware.SessionHeaderName, out var sessionHeader)
            && Guid.TryParse(sessionHeader.FirstOrDefault(), out var existingSessionId))
        {
            sessionId = existingSessionId;
        }
        else
        {
            var created = await sessionService.CreateSessionAsync(currentUser.UserIdentityId, cancellationToken);
            sessionId = created.SessionId;
            await signInAudit.WriteSignInSucceededAsync(currentUser.UserIdentityId, cancellationToken);
        }

        var profile = await meProfile.GetProfileAsync(currentUser.UserIdentityId, cancellationToken);
        if (profile is null)
        {
            return Results.Problem(
                title: "Unauthorized",
                detail: "Authentication is required.",
                statusCode: StatusCodes.Status401Unauthorized);
        }

        httpContext.Response.Headers.Append(UserSessionMiddleware.SessionHeaderName, sessionId.ToString());
        return Results.Ok(profile);
    }

    private static async Task<IResult> SetActiveAgencyAsync(
        SetActiveAgencyRequest request,
        ICurrentUser currentUser,
        IMeProfileService meProfile,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserIdentityId == Guid.Empty)
        {
            return Results.Problem(
                title: "Unauthorized",
                detail: "Authentication is required.",
                statusCode: StatusCodes.Status401Unauthorized);
        }

        var result = await meProfile.SetActiveAgencyAsync(
            currentUser.UserIdentityId,
            request.AgencyId,
            cancellationToken);

        return result switch
        {
            SetActiveAgencyResult.Succeeded => Results.NoContent(),
            SetActiveAgencyResult.Forbidden => Results.Problem(
                title: "Forbidden",
                detail: "You cannot use this agency.",
                statusCode: StatusCodes.Status403Forbidden),
            _ => Results.Problem(
                title: "Not found",
                detail: "The requested resource was not found.",
                statusCode: StatusCodes.Status404NotFound),
        };
    }

    private static async Task<IResult> SignOutAsync(
        HttpContext httpContext,
        ICurrentUser currentUser,
        IUserSessionService sessionService,
        SignInAuditService signInAudit,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserIdentityId == Guid.Empty)
        {
            return Results.NoContent();
        }

        if (httpContext.Items.TryGetValue(UserSessionMiddleware.SessionHeaderName, out var sessionObj)
            && sessionObj is Guid sessionId)
        {
            var ended = await sessionService.EndSessionAsync(
                sessionId,
                currentUser.UserIdentityId,
                cancellationToken);

            if (ended)
            {
                await signInAudit.WriteSessionTerminatedAsync(
                    currentUser.UserIdentityId,
                    UserSessionTerminationReason.NotFound,
                    cancellationToken);
            }
        }

        return Results.NoContent();
    }
}
