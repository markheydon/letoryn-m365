using Letoryn.Application.Abstractions.Operators;
using Letoryn.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Letoryn.Infrastructure.Services;

/// <inheritdoc />
public sealed class OperatorAssignmentStore(LetorynDbContext dbContext) : IOperatorAssignmentStore
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
