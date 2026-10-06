var builder = DistributedApplication.CreateBuilder(args);

const string azureAdInstance = "https://login.microsoftonline.com/";

var entraTenantId = builder.AddParameter("EntraTenantId", secret: true);
var entraWebClientId = builder.AddParameter("EntraWebClientId", secret: true);
var entraWebClientCertificatePfx = builder.AddParameter("EntraWebClientCertificatePfx", secret: true);
var entraWebClientCertificatePassword = builder.AddParameter("EntraWebClientCertificatePassword", secret: true);
var entraApiClientId = builder.AddParameter("EntraApiClientId", secret: true);
var entraApiAudience = builder.AddParameter("EntraApiAudience", secret: true);
var internalSignInAuditKey = builder.AddParameter("LetorynInternalSignInAuditKey", secret: true);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume("pg-data")
    .AddDatabase("letoryn");

#pragma warning disable ASPIREDOTNETPROJECT001 // Project v2 (AddDotnetProject) is the approved AppHost pattern for this repo.
#pragma warning disable ASPIREPROJECTS001 // AddEFMigrations on IDotnetProgramResource requires this paired suppression.
var apiService = builder.AddDotnetProject("apiservice", "../Letoryn.ApiService/Letoryn.ApiService.csproj")
    .WithReference(postgres)
    .WithHttpHealthCheck("/health")
    .WithEnvironment("AzureAd__Instance", azureAdInstance)
    .WithEnvironment("AzureAd__TenantId", entraTenantId)
    .WithEnvironment("AzureAd__ClientId", entraApiClientId)
    .WithEnvironment("AzureAd__Audience", entraApiAudience)
    .WithEnvironment("Letoryn__InternalSignInAuditKey", internalSignInAuditKey);

var migrations = apiService
    .AddEFMigrations("letoryn-migrations", "Letoryn.Infrastructure.Persistence.LetorynDbContext")
    .WithMigrationsProject("../Letoryn.Infrastructure/Letoryn.Infrastructure.csproj")
    .RunDatabaseUpdateOnStart()
    .WaitFor(postgres);

apiService.WaitForCompletion(migrations);
#pragma warning restore ASPIREPROJECTS001
#pragma warning restore ASPIREDOTNETPROJECT001

var letorynApiScope = ReferenceExpression.Create($"{entraApiAudience}/access_as_user");

builder.AddDotnetProject("webfrontend", "../Letoryn.Web/Letoryn.Web.csproj")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService)
    .WithEnvironment("AzureAd__Instance", azureAdInstance)
    .WithEnvironment("AzureAd__TenantId", entraTenantId)
    .WithEnvironment("AzureAd__ClientId", entraWebClientId)
    .WithEnvironment("AzureAd__ClientCredentials__0__SourceType", "Base64Encoded")
    .WithEnvironment("AzureAd__ClientCredentials__0__Base64EncodedValue", entraWebClientCertificatePfx)
    .WithEnvironment("AzureAd__ClientCredentials__0__CertificatePassword", entraWebClientCertificatePassword)
    .WithEnvironment("Letoryn__ApiScope", letorynApiScope)
    .WithEnvironment("Letoryn__InternalSignInAuditKey", internalSignInAuditKey);

builder.Build().Run();
