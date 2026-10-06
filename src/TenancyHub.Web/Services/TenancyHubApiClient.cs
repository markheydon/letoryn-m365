using System.Net.Http.Headers;
using Microsoft.Identity.Web;
using TenancyHub.Application.Abstractions.Memberships;
using TenancyHub.Application.Abstractions.Tenancy;
using TenancyHub.Application.Me;

namespace TenancyHub.Web.Services;

/// <summary>
/// Typed HTTP client for the Tenancy Hub API via Aspire service discovery (FR-012).
/// </summary>
/// <remarks>
/// <para>
/// Outbound calls acquire an Entra access token for <c>TenancyHub:ApiScope</c> (or the audience-derived
/// <c>access_as_user</c> scope) via <see cref="ITokenAcquisition"/> and send it as a Bearer token.
/// Token or cookie validation failures trigger interactive re-sign-in through <see cref="ExpiredApiSessionHandler"/>.
/// </para>
/// <para>
/// After bootstrap, <see cref="UserSessionState"/> supplies <see cref="TenancyHttpHeaders.SessionId"/> on API calls.
/// The first <c>GET /api/v1/me</c> after sign-in may send <see cref="TenancyHttpHeaders.EstablishSession"/> instead,
/// gated on the establishment cookie and a trusted internal audit key—see <c>TryPrepareRequestAsync</c>.
/// </para>
/// <para>
/// When the shell has selected an agency, <see cref="AgencyContextState.ActiveAgencyId"/> is forwarded as
/// <see cref="TenancyHttpHeaders.AgencyId"/> so the API can resolve tenancy context in
/// <c>TenancyContextMiddleware</c>.
/// </para>
/// </remarks>
public sealed partial class TenancyHubApiClient(
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

        var profile = await response.Content.ReadFromJsonAsync<MeProfileResponse>(cancellationToken);
        if (profile is null)
        {
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

        return profile;
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

    /// <summary>Lists pending invitations for the signed-in user's email.</summary>
    public async Task<IReadOnlyList<PendingInvitationDto>> GetPendingInvitationsAsync(
        CancellationToken cancellationToken = default)
    {
        using var response = await SendWithSessionRecoveryAsync(
            () => new HttpRequestMessage(HttpMethod.Get, "/api/v1/invitations/pending"),
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return [];
        }

        return await response.Content.ReadFromJsonAsync<List<PendingInvitationDto>>(cancellationToken) ?? [];
    }

    /// <summary>Accepts a pending invitation when the signed-in email matches.</summary>
    public async Task<InvitationActionResult> AcceptInvitationAsync(
        Guid membershipId,
        CancellationToken cancellationToken = default) =>
        await SendInvitationActionAsync(
            HttpMethod.Post,
            $"/api/v1/invitations/{membershipId}/accept",
            cancellationToken);

    /// <summary>Declines a pending invitation when the signed-in email matches.</summary>
    public async Task<InvitationActionResult> DeclineInvitationAsync(
        Guid membershipId,
        CancellationToken cancellationToken = default) =>
        await SendInvitationActionAsync(
            HttpMethod.Post,
            $"/api/v1/invitations/{membershipId}/decline",
            cancellationToken);

    /// <summary>Loads the membership roster for an agency.</summary>
    public async Task<MembershipRosterResult> GetMembershipRosterAsync(
        Guid agencyId,
        CancellationToken cancellationToken = default)
    {
        using var response = await SendWithSessionRecoveryAsync(
            () => new HttpRequestMessage(
                HttpMethod.Get,
                $"/api/v1/agencies/{agencyId}/memberships"),
            cancellationToken);

        if (response.StatusCode is System.Net.HttpStatusCode.Forbidden
            or System.Net.HttpStatusCode.NotFound)
        {
            return MembershipRosterResult.Denied();
        }

        if (!response.IsSuccessStatusCode)
        {
            return MembershipRosterResult.Failed();
        }

        var items = await response.Content.ReadFromJsonAsync<List<MembershipRosterDto>>(cancellationToken)
            ?? [];
        return MembershipRosterResult.Success(items);
    }

    /// <summary>Invites a user to the agency.</summary>
    public Task<MembershipMutationResult> InviteMemberAsync(
        Guid agencyId,
        string email,
        string role,
        CancellationToken cancellationToken = default) =>
        SendMembershipMutationAsync(
            HttpMethod.Post,
            $"/api/v1/agencies/{agencyId}/memberships/invite",
            new MembershipInviteRequest(email, role),
            cancellationToken);

    /// <summary>Provisions an active membership for a user.</summary>
    public Task<MembershipMutationResult> ProvisionMemberAsync(
        Guid agencyId,
        string email,
        string role,
        CancellationToken cancellationToken = default) =>
        SendMembershipMutationAsync(
            HttpMethod.Post,
            $"/api/v1/agencies/{agencyId}/memberships/provision",
            new MembershipInviteRequest(email, role),
            cancellationToken);

    /// <summary>Changes a member's agency role.</summary>
    public Task<MembershipMutationResult> ChangeMemberRoleAsync(
        Guid agencyId,
        Guid membershipId,
        string role,
        CancellationToken cancellationToken = default) =>
        SendMembershipMutationAsync(
            HttpMethod.Patch,
            $"/api/v1/agencies/{agencyId}/memberships/{membershipId}/role",
            new MembershipChangeRoleRequest(role),
            cancellationToken);

    /// <summary>Suspends an active member.</summary>
    public Task<MembershipMutationResult> SuspendMemberAsync(
        Guid agencyId,
        Guid membershipId,
        CancellationToken cancellationToken = default) =>
        SendMembershipMutationAsync(
            HttpMethod.Post,
            $"/api/v1/agencies/{agencyId}/memberships/{membershipId}/suspend",
            content: null,
            cancellationToken);

    /// <summary>Reactivates a suspended member.</summary>
    public Task<MembershipMutationResult> ReactivateMemberAsync(
        Guid agencyId,
        Guid membershipId,
        CancellationToken cancellationToken = default) =>
        SendMembershipMutationAsync(
            HttpMethod.Post,
            $"/api/v1/agencies/{agencyId}/memberships/{membershipId}/reactivate",
            content: null,
            cancellationToken);

    /// <summary>Removes a member from the agency.</summary>
    public Task<MembershipMutationResult> RemoveMemberAsync(
        Guid agencyId,
        Guid membershipId,
        CancellationToken cancellationToken = default) =>
        SendMembershipMutationAsync(
            HttpMethod.Delete,
            $"/api/v1/agencies/{agencyId}/memberships/{membershipId}",
            content: null,
            cancellationToken);

    /// <summary>Revokes a pending invitation.</summary>
    public Task<MembershipMutationResult> RevokeInvitationAsync(
        Guid agencyId,
        Guid membershipId,
        CancellationToken cancellationToken = default) =>
        SendMembershipMutationAsync(
            HttpMethod.Delete,
            $"/api/v1/agencies/{agencyId}/memberships/{membershipId}/invitation",
            content: null,
            cancellationToken);

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

    private async Task<MembershipMutationResult> SendMembershipMutationAsync(
        HttpMethod method,
        string path,
        object? content,
        CancellationToken cancellationToken)
    {
        using var response = await SendWithSessionRecoveryAsync(
            () =>
            {
                var request = new HttpRequestMessage(method, path);
                if (content is not null)
                {
                    request.Content = JsonContent.Create(content);
                }

                return request;
            },
            cancellationToken);

        return await MembershipMutationResult.FromResponseAsync(response, cancellationToken);
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
        // even if ProtectedSessionStorage still holds a stale id. Other calls send the session id once bootstrap
        // succeeds; sign-out still sends a persisted id so the API can end the prior server session.
        var hasEstablishCookie = httpContextAccessor.HttpContext?.Request.Cookies
            .ContainsKey(SessionEstablishmentCookie.Name) == true;

        var shouldEstablishSession = IsMeBootstrapRequest(request)
            && !sessionState.ApiSessionEstablished
            && hasEstablishCookie;

        if (shouldEstablishSession)
        {
            request.Headers.TryAddWithoutValidation(TenancyHttpHeaders.EstablishSession, "true");
            TryAddInternalAuditKeyHeader(request);
        }
        else if (ShouldSendPersistedSessionId(request, hasEstablishCookie)
            && sessionState.SessionId is Guid sessionId)
        {
            request.Headers.TryAddWithoutValidation(TenancyHttpHeaders.SessionId, sessionId.ToString());
        }

        if (agencyContext.ActiveAgencyId is Guid agencyId)
        {
            request.Headers.TryAddWithoutValidation(TenancyHttpHeaders.AgencyId, agencyId.ToString());
        }

        return true;
    }

    private async Task<InvitationActionResult> SendInvitationActionAsync(
        HttpMethod method,
        string path,
        CancellationToken cancellationToken)
    {
        using var response = await SendWithSessionRecoveryAsync(
            () => new HttpRequestMessage(method, path),
            cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return InvitationActionResult.Ok;
        }

        var (message, _) = await ReadProblemAsync(response, cancellationToken);
        return new InvitationActionResult(
            false,
            message ?? "We could not complete that action. Please try again.");
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

    private bool ShouldSendPersistedSessionId(HttpRequestMessage request, bool hasEstablishCookie)
    {
        if (sessionState.SessionId is null)
        {
            return false;
        }

        if (sessionState.ApiSessionEstablished || !hasEstablishCookie)
        {
            return true;
        }

        return IsMeSignOutRequest(request);
    }

    private static bool IsMeBootstrapRequest(HttpRequestMessage request) =>
        IsMePath(request, HttpMethod.Get, "/api/v1/me");

    private static bool IsMeSignOutRequest(HttpRequestMessage request) =>
        IsMePath(request, HttpMethod.Post, "/api/v1/me/sign-out");

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

/// <summary>Outcome of an invitation accept or decline API call from the Web client.</summary>
public sealed record InvitationActionResult(bool Succeeded, string? ErrorMessage)
{
    /// <summary>Successful invitation action.</summary>
    public static InvitationActionResult Ok { get; } = new(true, null);
}
