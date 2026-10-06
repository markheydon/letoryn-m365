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
            await EnsureEmailAvailableAsync(identity.Id, normalizedEmail, cancellationToken);
            identity.Email = normalizedEmail;
        }

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            var reconciled = await ReconcileAfterUniqueConstraintViolationAsync(
                entraObjectId,
                normalizedEmail,
                identity,
                cancellationToken);
            if (reconciled is null)
            {
                throw;
            }

            identity = reconciled;
        }

        return new EnsuredUserIdentity(
            identity.Id,
            identity.EntraObjectId,
            identity.Email,
            identity.IsPlatformOperator,
            identity.LastUsedAgencyId);
    }

    private async Task EnsureEmailAvailableAsync(
        Guid identityId,
        string normalizedEmail,
        CancellationToken cancellationToken)
    {
        var emailTaken = await dbContext.UserIdentities
            .AnyAsync(u => u.Email == normalizedEmail && u.Id != identityId, cancellationToken);

        if (emailTaken)
        {
            throw new UserIdentityBindingConflictException();
        }
    }

    private async Task<UserIdentity?> ReconcileAfterUniqueConstraintViolationAsync(
        string entraObjectId,
        string normalizedEmail,
        UserIdentity attempted,
        CancellationToken cancellationToken)
    {
        if (dbContext.Entry(attempted).State is EntityState.Added)
        {
            dbContext.Entry(attempted).State = EntityState.Detached;
        }

        var byOid = await dbContext.UserIdentities
            .FirstOrDefaultAsync(u => u.EntraObjectId == entraObjectId, cancellationToken);
        if (byOid is not null)
        {
            return byOid;
        }

        var byEmail = await dbContext.UserIdentities
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);
        if (byEmail is not null)
        {
            if (UserIdentityLinkConstants.IsUnlinkedEntraObjectId(byEmail.EntraObjectId))
            {
                byEmail.EntraObjectId = entraObjectId;
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            else if (!string.Equals(byEmail.EntraObjectId, entraObjectId, StringComparison.Ordinal))
            {
                throw new UserIdentityBindingConflictException();
            }

            return byEmail;
        }

        return null;
    }
}
