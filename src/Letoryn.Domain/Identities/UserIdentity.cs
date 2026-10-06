namespace Letoryn.Domain.Identities;

/// <summary>
/// Product identity mapped from Microsoft Entra object id and email.
/// </summary>
public class UserIdentity
{
    /// <summary>Internal primary key.</summary>
    public Guid Id { get; set; }

    /// <summary>Entra object id from token <c>oid</c>; unique.</summary>
    public string EntraObjectId { get; set; } = string.Empty;

    /// <summary>Normalized email used for sign-in and invite matching.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Whether this user is a platform operator (separate from agency roles).</summary>
    public bool IsPlatformOperator { get; set; }

    /// <summary>Last agency used in the shell, for default context.</summary>
    public Guid? LastUsedAgencyId { get; set; }

    /// <summary>When the identity was first provisioned (UTC).</summary>
    public DateTimeOffset CreatedAt { get; set; }
}
