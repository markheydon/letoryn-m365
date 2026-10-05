using Microsoft.EntityFrameworkCore;
using TenancyHub.Application.Abstractions.Operators;
using TenancyHub.Infrastructure.Persistence;

namespace TenancyHub.Infrastructure.Services;

/// <inheritdoc />
public sealed class OperatorAssignmentStore(TenancyHubDbContext dbContext) : IOperatorAssignmentStore
{
    /// <inheritdoc />
    public Task<bool> IsAssignedAsync(
        Guid userIdentityId,
        Guid agencyId,
        CancellationToken cancellationToken = default) =>
        dbContext.PlatformOperatorAssignments
            .AsNoTracking()
            .AnyAsync(
                a => a.UserIdentityId == userIdentityId && a.AgencyId == agencyId,
                cancellationToken);
}
