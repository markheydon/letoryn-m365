using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
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

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration);

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
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<CurrentUserMiddleware>();
app.UseMiddleware<TenancyContextMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/", () => Results.Ok(new { service = "TenancyHub.ApiService", status = "running" }));

app.MapDefaultEndpoints();

app.Run();

/// <summary>Entry point marker for integration tests.</summary>
public partial class Program;
