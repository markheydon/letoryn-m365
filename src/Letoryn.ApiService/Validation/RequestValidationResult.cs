namespace Letoryn.ApiService.Validation;

/// <summary>
/// Outcome of validating an inbound API request at the HTTP trust boundary.
/// </summary>
public sealed class RequestValidationResult
{
    private RequestValidationResult(IReadOnlyList<ValidationFailure> failures)
    {
        Failures = failures;
    }

    /// <summary>Validation failures when <see cref="IsValid"/> is false.</summary>
    public IReadOnlyList<ValidationFailure> Failures { get; }

    /// <summary>Whether the request passed validation.</summary>
    public bool IsValid => Failures.Count == 0;

    /// <summary>Creates a successful validation result.</summary>
    public static RequestValidationResult Success() => new([]);

    /// <summary>Creates a failed validation result.</summary>
    public static RequestValidationResult Failure(params ValidationFailure[] failures) =>
        new(failures);

    /// <summary>Creates a failed validation result from a list.</summary>
    public static RequestValidationResult Failure(IReadOnlyList<ValidationFailure> failures) =>
        new(failures);
}
