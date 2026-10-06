using Letoryn.Application.Abstractions.Agencies;
using Letoryn.Application.Abstractions.Audit;
using Letoryn.Application.Abstractions.Identities;
using Letoryn.Application.Abstractions.Me;
using Letoryn.Application.Abstractions.Memberships;
using Letoryn.Application.Abstractions.Notifications;
using Letoryn.Application.Abstractions.Operators;
using Letoryn.Application.Abstractions.Sessions;
using Letoryn.Application.Memberships;
using Letoryn.Application.Notifications;
using Letoryn.Application.Operators;
using Letoryn.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Letoryn.Infrastructure;

/// <summary>Infrastructure service registration.</summary>
public static class DependencyInjection
{
    /// <summary>Registers EF-backed writers and supporting services.</summary>
    public static IServiceCollection AddLetorynInfrastructure(this IServiceCollection services)
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
