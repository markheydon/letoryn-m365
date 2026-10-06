namespace TenancyHub.Application.Memberships;

/// <summary>Normalizes work-account emails for invite matching.</summary>
public static class MembershipEmailNormalizer
{
    /// <summary>Normalizes email for storage and comparison.</summary>
    public static string Normalize(string email) => email.Trim().ToLowerInvariant();
}
