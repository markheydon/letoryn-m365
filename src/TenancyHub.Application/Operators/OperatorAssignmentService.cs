using TenancyHub.Application.Abstractions.Operators;

namespace TenancyHub.Application.Operators;

/// <inheritdoc />
public sealed class OperatorAssignmentService(IOperatorAssignmentStore store) : IOperatorAssignmentService
{
    /// <inheritdoc />
    public Task<bool> IsAssignedAsync(
        Guid userIdentityId,
        Guid agencyId,
        CancellationToken cancellationToken = default) =>
        store.IsAssignedAsync(userIdentityId, agencyId, cancellationToken);
}
