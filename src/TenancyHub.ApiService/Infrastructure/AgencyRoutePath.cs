namespace TenancyHub.ApiService.Infrastructure;

/// <summary>
/// Parses agency identifiers from versioned API paths for tenancy enforcement (T050).
/// </summary>
public static class AgencyRoutePath
{
    private const string AgenciesSegment = "agencies";

    /// <summary>
    /// When the path is a routine agency-scoped route (<c>/api/v1/agencies/{agencyId}/...</c>), returns the route agency id.
    /// Operator cross-agency routes are excluded.
    /// </summary>
    public static bool TryGetRoutineAgencyId(PathString path, out Guid agencyId)
    {
        agencyId = default;
        if (!path.HasValue)
        {
            return false;
        }

        var segments = path.Value!.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length < 4)
        {
            return false;
        }

        if (!string.Equals(segments[0], "api", StringComparison.OrdinalIgnoreCase)
            || !string.Equals(segments[1], "v1", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (string.Equals(segments[2], "operator", StringComparison.OrdinalIgnoreCase))
        {
            if (segments.Length >= 5
                && string.Equals(segments[3], AgenciesSegment, StringComparison.OrdinalIgnoreCase)
                && Guid.TryParse(segments[4], out agencyId))
            {
                return false;
            }

            return false;
        }

        if (!string.Equals(segments[2], AgenciesSegment, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return Guid.TryParse(segments[3], out agencyId);
    }
}
