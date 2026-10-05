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
    ExpiredApiSessionHandler expiredApiSessionHandler)
{
    /// <summary>Loads the signed-in user profile.</summary>
    public async Task<MeProfileResponse?> GetMeAsync(CancellationToken cancellationToken = default)
    {
        await sessionState.EnsureLoadedFromBrowserAsync(cancellationToken);

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/me");
        await PrepareRequestAsync(request, cancellationToken);

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
        await PrepareRequestAsync(request, cancellationToken);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        sessionState.SessionId = null;
        await sessionState.PersistToBrowserAsync(cancellationToken);
    }

    private async Task PrepareRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await sessionState.EnsureLoadedFromBrowserAsync(cancellationToken);
        var scope = configuration["TenancyHub:ApiScope"]
            ?? $"{configuration["AzureAd:Audience"]}/access_as_user";
        var token = await tokenAcquisition.GetAccessTokenForUserAsync([scope]);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        if (httpContextAccessor.HttpContext?.Request.Cookies.ContainsKey(SessionEstablishmentCookie.Name) == true)
        {
            request.Headers.TryAddWithoutValidation(TenancyHttpHeaders.EstablishSession, "true");
        }
        else if (sessionState.SessionId is Guid sessionId)
        {
            request.Headers.TryAddWithoutValidation(TenancyHttpHeaders.SessionId, sessionId.ToString());
        }

        if (agencyContext.ActiveAgencyId is Guid agencyId)
        {
            request.Headers.TryAddWithoutValidation(TenancyHttpHeaders.AgencyId, agencyId.ToString());
        }
    }

    private async Task<HttpResponseMessage> SendWithSessionRecoveryAsync(
        Func<HttpRequestMessage> requestFactory,
        CancellationToken cancellationToken)
    {
        using var request = requestFactory();
        await PrepareRequestAsync(request, cancellationToken);
        var response = await httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            await expiredApiSessionHandler.HandleAsync(cancellationToken);
        }

        return response;
    }

    private void ClearEstablishSessionCookieIfPresent()
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext?.Request.Cookies.ContainsKey(SessionEstablishmentCookie.Name) != true)
        {
            return;
        }

        httpContext.Response.Cookies.Delete(SessionEstablishmentCookie.Name);
    }
}
