using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TenancyHub.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for EF Core CLI migrations (local developer machines only).
/// </summary>
/// <remarks>
/// Set <c>TENANCYHUB_DESIGN_CONNECTION</c> to override the default local connection string.
/// </remarks>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<TenancyHubDbContext>
{
    /// <inheritdoc />
    public TenancyHubDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("TENANCYHUB_DESIGN_CONNECTION")
            ?? "Host=localhost;Database=tenancyhub_design;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<TenancyHubDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        return new TenancyHubDbContext(optionsBuilder.Options);
    }
}
