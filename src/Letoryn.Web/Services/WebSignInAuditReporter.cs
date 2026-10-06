using System.Security.Claims;
using Letoryn.Application.Abstractions.Tenancy;

namespace Letoryn.Web.Services;

/// <summary>Reports Web cookie validation failures to the API audit log (FR-008).</summary>
public sealed class WebSignInAuditReporter(HttpClient httpClient, IConfiguration configuration)
{
    /// <summary>Writes a failed sign-in audit row when Entra token refresh fails.</summary>
    public async Task ReportCookieValidationFailedAsync(ClaimsPrincipal? principal, CancellationToken cancellationToken)
    {
        var auditKey = configuration["Letoryn:InternalSignInAuditKey"];
        if (string.IsNullOrWhiteSpace(auditKey))
        {
            return;
        }

        var entraObjectId = principal?.FindFirstValue("oid")
            ?? principal?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal?.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier");

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/sign-in-failed")
            {
                Content = JsonContent.Create(new { entraObjectId }),
            };
            request.Headers.TryAddWithoutValidation(TenancyHttpHeaders.InternalAuditKey, auditKey);

            using var response = await httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
        }
        catch
        {
            // Audit must not block principal rejection.
        }
    }
}
