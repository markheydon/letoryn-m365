using Microsoft.Identity.Client;
using Microsoft.Identity.Web;

namespace TenancyHub.Web.Services;

/// <summary>Classifies Entra/MSAL failures from Identity.Web token acquisition.</summary>
internal static class EntraTokenErrors
{
    /// <summary>
    /// True when the user must complete interactive sign-in (for example after an app restart cleared the in-memory MSAL cache).
    /// </summary>
    internal static bool RequiresInteractiveSignIn(Exception exception)
    {
        for (var ex = exception; ex is not null; ex = ex.InnerException)
        {
            if (ex is MicrosoftIdentityWebChallengeUserException)
            {
                return true;
            }

            if (ex is MsalUiRequiredException)
            {
                return true;
            }
        }

        return false;
    }
}
