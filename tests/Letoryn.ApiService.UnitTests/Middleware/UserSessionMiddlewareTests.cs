using System.Security.Claims;
using Letoryn.ApiService.Auth;
using Letoryn.ApiService.Middleware;
using Letoryn.Application.Abstractions.Audit;
using Letoryn.Application.Abstractions.Tenancy;
using Letoryn.Domain.Sessions;
using Letoryn.Infrastructure.Persistence;
using Letoryn.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Letoryn.ApiService.UnitTests.Middleware;

public sealed class UserSessionMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_GetMeWithExpiredSession_AllowsBootstrap()
    {
        await using var db = CreateDbContext();
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var staleActivity = DateTimeOffset.UtcNow.AddMinutes(-36);
        db.UserSessions.Add(new UserSession
        {
            Id = sessionId,
            UserIdentityId = userId,
            StartedAt = staleActivity,
            LastActivityAt = staleActivity,
        });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var context = CreateHttpContext("/api/v1/me", HttpMethods.Get, sessionId);
        context.User = new ClaimsPrincipal(
            new ClaimsIdentity([new Claim(ClaimTypes.Name, "test")], "Bearer"));
        var sessionService = new UserSessionService(db);
        var auditWriter = Substitute.For<IAuditWriter>();
        var signInAudit = new SignInAuditService(auditWriter);
        var currentUser = new TestCurrentUser(userId);
        var invoked = false;

        var middleware = new UserSessionMiddleware(_ =>
        {
            invoked = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context, sessionService, currentUser, signInAudit);

        Assert.True(invoked);
        Assert.False(context.Items.ContainsKey(UserSessionMiddleware.SessionHeaderName));
        await auditWriter.Received(1).WriteAsync(
            Arg.Is<AuditEventWrite>(e => e.ActionType == AuditActionTypes.SessionTerminated),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InvokeAsync_AuthenticatedWithoutProvisionedIdentity_WritesSignInFailedAudit()
    {
        var context = CreateHttpContext("/api/v1/agencies/settings", HttpMethods.Get, Guid.NewGuid());
        context.User = new ClaimsPrincipal(
            new ClaimsIdentity([new Claim(ClaimTypes.Name, "test")], "Bearer"));
        await using var db = CreateDbContext();
        var sessionService = new UserSessionService(db);
        var auditWriter = Substitute.For<IAuditWriter>();
        var signInAudit = new SignInAuditService(auditWriter);
        var currentUser = new TestCurrentUser(Guid.Empty);

        var middleware = new UserSessionMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context, sessionService, currentUser, signInAudit);

        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        await auditWriter.Received(1).WriteAsync(
            Arg.Is<AuditEventWrite>(e => e.ActionType == AuditActionTypes.SignInFailed),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InvokeAsync_ApiCallWithExpiredSession_Returns401()
    {
        await using var db = CreateDbContext();
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var staleActivity = DateTimeOffset.UtcNow.AddMinutes(-36);
        db.UserSessions.Add(new UserSession
        {
            Id = sessionId,
            UserIdentityId = userId,
            StartedAt = staleActivity,
            LastActivityAt = staleActivity,
        });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var context = CreateHttpContext("/api/v1/agencies/settings", HttpMethods.Get, sessionId);
        context.User = new ClaimsPrincipal(
            new ClaimsIdentity([new Claim(ClaimTypes.Name, "test")], "Bearer"));
        var sessionService = new UserSessionService(db);
        var auditWriter = Substitute.For<IAuditWriter>();
        var signInAudit = new SignInAuditService(auditWriter);
        var currentUser = new TestCurrentUser(userId);
        var invoked = false;

        var middleware = new UserSessionMiddleware(_ =>
        {
            invoked = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context, sessionService, currentUser, signInAudit);

        Assert.False(invoked);
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    }

    private static LetorynDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<LetorynDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new LetorynDbContext(options);
    }

    private static DefaultHttpContext CreateHttpContext(string path, string method, Guid sessionId)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Request.Method = method;
        context.Request.Headers[UserSessionMiddleware.SessionHeaderName] = sessionId.ToString();
        context.Response.Body = new MemoryStream();
        context.RequestServices = new ServiceCollection()
            .AddLogging()
            .AddProblemDetails()
            .BuildServiceProvider();
        return context;
    }

    private sealed class TestCurrentUser(Guid userIdentityId) : ICurrentUser
    {
        public Guid UserIdentityId { get; } = userIdentityId;
        public string EntraObjectId { get; } = "test-oid";
        public string Email { get; } = "test@contoso.com";
        public bool IsPlatformOperator { get; } = false;
    }
}
