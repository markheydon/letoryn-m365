namespace Letoryn.Domain.PlatformOperators;

/// <summary>
/// Records a platform operator's assignment to an agency (unique per user and agency).
/// </summary>
public class PlatformOperatorAssignment
{
    /// <summary>Primary key.</summary>
    public Guid Id { get; set; }

    /// <summary>Platform operator user identity.</summary>
    public Guid UserIdentityId { get; set; }

    /// <summary>Agency the operator is assigned to.</summary>
    public Guid AgencyId { get; set; }

    /// <summary>When the assignment was created (UTC).</summary>
    public DateTimeOffset AssignedAt { get; set; }

    /// <summary>User identity that created the assignment.</summary>
    public Guid AssignedByUserIdentityId { get; set; }
}
