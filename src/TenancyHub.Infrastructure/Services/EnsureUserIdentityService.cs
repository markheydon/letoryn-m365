using Microsoft.EntityFrameworkCore;
using TenancyHub.Application.Abstractions.Identities;
using TenancyHub.Application.Identities;
using TenancyHub.Domain.Identities;
using TenancyHub.Infrastructure.Persistence;

namespace TenancyHub.Infrastructure.Services;

/// <inheritdoc />
public sealed class EnsureUserIdentityService(TenancyHubDbContext dbContext) : IEnsureUserIdentityService
{
    /// <inheritdoc />
    public async Task<EnsuredUserIdentity> EnsureAsync(
        string entraObjectId,
        string email,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var identity = await dbContext.UserIdentities
            .FirstOrDefaultAsync(u => u.EntraObjectId == entraObjectId, cancellationToken);

        if (identity is null)
        {
            identity = await dbContext.UserIdentities
                .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

            if (identity is not null)
            {
                if (UserIdentityLinkConstants.IsUnlinkedEntraObjectId(identity.EntraObjectId))
                {
                    identity.EntraObjectId = entraObjectId;
                }
                else if (!string.Equals(identity.EntraObjectId, entraObjectId, StringComparison.Ordinal))
                {
                    throw new UserIdentityBindingConflictException();
                }
            }
            else
            {
                identity = new UserIdentity
                {
                    Id = Guid.NewGuid(),
                    EntraObjectId = entraObjectId,
                    Email = normalizedEmail,
                    CreatedAt = DateTimeOffset.UtcNow,
                };
                dbContext.UserIdentities.Add(identity);
            }
        }
        else if (!string.Equals(identity.Email, normalizedEmail, StringComparison.Ordinal))
        {
            identity.Email = normalizedEmail;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return new EnsuredUserIdentity(
            identity.Id,
            identity.EntraObjectId,
            identity.Email,
            identity.IsPlatformOperator,
            identity.LastUsedAgencyId);
    }
}
