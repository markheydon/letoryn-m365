namespace TenancyHub.ApiService.Infrastructure;

/// <summary>
/// Tenant-safe HTTP results that avoid leaking agency or membership details (FR-013).
/// </summary>
public static class TenantSafeResults
{
    /// <summary>Generic title for cross-tenant not-found and forbidden responses.</summary>
    public const string ResourceNotAvailableTitle = "Resource not available";

    /// <summary>Generic detail shared by cross-tenant not-found and forbidden responses.</summary>
    public const string ResourceNotAvailableDetail =
        "The requested resource is not available or you do not have access to it.";

    /// <summary>
    /// Returns 403 with a generic message when the caller is authenticated but not permitted.
    /// </summary>
    public static IResult Forbidden() =>
        Results.Problem(
            title: "Forbidden",
            detail: "You do not have permission to perform this action.",
            statusCode: StatusCodes.Status403Forbidden,
            type: "https://tools.ietf.org/html/rfc9110#section-15.5.4");

    /// <summary>
    /// Returns 404 with the same outward shape used for cross-tenant isolation (FR-013, api-v1).
    /// </summary>
    /// <remarks>
    /// Cross-tenant <c>not-found</c> and <c>forbidden</c> cases MUST share this ProblemDetails shape (T049).
    /// </remarks>
    public static IResult NotFoundOrForbidden() =>
        ResourceNotAvailable(StatusCodes.Status404NotFound);

    /// <summary>
    /// Cross-tenant denial with the identical ProblemDetails shape as <see cref="NotFoundOrForbidden"/> (FR-013).
    /// </summary>
    public static IResult CrossTenantDenied() => NotFoundOrForbidden();

    /// <summary>Builds the canonical tenant-safe resource response for the given status code.</summary>
    public static IResult ResourceNotAvailable(int statusCode) =>
        Results.Problem(
            title: ResourceNotAvailableTitle,
            detail: ResourceNotAvailableDetail,
            statusCode: statusCode,
            type: statusCode == StatusCodes.Status404NotFound
                ? "https://tools.ietf.org/html/rfc9110#section-15.5.5"
                : "https://tools.ietf.org/html/rfc9110#section-15.5.4");
}
