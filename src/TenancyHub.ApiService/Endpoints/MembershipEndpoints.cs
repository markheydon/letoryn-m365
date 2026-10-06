using TenancyHub.ApiService.Infrastructure;
using TenancyHub.Application.Abstractions.Memberships;
using TenancyHub.Application.Abstractions.Tenancy;
using TenancyHub.Application.Memberships;
using TenancyHub.Domain.Memberships;

namespace TenancyHub.ApiService.Endpoints;

/// <summary>Agency membership management routes.</summary>
public static class MembershipEndpoints
{
    /// <summary>Maps membership routes under agencies.</summary>
    public static IEndpointRouteBuilder MapMembershipEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapAgencyScopedApiGroup();
        group.MapGet("/{agencyId:guid}/memberships", ListRosterAsync);
        group.MapPost("/{agencyId:guid}/memberships/invite", InviteAsync);
        group.MapPost("/{agencyId:guid}/memberships/provision", ProvisionAsync);
        group.MapPatch("/{agencyId:guid}/memberships/{membershipId:guid}/role", ChangeRoleAsync);
        group.MapPost("/{agencyId:guid}/memberships/{membershipId:guid}/suspend", SuspendAsync);
        group.MapPost("/{agencyId:guid}/memberships/{membershipId:guid}/reactivate", ReactivateAsync);
        group.MapDelete("/{agencyId:guid}/memberships/{membershipId:guid}", RemoveAsync);
        group.MapDelete("/{agencyId:guid}/memberships/{membershipId:guid}/invitation", RevokeInviteAsync);

        return endpoints;
    }

    private static async Task<IResult> ListRosterAsync(
        Guid agencyId,
        IAgencyContext agencyContext,
        IMembershipOperations operations,
        CancellationToken cancellationToken)
    {
        if (!MembershipAccessHelper.CanViewRoster(agencyContext))
        {
            return TenantSafeResults.NotFoundOrForbidden();
        }

        var roster = await operations.ListRosterAsync(agencyId, true, cancellationToken);
        return Results.Ok(roster ?? []);
    }

    private static async Task<IResult> InviteAsync(
        Guid agencyId,
        InviteMemberRequest request,
        ICurrentUser currentUser,
        IAgencyContext agencyContext,
        InviteMemberHandler handler,
        CancellationToken cancellationToken)
    {
        if (!MembershipAccessHelper.CanManageMemberships(agencyContext))
        {
            return TenantSafeResults.NotFoundOrForbidden();
        }

        if (!Enum.TryParse<AgencyRole>(request.Role, ignoreCase: true, out var role))
        {
            return Results.Problem(title: "Validation failed", detail: "Role is invalid.", statusCode: 400);
        }

        var result = await handler.HandleAsync(
            currentUser.UserIdentityId,
            agencyId,
            request.Email,
            role,
            MembershipAccessHelper.IsActiveAgencyAdministrator(agencyContext),
            agencyContext.IsOperatorAssignedToActiveAgency,
            cancellationToken);

        return result.ToHttpResult();
    }

    private static async Task<IResult> ProvisionAsync(
        Guid agencyId,
        InviteMemberRequest request,
        ICurrentUser currentUser,
        IAgencyContext agencyContext,
        ProvisionMemberHandler handler,
        CancellationToken cancellationToken)
    {
        if (!MembershipAccessHelper.CanManageMemberships(agencyContext))
        {
            return TenantSafeResults.NotFoundOrForbidden();
        }

        if (!Enum.TryParse<AgencyRole>(request.Role, ignoreCase: true, out var role))
        {
            return Results.Problem(title: "Validation failed", detail: "Role is invalid.", statusCode: 400);
        }

        var result = await handler.HandleAsync(
            currentUser.UserIdentityId,
            agencyId,
            request.Email,
            role,
            MembershipAccessHelper.IsActiveAgencyAdministrator(agencyContext),
            agencyContext.IsOperatorAssignedToActiveAgency,
            cancellationToken);

        return result.ToHttpResult();
    }

    private static async Task<IResult> ChangeRoleAsync(
        Guid agencyId,
        Guid membershipId,
        ChangeRoleRequest request,
        ICurrentUser currentUser,
        IAgencyContext agencyContext,
        IMembershipOperations operations,
        CancellationToken cancellationToken)
    {
        if (!MembershipAccessHelper.CanManageMemberships(agencyContext))
        {
            return TenantSafeResults.NotFoundOrForbidden();
        }

        if (!Enum.TryParse<AgencyRole>(request.Role, ignoreCase: true, out var role))
        {
            return Results.Problem(title: "Validation failed", detail: "Role is invalid.", statusCode: 400);
        }

        var result = await operations.ChangeRoleAsync(
            currentUser.UserIdentityId,
            agencyId,
            membershipId,
            role,
            MembershipAccessHelper.IsActiveAgencyAdministrator(agencyContext),
            agencyContext.IsOperatorAssignedToActiveAgency,
            cancellationToken);

        return result.ToHttpResult();
    }

    private static async Task<IResult> SuspendAsync(
        Guid agencyId,
        Guid membershipId,
        ICurrentUser currentUser,
        IAgencyContext agencyContext,
        IMembershipOperations operations,
        CancellationToken cancellationToken)
    {
        if (!MembershipAccessHelper.CanManageMemberships(agencyContext))
        {
            return TenantSafeResults.NotFoundOrForbidden();
        }

        var result = await operations.SuspendMemberAsync(
            currentUser.UserIdentityId,
            agencyId,
            membershipId,
            MembershipAccessHelper.IsActiveAgencyAdministrator(agencyContext),
            agencyContext.IsOperatorAssignedToActiveAgency,
            cancellationToken);

        return result.ToHttpResult();
    }

    private static async Task<IResult> ReactivateAsync(
        Guid agencyId,
        Guid membershipId,
        ICurrentUser currentUser,
        IAgencyContext agencyContext,
        IMembershipOperations operations,
        CancellationToken cancellationToken)
    {
        if (!MembershipAccessHelper.CanManageMemberships(agencyContext))
        {
            return TenantSafeResults.NotFoundOrForbidden();
        }

        var result = await operations.ReactivateMemberAsync(
            currentUser.UserIdentityId,
            agencyId,
            membershipId,
            MembershipAccessHelper.IsActiveAgencyAdministrator(agencyContext),
            agencyContext.IsOperatorAssignedToActiveAgency,
            cancellationToken);

        return result.ToHttpResult();
    }

    private static async Task<IResult> RemoveAsync(
        Guid agencyId,
        Guid membershipId,
        ICurrentUser currentUser,
        IAgencyContext agencyContext,
        IMembershipOperations operations,
        CancellationToken cancellationToken)
    {
        if (!MembershipAccessHelper.CanManageMemberships(agencyContext))
        {
            return TenantSafeResults.NotFoundOrForbidden();
        }

        var result = await operations.RemoveMemberAsync(
            currentUser.UserIdentityId,
            agencyId,
            membershipId,
            MembershipAccessHelper.IsActiveAgencyAdministrator(agencyContext),
            agencyContext.IsOperatorAssignedToActiveAgency,
            cancellationToken);

        return result.ToHttpResult();
    }

    private static async Task<IResult> RevokeInviteAsync(
        Guid agencyId,
        Guid membershipId,
        ICurrentUser currentUser,
        IAgencyContext agencyContext,
        IMembershipOperations operations,
        CancellationToken cancellationToken)
    {
        if (!MembershipAccessHelper.CanManageMemberships(agencyContext))
        {
            return TenantSafeResults.NotFoundOrForbidden();
        }

        var result = await operations.RevokeInvitationAsync(
            currentUser.UserIdentityId,
            agencyId,
            membershipId,
            MembershipAccessHelper.IsActiveAgencyAdministrator(agencyContext),
            agencyContext.IsOperatorAssignedToActiveAgency,
            cancellationToken);

        return result.ToHttpResult();
    }

    private sealed record InviteMemberRequest(string Email, string Role);

    private sealed record ChangeRoleRequest(string Role);
}
