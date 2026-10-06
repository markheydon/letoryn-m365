using Letoryn.Application.Abstractions.Memberships;

namespace Letoryn.ApiService.Infrastructure;

/// <summary>Maps membership operation results to HTTP responses.</summary>
public static class MembershipOperationHttpExtensions
{
    /// <summary>Converts a membership operation result to an HTTP result.</summary>
    public static IResult ToHttpResult(this MembershipOperationResult result) =>
        result.Status switch
        {
            MembershipOperationStatus.Succeeded => result.MembershipId is Guid id
                ? Results.Ok(new { membershipId = id })
                : Results.NoContent(),
            MembershipOperationStatus.ValidationFailed => Results.Problem(
                title: "Validation failed",
                detail: result.UserMessage ?? "The request could not be processed.",
                statusCode: StatusCodes.Status400BadRequest),
            MembershipOperationStatus.Forbidden => Results.Problem(
                title: "Forbidden",
                detail: result.UserMessage ?? "You do not have permission to perform this action.",
                statusCode: StatusCodes.Status403Forbidden),
            MembershipOperationStatus.Conflict => Results.Problem(
                title: "Conflict",
                detail: result.UserMessage ?? "The request conflicts with existing data.",
                statusCode: StatusCodes.Status409Conflict),
            _ => TenantSafeResults.NotFoundOrForbidden(),
        };
}
