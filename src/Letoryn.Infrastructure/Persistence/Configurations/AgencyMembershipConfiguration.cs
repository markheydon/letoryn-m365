using Letoryn.Domain.Agencies;
using Letoryn.Domain.Identities;
using Letoryn.Domain.Memberships;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Letoryn.Infrastructure.Persistence.Configurations;

/// <summary>EF configuration for <see cref="AgencyMembership"/>.</summary>
public sealed class AgencyMembershipConfiguration : IEntityTypeConfiguration<AgencyMembership>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AgencyMembership> builder)
    {
        builder.ToTable("AgencyMemberships");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(m => m.AgencyRole).HasConversion<string>().HasMaxLength(32);
        builder.Property(m => m.InvitedRoleSnapshot).HasConversion<string>().HasMaxLength(32);
        builder.HasIndex(m => new { m.AgencyId, m.UserIdentityId }).IsUnique();
        builder.HasIndex(m => new { m.AgencyId, m.Status });
        builder.HasIndex(m => new { m.UserIdentityId, m.Status });
        builder.HasOne<Agency>()
            .WithMany()
            .HasForeignKey(m => m.AgencyId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<UserIdentity>()
            .WithMany()
            .HasForeignKey(m => m.UserIdentityId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
