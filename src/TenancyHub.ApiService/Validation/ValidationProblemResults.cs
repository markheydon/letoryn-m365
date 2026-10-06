namespace TenancyHub.ApiService.Validation;

/// <summary>
/// Maps validation failures to RFC 7807 ProblemDetails (400, application/problem+json).
/// </summary>
public static class ValidationProblemResults
{
    /// <summary>
    /// Returns 400 ProblemDetails with an <c>errors</c> extension dictionary keyed by field name.
    /// </summary>
    public static IResult FromFailures(IReadOnlyList<ValidationFailure> failures)
    {
        var errors = failures
            .GroupBy(f => f.Field, StringComparer.Ordinal)
            .ToDictionary(
                g => g.Key,
                g => g.Select(f => f.Message).ToArray(),
                StringComparer.Ordinal);

        return Results.ValidationProblem(
            errors,
            title: "Validation failed",
            detail: "One or more validation errors occurred.",
            statusCode: StatusCodes.Status400BadRequest,
            type: "https://tools.ietf.org/html/rfc9110#section-15.5.1");
    }

    /// <summary>
    /// Returns 400 ProblemDetails when validation failed; otherwise <see langword="null"/>.
    /// </summary>
    public static IResult? FromResult(RequestValidationResult result) =>
        result.IsValid ? null : FromFailures(result.Failures);
}
