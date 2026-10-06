namespace Letoryn.Domain.Identities;

/// <summary>Conventions for linking Entra directory accounts to product identities.</summary>
public static class UserIdentityLinkConstants
{
    /// <summary>Prefix for placeholder Entra object ids created before first sign-in.</summary>
    public const string UnlinkedEntraObjectIdPrefix = "unlinked:";

    /// <summary>Returns whether <paramref name="entraObjectId"/> is a pre-sign-in placeholder.</summary>
    public static bool IsUnlinkedEntraObjectId(string entraObjectId) =>
        entraObjectId.StartsWith(UnlinkedEntraObjectIdPrefix, StringComparison.Ordinal);
}
