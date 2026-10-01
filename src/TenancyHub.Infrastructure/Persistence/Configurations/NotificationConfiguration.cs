using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TenancyHub.Domain.Notifications;

namespace TenancyHub.Infrastructure.Persistence.Configurations;

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
    }
}
