using System.Security.Claims;

namespace TenancyHub.Web.Services;

/// <summary>Reports Web cookie validation failures to the API audit log (FR-008).</summary>
public sealed class WebSignInAuditReporter(HttpClient httpClient)
{
    /// <summary>Writes a failed sign-in audit row when Entra token refresh fails.</summary>
    public async Task ReportCookieValidationFailedAsync(ClaimsPrincipal? principal, CancellationToken cancellationToken)
    {
        var entraObjectId = principal?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal?.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier");

        try
        {
            using var response = await httpClient.PostAsJsonAsync(
                "/api/v1/auth/sign-in-failed",
                new { entraObjectId },
                cancellationToken);
            response.EnsureSuccessStatusCode();
        }
        catch
        {
            // Audit must not block principal rejection.
        }
    }
}
