# Implementation Plan: Platform foundation

**Branch**: `001-platform-foundation` | **Date**: 2026-10-01 | **Spec**: [spec.md](./spec.md)

**Input**: Roadmap **R1**: multi-tenant SaaS shell (PostgreSQL, Entra ID, agency isolation, roles, audit, notifications, shared Fluent UI chrome).

**Planning constraints** (from `/speckit-plan` input):

- Wire **infrastructure through .NET Aspire integrations** (`AddPostgres`, `WithReference`, client packages such as `Aspire.Npgsql.EntityFrameworkCore.PostgreSQL`).
- Application code uses **DI-provided** `DbContext`, `HttpClient`, and Identity.Web services: no hand-built connection strings, token endpoints, or `new HttpClient()`.
- **Operational rebuild docs**: authoritative **[docs/operations-rebuild-runbook.md](../../docs/operations-rebuild-runbook.md)** (maintained in `docs/` for incident recovery; spec folder holds a pointer only).

## Summary

Deliver the R1 platform foundation as an Aspire-orchestrated distributed app: PostgreSQL holds tenancy, membership, audit, and notifications; Entra ID authenticates work accounts via Microsoft.Identity.Web; the API enforces agency isolation and role rules; Blazor + Fluent UI provides the signed-in shell, operator flows, and invitation acceptance. Clean architecture libraries separate domain rules from EF Core and HTTP. Aspire AppHost owns resource graph and secret parameters; product logic stays out of AppHost.

## Technical Context

**Language/Version**: C# / .NET 10 (`global.json`)

**Primary Dependencies**: Aspire 13.6 AppHost (`AddDotnetProject`), `Aspire.Hosting.PostgreSQL`, `Aspire.Npgsql.EntityFrameworkCore.PostgreSQL`, EF Core 10, Microsoft.Identity.Web, Fluent UI Blazor v5, `TenancyHub.ServiceDefaults`

**Storage**: PostgreSQL (Aspire-hosted locally; Azure PostgreSQL via Aspire Azure integrations when deploying)

**Testing**: xUnit v3, NSubstitute; Playwright C# for high-value journeys; no AppHost unit tests

**Target Platform**: ASP.NET Core API + Blazor Server; local via `aspire run`; production direction Azure (Aspire publish)

**Performance Goals**: POC-scale (small agencies). Informal local-dev target: API p95 &lt; 500 ms for shell/API reads on Aspire: **not** a release gate, success criterion (SC-002 is manual sign-in timing only), or load-test obligation in R1; no performance tasks in [tasks.md](./tasks.md) unless product adds an explicit gate later.

**Constraints**: Security-first tenant isolation; English-only UI; in-app notifications only; warnings as errors; XML docs on public library APIs

**Scale/Scope**: ~15–20 API route groups, ~10 shell areas, single PostgreSQL database, one Entra tenant for dev POC

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Plan compliance |
|-----------|-----------------|
| I. Security First | AuthN/Z explicit; agency context on every API path; ProblemDetails without leakage; input validation at API boundary; secrets via AppHost parameters only |
| II. Documentation Completeness | XML on new libraries; **[docs/operations-rebuild-runbook.md](../../docs/operations-rebuild-runbook.md)** as authoritative rebuild/runbook; feature quickstart + contracts; public API documented in implement |
| III. Clean Architecture | Domain/Application/Infrastructure split; AppHost orchestration-only; Graph/Postgres not in Blazor |
| IV. Testing Standards | xUnit v3 + NSubstitute; meaningful application tests; E2E journeys aligned to quickstart |
| V. UX Consistency | Fluent UI shell; hidden forbidden nav; consistent error/empty states |
| VI. E2E ↔ Docs | quickstart scenarios map to Playwright journeys when added |

**Post-design re-check**: No unjustified gate violations. Complexity table empty.

## Project Structure

### Documentation (this feature)

```text
docs/
└── operations-rebuild-runbook.md   # AUTHORITATIVE: Entra, Postgres, incident recovery

specs/001-platform-foundation/
├── plan.md              # This file
├── research.md          # Phase 0
├── data-model.md        # Phase 1
├── quickstart.md        # Phase 1 validation
├── ops-runbook.md       # Pointer → docs/operations-rebuild-runbook.md
├── contracts/
│   └── api-v1.md        # HTTP contract
└── tasks.md             # Phase 2 (/speckit-tasks)
```

### Source Code (repository root)

```text
src/
├── TenancyHub.AppHost/           # AddPostgres, parameters, WithReference wiring
├── TenancyHub.ServiceDefaults/   # Existing OTEL, discovery, resilience
├── TenancyHub.Domain/            # NEW: entities, enums, domain errors
├── TenancyHub.Application/       # NEW: use cases, IAgencyContext, authz rules
├── TenancyHub.Infrastructure/    # NEW: DbContext, EF configs, migrations
├── TenancyHub.ApiService/        # Minimal APIs/controllers, JWT, tenancy middleware
└── TenancyHub.Web/               # Fluent shell, Identity.Web, typed API client

tests/
├── TenancyHub.Application.UnitTests/   # NEW
├── TenancyHub.ApiService.UnitTests/    # Extend existing
└── TenancyHub.E2E/                     # NEW when journeys added (Playwright C#)
```

**Structure Decision**: Extend current Aspire solution with three product libraries plus tests. Matches `docs/tech-stack.md` layout conventions (`TenancyHub.*` prefix).

## Authoritative operational documentation

Rebuild and incident-recovery procedures MUST live under **`docs/`**, not only under feature specs:

| Role | Location |
|------|----------|
| **Source of truth** | [docs/operations-rebuild-runbook.md](../../docs/operations-rebuild-runbook.md) |
| Feature spec pointer | [ops-runbook.md](./ops-runbook.md) (links to `docs/`; no duplicated steps) |
| Day-to-day dev | [docs/local-development.md](../../docs/local-development.md) |
| Feature acceptance | [quickstart.md](./quickstart.md) |

**Implement phase obligations**:

1. **Create or extend** `docs/operations-rebuild-runbook.md` when Entra wiring, Aspire parameters, migrations, or seed scripts land: same PR as the code that changes those steps.
2. **Never** add a second full runbook under another `specs/*` folder; new features append sections to the docs runbook (or add clearly linked `docs/` pages) and reference them from specs.
3. **Index** the runbook from [docs/README.md](../../docs/README.md) (done in plan refresh).
4. After major incidents or process changes, update the docs runbook first, then adjust spec pointers/quickstart links if section anchors change.

Rationale: contributors and on-call should not assemble recovery steps from scattered spec folders; specs govern *what* to build, `docs/` governs *how to run and restore* the product.

## Complexity Tracking

> No constitution violations requiring justification.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| (none) | (none) | (none) |

---

## Phase 0: Research (complete)

See [research.md](./research.md). All **NEEDS CLARIFICATION** items resolved:

- PostgreSQL: Aspire hosting + `AddNpgsqlDbContext`
- Entra: Identity.Web + AppHost secret parameters (no Aspire Entra package)
- Web/API split with DI HttpClient + service discovery
- Session idle/absolute rules: app-tracked metadata + cookie auth
- Rebuild documentation: [docs/operations-rebuild-runbook.md](../../docs/operations-rebuild-runbook.md)

---

## Phase 1: Design (complete)

| Artifact | Path |
|----------|------|
| Data model | [data-model.md](./data-model.md) |
| API contract | [contracts/api-v1.md](./contracts/api-v1.md) |
| Validation guide | [quickstart.md](./quickstart.md) |
| **Authoritative rebuild runbook** | [docs/operations-rebuild-runbook.md](../../docs/operations-rebuild-runbook.md) |
| Spec pointer | [ops-runbook.md](./ops-runbook.md) |

### AppHost target graph (implement)

```csharp
// Illustrative: confirm APIs via `aspire docs api search` before coding
var postgres = builder.AddPostgres("postgres").AddDatabase("tenancyhub");

var apiService = builder.AddDotnetProject("apiservice", "../TenancyHub.ApiService/...")
    .WithReference(postgres)
    .WithHttpHealthCheck("/health");

builder.AddDotnetProject("webfrontend", "../TenancyHub.Web/...")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService)
    .WithHttpHealthCheck("/health");
```

**Aspire resource names** (must match [quickstart.md](./quickstart.md) and `aspire wait` commands): `postgres`, `apiservice`, `webfrontend`: project paths point at `TenancyHub.ApiService` and `TenancyHub.Web` under `src/`.

Entra parameters injected with `AddParameter(..., secret: true)` and environment mapping: details in [docs/operations-rebuild-runbook.md §3](../../docs/operations-rebuild-runbook.md#3-local-configuration-aspire-parameters).

### API service registration (implement)

- `builder.AddNpgsqlDbContext<TenancyHubDbContext>(connectionName: "tenancyhub")`: connection name matches database resource.
- Register application services in `TenancyHub.Application` extension methods.
- Tenancy middleware: resolve active agency header + membership validation before handlers.

### Web (implement)

- `AddMicrosoftIdentityWebApp` + cookie options for idle/absolute session policy.
- `AddHttpClient<TenancyHubApiClient>()` with base address from service discovery.
- Fluent UI layout: header agency context, nav from permission model, operator area.

### Aspire workflow references

- Router: `.agents/skills/aspire/SKILL.md`
- AppHost wiring: `.agents/skills/aspireify/references/apphost-wiring.md`
- Local lifecycle: `.agents/skills/aspire-orchestration/SKILL.md`
- Add packages: `aspire add postgresql` then `aspire add` client package per docs (do not guess package names)

---

## Phase 2: Tasks (complete)

Dependency-ordered implementation tasks: [tasks.md](./tasks.md) (updated after `/speckit-analyze` remediation through **pass 8** on 2026-10-01: operator audit route `GET /operator/agencies/{agencyId}/audit`, quickstart §3.9 disabled-account / §3.10–§3.13 renumber, US1/quickstart joint gates, T090→T089, shell nav policy, FR-001 POC bound, §4.1 / §7 matrix, FR-008 sign-in audit).

**Next command**: `/speckit-implement` (or `/speckit-analyze` again after material spec/plan changes).

Epic breakdown (maps to task phases):

1. AppHost Postgres + parameters + project references  
2. Domain + Infrastructure + initial migration  
3. Entra auth (Web + API) + update [docs/operations-rebuild-runbook.md](../../docs/operations-rebuild-runbook.md) with final parameter names  
4. Core API: `/me`, active agency, isolation middleware  
5. Agencies, membership, invite/provision flows  
6. Audit + operator diagnostics  
7. Notifications  
8. Blazor shell, invitation UX, operator UX  
9. Unit tests + Playwright journeys tied to quickstart  

---

## Post-design Constitution Check

Re-evaluated after Phase 1: design stays within clean architecture, Aspire integration-first hosting, DI-only infrastructure access, and documented rebuild paths. Ready for implementation per [tasks.md](./tasks.md).
