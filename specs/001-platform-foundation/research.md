# Research: Platform foundation (R1)

**Feature**: `001-platform-foundation`  
**Date**: 2026-10-01

Resolves technical unknowns from the implementation plan. User constraint: **use .NET Aspire hosting and client integrations where they exist; consume framework-provided clients via DI—do not hand-roll connection factories, token handlers, or ad-hoc HTTP to infrastructure.**

---

## PostgreSQL persistence

**Decision**: Model PostgreSQL in `TenancyHub.AppHost` with `AddPostgres("postgres").AddDatabase("tenancyhub")`, reference the database from `TenancyHub.ApiService` via `WithReference`, and register EF Core with **`Aspire.Npgsql.EntityFrameworkCore.PostgreSQL`** using **`AddNpgsqlDbContext<TenancyHubDbContext>`** in the API.

**Rationale**: Matches `docs/tech-stack.md` (PostgreSQL as system of record), Aspire 13.6 first-party hosting integration, and documented client wiring (`connect-to-postgresql-with-ef-core` on aspire.dev). Connection strings, credentials, and service discovery are injected by Aspire; application code uses `TenancyHubDbContext` from DI only.

**Alternatives considered**:

| Alternative | Rejected because |
|-------------|------------------|
| Manual `ConnectionStrings` in `appsettings.Development.json` | Breaks Aspire parity, secrets in repo risk, no dashboard resource graph |
| Raw `NpgsqlDataSource` built in `Program.cs` without Aspire client package | Duplicates what `AddNpgsqlDbContext` configures (resilience, OTEL, config binding) |
| Azure Database for PostgreSQL in local dev | Heavier POC setup; defer `AddAzurePostgreSQL` / provisioning packages until deploy skill workflow targets Azure |

**Migration strategy**: EF Core migrations owned by a dedicated class library or the API project; optional Aspire migration worker (`apply-ef-core-migrations-in-aspire`) evaluated during implement—default is `dotnet ef database update` in quickstart until a migration resource is added.

**Volume rebuild note**: Stale Postgres container volumes after AppHost secret reset cause auth failures (see `.agents/skills/aspireify/references/apphost-wiring.md`); documented in [docs/operations-rebuild-runbook.md](../../docs/operations-rebuild-runbook.md).

---

## Microsoft Entra ID (organizational sign-in)

**Decision**: Use **Microsoft.Identity.Web** (not a separate Aspire Entra hosting package—none exists in `aspire integration search`) with configuration supplied from the AppHost via **`AddParameter`** / user secrets for tenant ID, client IDs, and client secrets. **Web** registers interactive sign-in (`AddAuthentication().AddMicrosoftIdentityWebApp`); **API** validates bearer tokens (`AddMicrosoftIdentityWebApi`) for the same Entra app registration or a dedicated API app per security review.

**Rationale**: FR-001 requires Microsoft 365 work-account sign-in; Identity.Web is the maintained ASP.NET Core stack and integrates with DI (`ITokenAcquisition` when Graph is added later). Aspire’s role is **orchestration and secret injection**, not replacing Entra.

**Alternatives considered**:

| Alternative | Rejected because |
|-------------|------------------|
| Custom OIDC middleware configuration without Identity.Web | More error-prone; diverges from Microsoft guidance and future Graph token acquisition |
| Blazor calling Entra directly without server-side auth | Violates `docs/tech-stack.md` integration boundaries |
| Keycloak / local fake IdP only | Does not meet product assumption (M365 work accounts) for POC |

**Manual rebuild**: Entra app registration steps are **not** fully automatable in git; [docs/operations-rebuild-runbook.md](../../docs/operations-rebuild-runbook.md) documents portal steps for humans and AI implementers.

---

## Application architecture (clean boundaries)

**Decision**: Introduce layered class libraries under `src/`:

| Project | Responsibility |
|---------|----------------|
| `TenancyHub.Domain` | Entities, value objects, domain invariants (no EF, no HTTP) |
| `TenancyHub.Application` | Use cases, authorization policies as interfaces, DTOs/records |
| `TenancyHub.Infrastructure` | EF Core `DbContext`, repositories, audit/notification writers |
| `TenancyHub.ApiService` | HTTP endpoints, authZ enforcement, maps to application services |
| `TenancyHub.Web` | Fluent UI shell, Blazor auth state, typed `HttpClient` to API via service discovery |

**Rationale**: Constitution Principle III (clean architecture); keeps AppHost free of product logic.

**Alternatives considered**: Monolith all-in-API—rejected because R2/R3 need stable tenancy APIs and testable domain rules without UI coupling.

---

## Tenancy and authorization enforcement

**Decision**: **Server-side** agency isolation on every mutating and read path: resolve `IAgencyContext` (active agency id, membership role, operator flags) from authenticated identity + session store (PostgreSQL or distributed cache later); enforce in application layer before persistence. API returns generic 403/404 per FR-013. Blazor hides nav per FR-011 but never relies on UI alone.

**Rationale**: SC-001 (100% cross-tenant block); aligns with security-first constitution.

**Alternatives considered**: Row-level security only in PostgreSQL— useful defense-in-depth later, not sole gate for R1 (application rules encode membership states, invitations, operator assignments).

---

## Session lifetime (idle 30 min, absolute 12 h)

**Decision**: ASP.NET Core cookie authentication with **sliding expiration** for idle (30 minutes) and **absolute** `ExpiresUtc` cap at 12 hours from initial sign-in stored in session record or auth properties; validate on each API call via `HttpContext.RequestAborted` and auth middleware. Concurrent sessions allowed (separate cookie sessions per browser).

**Rationale**: Matches clarified spec; standard cookie + server session metadata in PostgreSQL.

**Alternatives considered**: Entra session lifetime only—insufficient for product-specific idle/absolute rules without app-side tracking.

---

## Web ↔ API communication

**Decision**: Blazor Server **does not** access PostgreSQL. Typed **`HttpClient`** registered with **`AddHttpClient<TenancyHubApiClient>()`** and **`AddServiceDefaults()`** service discovery base address for `apiservice`. Pass bearer token or use **cookie + BFF** pattern (Web obtains token via Identity.Web and attaches on outbound API calls)—exact variant chosen in implement with security review; both use DI-registered clients, not `new HttpClient()`.

**Rationale**: `docs/tech-stack.md` and `docs/csharp-patterns.md`.

---

## Observability and health

**Decision**: Continue `TenancyHub.ServiceDefaults`; add health checks for PostgreSQL via Aspire/EF health checks when DB lands. No custom tracing in R1 operator diagnostics (FR-009).

**Rationale**: Constitution and existing AppHost `WithHttpHealthCheck`.

---

## Testing approach

**Decision**: Unit tests on application/domain with NSubstitute; integration tests for API with `WebApplicationFactory` and test containers **or** Aspire test resources only if already in repo patterns—prefer focused unit + few Playwright C# journeys per constitution VI.

**Rationale**: `docs/testing.md`.

---

## Operational documentation (rebuild)

**Decision**: Maintain a single **authoritative** runbook at **`docs/operations-rebuild-runbook.md`** with step-by-step Entra app registration, local Aspire secrets, Postgres volume reset, initial platform operator seeding, and incident-ordered recovery. [specs/001-platform-foundation/ops-runbook.md](./ops-runbook.md) is a pointer only so Spec Kit artifacts link to the same doc without duplicating content.

**Rationale**: Explicit plan input—operators must not hunt through `specs/*` after a major incident; `docs/` is the stable home for runbooks that evolve across features. Constitution Principle II (documentation completeness).

**Alternatives considered**:

| Alternative | Rejected because |
|-------------|------------------|
| Runbook only under `specs/001-platform-foundation/` | Hard to discover; fragments as each feature adds ops steps |
| Duplicate full text in spec + docs | Drift guaranteed when Entra or Aspire wiring changes |
