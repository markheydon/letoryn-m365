using Microsoft.AspNetCore.Diagnostics;
using TenancyHub.Application.Identities;

namespace TenancyHub.ApiService.Infrastructure;

/// <summary>
/// Maps unhandled exceptions to generic ProblemDetails without leaking internal details (FR-013).
/// </summary>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    /// <inheritdoc />
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is UserIdentityBindingConflictException)
        {
            var conflict = Results.Problem(
                title: "Account binding conflict",
                detail: exception.Message,
                statusCode: StatusCodes.Status403Forbidden,
                type: "https://tools.ietf.org/html/rfc9110#section-15.5.4");
            await conflict.ExecuteAsync(httpContext);
            return true;
        }

        logger.LogError(
            exception,
            "Unhandled exception processing {Method} {Path}",
            httpContext.Request.Method,
            httpContext.Request.Path);

        var problem = Results.Problem(
            title: "An error occurred",
            detail: "An unexpected error occurred while processing your request.",
            statusCode: StatusCodes.Status500InternalServerError,
            type: "https://tools.ietf.org/html/rfc9110#section-15.6.1");

        await problem.ExecuteAsync(httpContext);
        return true;
    }
}
