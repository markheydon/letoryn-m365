# C# patterns

Modern C# patterns and practices for Tenancy Hub. Apply these in all new code; tighten existing code when you touch an area.

## Patterns to follow

- Use dependency injection via `IServiceCollection`.
- Replace `HttpContext.Current` with `IHttpContextAccessor`.
- Use `async/await` throughout; do not use `.Result` or `.Wait()`.
- Propagate `CancellationToken` on async public APIs (especially I/O and HTTP).
- Configuration via the `IOptions<T>` pattern.
- Outbound HTTP: use `IHttpClientFactory` and **typed clients**:
  - A small class that takes `HttpClient` in its constructor and wraps one API; register with `AddHttpClient<TClient>()`.
  - Do not `new HttpClient()` per call or register a long-lived singleton `HttpClient` yourself.
- Enable nullable reference types on new projects (`<Nullable>enable</Nullable>`). This solution sets nullable via root `Directory.Build.props`.
- Prefer `record` / `record struct` for immutable DTOs, options snapshots, and message shapes.

## Typed HTTP client

```csharp
public sealed class GitHubApiClient(HttpClient http)
{
    public Task<User?> GetUserAsync(string login, CancellationToken cancellationToken = default)
        => http.GetFromJsonAsync<User>($"users/{login}", cancellationToken);
}

// In DI (app or library extension method):
services.AddHttpClient<GitHubApiClient>(client =>
{
    client.BaseAddress = new Uri("https://api.github.com/");
});
```

Consumers inject `GitHubApiClient`, not raw `HttpClient` or `IHttpClientFactory`, unless you truly need ad hoc named clients.

## Where each pattern applies

| Pattern | Scope |
|---------|--------|
| `async/await` | All codebases |
| `CancellationToken` on public async methods | All codebases; in ASP.NET pass `HttpContext.RequestAborted` when appropriate |
| DI (constructors in libraries; `IServiceCollection` in apps) | All codebases |
| `IOptions<T>` | Hosted apps only—not libraries (libraries take plain options objects or configure via extension methods) |
| `IHttpContextAccessor` | ASP.NET web apps only—not libraries |
| `IHttpClientFactory` / typed clients | Any code that calls HTTP; libraries expose `IServiceCollection` extension methods that call `AddHttpClient<T>()`, they do not construct `HttpClient` internally |
| Retries, timeouts, circuit breaking | Configure on the `AddHttpClient` handler pipeline (e.g. `Microsoft.Extensions.Http.Resilience`) in hosted apps—not hand-rolled per call |
| Nullable reference types | All new projects; tighten existing projects when touching an area |
| `record` DTOs | API models, config binding types, domain events; use classes when you need mutable identity or inheritance |

## Solution defaults

- Central package versions: `Directory.Packages.props`
- Shared MSBuild properties (nullable, analyzers, XML docs policy): `Directory.Build.props`
- HTTP resilience package is already pinned for hosted apps: `Microsoft.Extensions.Http.Resilience`
