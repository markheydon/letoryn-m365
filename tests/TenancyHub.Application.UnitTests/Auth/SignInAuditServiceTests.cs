using TenancyHub.ApiService.Auth;
using TenancyHub.Application.Abstractions.Audit;
using TenancyHub.Application.Abstractions.Sessions;

namespace TenancyHub.Application.UnitTests.Auth;

public sealed class SignInAuditServiceTests
{
    private readonly IAuditWriter _auditWriter = Substitute.For<IAuditWriter>();

    [Fact]
    public async Task WriteSignInSucceededAsync_WritesGlobalAuditWithExpectedActionType()
    {
        var userId = Guid.NewGuid();
        var service = new SignInAuditService(_auditWriter);

        await service.WriteSignInSucceededAsync(userId, TestContext.Current.CancellationToken);

        await _auditWriter.Received(1).WriteAsync(
            Arg.Is<AuditEventWrite>(e =>
                e.AgencyId == null
                && e.ActorUserIdentityId == userId
                && e.ActionType == AuditActionTypes.SignInSucceeded),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task WriteSignInFailedAsync_WritesGlobalAuditWithExpectedActionType()
    {
        var service = new SignInAuditService(_auditWriter);

        await service.WriteSignInFailedAsync(
            null,
            "Bearer token validation failed.",
            TestContext.Current.CancellationToken);

        await _auditWriter.Received(1).WriteAsync(
            Arg.Is<AuditEventWrite>(e =>
                e.AgencyId == null
                && e.ActorUserIdentityId == null
                && e.ActionType == AuditActionTypes.SignInFailed
                && e.Summary == "Bearer token validation failed."),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task WriteSessionTerminatedAsync_WritesSessionTerminatedActionType()
    {
        var userId = Guid.NewGuid();
        var service = new SignInAuditService(_auditWriter);

        await service.WriteSessionTerminatedAsync(
            userId,
            UserSessionTerminationReason.SignOut,
            TestContext.Current.CancellationToken);

        await _auditWriter.Received(1).WriteAsync(
            Arg.Is<AuditEventWrite>(e =>
                e.ActionType == AuditActionTypes.SessionTerminated
                && e.ActorUserIdentityId == userId),
            Arg.Any<CancellationToken>());
    }
}
