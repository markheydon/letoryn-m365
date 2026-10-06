namespace Letoryn.Application.Abstractions.Audit;

/// <summary>
/// Application seam for recording security-relevant audit events (FR-008, FR-012).
/// </summary>
/// <remarks>
/// Infrastructure implementations append to durable storage (FR-014). Callers remain in the application or API layers;
/// feature modules MUST NOT write audit rows directly to the database.
/// </remarks>
public interface IAuditWriter
{
    /// <summary>
    /// Persists a single audit event.
    /// </summary>
    /// <param name="auditEvent">Event payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task WriteAsync(AuditEventWrite auditEvent, CancellationToken cancellationToken = default);
}
