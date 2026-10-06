namespace Letoryn.Web.Services;

/// <summary>Outcome of a platform operator management API call.</summary>
public sealed record PlatformOperatorActionResult(bool Succeeded, string? ErrorMessage)
{
    /// <summary>Successful action.</summary>
    public static PlatformOperatorActionResult Success { get; } = new(true, null);

    /// <summary>Failed action with a user-visible message.</summary>
    public static PlatformOperatorActionResult Failed(string? message) =>
        new(false, message ?? "We could not complete that action. Please try again.");
}

/// <summary>Request body for PUT platform operator assignments.</summary>
public sealed record SetPlatformOperatorAssignmentsRequest(IReadOnlyList<Guid> AgencyIds);
