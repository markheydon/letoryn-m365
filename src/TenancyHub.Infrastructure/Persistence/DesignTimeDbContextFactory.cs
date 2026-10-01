using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TenancyHub.Infrastructure.Persistence;

/// <summary>Design-time factory for EF Core CLI migrations.</summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<TenancyHubDbContext>
{
    /// <inheritdoc />
    public TenancyHubDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TenancyHubDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=tenancyhub_design;Username=postgres;Password=postgres");
        return new TenancyHubDbContext(optionsBuilder.Options);
    }
}
