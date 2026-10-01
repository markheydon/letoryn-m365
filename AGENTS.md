# AGENTS.md — Tenancy Hub

Guidance for AI agents and contributors working in this repository.

## Project

**Tenancy Hub** is a .NET Aspire distributed application. **Stack and architecture direction:** [docs/tech-stack.md](docs/tech-stack.md).

| Project | Role |
|---------|------|
| `TenancyHub.AppHost` | Aspire orchestration |
| `TenancyHub.ApiService` | Backend API |
| `TenancyHub.Web` | Blazor web UI (Fluent UI) |
| `TenancyHub.ServiceDefaults` | Shared OpenTelemetry, health, service discovery |

- **SDK:** .NET 10 (`global.json`)
- **Packages:** Central versioning in `Directory.Packages.props`
- **Skills:** Aspire, Fluent UI, Playwright CLI, and related workflows live under `.agents/skills/`

## Repository layout

```
src/           Application and AppHost projects (TenancyHub.* naming)
tests/         Unit and E2E test projects (TenancyHub.*.UnitTests, etc.)
docs/          Human-readable standards (see below)
specs/         Spec Kit features; platform epic index at specs/tenancy-hub-platform/roadmap.md
.cursor/rules/ Cursor rules (standards pointer)
.github/       CI workflow and Dependabot
.agents/skills/ Cursor/Aspire agent skills
```

**Product work:** Spec Kit SDD only—no PRD track. The repo may go public; keep specs free of customer-confidential detail (see [specs/tenancy-hub-platform/roadmap.md](specs/tenancy-hub-platform/roadmap.md)).

## Documentation (source of truth)

Follow these when writing or reviewing C# and tests:

- [docs/local-development.md](docs/local-development.md) — prerequisites, build, test, Aspire, secrets
- [docs/operations-rebuild-runbook.md](docs/operations-rebuild-runbook.md) — authoritative Entra/Postgres/incident recovery (update here, not in specs)
- [docs/build-quality.md](docs/build-quality.md) — warnings as errors, analyzers, XML docs on libraries
- [docs/tech-stack.md](docs/tech-stack.md) — Aspire/.NET 10 layout, Azure + PostgreSQL + Graph direction, integration boundaries
- [docs/csharp-patterns.md](docs/csharp-patterns.md) — DI, async/`CancellationToken`, `IOptions<T>`, typed `HttpClient`, records, nullable
- [docs/testing.md](docs/testing.md) — xUnit v3, NSubstitute, no FluentAssertions/Moq/etc., Playwright C# for E2E, no AppHost unit tests
- [docs/web-ui-and-css.md](docs/web-ui-and-css.md) — Fluent UI over custom CSS; branding in one owned stylesheet; Blazor chrome exception

Do not contradict these documents unless the user explicitly overrides them for a task.

## GitHub issues and pull requests

When creating or updating GitHub issues and pull requests, follow the canonical taxonomy and PR metadata rules (UK English, template headings, labels on both issue and PR):

- [docs/label-strategy.md](docs/label-strategy.md) — `type/`, `priority/`, `status/`, `area/` (mapped to roadmap **R1–R15** in [specs/tenancy-hub-platform/roadmap.md](specs/tenancy-hub-platform/roadmap.md)), and `size/`.
- [docs/milestone-strategy.md](docs/milestone-strategy.md) — GitHub milestones **POC**, **Go-live**, **Later** (delivery phases from [product-vision.md](docs/product-vision.md#delivery-phases)).
- [docs/pull-request-policy.md](docs/pull-request-policy.md) — PR title shape `[<Type>] <Imperative summary> (#N)`, [`.github/pull_request_template.md`](.github/pull_request_template.md), linking (`Closes` / `References`), milestones, and Dependabot label set.

Apply at least one `type/` and one `priority/` label on every issue and PR; add `area/*` when the roadmap slice or cross-cutting area is known. When this document’s taxonomy changes, update GitHub labels via the maintainer’s cross-repo label sync—not ad-hoc one-off labels outside [label-strategy.md](docs/label-strategy.md).

## Agent checklist

### C#

- Async all the way; no `.Result` / `.Wait()`.
- Pass `CancellationToken` on public async APIs; use `HttpContext.RequestAborted` in ASP.NET handlers.
- Outbound HTTP: typed clients + `AddHttpClient<T>()`; resilience via handler pipeline in hosted apps.
- Libraries: constructor injection and `IServiceCollection` extensions—no `IOptions` or `IHttpContextAccessor` inside libraries.
- Prefer `record` for immutable DTOs and messages.

### Web UI (`TenancyHub.Web`)

- Use **Fluent UI Blazor** components and parameters (`Margin`, `Padding`, layout APIs)—not custom CSS to mimic or override the library.
- Custom CSS only for **Blazor platform chrome** (error/reload/loading), shrinking shell layout hooks, or future **branding** in a single owned file with design tokens—no scattered hex/spacing in Razor or inline `style`.
- See [docs/web-ui-and-css.md](docs/web-ui-and-css.md) and `.agents/skills/fluentui-blazor-usage/`.

### Tests

- Generate unit tests with **xUnit v3** and **NSubstitute** only.
- Use xUnit’s built-in assertions—never add FluentAssertions, Moq, NUnit, or MSTest.
- Do not add tests for AppHost resource graphs.
- E2E: **Playwright C#**, few high-value journey tests—not TS Playwright, not UI micro-tests.

### Scope and changes

- Minimize diff scope; match existing project conventions.
- Do not commit unless the user asks.
- Opening a PR: follow [docs/pull-request-policy.md](docs/pull-request-policy.md) (not Conventional Commits titles or vendor draft defaults when the work is ready for review).
- Nullable reference types and **warnings as errors** are enabled solution-wide via `Directory.Build.props` (see [docs/build-quality.md](docs/build-quality.md)).

## Common commands

```bash
dotnet build TenancyHub.slnx
dotnet test TenancyHub.slnx
aspire run                   # from repo root; see docs/local-development.md
```

CI runs Release **lint** (`dotnet format --verify-no-changes`), **build**, and **test** on push/PR to `main` (`.github/workflows/ci.yml`).

Refer to `.agents/skills/aspire/SKILL.md` for Aspire lifecycle (start/stop, wiring, deployment).
