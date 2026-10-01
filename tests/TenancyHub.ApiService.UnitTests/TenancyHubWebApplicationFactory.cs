using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using TenancyHub.Infrastructure.Persistence;

namespace TenancyHub.ApiService.UnitTests;

/// <summary>Test host with in-memory EF for health and smoke tests.</summary>
public sealed class TenancyHubWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = Guid.NewGuid().ToString("N");

    /// <inheritdoc />
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting(WebHostDefaults.EnvironmentKey, Environments.Development);
        builder.ConfigureTestServices(ReplaceTenancyHubDbContextWithInMemory);
    }

    private void ReplaceTenancyHubDbContextWithInMemory(IServiceCollection services)
    {
        foreach (var descriptor in services.ToList())
        {
            var serviceType = descriptor.ServiceType;
            if (serviceType == typeof(TenancyHubDbContext)
                || serviceType == typeof(DbContextOptions<TenancyHubDbContext>)
                || (serviceType.IsGenericType
                    && serviceType.GetGenericArguments().Any(t => t == typeof(TenancyHubDbContext))))
            {
                services.Remove(descriptor);
            }
        }

        services.RemoveAll<TenancyHubDbContext>();
        services.RemoveAll<DbContextOptions<TenancyHubDbContext>>();
        services.AddDbContext<TenancyHubDbContext>(options =>
            options.UseInMemoryDatabase(_databaseName));
    }
}
