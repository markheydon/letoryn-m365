using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TenancyHub.ApiService.Middleware;
using TenancyHub.Application.Abstractions.Audit;
using TenancyHub.Application.Abstractions.Me;
using TenancyHub.Application.Agencies;
using TenancyHub.Application.Me;
using TenancyHub.Domain.Identities;
using TenancyHub.Domain.Sessions;
using TenancyHub.Infrastructure.Persistence;

namespace TenancyHub.ApiService.UnitTests.Endpoints;

public sealed class MeEndpointsTests
{
    [Fact]
    public async Task PutActiveAgency_WhenAuthorized_DoesNotWriteAudit()
    {
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var agencyId = Guid.NewGuid();
        var auditWriter = Substitute.For<IAuditWriter>();
        var meProfile = Substitute.For<IMeProfileService>();
        meProfile
            .SetActiveAgencyAsync(userId, agencyId, Arg.Any<CancellationToken>())
            .Returns(new SetActiveAgencyOutcome(SetActiveAgencyResult.Succeeded));

        await using var factory = new MeTestWebApplicationFactory(
            userId,
            sessionId,
            auditWriter,
            meProfile);

        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(UserSessionMiddleware.SessionHeaderName, sessionId.ToString());

        using var response = await client.PutAsJsonAsync(
            "/api/v1/me/active-agency",
            new { agencyId },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        await auditWriter.DidNotReceive().WriteAsync(Arg.Any<AuditEventWrite>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PutActiveAgency_WhenForbidden_Returns403ProblemDetails()
    {
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var agencyId = Guid.NewGuid();
        var auditWriter = Substitute.For<IAuditWriter>();
        var meProfile = Substitute.For<IMeProfileService>();
        meProfile
            .SetActiveAgencyAsync(userId, agencyId, Arg.Any<CancellationToken>())
            .Returns(new SetActiveAgencyOutcome(
                SetActiveAgencyResult.Forbidden,
                AgencyAccessRules.AgencySuspendedMemberMessage));

        await using var factory = new MeTestWebApplicationFactory(
            userId,
            sessionId,
            auditWriter,
            meProfile);

        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(UserSessionMiddleware.SessionHeaderName, sessionId.ToString());

        using var response = await client.PutAsJsonAsync(
            "/api/v1/me/active-agency",
            new { agencyId },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetailsDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(problem);
        Assert.Equal("Forbidden", problem.Title);
        Assert.Contains("suspended", problem.Detail, StringComparison.OrdinalIgnoreCase);
    }

    private sealed record ProblemDetailsDto(string? Title, string? Detail, int? Status);

    private sealed class MeTestWebApplicationFactory(
        Guid userIdentityId,
        Guid sessionId,
        IAuditWriter auditWriter,
        IMeProfileService meProfile) : WebApplicationFactory<Program>
    {
        private readonly string _databaseName = Guid.NewGuid().ToString("N");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting(WebHostDefaults.EnvironmentKey, Environments.Development);
            builder.ConfigureTestServices(services =>
            {
                ReplaceDbContext(services);
                SeedSession(services);

                services.RemoveAll<IAuditWriter>();
                services.AddScoped(_ => auditWriter);

                services.RemoveAll<IMeProfileService>();
                services.AddScoped(_ => meProfile);

                services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                        options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                    })
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        TestAuthHandler.SchemeName,
                        _ => { });
            });
        }

        private void ReplaceDbContext(IServiceCollection services)
        {
            foreach (var descriptor in services.ToList())
            {
                var serviceType = descriptor.ServiceType;
                if (serviceType == typeof(TenancyHubDbContext)
                    || serviceType == typeof(DbContextOptions<TenancyHubDbContext>)
                    || (serviceType.IsGenericType
                        && serviceType.GetGenericArguments().Any(t => t == typeof(TenancyHubDbContext))))
                {
                    services.Remove(descriptor);
                }
            }

            services.RemoveAll<TenancyHubDbContext>();
            services.RemoveAll<DbContextOptions<TenancyHubDbContext>>();
            services.AddDbContext<TenancyHubDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
        }

        private void SeedSession(IServiceCollection services)
        {
            services.AddSingleton(new MeTestSeed(userIdentityId, sessionId));
            services.AddHostedService<MeTestSeedHostedService>();
        }
    }

    private sealed record MeTestSeed(Guid UserIdentityId, Guid SessionId);

    private sealed class MeTestSeedHostedService(IServiceProvider services, MeTestSeed seed) : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await using var scope = services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<TenancyHubDbContext>();
            var now = DateTimeOffset.UtcNow;
            db.UserIdentities.Add(new UserIdentity
            {
                Id = seed.UserIdentityId,
                EntraObjectId = TestAuthHandler.TestOid,
                Email = "user@contoso.com",
                CreatedAt = now,
            });
            db.UserSessions.Add(new UserSession
            {
                Id = seed.SessionId,
                UserIdentityId = seed.UserIdentityId,
                StartedAt = now,
                LastActivityAt = now,
            });
            await db.SaveChangesAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        internal const string SchemeName = "Test";
        internal const string TestOid = "me-endpoint-test-oid";

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[]
            {
                new Claim("oid", TestOid),
                new Claim("preferred_username", "user@contoso.com"),
            };
            var identity = new ClaimsIdentity(claims, SchemeName);
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
