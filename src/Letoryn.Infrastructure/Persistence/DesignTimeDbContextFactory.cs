using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Letoryn.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for EF Core CLI migrations (local developer machines only).
/// </summary>
/// <remarks>
/// Aspire injects <c>ConnectionStrings__letoryn</c> / <c>LETORYN_URI</c> for the AppHost
/// <c>letoryn-migrations</c> resource. Set <c>LETORYN_DESIGN_CONNECTION</c> to override when
/// running <c>dotnet ef</c> outside Aspire.
/// </remarks>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<LetorynDbContext>
{
    /// <inheritdoc />
    public LetorynDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__letoryn")
            ?? Environment.GetEnvironmentVariable("LETORYN_URI")
            ?? Environment.GetEnvironmentVariable("LETORYN_DESIGN_CONNECTION")
            ?? "Host=localhost;Database=letoryn_design;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<LetorynDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        return new LetorynDbContext(optionsBuilder.Options);
    }
}
