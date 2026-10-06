using Microsoft.Extensions.DependencyInjection;
using TenancyHub.Application.Abstractions.Agencies;
using TenancyHub.Application.Abstractions.Audit;
using TenancyHub.Application.Abstractions.Identities;
using TenancyHub.Application.Abstractions.Me;
using TenancyHub.Application.Abstractions.Memberships;
using TenancyHub.Application.Abstractions.Notifications;
using TenancyHub.Application.Abstractions.Operators;
using TenancyHub.Application.Abstractions.Sessions;
using TenancyHub.Application.Memberships;
using TenancyHub.Application.Notifications;
using TenancyHub.Application.Operators;
using TenancyHub.Infrastructure.Services;

namespace TenancyHub.Infrastructure;

/// <summary>Infrastructure service registration.</summary>
public static class DependencyInjection
{
    /// <summary>Registers EF-backed writers and supporting services.</summary>
    public static IServiceCollection AddTenancyHubInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IAuditWriter, AuditWriter>();
        services.AddScoped<INotificationWriter, NotificationWriter>();
        services.AddScoped<IEnsureUserIdentityService, EnsureUserIdentityService>();
        services.AddScoped<IUserSessionService, UserSessionService>();
        services.AddScoped<IMeProfileService, MeProfileService>();
        services.AddScoped<IOperatorAssignmentStore, OperatorAssignmentStore>();
        services.AddScoped<IOperatorAssignmentService, OperatorAssignmentService>();
        services.AddScoped<IMembershipOperations, MembershipOperationsService>();
        services.AddScoped<IAgencyOperations, AgencyOperationsService>();
        services.AddScoped<IPlatformOperatorManagement, PlatformOperatorManagementService>();
        services.AddScoped<InviteMemberHandler>();
        services.AddScoped<ProvisionMemberHandler>();
        services.AddScoped<IAuditQueryService, AuditQueryService>();
        services.AddScoped<IOperatorDiagnosticsService, OperatorDiagnosticsService>();
        services.AddScoped<INotificationQueryService, NotificationQueryService>();
        services.AddScoped<NotificationTriggerService>();
        services.AddScoped<AgencyNotificationTriggerService>();
        return services;
    }
}
