namespace Letoryn.Domain.Memberships;

/// <summary>
/// Links a <see cref="Identities.UserIdentity"/> to an <see cref="Agencies.Agency"/> with role and status.
/// </summary>
public class AgencyMembership
{
    /// <summary>Primary key.</summary>
    public Guid Id { get; set; }

    /// <summary>Agency tenant foreign key.</summary>
    public Guid AgencyId { get; set; }

    /// <summary>User identity foreign key.</summary>
    public Guid UserIdentityId { get; set; }

    /// <summary>Current membership status.</summary>
    public MembershipStatus Status { get; set; }

    /// <summary>Agency-scoped role for this membership.</summary>
    public AgencyRole AgencyRole { get; set; }

    /// <summary>When the invite was created, if <see cref="Status"/> is <see cref="MembershipStatus.Invited"/>.</summary>
    public DateTimeOffset? InvitedAt { get; set; }

    /// <summary>Role captured at invite time while status is invited.</summary>
    public AgencyRole? InvitedRoleSnapshot { get; set; }

    /// <summary>Invite expiry (typically invite created time plus 30 days).</summary>
    public DateTimeOffset? ExpiresAt { get; set; }

    /// <summary>When the member activated, if applicable.</summary>
    public DateTimeOffset? ActivatedAt { get; set; }

    /// <summary>When the membership was last updated (UTC).</summary>
    public DateTimeOffset UpdatedAt { get; set; }
}
