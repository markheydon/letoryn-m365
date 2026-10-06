using Letoryn.Domain.Memberships;

namespace Letoryn.Application.Memberships;

/// <summary>Invite expiry and lazy evaluation rules (30-day window).</summary>
public static class InvitationRules
{
    /// <summary>Pending invite lifetime from creation.</summary>
    public static readonly TimeSpan InviteLifetime = TimeSpan.FromDays(30);

    /// <summary>Computes invite expiry from invite creation time.</summary>
    public static DateTimeOffset ComputeExpiresAt(DateTimeOffset invitedAt) => invitedAt.Add(InviteLifetime);

    /// <summary>Returns whether an invited membership has passed its expiry.</summary>
    public static bool IsExpired(AgencyMembership membership, DateTimeOffset utcNow)
    {
        if (membership.Status != MembershipStatus.Invited)
        {
            return false;
        }

        if (membership.ExpiresAt is DateTimeOffset expiresAt)
        {
            return utcNow > expiresAt;
        }

        if (membership.InvitedAt is DateTimeOffset invitedAt)
        {
            return utcNow > ComputeExpiresAt(invitedAt);
        }

        return false;
    }
}
