# AGENTS.md: Letoryn

Guidance for AI agents and contributors working in this repository.

## Project

**Letoryn** is a .NET Aspire distributed application. **Stack and architecture direction:** [docs/tech-stack.md](docs/tech-stack.md).

| Project | Role |
|---------|------|
| `Letoryn.AppHost` | Aspire orchestration |
| `Letoryn.ApiService` | Backend API |
| `Letoryn.Web` | Blazor web UI (Fluent UI) |
| `Letoryn.ServiceDefaults` | Shared OpenTelemetry, health, service discovery |

- **SDK:** .NET 10 (`global.json`)
- **Packages:** Central versioning in `Directory.Packages.props`
- **Skills:** Aspire, Fluent UI, Playwright CLI, and related workflows live under `.agents/skills/`

## Repository layout

```
src/           Application and AppHost projects (Letoryn.* naming)
tests/         Unit and E2E test projects (Letoryn.*.UnitTests, etc.)
docs/          Human-readable standards (see below)
specs/         Spec Kit features; platform epic index at specs/letoryn-platform/roadmap.md
.cursor/rules/ Cursor rules (standards pointer)
.github/       CI workflow and Dependabot
.agents/skills/ Cursor/Aspire agent skills
```

**Product work:** Spec Kit SDD only: no PRD track. The repo may go public; keep specs free of customer-confidential detail (see [specs/letoryn-platform/roadmap.md](specs/letoryn-platform/roadmap.md)).

## Documentation (source of truth)

Follow these when writing or reviewing C# and tests:

- [docs/local-development.md](docs/local-development.md): prerequisites, build, test, Aspire, secrets
- [docs/operations-rebuild-runbook.md](docs/operations-rebuild-runbook.md): authoritative Entra/Postgres/incident recovery (update here, not in specs)
- [docs/build-quality.md](docs/build-quality.md): warnings as errors, analyzers, XML docs on libraries
- [docs/tech-stack.md](docs/tech-stack.md): Aspire/.NET 10 layout, Azure + PostgreSQL + Graph direction, integration boundaries
- [docs/csharp-patterns.md](docs/csharp-patterns.md): DI, async/`CancellationToken`, `IOptions<T>`, typed `HttpClient`, records, nullable
- [docs/testing.md](docs/testing.md): xUnit v3, NSubstitute, no FluentAssertions/Moq/etc., Playwright C# for E2E, no AppHost unit tests
- [docs/web-ui-and-css.md](docs/web-ui-and-css.md): Fluent UI over custom CSS; branding in one owned stylesheet; Blazor chrome exception

Do not contradict these documents unless the user explicitly overrides them for a task.

## Writing style

Product copy, API problem details, Blazor UI strings, specs, issues, PR descriptions, and commit messages should read plainly in **UK English** (see [docs/pull-request-policy.md](docs/pull-request-policy.md)).

**Do not use em dashes (U+2014, `—`).** They are overused in technical writing and read as fussy. Agents and contributors must not introduce them in new or edited text unless there is no reasonable alternative without changing meaning (that should be rare).

Prefer instead:

- end the thought and start a new sentence, or use a comma where grammar allows;
- a **colon** before an explanation or list (`Invite: pending invitation…`);
- **parentheses** for a short aside;
- established **en dashes** only where the repo already uses them for ranges or labels (e.g. roadmap **R1–R15** in [label-strategy.md](docs/label-strategy.md)).

When you touch existing copy that contains em dashes, rewrite to one of the above rather than adding more. In tables, use `n/a` (or similar) for empty cells instead of an em dash.

## GitHub issues and pull requests

When creating or updating GitHub issues and pull requests, follow the canonical taxonomy and PR metadata rules (UK English, template headings, labels on both issue and PR):

- [docs/label-strategy.md](docs/label-strategy.md): `type/`, `priority/`, `status/`, `area/` (mapped to roadmap **R1–R15** in [specs/letoryn-platform/roadmap.md](specs/letoryn-platform/roadmap.md)), and `size/`.
- [docs/milestone-strategy.md](docs/milestone-strategy.md): GitHub milestones **POC**, **Go-live**, **Later** (delivery phases from [product-vision.md](docs/product-vision.md#delivery-phases)).
- [docs/pull-request-policy.md](docs/pull-request-policy.md): PR title shape `[<Type>] <Imperative summary> (#N)`, [`.github/pull_request_template.md`](.github/pull_request_template.md), linking (`Closes` / `References`), milestones, and Dependabot label set.

Apply at least one `type/` and one `priority/` label on every issue and PR; add `area/*` when the roadmap slice or cross-cutting area is known. When this document’s taxonomy changes, update GitHub labels via the maintainer’s cross-repo label sync: not ad-hoc one-off labels outside [label-strategy.md](docs/label-strategy.md).

## Agent checklist

### C#

- Async all the way; no `.Result` / `.Wait()`.
- Pass `CancellationToken` on public async APIs; use `HttpContext.RequestAborted` in ASP.NET handlers.
- Outbound HTTP: typed clients + `AddHttpClient<T>()`; resilience via handler pipeline in hosted apps.
- Libraries: constructor injection and `IServiceCollection` extensions: no `IOptions` or `IHttpContextAccessor` inside libraries.
- Prefer `record` for immutable DTOs and messages.

### Web UI (`Letoryn.Web`)

- Use **Fluent UI Blazor** components and parameters (`Margin`, `Padding`, layout APIs): not custom CSS to mimic or override the library.
- Custom CSS only for **Blazor platform chrome** (error/reload/loading), shrinking shell layout hooks, or future **branding** in a single owned file with design tokens: no scattered hex/spacing in Razor or inline `style`.
- See [docs/web-ui-and-css.md](docs/web-ui-and-css.md) and `.agents/skills/fluentui-blazor-usage/`.

### Tests

- Generate unit tests with **xUnit v3** and **NSubstitute** only.
- Use xUnit’s built-in assertions: never add FluentAssertions, Moq, NUnit, or MSTest.
- Do not add tests for AppHost resource graphs.
- E2E: **Playwright C#**, few high-value journey tests: not TS Playwright, not UI micro-tests.

### Aspire (local dev)

Start at [`.agents/skills/aspire/SKILL.md`](.agents/skills/aspire/SKILL.md) (routes to orchestration vs monitoring). Run CLI commands from the **repo root** with `--apphost src/Letoryn.AppHost/Letoryn.AppHost.csproj` when discovery is ambiguous. Resource names in this AppHost: **`webfrontend`**, **`apiservice`**, **`postgres`**.

**Read logs before guessing**: when the user reports dashboard/console errors, auth failures, or “the app is broken”, **inspect telemetry first**; do not ask them to paste logs if Aspire is running. Follow [`.agents/skills/aspire-monitoring/SKILL.md`](.agents/skills/aspire-monitoring/SKILL.md) and [monitoring.md](.agents/skills/aspire-monitoring/references/monitoring.md):

1. `aspire describe`: state, health, URLs (not `aspire ps` for per-resource status).
2. `aspire otel logs <resource>` (optionally `--search "error"`): structured logs first.
3. `aspire logs <resource>`: console stdout/stderr second (`webfrontend`, `apiservice`, `postgres`).
4. AppHost session file under `~/.aspire/logs/cli_*.log` for DCP/`crit` noise when the dashboard is unclear.

Letoryn: many postgres **ERROR** / **FATAL** lines at cold start are **benign** (DB create race, brief window before `letoryn-migrations` finishes); treat **Finished** resources and repeated **Unhealthy** after warm-up as real failures. AppHost applies EF migrations on start: use manual `dotnet ef database update` only off Aspire/CI. Connection strings for tools: `aspire describe apiservice --format Json` → `ConnectionStrings__letoryn` / `LETORYN_URI` (not `tcp://…` from the postgres table alone).

**Rebuild vs full restart**: see [`.agents/skills/aspire-orchestration/SKILL.md`](.agents/skills/aspire-orchestration/SKILL.md) and [resource-management.md](.agents/skills/aspire-orchestration/references/resource-management.md):

| Change | Action |
|--------|--------|
| `Letoryn.Web` / `Letoryn.ApiService` code | `aspire resource <name> rebuild` (recompile + restart); `restart` if process-only |
| `Letoryn.AppHost` (parameters, env wiring, new resources) | `aspire stop` then `aspire run` / `aspire start` |
| Stack already running and you changed Web/API | **Prefer `rebuild` on that resource**: do not tell the user to restart manually if you can run rebuild |

Example after editing the web project:

```bash
aspire resource webfrontend rebuild --non-interactive \
  --apphost src/Letoryn.AppHost/Letoryn.AppHost.csproj
aspire wait webfrontend --status healthy
```

Do not `dotnet run` projects under Aspire orchestration while the AppHost holds locks; do not stop the whole stack for a single-project code fix.

### Scope and changes

- Minimize diff scope; match existing project conventions.
- Do not commit unless the user asks.
- Opening a PR: follow [docs/pull-request-policy.md](docs/pull-request-policy.md) (not Conventional Commits titles or vendor draft defaults when the work is ready for review).
- Nullable reference types and **warnings as errors** are enabled solution-wide via `Directory.Build.props` (see [docs/build-quality.md](docs/build-quality.md)).

## Common commands

```bash
dotnet build Letoryn.slnx
dotnet test Letoryn.slnx
aspire run                   # from repo root; see docs/local-development.md
```

CI runs Release **lint** (`dotnet format --verify-no-changes`), **build**, and **test** on push/PR to `main` (`.github/workflows/ci.yml`).

See **Aspire (local dev)** above for logs, `rebuild`, and skill entry points.
