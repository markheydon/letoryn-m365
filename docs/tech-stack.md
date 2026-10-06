# Tech stack and architecture direction

Development-focused reference for **Letoryn** (this repository). Product scope is Spec Kit SDD under `specs/` ([platform roadmap](../specs/letoryn-platform/roadmap.md)); do not put confidential customer material in git.

## Current repository

What is implemented in `Letoryn.slnx` today:

| Layer | Choice | Notes |
|-------|--------|--------|
| Runtime | .NET 10 | Pinned in `global.json` |
| Orchestration | .NET Aspire **13.6** | `Letoryn.AppHost`: Project v2 (`AddDotnetProject`), coordinated builds, health checks |
| Backend | ASP.NET Core API | `Letoryn.ApiService` |
| Front end | Blazor + Fluent UI Blazor v5 | `Letoryn.Web`: see [web-ui-and-css.md](./web-ui-and-css.md) |
| Cross-cutting | `Letoryn.ServiceDefaults` | OpenTelemetry, HTTP resilience, service discovery defaults |
| Packages | Central management | `Directory.Packages.props`, shared MSBuild in `Directory.Build.props` |
| Tests | xUnit v3 + NSubstitute | `tests/Letoryn.ApiService.UnitTests`: see [testing.md](./testing.md) |
| CI | GitHub Actions | `.github/workflows/ci.yml`: Release build + test |

Local run and toolchain: [local-development.md](./local-development.md). Build/analyzer policy: [build-quality.md](./build-quality.md).

Local run: Aspire AppHost (`aspire run` / AppHost project). Deployment targets are expected to be **Azure** via Aspire publish flows when introduced (see `.agents/skills/aspire-deployment/`).

### Source layout (`src/`)

| Pattern | Use |
|---------|-----|
| `Letoryn.AppHost` | Aspire orchestration only |
| `Letoryn.*` executables | Web, API, future workers |
| `Letoryn.*` class libraries | Shared domain, clients, persistence: public API requires XML docs (see [build-quality.md](./build-quality.md)) |
| `Letoryn.ServiceDefaults` | Aspire service defaults template; extend carefully |

Add new projects under `src/` with the `Letoryn.` prefix unless a tool generates a different name.

### AppHost modelling (Aspire 13.6+)

- Register .NET apps with **`AddDotnetProject(name, path)`** via **`Aspire.Hosting.Dotnet`** (Project v2), not legacy `AddProject<Projects.*>`.
- The `Aspire.Hosting.Dotnet` package is currently a **13.6 preview** NuGet (`13.6.0-preview.*` in `Directory.Packages.props`); bump it with `aspire update` / `aspire add dotnet` when aligning with stable releases.
- Paths are relative to the AppHost project (for example `../Letoryn.ApiService/Letoryn.ApiService.csproj`). The AppHost does **not** use `ProjectReference` edges to those services solely for metadata; coordinated restore/build groups handle the graph.
- New .NET services: add the `.csproj` path in `AppHost.cs` and wire references, waits, and endpoints there. Keep `Aspire.AppHost.Sdk` and `Aspire.Hosting.*` packages on the same release family (`aspire update` from repo root).
- **Health checks:** `MapDefaultEndpoints` in ServiceDefaults maps `/health` only in **Development** (Aspire local runs use Development). Before non-Development deployment, define an explicit health endpoint policy; see https://aka.ms/aspire/healthchecks.

## Target platform direction

Architecture decisions from product research that **this codebase is expected to follow** as it grows:

| Area | Direction |
|------|-----------|
| UI | **Blazor** (this repo) |
| Application logic | **.NET** backend services and APIs |
| Hosting | **Azure** (production); Aspire for modelling and local parity |
| System of record | **PostgreSQL**: operational domain data; add to AppHost when persistence lands |
| Microsoft 365 | **Entra ID** (Microsoft 365 accounts) for identity; product should integrate with M365, not replace it |
| M365 data & workflows | **Microsoft Graph** as the integration layer (mail, calendar, Teams, SharePoint, To Do, etc.) |
| Platform core | **Not Dataverse** as the primary datastore: Graph integration is required either way; PostgreSQL keeps long-term SaaS flexibility |

Implement Graph and payment integrations as **backend concerns** with typed HTTP clients and DI registration per [csharp-patterns.md](./csharp-patterns.md). The Blazor app talks to our API, not directly to third-party APIs, unless a future security review explicitly allows otherwise.

## Integration boundaries (development rules)

- **Graph**: Dedicated client(s) or small service layer; permissions and token acquisition via Entra / MSAL patterns appropriate to the hosting model. Do not embed Graph calls ad hoc in UI components.
- **PostgreSQL**: Access from API (and future workers), not from the Blazor front end. Migrations and schema ownership TBD when the database project is added.
- **SharePoint / documents & media**: Graph-backed libraries for tenancy documents and **operational media** (property photos, repair/cleaning evidence, etc.). Metadata and relationships in PostgreSQL; binaries in SharePoint unless a sub-spec documents otherwise.
- **WordPress showcase** (roadmap R13): Server-side API and/or WordPress plugin: property data flows to an **existing** agency site; not a full website builder in v1.
- **Property syndication feed** (roadmap R14): Outbound property export compatible with aggregators such as [Data Export](https://dataexport.co.uk/): Letoryn replaces the CRM-as-feed-source; **not** a replacement for Data Export or portal contracts.
- **Payments / banking** (planned): Stripe, GoCardless, open banking, etc.: server-side only, secrets via configuration, no keys in the web client.
- **Observability**: Continue using ServiceDefaults / OpenTelemetry patterns; extend as new services are added to the AppHost.

## Related standards

Coding, testing, and UI rules are **not** duplicated here:

- [csharp-patterns.md](./csharp-patterns.md)
- [testing.md](./testing.md)
- [web-ui-and-css.md](./web-ui-and-css.md)

When stack choices conflict with those documents, the standards documents win unless the team explicitly updates them.

## Out of scope for this file

Domain features, market positioning, process maps, and delivery phases are intentionally omitted here. See [product-vision.md](./product-vision.md) and the [platform roadmap](../specs/letoryn-platform/roadmap.md).
