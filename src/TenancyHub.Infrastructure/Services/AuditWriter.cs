using TenancyHub.Application.Abstractions.Audit;
using TenancyHub.Domain.Audit;
using TenancyHub.Infrastructure.Persistence;

namespace TenancyHub.Infrastructure.Services;

/// <summary>EF-backed append-only audit writer.</summary>
public sealed class AuditWriter(TenancyHubDbContext dbContext) : IAuditWriter
{
    /// <inheritdoc />
    public async Task WriteAsync(AuditEventWrite auditEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(auditEvent);

        var entity = new AuditEvent
        {
            Id = Guid.NewGuid(),
            AgencyId = auditEvent.AgencyId,
            OccurredAt = DateTimeOffset.UtcNow,
            ActorUserIdentityId = auditEvent.ActorUserIdentityId,
            ActionType = auditEvent.ActionType,
            Summary = auditEvent.Summary,
            TargetUserIdentityId = auditEvent.TargetUserIdentityId,
            PayloadJson = auditEvent.PayloadJson,
        };

        dbContext.AuditEvents.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
