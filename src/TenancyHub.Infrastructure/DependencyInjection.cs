using Microsoft.Extensions.DependencyInjection;
using TenancyHub.Application.Abstractions.Audit;
using TenancyHub.Application.Abstractions.Notifications;
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
        return services;
    }
}
