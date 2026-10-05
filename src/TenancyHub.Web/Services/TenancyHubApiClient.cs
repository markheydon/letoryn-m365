using System.Net.Http.Headers;
using Microsoft.Identity.Web;
using TenancyHub.Application.Abstractions.Tenancy;
using TenancyHub.Application.Me;

namespace TenancyHub.Web.Services;

/// <summary>
/// Typed HTTP client for the Tenancy Hub API via Aspire service discovery (FR-012).
/// </summary>
public sealed class TenancyHubApiClient(
    HttpClient httpClient,
    ITokenAcquisition tokenAcquisition,
    IConfiguration configuration,
    AgencyContextState agencyContext,
    UserSessionState sessionState,
    IHttpContextAccessor httpContextAccessor,
    ExpiredApiSessionHandler expiredApiSessionHandler,
    WebSignInAuditReporter signInAuditReporter)
{
    /// <summary>Loads the signed-in user profile.</summary>
    public async Task<MeProfileResponse?> GetMeAsync(CancellationToken cancellationToken = default)
    {
        await sessionState.EnsureLoadedFromBrowserAsync(cancellationToken);

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/me");
        if (!await TryPrepareRequestAsync(request, cancellationToken))
        {
            return null;
        }

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await expiredApiSessionHandler.HandleAsync(cancellationToken);
            }

            return null;
        }

        if (response.Headers.TryGetValues(TenancyHttpHeaders.SessionId, out var sessionValues)
            && Guid.TryParse(sessionValues.FirstOrDefault(), out var sessionId))
        {
            sessionState.SessionId = sessionId;
            sessionState.ApiSessionEstablished = true;
            await sessionState.PersistToBrowserAsync(cancellationToken);
        }

        ClearEstablishSessionCookieIfPresent();

        return await response.Content.ReadFromJsonAsync<MeProfileResponse>(cancellationToken);
    }

    /// <summary>Sets the active agency on the server.</summary>
    public async Task<bool> SetActiveAgencyAsync(Guid agencyId, CancellationToken cancellationToken = default)
    {
        using var response = await SendWithSessionRecoveryAsync(
            () => new HttpRequestMessage(HttpMethod.Put, "/api/v1/me/active-agency")
            {
                Content = JsonContent.Create(new SetActiveAgencyRequest(agencyId)),
            },
            cancellationToken);

        return response.IsSuccessStatusCode;
    }

    /// <summary>Ends the current API session.</summary>
    public async Task SignOutAsync(CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/me/sign-out");
        if (!await TryPrepareRequestAsync(request, cancellationToken))
        {
            return;
        }

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        sessionState.SessionId = null;
        sessionState.ApiSessionEstablished = false;
        await sessionState.PersistToBrowserAsync(cancellationToken);
    }

    private async Task<bool> TryPrepareRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await sessionState.EnsureLoadedFromBrowserAsync(cancellationToken);
        var scope = configuration["TenancyHub:ApiScope"]
            ?? $"{configuration["AzureAd:Audience"]}/access_as_user";

        try
        {
            var token = await tokenAcquisition.GetAccessTokenForUserAsync([scope]);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        catch (Exception ex) when (EntraTokenErrors.RequiresInteractiveSignIn(ex))
        {
            await expiredApiSessionHandler.HandleAsync(cancellationToken);
            return false;
        }
        catch (Exception)
        {
            await signInAuditReporter.ReportCookieValidationFailedAsync(
                httpContextAccessor.HttpContext?.User,
                cancellationToken);
            await expiredApiSessionHandler.HandleAsync(cancellationToken);
            return false;
        }

        // After interactive sign-in, GET /me may bootstrap a new API session while the cookie is present,
        // even if ProtectedSessionStorage still holds a stale id. Other calls (and sign-out) must send the session id.
        var shouldEstablishSession = IsMeBootstrapRequest(request)
            && !sessionState.ApiSessionEstablished
            && httpContextAccessor.HttpContext?.Request.Cookies
                .ContainsKey(SessionEstablishmentCookie.Name) == true;

        if (shouldEstablishSession)
        {
            request.Headers.TryAddWithoutValidation(TenancyHttpHeaders.EstablishSession, "true");
            TryAddInternalAuditKeyHeader(request);
        }
        else if (sessionState.SessionId is Guid sessionId)
        {
            request.Headers.TryAddWithoutValidation(TenancyHttpHeaders.SessionId, sessionId.ToString());
        }

        if (agencyContext.ActiveAgencyId is Guid agencyId)
        {
            request.Headers.TryAddWithoutValidation(TenancyHttpHeaders.AgencyId, agencyId.ToString());
        }

        return true;
    }

    private async Task<HttpResponseMessage> SendWithSessionRecoveryAsync(
        Func<HttpRequestMessage> requestFactory,
        CancellationToken cancellationToken)
    {
        using var request = requestFactory();
        if (!await TryPrepareRequestAsync(request, cancellationToken))
        {
            return new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
        }

        var response = await httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            await expiredApiSessionHandler.HandleAsync(cancellationToken);
        }

        return response;
    }

    private void TryAddInternalAuditKeyHeader(HttpRequestMessage request)
    {
        var auditKey = configuration["TenancyHub:InternalSignInAuditKey"];
        if (!string.IsNullOrWhiteSpace(auditKey))
        {
            request.Headers.TryAddWithoutValidation(TenancyHttpHeaders.InternalAuditKey, auditKey);
        }
    }

    private static bool IsMeBootstrapRequest(HttpRequestMessage request) =>
        IsMePath(request, HttpMethod.Get, "/api/v1/me");

    private static bool IsMePath(HttpRequestMessage request, HttpMethod method, string pathSuffix)
    {
        if (request.Method != method)
        {
            return false;
        }

        var path = request.RequestUri?.IsAbsoluteUri == true
            ? request.RequestUri.AbsolutePath
            : request.RequestUri?.OriginalString ?? string.Empty;

        return path.EndsWith(pathSuffix, StringComparison.OrdinalIgnoreCase);
    }

    private void ClearEstablishSessionCookieIfPresent()
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext?.Request.Cookies.ContainsKey(SessionEstablishmentCookie.Name) != true)
        {
            return;
        }

        // Blazor Server circuits often run after the HTTP response has started; cookie writes then fault the circuit.
        if (!httpContext.Response.HasStarted)
        {
            httpContext.Response.Cookies.Delete(SessionEstablishmentCookie.Name);
        }
    }
}
