namespace TenancyHub.Application.Abstractions.Tenancy;

/// <summary>Shared HTTP header names for agency and session context.</summary>
public static class TenancyHttpHeaders
{
    /// <summary>Active agency context header (FR-002).</summary>
    public const string AgencyId = "X-TenancyHub-Agency-Id";

    /// <summary>Server-side session correlation header (FR-001).</summary>
    public const string SessionId = "X-TenancyHub-Session-Id";
}
