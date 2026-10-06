using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TenancyHub.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for EF Core CLI migrations (local developer machines only).
/// </summary>
/// <remarks>
/// Aspire injects <c>ConnectionStrings__tenancyhub</c> / <c>TENANCYHUB_URI</c> for the AppHost
/// <c>tenancyhub-migrations</c> resource. Set <c>TENANCYHUB_DESIGN_CONNECTION</c> to override when
/// running <c>dotnet ef</c> outside Aspire.
/// </remarks>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<TenancyHubDbContext>
{
    /// <inheritdoc />
    public TenancyHubDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__tenancyhub")
            ?? Environment.GetEnvironmentVariable("TENANCYHUB_URI")
            ?? Environment.GetEnvironmentVariable("TENANCYHUB_DESIGN_CONNECTION")
            ?? "Host=localhost;Database=tenancyhub_design;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<TenancyHubDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        return new TenancyHubDbContext(optionsBuilder.Options);
    }
}
