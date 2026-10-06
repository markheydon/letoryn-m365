namespace Letoryn.ApiService.Infrastructure;

/// <summary>
/// Registers agency-scoped minimal API groups with shared authorization filters (FR-003, T048).
/// </summary>
public static class AgencyEndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps a routine agency-scoped route group under <c>/api/v1/agencies</c>.
    /// </summary>
    public static RouteGroupBuilder MapAgencyScopedApiGroup(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGroup("/api/v1/agencies")
            .RequireAuthorization()
            .AddEndpointFilter<AgencyScopedAuthorizationFilter>();
    }

    /// <summary>
    /// Maps operator cross-agency routes under <c>/api/v1/operator/agencies</c>.
    /// </summary>
    public static RouteGroupBuilder MapOperatorAgencyApiGroup(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGroup("/api/v1/operator/agencies")
            .RequireAuthorization()
            .AddEndpointFilter<OperatorAgencyAuthorizationFilter>();
    }
}
