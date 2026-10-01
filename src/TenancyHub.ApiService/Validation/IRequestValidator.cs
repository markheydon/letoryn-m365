namespace TenancyHub.ApiService.Validation;

/// <summary>
/// Validates a request DTO before handler or application logic runs.
/// </summary>
/// <typeparam name="TRequest">Request body or command type.</typeparam>
public interface IRequestValidator<in TRequest>
{
    /// <summary>Validates the request and returns aggregated failures.</summary>
    RequestValidationResult Validate(TRequest request);
}
