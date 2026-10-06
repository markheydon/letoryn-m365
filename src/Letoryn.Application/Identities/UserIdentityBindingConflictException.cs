namespace Letoryn.Application.Identities;

/// <summary>
/// Thrown when an Entra sign-in cannot be linked to an existing identity row (email already bound to another directory account).
/// </summary>
public sealed class UserIdentityBindingConflictException : Exception
{
    /// <summary>Creates the exception.</summary>
    public UserIdentityBindingConflictException()
        : base("The signed-in directory account does not match the existing identity for this email.")
    {
    }
}
