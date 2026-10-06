using System.Net;
using Letoryn.Application.Abstractions.Memberships;

namespace Letoryn.Web.Services;

/// <summary>Outcome of a membership roster GET call.</summary>
public enum MembershipRosterAccess
{
    Success,
    Denied,
    Failed,
}

/// <summary>Result of loading the agency membership roster.</summary>
public sealed record MembershipRosterResult(
    MembershipRosterAccess Access,
    IReadOnlyList<MembershipRosterDto> Items)
{
    /// <summary>Successful roster load.</summary>
    public static MembershipRosterResult Success(IReadOnlyList<MembershipRosterDto> items) =>
        new(MembershipRosterAccess.Success, items);

    /// <summary>Caller is not permitted to view the roster.</summary>
    public static MembershipRosterResult Denied() =>
        new(MembershipRosterAccess.Denied, []);

    /// <summary>Request failed for a non-authorization reason.</summary>
    public static MembershipRosterResult Failed() =>
        new(MembershipRosterAccess.Failed, []);
}

/// <summary>Result of a membership mutation API call.</summary>
public sealed record MembershipMutationResult(bool Succeeded, string? ErrorMessage)
{
    /// <summary>Maps an HTTP response to a mutation result.</summary>
    public static async Task<MembershipMutationResult> FromResponseAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return new MembershipMutationResult(true, null);
        }

        var message = response.StatusCode switch
        {
            HttpStatusCode.Forbidden or HttpStatusCode.NotFound =>
                "You do not have permission to perform this action.",
            HttpStatusCode.Conflict => "The request conflicts with existing data.",
            HttpStatusCode.BadRequest => "The request could not be processed. Check the details and try again.",
            _ => "Something went wrong. Try again later.",
        };

        if (response.Content.Headers.ContentLength is not 0)
        {
            try
            {
                var problem = await response.Content.ReadFromJsonAsync<ApiProblemDetails>(cancellationToken);
                if (!string.IsNullOrWhiteSpace(problem?.Detail))
                {
                    message = problem.Detail;
                }
            }
            catch
            {
                // Use generic message when ProblemDetails cannot be parsed.
            }
        }

        return new MembershipMutationResult(false, message);
    }

    private sealed record ApiProblemDetails(string? Detail);
}

/// <summary>Request body for invite and provision membership routes.</summary>
public sealed record MembershipInviteRequest(string Email, string Role);

/// <summary>Request body for PATCH membership role.</summary>
public sealed record MembershipChangeRoleRequest(string Role);
