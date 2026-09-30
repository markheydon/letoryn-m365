# Tech stack and architecture direction

Development-focused reference for **Tenancy Hub** (this repository). Product requirements, domain modules, and business processes live outside this repo.

## Current repository

What is implemented in `TenancyHub.slnx` today:

| Layer | Choice | Notes |
|-------|--------|--------|
| Runtime | .NET 10 | Pinned in `global.json` |
| Orchestration | .NET Aspire **13.6** | `TenancyHub.AppHost` — Project v2 (`AddDotnetProject`), coordinated builds, health checks |
| Backend | ASP.NET Core API | `TenancyHub.ApiService` |
| Front end | Blazor + Fluent UI Blazor v5 | `TenancyHub.Web` — see [web-ui-and-css.md](./web-ui-and-css.md) |
| Cross-cutting | `TenancyHub.ServiceDefaults` | OpenTelemetry, HTTP resilience, service discovery defaults |
| Packages | Central management | `Directory.Packages.props`, shared MSBuild in `Directory.Build.props` |
| Tests | xUnit v3 + NSubstitute | `tests/TenancyHub.ApiService.UnitTests` — see [testing.md](./testing.md) |
| CI | GitHub Actions | `.github/workflows/ci.yml` — Release build + test |

Local run and toolchain: [local-development.md](./local-development.md). Build/analyzer policy: [build-quality.md](./build-quality.md).

Local run: Aspire AppHost (`aspire run` / AppHost project). Deployment targets are expected to be **Azure** via Aspire publish flows when introduced (see `.agents/skills/aspire-deployment/`).

### Source layout (`src/`)

| Pattern | Use |
|---------|-----|
| `TenancyHub.AppHost` | Aspire orchestration only |
| `TenancyHub.*` executables | Web, API, future workers |
| `TenancyHub.*` class libraries | Shared domain, clients, persistence—public API requires XML docs (see [build-quality.md](./build-quality.md)) |
| `TenancyHub.ServiceDefaults` | Aspire service defaults template; extend carefully |

Add new projects under `src/` with the `TenancyHub.` prefix unless a tool generates a different name.

### AppHost modelling (Aspire 13.6+)

- Register .NET apps with **`AddDotnetProject(name, path)`** via **`Aspire.Hosting.Dotnet`** (Project v2), not legacy `AddProject<Projects.*>`.
- The `Aspire.Hosting.Dotnet` package is currently a **13.6 preview** NuGet (`13.6.0-preview.*` in `Directory.Packages.props`); bump it with `aspire update` / `aspire add dotnet` when aligning with stable releases.
- Paths are relative to the AppHost project (for example `../TenancyHub.ApiService/TenancyHub.ApiService.csproj`). The AppHost does **not** use `ProjectReference` edges to those services solely for metadata; coordinated restore/build groups handle the graph.
- New .NET services: add the `.csproj` path in `AppHost.cs` and wire references, waits, and endpoints there. Keep `Aspire.AppHost.Sdk` and `Aspire.Hosting.*` packages on the same release family (`aspire update` from repo root).

## Target platform direction

Architecture decisions from product research that **this codebase is expected to follow** as it grows:

| Area | Direction |
|------|-----------|
| UI | **Blazor** (this repo) |
| Application logic | **.NET** backend services and APIs |
| Hosting | **Azure** (production); Aspire for modelling and local parity |
| System of record | **PostgreSQL** — operational domain data; add to AppHost when persistence lands |
| Microsoft 365 | **Entra ID** (Microsoft 365 accounts) for identity; product should integrate with M365, not replace it |
| M365 data & workflows | **Microsoft Graph** as the integration layer (mail, calendar, Teams, SharePoint, To Do, etc.) |
| Platform core | **Not Dataverse** as the primary datastore — Graph integration is required either way; PostgreSQL keeps long-term SaaS flexibility |

Implement Graph and payment integrations as **backend concerns** with typed HTTP clients and DI registration per [csharp-patterns.md](./csharp-patterns.md). The Blazor app talks to our API, not directly to third-party APIs, unless a future security review explicitly allows otherwise.

## Integration boundaries (development rules)

- **Graph**: Dedicated client(s) or small service layer; permissions and token acquisition via Entra / MSAL patterns appropriate to the hosting model. Do not embed Graph calls ad hoc in UI components.
- **PostgreSQL**: Access from API (and future workers), not from the Blazor front end. Migrations and schema ownership TBD when the database project is added.
- **SharePoint / documents**: Treat as Graph (or Graph-backed) storage for files metadata and retrieval; application metadata remains in PostgreSQL unless a deliberate hybrid model is documented later.
- **Payments / banking** (planned): Stripe, GoCardless, open banking, etc. — server-side only, secrets via configuration, no keys in the web client.
- **Observability**: Continue using ServiceDefaults / OpenTelemetry patterns; extend as new services are added to the AppHost.

## Related standards

Coding, testing, and UI rules are **not** duplicated here:

- [csharp-patterns.md](./csharp-patterns.md)
- [testing.md](./testing.md)
- [web-ui-and-css.md](./web-ui-and-css.md)

When stack choices conflict with those documents, the standards documents win unless the team explicitly updates them.

## Out of scope for this file

Domain features (CRM, compliance, portals, reporting), market positioning, process maps, and product roadmaps are intentionally omitted. Link or store that material in product documentation outside `docs/`.
