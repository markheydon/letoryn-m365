using Microsoft.AspNetCore.Components;

namespace TenancyHub.Web.Services;

/// <summary>Ends the Web sign-in flow when the API rejects the server-side session (FR-001).</summary>
public sealed class ExpiredApiSessionHandler(
    NavigationManager navigation,
    AgencyContextState agencyContext,
    UserSessionState sessionState)
{
    private int _redirectScheduled;

    /// <summary>Clears local session state and redirects to Entra sign-out so the user can sign in again.</summary>
    public async Task HandleAsync(CancellationToken cancellationToken = default)
    {
        if (Interlocked.Exchange(ref _redirectScheduled, 1) == 1)
        {
            return;
        }

        sessionState.SessionId = null;
        sessionState.ApiSessionEstablished = false;
        await sessionState.PersistToBrowserAsync(cancellationToken);
        agencyContext.Clear();
        navigation.NavigateTo("MicrosoftIdentity/Account/SignOut", forceLoad: true);
    }
}
