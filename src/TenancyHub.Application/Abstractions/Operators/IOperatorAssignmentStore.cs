namespace TenancyHub.Application.Abstractions.Operators;

/// <summary>
/// Persistence seam for operator assignment lookups (implemented in Infrastructure).
/// </summary>
public interface IOperatorAssignmentStore
{
    /// <summary>
    /// Returns whether the user has a platform operator assignment to the agency.
    /// </summary>
    Task<bool> IsAssignedAsync(
        Guid userIdentityId,
        Guid agencyId,
        CancellationToken cancellationToken = default);
}
