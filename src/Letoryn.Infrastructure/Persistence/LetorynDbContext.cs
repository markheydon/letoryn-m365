using Letoryn.Domain.Agencies;
using Letoryn.Domain.Audit;
using Letoryn.Domain.Identities;
using Letoryn.Domain.Memberships;
using Letoryn.Domain.Notifications;
using Letoryn.Domain.PlatformOperators;
using Letoryn.Domain.Sessions;
using Microsoft.EntityFrameworkCore;

namespace Letoryn.Infrastructure.Persistence;

/// <summary>
/// EF Core database context for Letoryn platform data (PostgreSQL).
/// </summary>
public sealed class LetorynDbContext(DbContextOptions<LetorynDbContext> options) : DbContext(options)
{
    /// <summary>Agency tenants.</summary>
    public DbSet<Agency> Agencies => Set<Agency>();

    /// <summary>Entra-backed product identities.</summary>
    public DbSet<UserIdentity> UserIdentities => Set<UserIdentity>();

    /// <summary>Agency memberships.</summary>
    public DbSet<AgencyMembership> AgencyMemberships => Set<AgencyMembership>();

    /// <summary>Operator agency assignments.</summary>
    public DbSet<PlatformOperatorAssignment> PlatformOperatorAssignments => Set<PlatformOperatorAssignment>();

    /// <summary>Append-only audit events.</summary>
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();

    /// <summary>In-app notifications.</summary>
    public DbSet<Notification> Notifications => Set<Notification>();

    /// <summary>Per-session metadata.</summary>
    public DbSet<UserSession> UserSessions => Set<UserSession>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LetorynDbContext).Assembly);
    }
}
