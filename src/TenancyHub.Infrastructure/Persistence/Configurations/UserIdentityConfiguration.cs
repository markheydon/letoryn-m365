using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TenancyHub.Domain.Identities;

namespace TenancyHub.Infrastructure.Persistence.Configurations;

/// <summary>EF configuration for <see cref="UserIdentity"/>.</summary>
public sealed class UserIdentityConfiguration : IEntityTypeConfiguration<UserIdentity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<UserIdentity> builder)
    {
        builder.ToTable("UserIdentities");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.EntraObjectId).HasMaxLength(128).IsRequired();
        builder.Property(u => u.Email).HasMaxLength(320).IsRequired();
        builder.HasIndex(u => u.EntraObjectId).IsUnique();
        builder.HasIndex(u => u.Email);
    }
}
