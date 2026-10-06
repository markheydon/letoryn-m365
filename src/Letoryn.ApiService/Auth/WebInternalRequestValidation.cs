using Letoryn.Application.Abstractions.Tenancy;

namespace Letoryn.ApiService.Auth;

/// <summary>Validates trusted calls from the Letoryn Web host.</summary>
public static class WebInternalRequestValidation
{
    /// <summary>Returns true when the request includes the configured internal shared key.</summary>
    public static bool IsTrustedWebRequest(HttpContext httpContext, IConfiguration configuration)
    {
        var expectedKey = configuration["Letoryn:InternalSignInAuditKey"];
        if (string.IsNullOrWhiteSpace(expectedKey))
        {
            return false;
        }

        return httpContext.Request.Headers.TryGetValue(TenancyHttpHeaders.InternalAuditKey, out var providedKey)
            && string.Equals(providedKey.FirstOrDefault(), expectedKey, StringComparison.Ordinal);
    }
}
