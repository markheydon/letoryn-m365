using Letoryn.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Letoryn.ApiService.UnitTests;

/// <summary>Test host with in-memory EF for health and smoke tests.</summary>
public sealed class LetorynWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = Guid.NewGuid().ToString("N");

    /// <inheritdoc />
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting(WebHostDefaults.EnvironmentKey, Environments.Development);
        builder.ConfigureTestServices(ReplaceLetorynDbContextWithInMemory);
    }

    private void ReplaceLetorynDbContextWithInMemory(IServiceCollection services)
    {
        foreach (var descriptor in services.ToList())
        {
            var serviceType = descriptor.ServiceType;
            if (serviceType == typeof(LetorynDbContext)
                || serviceType == typeof(DbContextOptions<LetorynDbContext>)
                || (serviceType.IsGenericType
                    && serviceType.GetGenericArguments().Any(t => t == typeof(LetorynDbContext))))
            {
                services.Remove(descriptor);
            }
        }

        services.RemoveAll<LetorynDbContext>();
        services.RemoveAll<DbContextOptions<LetorynDbContext>>();
        services.AddDbContext<LetorynDbContext>(options =>
            options.UseInMemoryDatabase(_databaseName));
    }
}
