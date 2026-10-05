namespace TenancyHub.Web.Services;

/// <summary>Stores the server-side session id returned by the API.</summary>
public sealed class UserSessionState
{
    /// <summary>Current API session id, if established.</summary>
    public Guid? SessionId { get; set; }
}
