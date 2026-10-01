using TenancyHub.ApiService.Validation.Operator;

namespace TenancyHub.ApiService.Validation;

/// <summary>
/// Registers API request validators for HTTP trust boundaries.
/// </summary>
public static class ValidationServiceCollectionExtensions
{
    /// <summary>
    /// Adds request validators used by minimal API routes and endpoint filters.
    /// </summary>
    public static IServiceCollection AddApiRequestValidation(this IServiceCollection services)
    {
        services.AddSingleton<IRequestValidator<CreateOperatorAgencyRequest>, CreateOperatorAgencyRequestValidator>();
        return services;
    }
}
