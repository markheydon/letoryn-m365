using Letoryn.ApiService.Infrastructure;
using Letoryn.Application.Abstractions.Operators;
using Letoryn.Application.Abstractions.Tenancy;

namespace Letoryn.ApiService.Endpoints;

/// <summary>Operator diagnostics summary routes.</summary>
public static class OperatorDiagnosticsEndpoints
{
    /// <summary>Maps operator diagnostics routes.</summary>
    public static IEndpointRouteBuilder MapOperatorDiagnosticsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapOperatorAgencyApiGroup();
        group.MapGet("/{agencyId:guid}/summary", GetSummaryAsync);
        return endpoints;
    }

    private static async Task<IResult> GetSummaryAsync(
        Guid agencyId,
        ICurrentUser currentUser,
        IOperatorDiagnosticsService diagnostics,
        CancellationToken cancellationToken)
    {
        var summary = await diagnostics.GetSummaryAsync(
            currentUser.UserIdentityId,
            agencyId,
            cancellationToken);

        return summary is null
            ? TenantSafeResults.NotFoundOrForbidden()
            : Results.Ok(summary);
    }
}
