using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TenancyHub.Infrastructure.Persistence;

namespace TenancyHub.ApiService.UnitTests;

/// <summary>Test host with in-memory EF for health and smoke tests.</summary>
public sealed class TenancyHubWebApplicationFactory : WebApplicationFactory<Program>
{
    /// <inheritdoc />
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting(WebHostDefaults.EnvironmentKey, Environments.Development);
        builder.ConfigureTestServices(services =>
        {
            foreach (var descriptor in services
                         .Where(d => d.ServiceType.FullName?.Contains(nameof(TenancyHubDbContext), StringComparison.Ordinal) == true
                                     || d.ImplementationType?.FullName?.Contains(nameof(TenancyHubDbContext), StringComparison.Ordinal) == true)
                         .ToList())
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<TenancyHubDbContext>(options =>
                options.UseInMemoryDatabase("TenancyHubApiTests"));
        });
    }
}
