using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TenancyHub.Domain.Agencies;

namespace TenancyHub.Infrastructure.Persistence.Configurations;

/// <summary>EF configuration for <see cref="Agency"/>.</summary>
public sealed class AgencyConfiguration : IEntityTypeConfiguration<Agency>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Agency> builder)
    {
        builder.ToTable("Agencies");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.DisplayName).HasMaxLength(256).IsRequired();
        builder.Property(a => a.PrimaryContactEmail).HasMaxLength(320).IsRequired();
        builder.Property(a => a.PrimaryContactPhone).HasMaxLength(64).IsRequired();
        builder.Property(a => a.LifecycleStatus).HasConversion<string>().HasMaxLength(32);
    }
}
