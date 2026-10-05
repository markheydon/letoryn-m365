namespace TenancyHub.Application.Abstractions.Operators;

/// <summary>
/// Verifies platform operator agency assignments for cross-agency operator routes (FR-006, FR-003).
/// </summary>
public interface IOperatorAssignmentService
{
    /// <summary>
    /// Returns whether <paramref name="userIdentityId"/> is assigned to <paramref name="agencyId"/> as a platform operator.
    /// </summary>
    Task<bool> IsAssignedAsync(
        Guid userIdentityId,
        Guid agencyId,
        CancellationToken cancellationToken = default);
}
