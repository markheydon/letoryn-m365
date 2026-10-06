using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using TenancyHub.Web.Components;
using TenancyHub.Web.Services;

var builder = WebApplication.CreateBuilder(args);

const string azureAdConfigurationSection = "AzureAd";
const string placeholderClientId = "00000000-0000-0000-0000-000000000000";
const string placeholderScopeSegment = "00000000-0000-0000-0000-000000000000";
var azureAdSection = builder.Configuration.GetSection(azureAdConfigurationSection);

static string? ResolveApiScope(IConfiguration configuration) =>
    configuration["TenancyHub:ApiScope"]
    ?? (string.IsNullOrWhiteSpace(configuration["AzureAd:Audience"])
        ? null
        : $"{configuration["AzureAd:Audience"]}/access_as_user");

static IEnumerable<string> ResolveInitialDownstreamScopes(IConfiguration configuration)
{
    var apiScope = ResolveApiScope(configuration);
    if (string.IsNullOrWhiteSpace(apiScope)
        || apiScope.Contains(placeholderScopeSegment, StringComparison.OrdinalIgnoreCase))
    {
        return [];
    }

    return [apiScope];
}

builder.AddServiceDefaults();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddFluentUIComponents();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(azureAdSection)
    .EnableTokenAcquisitionToCallDownstreamApi(ResolveInitialDownstreamScopes(builder.Configuration))
    .AddInMemoryTokenCaches();

builder.Services.PostConfigure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    var previous = options.Events.OnTokenValidated;
    options.Events.OnTokenValidated = async context =>
    {
        if (previous is not null)
        {
            await previous(context);
        }

        context.HttpContext.Response.Cookies.Append(
            SessionEstablishmentCookie.Name,
            SessionEstablishmentCookie.Value,
            new CookieOptions
            {
                HttpOnly = true,
                IsEssential = true,
                MaxAge = TimeSpan.FromMinutes(5),
                SameSite = SameSiteMode.Lax,
            });
    };
});

builder.Services.AddOptions<MicrosoftIdentityOptions>()
    .Bind(azureAdSection)
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.TenantId)
            && !string.IsNullOrWhiteSpace(options.ClientId),
        "AzureAd:TenantId and AzureAd:ClientId must be configured (Aspire maps Entra parameters per docs/operations-rebuild-runbook.md §3).")
    .Validate(
        options =>
        {
            if (string.Equals(options.ClientId, placeholderClientId, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(options.ClientSecret))
            {
                return false;
            }

            var credentialSection = builder.Configuration.GetSection($"{azureAdConfigurationSection}:ClientCredentials:0");
            var sourceType = credentialSection["SourceType"];
            var base64Value = credentialSection["Base64EncodedValue"];
            return string.Equals(sourceType, "Base64Encoded", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(base64Value);
        },
        "AzureAd client certificate credentials are required. Set Parameters:EntraWebClientCertificatePfx and Parameters:EntraWebClientCertificatePassword on the AppHost (see docs/operations-rebuild-runbook.md §3). Client secrets are not supported.")
    .ValidateOnStart();

builder.Services.AddOptions<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme)
    .Configure(options =>
    {
        options.LoginPath = "/MicrosoftIdentity/Account/SignIn";
        options.AccessDeniedPath = "/MicrosoftIdentity/Account/AccessDenied";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.Cookie.MaxAge = TimeSpan.FromHours(12);
        // Downstream API calls acquire and validate Entra tokens (see TenancyHubApiClient). Validating on every
        // cookie principal refresh multiplies MSAL noise after webfrontend restarts and wraps user_null as
        // MicrosoftIdentityWebChallengeUserException, which produced false sign-in-failed audit rows.
    });

builder.Services.AddControllersWithViews()
    .AddMicrosoftIdentityUI();

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = options.DefaultPolicy;
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AgencyContextState>();
builder.Services.AddScoped<UserSessionState>();
builder.Services.AddScoped<ExpiredApiSessionHandler>();
builder.Services.AddScoped<ShellNavigationService>();
builder.Services.AddHttpClient<TenancyHubApiClient>(static client =>
    client.BaseAddress = new Uri("https+http://apiservice"));
builder.Services.AddHttpClient<WebSignInAuditReporter>(static client =>
    client.BaseAddress = new Uri("https+http://apiservice"));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();
