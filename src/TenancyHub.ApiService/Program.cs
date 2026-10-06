using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
using TenancyHub.ApiService.Auth;
using TenancyHub.ApiService.Services;
using TenancyHub.ApiService.Endpoints;
using TenancyHub.ApiService.Infrastructure;
using TenancyHub.ApiService.Middleware;
using TenancyHub.ApiService.Tenancy;
using TenancyHub.ApiService.Validation;
using TenancyHub.Application.Abstractions.Authorization;
using TenancyHub.Application.Abstractions.Tenancy;
using TenancyHub.Application.Authorization;
using TenancyHub.Infrastructure;
using TenancyHub.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddNpgsqlDbContext<TenancyHubDbContext>(connectionName: "tenancyhub");
builder.Services.AddTenancyHubInfrastructure();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CurrentUserSnapshotCache>();
builder.Services.AddScoped<AgencyContextAccessor>();
builder.Services.AddScoped<IAgencyContext>(sp => sp.GetRequiredService<AgencyContextAccessor>());
builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();
builder.Services.AddScoped<IAgencyAuthorizationService, AgencyAuthorizationService>();
builder.Services.AddScoped<SignInAuditService>();

const string azureAdConfigurationSection = "AzureAd";
var azureAdSection = builder.Configuration.GetSection(azureAdConfigurationSection);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(
        azureAdSection,
        jwtBearerScheme: JwtBearerDefaults.AuthenticationScheme,
        subscribeToJwtBearerMiddlewareDiagnosticsEvents: builder.Environment.IsDevelopment());

builder.Services.PostConfigure<JwtBearerOptions>(
    JwtBearerDefaults.AuthenticationScheme,
    options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters.NameClaimType = JwtRegisteredClaimNames.Name;

        options.Events ??= new JwtBearerEvents();
        var priorAuthenticationFailed = options.Events.OnAuthenticationFailed;
        options.Events.OnAuthenticationFailed = async context =>
        {
            if (priorAuthenticationFailed is not null)
            {
                await priorAuthenticationFailed(context).ConfigureAwait(false);
            }

            if (context.Exception is SecurityTokenExpiredException)
            {
                return;
            }

            // Entra rejects tokens for disabled accounts, invalid signatures, and other validation failures (research.md).
            var audit = context.HttpContext.RequestServices.GetRequiredService<SignInAuditService>();
            await audit.WriteSignInFailedAsync(
                null,
                "Bearer token validation failed.",
                context.HttpContext.RequestAborted);
        };
    });

builder.Services.AddOptions<MicrosoftIdentityOptions>()
    .Bind(azureAdSection)
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.TenantId)
            && !string.IsNullOrWhiteSpace(options.ClientId),
        "AzureAd:TenantId and AzureAd:ClientId must be configured (Aspire maps Entra parameters per runbook).")
    .ValidateOnStart();

if (string.IsNullOrWhiteSpace(azureAdSection["Audience"]))
{
    throw new InvalidOperationException(
        "AzureAd:Audience must be configured (Aspire maps EntraApiAudience per runbook).");
}

builder.Services.AddAuthorization();

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Instance = context.HttpContext.Request.Path;
    };
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddApiRequestValidation();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Info.Title = "Tenancy Hub API";
        document.Info.Version = "v1";
        document.Info.Description =
            "Platform foundation (R1) HTTP API. Semantic contract: specs/001-platform-foundation/contracts/api-v1.md.";
        return Task.CompletedTask;
    });
});

builder.Services.AddHealthChecks()
    .AddDbContextCheck<TenancyHubDbContext>("postgresql");

builder.Services.AddHostedService<InvitationExpiryHostedService>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<CurrentUserMiddleware>();
app.UseMiddleware<UserSessionMiddleware>();
app.UseMiddleware<TenancyContextMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/", () => Results.Ok(new { service = "TenancyHub.ApiService", status = "running" }));

app.MapMeEndpoints();
app.MapInvitationEndpoints();
app.MapMembershipEndpoints();
app.MapAgencySettingsEndpoints();
app.MapOperatorAgencyEndpoints();
app.MapOperatorPlatformEndpoints();
app.MapOperatorDiagnosticsEndpoints();
app.MapNotificationEndpoints();
app.MapAuditEndpoints();
app.MapAuthAuditEndpoints();

app.MapDefaultEndpoints();

app.Run();

/// <summary>Entry point marker for integration tests.</summary>
public partial class Program;
