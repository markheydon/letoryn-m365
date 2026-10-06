namespace Letoryn.Application.Abstractions.Tenancy;

/// <summary>Shared HTTP header names for agency and session context.</summary>
public static class TenancyHttpHeaders
{
    /// <summary>Active agency context header (FR-002).</summary>
    public const string AgencyId = "X-Letoryn-Agency-Id";

    /// <summary>Server-side session correlation header (FR-001).</summary>
    public const string SessionId = "X-Letoryn-Session-Id";

    /// <summary>Signals interactive sign-in completed and a new server session may be created.</summary>
    public const string EstablishSession = "X-Letoryn-Establish-Session";

    /// <summary>Shared secret header for trusted Web-to-API audit calls.</summary>
    public const string InternalAuditKey = "X-Letoryn-Internal-Audit-Key";
}
