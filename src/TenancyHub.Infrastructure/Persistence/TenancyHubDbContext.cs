using Microsoft.EntityFrameworkCore;
using TenancyHub.Domain.Agencies;
using TenancyHub.Domain.Audit;
using TenancyHub.Domain.Identities;
using TenancyHub.Domain.Memberships;
using TenancyHub.Domain.Notifications;
using TenancyHub.Domain.PlatformOperators;
using TenancyHub.Domain.Sessions;

namespace TenancyHub.Infrastructure.Persistence;

/// <summary>
/// EF Core database context for Tenancy Hub platform data (PostgreSQL).
/// </summary>
public sealed class TenancyHubDbContext(DbContextOptions<TenancyHubDbContext> options) : DbContext(options)
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
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TenancyHubDbContext).Assembly);
    }
}
