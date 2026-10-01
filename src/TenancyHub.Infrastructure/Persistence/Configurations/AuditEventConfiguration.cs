using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TenancyHub.Domain.Agencies;
using TenancyHub.Domain.Audit;
using TenancyHub.Domain.Identities;

namespace TenancyHub.Infrastructure.Persistence.Configurations;

/// <summary>EF configuration for <see cref="AuditEvent"/>.</summary>
public sealed class AuditEventConfiguration : IEntityTypeConfiguration<AuditEvent>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AuditEvent> builder)
    {
        builder.ToTable("AuditEvents");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ActionType).HasMaxLength(128).IsRequired();
        builder.Property(e => e.Summary).HasMaxLength(1024).IsRequired();
        builder.Property(e => e.PayloadJson).HasColumnType("jsonb");
        builder.HasIndex(e => new { e.AgencyId, e.OccurredAt });
        builder.HasOne<Agency>()
            .WithMany()
            .HasForeignKey(e => e.AgencyId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<UserIdentity>()
            .WithMany()
            .HasForeignKey(e => e.ActorUserIdentityId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<UserIdentity>()
            .WithMany()
            .HasForeignKey(e => e.TargetUserIdentityId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
