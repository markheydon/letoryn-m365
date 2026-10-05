using TenancyHub.Application.Abstractions.Audit;
using TenancyHub.Application.Abstractions.Sessions;

namespace TenancyHub.ApiService.Auth;

/// <summary>Writes FR-008 sign-in and session termination audit events.</summary>
public sealed class SignInAuditService(IAuditWriter auditWriter)
{
    /// <summary>Records a successful sign-in for a new session.</summary>
    public Task WriteSignInSucceededAsync(Guid userIdentityId, CancellationToken cancellationToken) =>
        auditWriter.WriteAsync(
            new AuditEventWrite(
                AgencyId: null,
                ActorUserIdentityId: userIdentityId,
                ActionType: AuditActionTypes.SignInSucceeded,
                Summary: "User signed in successfully."),
            cancellationToken);

    /// <summary>Records a failed authentication attempt.</summary>
    public Task WriteSignInFailedAsync(Guid? userIdentityId, string summary, CancellationToken cancellationToken) =>
        auditWriter.WriteAsync(
            new AuditEventWrite(
                AgencyId: null,
                ActorUserIdentityId: userIdentityId,
                ActionType: AuditActionTypes.SignInFailed,
                Summary: summary),
            cancellationToken);

    /// <summary>Records session termination with a reason-specific summary.</summary>
    public Task WriteSessionTerminatedAsync(
        Guid userIdentityId,
        UserSessionTerminationReason reason,
        CancellationToken cancellationToken)
    {
        var summary = reason switch
        {
            UserSessionTerminationReason.IdleTimeout => "Session ended after idle timeout.",
            UserSessionTerminationReason.AbsoluteTimeout => "Session ended after the maximum session duration.",
            UserSessionTerminationReason.NotFound => "Session ended because it was invalid or already signed out.",
            _ => "Session ended.",
        };

        return auditWriter.WriteAsync(
            new AuditEventWrite(
                AgencyId: null,
                ActorUserIdentityId: userIdentityId,
                ActionType: AuditActionTypes.SessionTerminated,
                Summary: summary),
            cancellationToken);
    }
}
