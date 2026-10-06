using System.Security.Claims;
using Letoryn.ApiService.Middleware;
using Letoryn.ApiService.Tenancy;
using Letoryn.Application.Abstractions.Tenancy;
using Letoryn.Domain.Agencies;
using Letoryn.Domain.Identities;
using Letoryn.Domain.Memberships;
using Letoryn.Domain.PlatformOperators;
using Letoryn.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace Letoryn.ApiService.UnitTests.Middleware;

public sealed class TenancyContextMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_InvitedMembershipOnMemberRoute_Returns403()
    {
        await using var db = CreateDbContext();
        var agencyId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        db.Agencies.Add(CreateAgency(agencyId, AgencyLifecycleStatus.Active));
        db.UserIdentities.Add(new UserIdentity
        {
            Id = userId,
            EntraObjectId = "entra-1",
            Email = "member@contoso.com",
            CreatedAt = DateTimeOffset.UtcNow,
        });
        db.AgencyMemberships.Add(new AgencyMembership
        {
            Id = Guid.NewGuid(),
            AgencyId = agencyId,
            UserIdentityId = userId,
            Status = MembershipStatus.Invited,
            AgencyRole = AgencyRole.StandardMember,
            UpdatedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var context = CreateHttpContext("/api/v1/agencies/settings", agencyId);
        var accessor = new AgencyContextAccessor();
        var currentUser = new TestCurrentUser(userId);
        var invoked = false;

        var middleware = new TenancyContextMiddleware(_ =>
        {
            invoked = true;
            return Task.CompletedTask;
        }, NullLogger<TenancyContextMiddleware>.Instance);

        await middleware.InvokeAsync(context, accessor, currentUser, db);

        Assert.False(invoked);
        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_GlobalOperatorWithoutAssignment_Returns404()
    {
        await using var db = CreateDbContext();
        var agencyId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        db.Agencies.Add(CreateAgency(agencyId, AgencyLifecycleStatus.Active));
        db.UserIdentities.Add(new UserIdentity
        {
            Id = userId,
            EntraObjectId = "operator-1",
            Email = "operator@contoso.com",
            IsPlatformOperator = true,
            CreatedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var context = CreateHttpContext("/api/v1/agencies/settings", agencyId);
        var accessor = new AgencyContextAccessor();
        var currentUser = new TestCurrentUser(userId, isPlatformOperator: true);
        var invoked = false;

        var middleware = new TenancyContextMiddleware(_ =>
        {
            invoked = true;
            return Task.CompletedTask;
        }, NullLogger<TenancyContextMiddleware>.Instance);

        await middleware.InvokeAsync(context, accessor, currentUser, db);

        Assert.False(invoked);
        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_AgencyHeaderWithoutAuthentication_Returns401()
    {
        await using var db = CreateDbContext();
        var agencyId = Guid.NewGuid();
        db.Agencies.Add(CreateAgency(agencyId, AgencyLifecycleStatus.Active));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var context = CreateHttpContext("/api/v1/agencies/settings", agencyId);
        var accessor = new AgencyContextAccessor();
        var currentUser = new TestCurrentUser(Guid.Empty);

        var middleware = new TenancyContextMiddleware(_ => Task.CompletedTask, NullLogger<TenancyContextMiddleware>.Instance);

        await middleware.InvokeAsync(context, accessor, currentUser, db);

        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        Assert.Null(accessor.ActiveAgencyId);
    }

    [Fact]
    public async Task InvokeAsync_OperatorRouteWithAssignment_AllowsArchivedAgency()
    {
        await using var db = CreateDbContext();
        var agencyId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        db.Agencies.Add(CreateAgency(agencyId, AgencyLifecycleStatus.Archived));
        db.UserIdentities.Add(new UserIdentity
        {
            Id = userId,
            EntraObjectId = "operator-2",
            Email = "operator@contoso.com",
            IsPlatformOperator = true,
            CreatedAt = DateTimeOffset.UtcNow,
        });
        db.PlatformOperatorAssignments.Add(new PlatformOperatorAssignment
        {
            Id = Guid.NewGuid(),
            AgencyId = agencyId,
            UserIdentityId = userId,
            AssignedAt = DateTimeOffset.UtcNow,
            AssignedByUserIdentityId = userId,
        });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var context = CreateHttpContext("/api/v1/operator/agencies/" + agencyId, agencyId);
        var accessor = new AgencyContextAccessor();
        var currentUser = new TestCurrentUser(userId, isPlatformOperator: true);
        var invoked = false;

        var middleware = new TenancyContextMiddleware(_ =>
        {
            invoked = true;
            return Task.CompletedTask;
        }, NullLogger<TenancyContextMiddleware>.Instance);

        await middleware.InvokeAsync(context, accessor, currentUser, db);

        Assert.True(invoked);
        Assert.Equal(agencyId, accessor.ActiveAgencyId);
    }

    [Fact]
    public async Task InvokeAsync_AgencyHeaderWithoutUserIdentityWhenAuthenticated_Returns404()
    {
        await using var db = CreateDbContext();
        var agencyId = Guid.NewGuid();
        db.Agencies.Add(CreateAgency(agencyId, AgencyLifecycleStatus.Active));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var context = CreateHttpContext("/api/v1/agencies/settings", agencyId);
        context.User = new ClaimsPrincipal(new ClaimsIdentity("test"));
        var accessor = new AgencyContextAccessor();
        var currentUser = new TestCurrentUser(Guid.Empty);

        var middleware = new TenancyContextMiddleware(_ => Task.CompletedTask, NullLogger<TenancyContextMiddleware>.Instance);

        await middleware.InvokeAsync(context, accessor, currentUser, db);

        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
        Assert.Null(accessor.ActiveAgencyId);
    }

    private static LetorynDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<LetorynDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new LetorynDbContext(options);
    }

    private static Agency CreateAgency(Guid id, AgencyLifecycleStatus status) =>
        new()
        {
            Id = id,
            DisplayName = "Test Agency",
            PrimaryContactEmail = "admin@contoso.com",
            PrimaryContactPhone = "+441234567890",
            LifecycleStatus = status,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            LastLifecycleChangeAt = DateTimeOffset.UtcNow,
        };

    private static DefaultHttpContext CreateHttpContext(string path, Guid agencyId)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Request.Headers[TenancyContextMiddleware.AgencyHeaderName] = agencyId.ToString();
        context.Response.Body = new MemoryStream();
        context.RequestServices = new ServiceCollection()
            .AddLogging()
            .AddProblemDetails()
            .BuildServiceProvider();
        return context;
    }

    private sealed class TestCurrentUser(Guid userIdentityId, bool isPlatformOperator = false) : ICurrentUser
    {
        public Guid UserIdentityId { get; } = userIdentityId;
        public string EntraObjectId { get; } = "test-oid";
        public string Email { get; } = "test@contoso.com";
        public bool IsPlatformOperator { get; } = isPlatformOperator;
    }
}
