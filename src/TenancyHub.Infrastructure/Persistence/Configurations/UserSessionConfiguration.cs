using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TenancyHub.Domain.Sessions;

namespace TenancyHub.Infrastructure.Persistence.Configurations;

/// <summary>EF configuration for <see cref="UserSession"/>.</summary>
public sealed class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.ToTable("UserSessions");
        builder.HasKey(s => s.Id);
        builder.HasIndex(s => s.UserIdentityId);
    }
}
