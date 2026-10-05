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
    UserSessionState sessionState)
{
    /// <summary>Loads the signed-in user profile.</summary>
    public async Task<MeProfileResponse?> GetMeAsync(CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/me");
        await PrepareRequestAsync(request, cancellationToken);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        if (response.Headers.TryGetValues(TenancyHttpHeaders.SessionId, out var sessionValues)
            && Guid.TryParse(sessionValues.FirstOrDefault(), out var sessionId))
        {
            sessionState.SessionId = sessionId;
        }

        return await response.Content.ReadFromJsonAsync<MeProfileResponse>(cancellationToken);
    }

    /// <summary>Sets the active agency on the server.</summary>
    public async Task<bool> SetActiveAgencyAsync(Guid agencyId, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, "/api/v1/me/active-agency")
        {
            Content = JsonContent.Create(new SetActiveAgencyRequest(agencyId)),
        };
        await PrepareRequestAsync(request, cancellationToken);

        using var response = await httpClient.SendAsync(request, cancellationToken);
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
    }

    private async Task PrepareRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var scope = configuration["TenancyHub:ApiScope"]
            ?? $"{configuration["AzureAd:Audience"]}/access_as_user";
        var token = await tokenAcquisition.GetAccessTokenForUserAsync([scope]);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        if (sessionState.SessionId is Guid sessionId)
        {
            request.Headers.Add(TenancyHttpHeaders.SessionId, sessionId.ToString());
        }

        if (agencyContext.ActiveAgencyId is Guid agencyId)
        {
            request.Headers.Add(TenancyHttpHeaders.AgencyId, agencyId.ToString());
        }
    }
}
