namespace TenancyHub.Application.Abstractions.Tenancy;

/// <summary>
/// Authenticated work-account identity for the current request or unit of work (FR-012).
/// </summary>
/// <remarks>
/// Downstream feature modules MUST resolve permissions through <see cref="IAgencyContext"/> and
/// <see cref="Authorization.IAgencyAuthorizationService"/> rather than re-parsing tokens or duplicating
/// membership lookups. Operator status (<see cref="IsPlatformOperator"/>) is separate from agency roles (FR-006).
/// </remarks>
public interface ICurrentUser
{
    /// <summary>
    /// Internal product identity identifier (<c>UserIdentity.Id</c>).
    /// </summary>
    Guid UserIdentityId { get; }

    /// <summary>
    /// Microsoft Entra object id (<c>oid</c>) from the sign-in token.
    /// </summary>
    string EntraObjectId { get; }

    /// <summary>
    /// Normalized work-account email used for sign-in and invite matching (FR-007).
    /// </summary>
    string Email { get; }

    /// <summary>
    /// Whether this identity is a platform operator (FR-006); does not imply agency membership or role.
    /// </summary>
    bool IsPlatformOperator { get; }
}
