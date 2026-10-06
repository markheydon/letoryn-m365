using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace Letoryn.Web.Services;

/// <summary>Stores the server-side session id returned by the API.</summary>
public sealed class UserSessionState(ProtectedSessionStorage sessionStorage)
{
    private const string StorageKey = "letoryn.api-session-id";

    private bool _loadedFromBrowser;

    /// <summary>Current API session id, if established.</summary>
    public Guid? SessionId { get; set; }

    /// <summary>True after the API has returned a session id for this interactive circuit.</summary>
    public bool ApiSessionEstablished { get; set; }

    /// <summary>Loads a persisted session id from the browser session store.</summary>
    public async Task EnsureLoadedFromBrowserAsync(CancellationToken cancellationToken = default)
    {
        if (_loadedFromBrowser)
        {
            return;
        }

        _loadedFromBrowser = true;
        var stored = await sessionStorage.GetAsync<Guid>(StorageKey);
        if (stored.Success)
        {
            SessionId = stored.Value;
        }
    }

    /// <summary>Persists or clears the session id for full page reloads.</summary>
    public async Task PersistToBrowserAsync(CancellationToken cancellationToken = default)
    {
        if (SessionId is Guid sessionId)
        {
            await sessionStorage.SetAsync(StorageKey, sessionId);
        }
        else
        {
            await sessionStorage.DeleteAsync(StorageKey);
        }
    }
}
