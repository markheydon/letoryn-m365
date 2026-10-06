using Microsoft.EntityFrameworkCore;
using TenancyHub.ApiService.Infrastructure;
using TenancyHub.ApiService.Tenancy;
using TenancyHub.Application.Abstractions.Tenancy;
using TenancyHub.Application.Agencies;
using TenancyHub.Infrastructure.Persistence;

namespace TenancyHub.ApiService.Middleware;

/// <summary>
/// Resolves active agency context from <see cref="AgencyHeaderName"/> and populates <see cref="AgencyContextAccessor"/> (FR-002).
/// </summary>
/// <remarks>
/// <para>
/// The agency header is optional: requests without it pass through unchanged (global routes such as <c>/api/v1/me</c>).
/// When present, the value must match any agency id embedded in the URL path; mismatches yield a tenant-safe 404/403.
/// </para>
/// <para>
/// Authorization requires an authenticated product identity with either an agency membership or a platform-operator
/// assignment to that agency. Lifecycle and membership status are evaluated via <see cref="AgencyAccessRules"/>;
/// operator API routes under <c>/api/v1/operator</c> follow distinct rules for suspended agencies.
/// </para>
/// <para>
/// On success, downstream handlers read agency id, lifecycle, membership role, and operator-assignment flags from
/// <see cref="AgencyContextAccessor"/>—the header alone is never treated as proof of access.
/// </para>
/// </remarks>
public sealed class TenancyContextMiddleware(
    RequestDelegate next,
    ILogger<TenancyContextMiddleware> logger)
{
    /// <summary>
    /// Agency context request header name (<see cref="TenancyHttpHeaders.AgencyId"/>).
    /// </summary>
    public const string AgencyHeaderName = TenancyHttpHeaders.AgencyId;

    private static readonly PathString OperatorApiPrefix = new("/api/v1/operator");

    /// <inheritdoc />
    public async Task InvokeAsync(
        HttpContext context,
        AgencyContextAccessor agencyContext,
        ICurrentUser currentUser,
        TenancyHubDbContext dbContext)
    {
        if (AgencyRoutePath.TryGetRoutineAgencyId(context.Request.Path, out var routeAgencyId)
            && context.Request.Headers.TryGetValue(AgencyHeaderName, out var routeHeaderValues)
            && Guid.TryParse(routeHeaderValues.FirstOrDefault(), out var headerForRoute)
            && headerForRoute != routeAgencyId)
        {
            await TenantSafeResults.NotFoundOrForbidden().ExecuteAsync(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(AgencyHeaderName, out var headerValues)
            || !Guid.TryParse(headerValues.FirstOrDefault(), out var agencyId))
        {
            await next(context);
            return;
        }

        if (AgencyRoutePath.TryGetRoutineAgencyId(context.Request.Path, out routeAgencyId)
            && routeAgencyId != agencyId)
        {
            await TenantSafeResults.NotFoundOrForbidden().ExecuteAsync(context);
            return;
        }

        if (currentUser.UserIdentityId == Guid.Empty)
        {
            if (context.User.Identity?.IsAuthenticated != true)
            {
                await Results.Problem(
                    title: "Unauthorized",
                    detail: "Authentication is required.",
                    statusCode: StatusCodes.Status401Unauthorized,
                    type: "https://tools.ietf.org/html/rfc9110#section-15.5.2").ExecuteAsync(context);
                return;
            }

            await TenantSafeResults.NotFoundOrForbidden().ExecuteAsync(context);
            return;
        }

        var agency = await dbContext.Agencies
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == agencyId, context.RequestAborted);

        if (agency is null)
        {
            await TenantSafeResults.NotFoundOrForbidden().ExecuteAsync(context);
            return;
        }

        var membership = await dbContext.AgencyMemberships
            .AsNoTracking()
            .FirstOrDefaultAsync(
                m => m.AgencyId == agencyId && m.UserIdentityId == currentUser.UserIdentityId,
                context.RequestAborted);

        var operatorAssigned = await dbContext.PlatformOperatorAssignments
            .AsNoTracking()
            .AnyAsync(
                a => a.AgencyId == agencyId && a.UserIdentityId == currentUser.UserIdentityId,
                context.RequestAborted);

        if (membership is null && !operatorAssigned)
        {
            await TenantSafeResults.NotFoundOrForbidden().ExecuteAsync(context);
            return;
        }

        var isOperatorRoute = context.Request.Path.StartsWithSegments(OperatorApiPrefix, StringComparison.OrdinalIgnoreCase);
        var access = AgencyAccessRules.EvaluateAgencyHeaderAccess(
            agency.LifecycleStatus,
            membership?.Status,
            operatorAssigned,
            isOperatorRoute);

        if (!access.IsAuthorized)
        {
            logger.LogDebug(
                "Blocked agency header request for agency {AgencyId} on path {Path}",
                agencyId,
                context.Request.Path);
            await access.ExecuteFailureAsync(context);
            return;
        }

        agencyContext.ActiveAgencyId = agencyId;
        agencyContext.ActiveAgencyLifecycleStatus = agency.LifecycleStatus;
        agencyContext.IsOperatorAssignedToActiveAgency = operatorAssigned;

        if (membership is not null)
        {
            agencyContext.ActiveMembershipId = membership.Id;
            agencyContext.ActiveMembershipStatus = membership.Status;
            agencyContext.ActiveAgencyRole = membership.AgencyRole;
        }

        await next(context);
    }
}
