using Letoryn.Domain.Agencies;
using Letoryn.Domain.Identities;
using Letoryn.Domain.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Letoryn.Infrastructure.Persistence.Configurations;

/// <summary>EF configuration for <see cref="Notification"/>.</summary>
public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Category).HasMaxLength(128).IsRequired();
        builder.Property(n => n.Title).HasMaxLength(256).IsRequired();
        builder.Property(n => n.Body).HasMaxLength(2048).IsRequired();
        builder.HasIndex(n => new { n.UserIdentityId, n.AgencyId, n.CreatedAt });
        builder.HasOne<Agency>()
            .WithMany()
            .HasForeignKey(n => n.AgencyId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<UserIdentity>()
            .WithMany()
            .HasForeignKey(n => n.UserIdentityId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
