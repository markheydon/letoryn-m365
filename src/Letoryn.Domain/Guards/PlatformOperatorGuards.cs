namespace Letoryn.Domain.Guards;

/// <summary>
/// Platform operator invariants (FR-006).
/// </summary>
public static class PlatformOperatorGuards
{
    /// <summary>
    /// Ensures revoking operator status would not remove the last platform operator.
    /// </summary>
    /// <param name="remainingOperatorCount">Operators with <c>IsPlatformOperator</c> after revoke.</param>
    /// <exception cref="InvalidOperationException">Would remove the last platform operator.</exception>
    public static void EnsureAtLeastOnePlatformOperator(int remainingOperatorCount)
    {
        if (remainingOperatorCount < 1)
        {
            throw new InvalidOperationException("At least one platform operator must remain.");
        }
    }
}
