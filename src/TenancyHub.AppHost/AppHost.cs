var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddDotnetProject("apiservice", "../TenancyHub.ApiService/TenancyHub.ApiService.csproj")
    .WithHttpHealthCheck("/health");

builder.AddDotnetProject("webfrontend", "../TenancyHub.Web/TenancyHub.Web.csproj")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
