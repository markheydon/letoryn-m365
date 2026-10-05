using Microsoft.Extensions.DependencyInjection;
using TenancyHub.Application.Abstractions.Audit;
using TenancyHub.Application.Abstractions.Identities;
using TenancyHub.Application.Abstractions.Me;
using TenancyHub.Application.Abstractions.Notifications;
using TenancyHub.Application.Abstractions.Sessions;
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
        return services;
    }
}
