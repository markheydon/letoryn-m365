using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TenancyHub.Domain.PlatformOperators;

namespace TenancyHub.Infrastructure.Persistence.Configurations;

/// <summary>EF configuration for <see cref="PlatformOperatorAssignment"/>.</summary>
public sealed class PlatformOperatorAssignmentConfiguration : IEntityTypeConfiguration<PlatformOperatorAssignment>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PlatformOperatorAssignment> builder)
    {
        builder.ToTable("PlatformOperatorAssignments");
        builder.HasKey(a => a.Id);
        builder.HasIndex(a => new { a.UserIdentityId, a.AgencyId }).IsUnique();
    }
}
