var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/", () => Results.Ok(new { service = "TenancyHub.ApiService", status = "running" }));

app.MapDefaultEndpoints();

app.Run();

/// <summary>Entry point marker for integration tests.</summary>
public partial class Program;
