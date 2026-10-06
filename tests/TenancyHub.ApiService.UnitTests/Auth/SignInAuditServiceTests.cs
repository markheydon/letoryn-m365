using TenancyHub.ApiService.Auth;
using TenancyHub.Application.Abstractions.Audit;

namespace TenancyHub.ApiService.UnitTests.Auth;

public sealed class SignInAuditServiceTests
{
    [Fact]
    public async Task WriteSignInSucceededAsync_PassesNullAgencyId()
    {
        var auditWriter = Substitute.For<IAuditWriter>();
        var service = new SignInAuditService(auditWriter);
        var userId = Guid.NewGuid();

        await service.WriteSignInSucceededAsync(userId, TestContext.Current.CancellationToken);

        await auditWriter.Received(1).WriteAsync(
            Arg.Is<AuditEventWrite>(e => e.AgencyId == null && e.ActionType == AuditActionTypes.SignInSucceeded),
            Arg.Any<CancellationToken>());
    }
}
