using Letoryn.ApiService.Auth;
using Letoryn.ApiService.Infrastructure;
using Letoryn.ApiService.Middleware;
using Letoryn.Application.Abstractions.Me;
using Letoryn.Application.Abstractions.Sessions;
using Letoryn.Application.Abstractions.Tenancy;
using Letoryn.Application.Me;
using UserSessionTerminationReason = Letoryn.Application.Abstractions.Sessions.UserSessionTerminationReason;

namespace Letoryn.ApiService.Endpoints;

/// <summary>
/// Session and identity endpoints for the signed-in user (FR-001).
/// </summary>
/// <remarks>
/// <para>
/// All routes require a validated JWT; <see cref="ICurrentUser"/> supplies the product identity.
/// Server-side sessions are correlated with <see cref="TenancyHttpHeaders.SessionId"/> on subsequent calls.
/// </para>
/// <para>
/// <c>GET /</c> accepts an existing session id (validated by upstream middleware or the handler),
/// or creates one when the trusted Blazor host sends <see cref="TenancyHttpHeaders.EstablishSession"/>
/// after interactive Entra sign-in (see <see cref="WebInternalRequestValidation"/>).
/// The issued session id is returned on the response as <see cref="TenancyHttpHeaders.SessionId"/>.
/// </para>
/// <para>
/// <c>PUT /active-agency</c> persists shell agency selection; it does not set tenancy headers.
/// Callers must continue sending <see cref="TenancyHttpHeaders.AgencyId"/> on agency-scoped API traffic.
/// </para>
/// </remarks>
public static class MeEndpoints
{
    /// <summary>Maps <c>/api/v1/me</c> routes (profile, active agency, sign-out).</summary>
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
        IConfiguration configuration,
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
        var recordSignInSucceededAudit = false;
        if (httpContext.Items.TryGetValue(UserSessionMiddleware.SessionHeaderName, out var validatedSession)
            && validatedSession is Guid validatedId)
        {
            sessionId = validatedId;
        }
        else if (httpContext.Request.Headers.TryGetValue(TenancyHttpHeaders.EstablishSession, out var establishHeader)
            && string.Equals(establishHeader.FirstOrDefault(), "true", StringComparison.OrdinalIgnoreCase)
            && WebInternalRequestValidation.IsTrustedWebRequest(httpContext, configuration))
        {
            var created = await sessionService.CreateSessionAsync(currentUser.UserIdentityId, cancellationToken);
            sessionId = created.SessionId;
            recordSignInSucceededAudit = true;
        }
        else if (httpContext.Request.Headers.TryGetValue(UserSessionMiddleware.SessionHeaderName, out var sessionHeader)
            && Guid.TryParse(sessionHeader.FirstOrDefault(), out var existingSessionId))
        {
            var validation = await sessionService.ValidateAndTouchAsync(
                existingSessionId,
                currentUser.UserIdentityId,
                cancellationToken);
            if (!validation.IsValid)
            {
                return Results.Problem(
                    title: "Unauthorized",
                    detail: "Your session has ended. Sign in again.",
                    statusCode: StatusCodes.Status401Unauthorized);
            }

            sessionId = existingSessionId;
        }
        else
        {
            return Results.Problem(
                title: "Unauthorized",
                detail: "A valid session is required.",
                statusCode: StatusCodes.Status401Unauthorized);
        }

        var profile = await meProfile.GetProfileAsync(currentUser.UserIdentityId, cancellationToken);
        if (profile is null)
        {
            if (recordSignInSucceededAudit)
            {
                await sessionService.EndSessionAsync(sessionId, currentUser.UserIdentityId, cancellationToken);
            }

            return Results.Problem(
                title: "Unauthorized",
                detail: "Authentication is required.",
                statusCode: StatusCodes.Status401Unauthorized);
        }

        if (recordSignInSucceededAudit)
        {
            await signInAudit.WriteSignInSucceededAsync(currentUser.UserIdentityId, cancellationToken);
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

        return result.Result switch
        {
            SetActiveAgencyResult.Succeeded => Results.NoContent(),
            SetActiveAgencyResult.Forbidden => Results.Problem(
                title: "Forbidden",
                detail: result.UserMessage ?? "You cannot use this agency.",
                statusCode: StatusCodes.Status403Forbidden),
            _ => Results.Problem(
                title: TenantSafeResults.ResourceNotAvailableTitle,
                detail: TenantSafeResults.ResourceNotAvailableDetail,
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
            && sessionObj is Guid validatedSessionId)
        {
            var ended = await sessionService.EndSessionAsync(
                validatedSessionId,
                currentUser.UserIdentityId,
                cancellationToken);

            if (ended)
            {
                await signInAudit.WriteSessionTerminatedAsync(
                    currentUser.UserIdentityId,
                    UserSessionTerminationReason.SignOut,
                    cancellationToken);
            }
        }

        return Results.NoContent();
    }
}
