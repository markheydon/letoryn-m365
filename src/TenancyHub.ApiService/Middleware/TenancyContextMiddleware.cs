using Microsoft.EntityFrameworkCore;
using TenancyHub.ApiService.Infrastructure;
using TenancyHub.ApiService.Tenancy;
using TenancyHub.Application.Abstractions.Tenancy;
using TenancyHub.Application.Agencies;
using TenancyHub.Infrastructure.Persistence;

namespace TenancyHub.ApiService.Middleware;

/// <summary>
/// Resolves active agency context from <c>X-TenancyHub-Agency-Id</c> and membership or operator assignment (FR-002).
/// </summary>
public sealed class TenancyContextMiddleware(
    RequestDelegate next,
    ILogger<TenancyContextMiddleware> logger)
{
    /// <summary>Agency context request header name.</summary>
    public const string AgencyHeaderName = "X-TenancyHub-Agency-Id";

    private static readonly PathString OperatorApiPrefix = new("/api/v1/operator");

    /// <inheritdoc />
    public async Task InvokeAsync(
        HttpContext context,
        AgencyContextAccessor agencyContext,
        ICurrentUser currentUser,
        TenancyHubDbContext dbContext)
    {
        if (!context.Request.Headers.TryGetValue(AgencyHeaderName, out var headerValues)
            || !Guid.TryParse(headerValues.FirstOrDefault(), out var agencyId))
        {
            await next(context);
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

        agencyContext.ActiveAgencyId = agencyId;
        agencyContext.ActiveAgencyLifecycleStatus = agency.LifecycleStatus;
        agencyContext.IsOperatorAssignedToActiveAgency = operatorAssigned;

        if (membership is not null)
        {
            agencyContext.ActiveMembershipId = membership.Id;
            agencyContext.ActiveMembershipStatus = membership.Status;
            agencyContext.ActiveAgencyRole = membership.AgencyRole;
        }

        var isOperatorRoute = context.Request.Path.StartsWithSegments(OperatorApiPrefix, StringComparison.OrdinalIgnoreCase);
        var access = AgencyAccessRules.EvaluateAgencyHeaderAccess(
            agency.LifecycleStatus,
            agencyContext.ActiveMembershipStatus,
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

        await next(context);
    }
}
