namespace TenancyHub.Web.Services;

/// <summary>Short-lived cookie set after interactive Entra sign-in to allow one server session bootstrap.</summary>
public static class SessionEstablishmentCookie
{
    /// <summary>Cookie name on the Web host.</summary>
    public const string Name = "TenancyHub.EstablishSession";

    /// <summary>Cookie value indicating session establishment is permitted.</summary>
    public const string Value = "1";
}
