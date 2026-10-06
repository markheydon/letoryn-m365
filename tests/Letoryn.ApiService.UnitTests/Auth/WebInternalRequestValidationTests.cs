using Letoryn.ApiService.Auth;
using Letoryn.Application.Abstractions.Tenancy;
using Microsoft.Extensions.Configuration;

namespace Letoryn.ApiService.UnitTests.Auth;

public sealed class WebInternalRequestValidationTests
{
    [Fact]
    public void IsTrustedWebRequest_MatchingKey_ReturnsTrue()
    {
        const string key = "test-audit-key";
        var context = new DefaultHttpContext();
        context.Request.Headers[TenancyHttpHeaders.InternalAuditKey] = key;
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
            [
                new KeyValuePair<string, string?>("Letoryn:InternalSignInAuditKey", key),
            ])
            .Build();

        Assert.True(WebInternalRequestValidation.IsTrustedWebRequest(context, configuration));
    }

    [Fact]
    public void IsTrustedWebRequest_WrongKey_ReturnsFalse()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers[TenancyHttpHeaders.InternalAuditKey] = "wrong";
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
            [
                new KeyValuePair<string, string?>("Letoryn:InternalSignInAuditKey", "expected"),
            ])
            .Build();

        Assert.False(WebInternalRequestValidation.IsTrustedWebRequest(context, configuration));
    }

    [Fact]
    public void IsTrustedWebRequest_MissingConfiguration_ReturnsFalse()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers[TenancyHttpHeaders.InternalAuditKey] = "any";
        var configuration = new ConfigurationBuilder().Build();

        Assert.False(WebInternalRequestValidation.IsTrustedWebRequest(context, configuration));
    }
}
