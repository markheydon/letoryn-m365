namespace Letoryn.ApiService.Validation;

/// <summary>
/// A single field-level validation error for API request bodies.
/// </summary>
/// <param name="Field">JSON property or logical field name.</param>
/// <param name="Message">User-visible error message (UK English).</param>
public sealed record ValidationFailure(string Field, string Message);
