using Microsoft.EntityFrameworkCore;
using TenancyHub.ApiService.Infrastructure;
using TenancyHub.ApiService.Tenancy;
using TenancyHub.Application.Abstractions.Tenancy;
using TenancyHub.Domain.Agencies;
using TenancyHub.Domain.Memberships;
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
            await next(context);
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

        if (membership is null && !operatorAssigned && !currentUser.IsPlatformOperator)
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

        if (agency.LifecycleStatus == AgencyLifecycleStatus.Archived
            && context.Request.Path.StartsWithSegments("/api/v1/agencies", StringComparison.OrdinalIgnoreCase)
            && !context.Request.Path.StartsWithSegments("/api/v1/operator", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogDebug("Blocked agency-scoped request for archived agency {AgencyId}", agencyId);
            await TenantSafeResults.Forbidden().ExecuteAsync(context);
            return;
        }

        if (membership?.Status == MembershipStatus.Suspended)
        {
            await TenantSafeResults.Forbidden().ExecuteAsync(context);
            return;
        }

        await next(context);
    }
}
