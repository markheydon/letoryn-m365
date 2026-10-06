using Letoryn.Domain.Identities;
using Letoryn.Domain.Sessions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Letoryn.Infrastructure.Persistence.Configurations;

/// <summary>EF configuration for <see cref="UserSession"/>.</summary>
public sealed class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.ToTable("UserSessions");
        builder.HasKey(s => s.Id);
        builder.HasIndex(s => s.UserIdentityId);
        builder.HasOne<UserIdentity>()
            .WithMany()
            .HasForeignKey(s => s.UserIdentityId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
