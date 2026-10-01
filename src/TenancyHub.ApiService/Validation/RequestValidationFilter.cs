namespace TenancyHub.ApiService.Validation;

/// <summary>
/// Endpoint filter that runs a registered <see cref="IRequestValidator{TRequest}"/> before the handler.
/// </summary>
/// <typeparam name="TRequest">Request type bound from the route handler signature.</typeparam>
public sealed class RequestValidationFilter<TRequest>(IRequestValidator<TRequest> validator) : IEndpointFilter
{
    /// <inheritdoc />
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var request = context.Arguments.OfType<TRequest>().FirstOrDefault();
        if (request is null)
        {
            return await next(context);
        }

        var validation = validator.Validate(request);
        var problem = ValidationProblemResults.FromResult(validation);
        if (problem is not null)
        {
            return problem;
        }

        return await next(context);
    }
}

/// <summary>
/// Registers request validation filters on minimal API routes.
/// </summary>
public static class RequestValidationFilterExtensions
{
    /// <summary>
    /// Adds <see cref="RequestValidationFilter{TRequest}"/> to the route pipeline.
    /// </summary>
    public static RouteHandlerBuilder WithRequestValidation<TRequest>(this RouteHandlerBuilder builder) =>
        builder.AddEndpointFilter<RequestValidationFilter<TRequest>>();
}
