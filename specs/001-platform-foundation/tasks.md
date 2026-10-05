---
description: "Task list for feature implementation"
---

# Tasks: Platform foundation (R1)

**Input**: Design documents from `/specs/001-platform-foundation/`

**Prerequisites**: [plan.md](./plan.md), [spec.md](./spec.md), [research.md](./research.md), [data-model.md](./data-model.md), [contracts/api-v1.md](./contracts/api-v1.md), [quickstart.md](./quickstart.md)

**Tests**: Bulk unit/E2E work is in Phase 9; **story-phase unit tests** (for example US2 isolation) are allowed where marked. No test-first phases unless implement chooses TDD for a slice.

**Organization**: Tasks grouped by user story for independent delivery and validation per [quickstart.md](./quickstart.md).

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies on incomplete tasks)
- **[Story]**: User story label (US1–US6) on story-phase tasks only

## Path conventions

Per [plan.md](./plan.md): `src/TenancyHub.*` libraries, `tests/TenancyHub.*` test projects, AppHost orchestration-only.

---

## Phase 1: Setup (shared infrastructure)

**Purpose**: Solution layout, packages, and Aspire integration baseline before domain work.

- [x] T001 Add `TenancyHub.Domain`, `TenancyHub.Application`, and `TenancyHub.Infrastructure` projects under `src/` and include them in `TenancyHub.slnx`
- [x] T002 Add `TenancyHub.Application.UnitTests` under `tests/` and include it in `TenancyHub.slnx`
- [x] T003 [P] Wire project references: `Infrastructure` → `Domain` + `Application`; `Application` → `Domain`; `ApiService` → `Application` + `Infrastructure`; `Web` → `Application` (DTOs/contracts only, no Infrastructure)
- [x] T004 [P] Add central package versions in `Directory.Packages.props` for EF Core 10, `Aspire.Npgsql.EntityFrameworkCore.PostgreSQL`, `Microsoft.Identity.Web`, and EF design tools per [research.md](./research.md)
- [x] T005 [P] Enable XML documentation and nullable settings on new library projects matching `Directory.Build.props` in `docs/build-quality.md`
- [x] T006 Run Aspire package wiring from repo root: add PostgreSQL hosting to `src/TenancyHub.AppHost/` and Npgsql EF client to `src/TenancyHub.ApiService/` per `.agents/skills/aspireify/` (no hand-rolled connection strings)
- [x] T007 Update `src/TenancyHub.AppHost/AppHost.cs` with `AddPostgres("postgres").AddDatabase("tenancyhub")`, `WithReference(postgres)` on **apiservice**, and `AddDotnetProject` v2 for **`apiservice`** → `TenancyHub.ApiService` and **`webfrontend`** → `TenancyHub.Web` (resource names MUST match [quickstart.md](./quickstart.md) `aspire wait` targets)
- [x] T008 [P] Add Entra-related `AddParameter(..., secret: true)` placeholders in `src/TenancyHub.AppHost/AppHost.cs` and map to Web/API environment variables per [docs/operations-rebuild-runbook.md](../../docs/operations-rebuild-runbook.md)
- [x] T009 [P] Verify `specs/001-platform-foundation/ops-runbook.md` points to `docs/operations-rebuild-runbook.md` only (no duplicated steps; update link text if anchors change)

---

## Phase 2: Foundational (blocking prerequisites)

**Purpose**: Domain model, persistence, application seams, and API hosting hooks required by every user story.

**⚠️ CRITICAL**: No user story phases until this checkpoint passes (`dotnet build`, migration applies, health checks green).

- [x] T010 [P] Add agency lifecycle enum `Active`, `Suspended`, `Archived` in `src/TenancyHub.Domain/Agencies/AgencyLifecycleStatus.cs`
- [x] T011 [P] Add membership status enum `Invited`, `Active`, `Suspended`, `Removed` in `src/TenancyHub.Domain/Memberships/MembershipStatus.cs`
- [x] T012 [P] Add agency role enum `Administrator`, `StandardMember`, `ReadOnlyMember` in `src/TenancyHub.Domain/Memberships/AgencyRole.cs`
- [x] T013 [P] Implement `Agency` entity in `src/TenancyHub.Domain/Agencies/Agency.cs` with required `DisplayName`, `PrimaryContactEmail`, `PrimaryContactPhone`, `LifecycleStatus`, `CreatedAt`, `UpdatedAt`, `LastLifecycleChangeAt` per [data-model.md](./data-model.md)
- [x] T014 [P] Implement `UserIdentity` entity in `src/TenancyHub.Domain/Identities/UserIdentity.cs` with unique `EntraObjectId`, normalized `Email`, `IsPlatformOperator`, nullable `LastUsedAgencyId`, `CreatedAt`
- [x] T015 [P] Implement `AgencyMembership` entity in `src/TenancyHub.Domain/Memberships/AgencyMembership.cs` with exactly one `AgencyRole`, invite fields (`InvitedAt`, `ExpiresAt` = Created + 30 days for invites), and status rules from [data-model.md](./data-model.md)
- [x] T016 [P] Implement `PlatformOperatorAssignment`, `AuditEvent`, `Notification`, and optional `UserSession` entities in `src/TenancyHub.Domain/` matching [data-model.md](./data-model.md) field constraints
- [x] T017 Add domain guard helpers (last active administrator, last platform operator, archived-agency mutation block) in `src/TenancyHub.Domain/` per FR-006, FR-007, FR-004
- [x] T018 Implement `TenancyHubDbContext` and fluent EF configurations in `src/TenancyHub.Infrastructure/Persistence/` including indexes listed in [data-model.md](./data-model.md)
- [x] T019 Register `AddNpgsqlDbContext<TenancyHubDbContext>(connectionName: "tenancyhub")` in `src/TenancyHub.ApiService/Program.cs` and infrastructure DI extension in `src/TenancyHub.Infrastructure/DependencyInjection.cs`
- [x] T020 Create initial EF Core migration in `src/TenancyHub.Infrastructure/Persistence/Migrations/` and document apply command in [docs/operations-rebuild-runbook.md](../../docs/operations-rebuild-runbook.md)
- [x] T021 [P] Define `IAgencyContext`, `ICurrentUser`, and authorization result types in `src/TenancyHub.Application/Abstractions/`
- [x] T022 [P] Add application service interfaces for audit writing and notification writing in `src/TenancyHub.Application/Abstractions/`
- [x] T023 Implement EF-backed audit and notification writers in `src/TenancyHub.Infrastructure/Services/`
- [x] T024 Implement agency context resolution from `X-TenancyHub-Agency-Id` header + membership/assignment validation in `src/TenancyHub.ApiService/Middleware/TenancyContextMiddleware.cs`
- [x] T025 Add global ProblemDetails and safe 403/404 mapping in `src/TenancyHub.ApiService/Program.cs` per FR-013 and [contracts/api-v1.md](./contracts/api-v1.md)
- [x] T026 [P] Add API request validation at HTTP trust boundaries (membership invite/provision bodies, agency settings, operator agency create/lifecycle, platform operator grant/revoke/assignments) returning 400 ProblemDetails in `src/TenancyHub.ApiService/Validation/` per constitution Principle I and `docs/csharp-patterns.md` (pair with Web form validation in T099)
- [x] T027 [P] Add PostgreSQL health check via Aspire/EF in `src/TenancyHub.ApiService/Program.cs`
- [x] T028 Extend [docs/operations-rebuild-runbook.md](../../docs/operations-rebuild-runbook.md) §4–§5 with final Aspire parameter names, migration steps, and link to [scripts/r1/seed-platform-operator.sql](../../scripts/r1/seed-platform-operator.sql) (paired with T102)
- [x] T102 **(execute after T020)** Align [scripts/r1/seed-platform-operator.sql](../../scripts/r1/seed-platform-operator.sql) table/column names with EF migration output, document `psql`/Aspire connection usage in runbook §5, and verify seed grants `IsPlatformOperator` for a known Entra `oid` + email per [quickstart.md](./quickstart.md) §2
- [x] T029 [P] Document FR-012 tenancy seams (`IAgencyContext`, `ICurrentUser`, authorization entry points) with XML comments on public types in `src/TenancyHub.Application/Abstractions/` for downstream feature modules

**Checkpoint**: Foundation ready — user story work may proceed in parallel.

---

## Phase 3: User Story 1 — Sign in to the correct agency (Priority: P1) 🎯 MVP

**Goal**: Organizational sign-in, session rules (30 min idle, 12 h absolute per session), active agency context, last-used default, multi-agency switch, operator assignment rules, invitation gate when no active membership.

**Independent Test**: [quickstart.md](./quickstart.md) §3.1–§3.5 and §3.8–§3.11 — two agencies, two browsers; multi-agency switch; sign-in audit; disabled-directory denial; access-not-configured; operator without assignments. Optional §3.12–§3.13 (session caps). Steps **§3.6–§3.7** (invitation routing) are a **joint US1 + US3** gate (T053, T064, T096, T101)—not required to mark US1 done alone.

### Implementation for User Story 1

- [x] T030 [P] [US1] Configure Microsoft.Identity.Web API JWT validation in `src/TenancyHub.ApiService/Program.cs` per [research.md](./research.md)
- [x] T031 [P] [US1] Configure Microsoft.Identity.Web interactive sign-in and cookie auth in `src/TenancyHub.Web/Program.cs` with sliding 30-minute idle and 12-hour absolute cap per FR-001
- [x] T032 [P] [US1] Keep `specs/001-platform-foundation/research.md` **Disabled directory account** and session sections current (token validation + cookie refresh; no Graph in R1; runbook access-token lifetime note) — pair with T033, T034, T094
- [x] T033 [US1] Enforce FR-001 loss of access per research.md **Disabled directory account**: JWT validation on every API request; Web cookie validation with failed Entra token refresh → sign-out; access-token lifetime guidance in runbook §3 (pair with T044 audit)
- [x] T034 [US1] Implement `UserSession` persistence and per-request session validation in `src/TenancyHub.Infrastructure/Services/UserSessionService.cs` (concurrent sessions = multiple rows; idle/absolute expiry with framework-default clock-skew tolerance per research.md; R1 session store is PostgreSQL)
- [x] T035 [US1] Implement identity provisioning on first token (`EntraObjectId`, `Email`) in `src/TenancyHub.Infrastructure/Services/EnsureUserIdentityService.cs` (`IEnsureUserIdentityService`)
- [x] T036 [US1] Implement `GET /api/v1/me` in `src/TenancyHub.ApiService/Endpoints/MeEndpoints.cs` per [contracts/api-v1.md](./contracts/api-v1.md)
- [x] T037 [US1] Implement `PUT /api/v1/me/active-agency` with membership/assignment checks and `LastUsedAgencyId` update in `src/TenancyHub.ApiService/Endpoints/MeEndpoints.cs` (no FR-008 audit row for agency switch in R1—see [contracts/api-v1.md](./contracts/api-v1.md))
- [x] T038 [P] [US1] Add typed `TenancyHubApiClient` with `AddHttpClient` + service discovery in `src/TenancyHub.Web/Services/TenancyHubApiClient.cs`
- [x] T039 [US1] Wire bearer/cookie token forwarding from Web to API in `src/TenancyHub.Web/Services/TenancyHubApiClient.cs` (DI-only HTTP; no `new HttpClient()`)
- [x] T040 [P] [US1] Add agency context header propagation and switch handler in `src/TenancyHub.Web/Services/AgencyContextState.cs`
- [x] T041 [US1] Build signed-in shell chrome with agency name in `src/TenancyHub.Web/Components/Layout/MainLayout.razor` using Fluent UI parameters per `docs/web-ui-and-css.md` (reserve header slot for sign-out wired in T094; **do not** render agency notification bell until FR-002 active agency + **Active** membership—T084/T085)
- [x] T042 [US1] Add agency switcher UI in `src/TenancyHub.Web/Components/AgencySwitcher.razor` (last-used default, invalid last-used fallback)
- [x] T043 [US1] Add no-membership and operator-no-assignment pages in `src/TenancyHub.Web/Components/Pages/AccessNotConfigured.razor` and `src/TenancyHub.Web/Components/Pages/OperatorHome.razor`
- [x] T044 [US1] Audit **successful and failed** sign-in attempts, directory-disabled denial, and session termination (idle, absolute cap, validation failure, per-session sign-out) via `IAuditWriter` in `src/TenancyHub.ApiService/` per FR-008—use nullable `AgencyId` on sign-in rows when no active agency context yet; pair with [quickstart.md](./quickstart.md) §3.8–§3.9 and §6 (SC-004)
- [x] T045 [US1] Update [docs/operations-rebuild-runbook.md](../../docs/operations-rebuild-runbook.md) §3 with Entra redirect URIs and secret parameter names after auth wiring
- [x] T094 [US1] Implement per-session sign-out per FR-001/FR-011 and [contracts/api-v1.md](./contracts/api-v1.md): `POST /api/v1/me/sign-out` in `src/TenancyHub.ApiService/Endpoints/MeEndpoints.cs` invalidates only the current `UserSession` row; Fluent UI sign-out in `src/TenancyHub.Web/Components/Layout/MainLayout.razor` calls Web sign-out without terminating other concurrent sessions; verify via [quickstart.md](./quickstart.md) §3.5
- [x] T096 [US1] When `GET /api/v1/me` reports pending **Invited** memberships and at least one **Active** membership, route signed-in users into the normal agency shell on last-used **Active** agency and surface a Fluent UI banner or nav entry in `src/TenancyHub.Web/Components/Layout/MainLayout.razor` linking to `Invitations.razor` until each invite is accepted or declined (no auto-activation); verify [quickstart.md](./quickstart.md) §3.6 (depends on T053 accept/decline APIs and T064 `Invitations.razor`; banner may ship with stub route until T064)
- [x] T101 [US1] When `GET /api/v1/me` reports one or more **Invited** memberships, **no** **Active** agency membership, and **no** valid operator shell path (FR-002), route post-sign-in users to `Invitations.razor` (minimal chrome—not routine agency shell); block routine shell URLs and agency-scoped API calls until accept/decline or operator assignment; no notification bell; verify [quickstart.md](./quickstart.md) §3.7 (depends on T036, T053, T064; may stub `Invitations.razor` until T064 lands)

**Checkpoint**: User Story 1 independently testable via quickstart **§3.1–§3.5 and §3.8–§3.11** (optional §3.12–§3.13; full §3 including §3.6–§3.7 after US3 invitation work).

---

## Phase 4: User Story 2 — Enforce agency data isolation (Priority: P1)

**Goal**: Every agency-scoped read/write blocked across tenants; lifecycle gates member access; operators limited to assigned agencies.

**Independent Test**: [quickstart.md](./quickstart.md) §4 and §4.1 — cross-agency IDs and forged headers; agency/membership lifecycle access gates.

### Implementation for User Story 2

- [x] T046 [US2] [FR-003] Centralize `IAuthorizationService` / policy checks in `src/TenancyHub.Application/Authorization/AgencyAuthorizationService.cs` for agency `LifecycleStatus` and membership status (`Active`, `Invited`, `Suspended`, `Removed`)—**Suspended** membership MUST block agency shell/API access with a clear UK English message while other agencies remain usable
- [x] T047 [US2] [FR-003] Enforce member sign-in and active-context denial for agency `LifecycleStatus` **Suspended** and **Archived**, and for membership **Suspended**, with clear UK English messages in `src/TenancyHub.Application/Agencies/AgencyAccessRules.cs` (distinct copy for agency vs membership suspension)
- [x] T048 [US2] [FR-003] Apply authorization filters to all agency-scoped minimal API groups in `src/TenancyHub.ApiService/Program.cs`
- [x] T049 [US2] [FR-013] Return identical outward shape for cross-tenant not-found vs forbidden in `src/TenancyHub.ApiService/Infrastructure/TenantSafeResults.cs` per FR-013 (supports FR-003 isolation without leakage)
- [x] T050 [US2] [FR-003] Block agency-scoped routes when active header agency does not match route `agencyId` in `src/TenancyHub.ApiService/Middleware/TenancyContextMiddleware.cs`
- [x] T051 [US2] [FR-003] Implement operator assignment verification for `/api/v1/operator/agencies/{agencyId}/...` in `src/TenancyHub.Application/Operators/OperatorAssignmentService.cs`
- [x] T052 [P] [US2] [FR-003] Add integration-focused unit tests for isolation matrix (cross-tenant access, membership **Suspended**, agency **Suspended**/**Archived**) in `tests/TenancyHub.Application.UnitTests/Authorization/AgencyIsolationTests.cs`

**Checkpoint**: User Story 2 independently testable via quickstart §4.

---

## Phase 5: User Story 3 — Manage agency membership and roles (Priority: P2)

**Goal**: Invite, provision, revoke, suspend/reactivate/remove memberships, role changes, agency settings, operator agency creation and assignments, platform operator grant/revoke with guards.

**Independent Test**: [quickstart.md](./quickstart.md) §5 — operator create/assignments, invite accept/decline, provision, agency settings, read-only vs standard permissions, last-admin guard, role change effective within one minute without full logout.

### Implementation for User Story 3

- [ ] T053 [P] [US3] Implement invitee routes `GET /api/v1/invitations/pending`, `POST /api/v1/invitations/{membershipId}/accept`, and `POST /api/v1/invitations/{membershipId}/decline` in `src/TenancyHub.ApiService/Endpoints/InvitationEndpoints.cs` per [contracts/api-v1.md](./contracts/api-v1.md)
- [ ] T054 [US3] Implement invite idempotency (at most one pending `Invited` per agency+email) in `src/TenancyHub.Application/Memberships/InviteMemberHandler.cs`
- [ ] T055 [US3] Implement provision → immediate `Active` membership in `src/TenancyHub.Application/Memberships/ProvisionMemberHandler.cs`
- [ ] T056 [US3] Implement membership lifecycle handlers (suspend, reactivate, remove, revoke invitation, role patch) in `src/TenancyHub.Application/Memberships/`
- [ ] T057 [US3] Enforce 30-day invite expiry with lazy evaluation on pending list, accept, and decline; optional `InvitationExpiryHostedService` sweep in `src/TenancyHub.Application/Memberships/InvitationRules.cs` and `src/TenancyHub.ApiService/` registration
- [ ] T058 [US3] Ensure role and membership permission checks read current database state (or cache TTL ≤ 1 minute) so role changes apply within one minute without full logout in `src/TenancyHub.Application/Authorization/AgencyAuthorizationService.cs`; add quickstart §5 verification step
- [ ] T059 [US3] Implement agency membership API group `/api/v1/agencies/{agencyId}/memberships` in `src/TenancyHub.ApiService/Endpoints/MembershipEndpoints.cs`; **read-only members** MUST receive **403** (or tenant-safe equivalent) on roster `GET`—only administrators, operators, and **standard members** may list names and roles per spec US3 scenario 11
- [ ] T060 [US3] Implement canonical `PATCH /api/v1/agencies/{agencyId}/settings` only (no separate operator settings route) with admin-active vs operator-assigned rules in `src/TenancyHub.ApiService/Endpoints/AgencySettingsEndpoints.cs` per [contracts/api-v1.md](./contracts/api-v1.md); deny updates when agency is **Archived** (FR-015); operators may update on **active**/**suspended** assigned agencies only
- [ ] T061 [US3] Implement operator agency create `POST /api/v1/operator/agencies` with auto-assignment in `src/TenancyHub.ApiService/Endpoints/OperatorAgencyEndpoints.cs` per FR-016
- [ ] T062 [US3] Implement operator lifecycle `POST /api/v1/operator/agencies/{agencyId}/lifecycle` in `src/TenancyHub.ApiService/Endpoints/OperatorAgencyEndpoints.cs`
- [ ] T063 [US3] Implement platform operator management in `src/TenancyHub.ApiService/Endpoints/OperatorPlatformEndpoints.cs`: `GET /api/v1/operator/platform/operators`, `POST .../operators/{userId}/grant`, `POST .../operators/{userId}/revoke` (last-operator block), and `PUT .../operators/{userId}/assignments` per [contracts/api-v1.md](./contracts/api-v1.md)
- [ ] T064 [P] [US3] Build invitation acceptance page in `src/TenancyHub.Web/Components/Pages/Invitations.razor` wired to accept/decline API routes (explicit actions; matching email only)
- [ ] T065 [P] [US3] Build member roster and management pages in `src/TenancyHub.Web/Components/Pages/Members.razor` (admin/operator manage; standard member read-only list); omit member-list nav for **read-only members**; direct URL attempts show permission denied without data leakage
- [ ] T066 [P] [US3] Build operator agency create and lifecycle UI in `src/TenancyHub.Web/Components/Pages/Operator/Agencies.razor` (covers [quickstart.md](./quickstart.md) §5.1 agency create)
- [ ] T097 [P] [US3] Build platform operator grant/revoke and agency assignment UI in `src/TenancyHub.Web/Components/Pages/Operator/PlatformOperators.razor` wired to T063 endpoints (FR-006; [quickstart.md](./quickstart.md) §5.1)
- [ ] T098 [P] [US3] Build agency settings page in `src/TenancyHub.Web/Components/Pages/AgencySettings.razor` for administrators on **active** agencies and operators on assigned **active**/**suspended** agencies via T060; **do not** add ad-hoc `NavMenu.razor` links here—agency settings nav entry is wired in T075 via `ShellNavigationPolicy` after T074 (FR-011, FR-015; [quickstart.md](./quickstart.md) §5.2)
- [ ] T099 [US3] Add Fluent UI form validation (required fields, email/phone formats) on invite, provision, agency settings, operator agency create, and platform operator flows in `src/TenancyHub.Web/Components/` with server-side enforcement via API (constitution Principle I; pairs with T026)
- [ ] T067 [US3] Emit audit entries via `IAuditWriter` for every FR-008 category not covered solely by T044/T073: agency creation (T061) and auto-assign; agency lifecycle transitions (T062); agency settings changes (T060); membership invite, provision, accept, decline, revoke, suspend, reactivate, remove, and role changes including concurrent-update attempts (T053–T056); platform operator agency assignment changes and **platform operator** status grant/revoke including blocked last-operator attempts (T063); failed invitation accept/decline identity mismatch

**Checkpoint**: User Story 3 independently testable via quickstart §5.

---

## Phase 6: User Story 4 — Review audit history for accountability (Priority: P2)

**Goal**: Agency administrators view agency-scoped audit; operators view diagnostics summary and audit on assigned agencies only.

**Independent Test**: [quickstart.md](./quickstart.md) §6 — role change appears in audit; standard member denied.

### Implementation for User Story 4

- [ ] T068 [US4] Implement cursor-paginated audit queries in `src/TenancyHub.ApiService/Endpoints/AuditEndpoints.cs` per [contracts/api-v1.md](./contracts/api-v1.md): `GET /api/v1/agencies/{agencyId}/audit` for **agency administrators** (**Active** agency + **Active** membership; requires matching `X-TenancyHub-Agency-Id`) and `GET /api/v1/operator/agencies/{agencyId}/audit` for assigned operators on **Active**, **Suspended**, or **Archived** agencies (no active-agency header requirement; FR-009)
- [ ] T069 [US4] Implement `GET /api/v1/operator/agencies/{agencyId}/summary` (identity ref, display name, lifecycle, contacts, role counts including `Invited`, `LastLifecycleChangeAt`) in `src/TenancyHub.ApiService/Endpoints/OperatorDiagnosticsEndpoints.cs`
- [ ] T070 [US4] Add audit query service with agency filter, descending `OccurredAt`, and caller authorization matching FR-009 (admin + active agency + active membership vs operator assignment) in `src/TenancyHub.Application/Audit/AuditQueryService.cs`
- [ ] T071 [P] [US4] Build agency administrator audit page in `src/TenancyHub.Web/Components/Pages/Audit.razor` with Fluent UI data grid
- [ ] T072 [P] [US4] Build operator diagnostics page in `src/TenancyHub.Web/Components/Pages/Operator/Diagnostics.razor` linking to `GET /api/v1/operator/agencies/{agencyId}/audit` (not the agency-admin route)
- [ ] T073 [US4] Audit every operator cross-agency view action in `src/TenancyHub.Application/Operators/OperatorDiagnosticsService.cs`

**Checkpoint**: User Story 4 independently testable via quickstart §6.

---

## Phase 7: User Story 6 — Navigate a consistent application shell (Priority: P2)

> **Note**: **Phase 7 = US6** (shell polish). **Phase 8 = US5** (notifications)—story numbers differ from phase order because US5 depends on US3 membership/agency event hooks.

**Goal**: Shared Fluent UI navigation, permission-filtered nav (hidden entries), home coming-soon summary for admin/standard only, sign-out, English UK copy.

**Independent Test**: [quickstart.md](./quickstart.md) §8 — role-based nav and home placeholders.

### Implementation for User Story 6

- [ ] T074 [P] [US6] Define permission-to-nav map in `src/TenancyHub.Application/Navigation/ShellNavigationPolicy.cs` per FR-011
- [ ] T075 [US6] Refactor `src/TenancyHub.Web/Components/Layout/NavMenu.razor` to render only permitted routes from `ShellNavigationPolicy` (no disabled tease); include agency settings route when T060/T098 page exists and caller is permitted (depends T074, T098)
- [ ] T076 [US6] Update `src/TenancyHub.Web/Components/Pages/Home.razor` with coming-soon module summary for administrator and standard member on active agency only
- [ ] T077 [US6] Add consistent empty, error, and permission-denied components in `src/TenancyHub.Web/Components/Shared/StatusMessage.razor` using Fluent UI
- [ ] T078 [US6] Ensure signed-out users hitting in-app URLs redirect to sign-in without agency data in `src/TenancyHub.Web/Program.cs`
- [ ] T079 [P] [US6] Centralize UK English user strings in `src/TenancyHub.Web/Resources/UiStrings.cs` (no language selector)

**Checkpoint**: User Story 6 independently testable via quickstart §8.

---

## Phase 8: User Story 5 — Receive in-app notifications (Priority: P3)

> **Note**: **Phase 8 = US5**—implemented after US3 handlers (T056, T060–T062) supply FR-010 triggers.

**Goal**: Agency-scoped in-app notifications for FR-010 triggers; standard members mark read; read-only view-only; no dismiss.

**Independent Test**: [quickstart.md](./quickstart.md) §7 — trigger on role change; read state rules.

### Implementation for User Story 5

- [ ] T080 [US5] Implement membership notification triggers in `src/TenancyHub.Application/Notifications/NotificationTriggerService.cs` per FR-010: **agency bell/list** for activated, role changed, membership suspended/reactivated/removed when user has shell context; on **invite** creating **Invited** status, MAY persist a notification row but MUST NOT deliver to agency bell (invitee uses invitation-acceptance experience per T101/T064)
- [ ] T081 [US5] Implement agency settings and lifecycle notification triggers (display name/contact changes, suspend/archive/reactivate) including administrator copies where applicable in `src/TenancyHub.Application/Notifications/AgencyNotificationTriggerService.cs` per FR-010
- [ ] T082 [US5] Implement `GET /api/v1/agencies/{agencyId}/notifications` in `src/TenancyHub.ApiService/Endpoints/NotificationEndpoints.cs`
- [ ] T083 [US5] Implement `POST /api/v1/agencies/{agencyId}/notifications/{id}/read` (standard members only) in `src/TenancyHub.ApiService/Endpoints/NotificationEndpoints.cs`
- [ ] T084 [P] [US5] Add notification bell and list UI in `src/TenancyHub.Web/Components/Notifications/NotificationPanel.razor` only when user has FR-002 routine shell context with **Active** membership for the active agency (hide bell on invite-only layout T101, operator-no-assignment pages, and before accept); wire in `MainLayout.razor` per T041
- [ ] T085 [US5] Scope notification queries to active agency context only in `src/TenancyHub.Infrastructure/Services/NotificationQueryService.cs`

**Checkpoint**: User Story 5 independently testable via quickstart §7.

---

## Phase 9: Polish & cross-cutting concerns

**Purpose**: OpenAPI, tests, E2E, documentation alignment, and release readiness.

- [ ] T086 [P] Expose OpenAPI in Development from `src/TenancyHub.ApiService/Program.cs` and add generated snapshot to `specs/001-platform-foundation/contracts/openapi.yaml` when stable
- [ ] T087 [P] Add focused application unit tests for invite expiry, last-admin guard, last-operator guard, read-only roster **403**, membership/agency suspension access rules, FR-008 sign-in audit (assert **successful** and **failed** sign-in produce `IAuditWriter` calls with expected action types; assert `PUT /api/v1/me/active-agency` does **not** write an audit row in R1), and FR-010 notification trigger matrix (assert invite-pending does **not** enqueue agency-bell delivery; assert bell triggers after **Active** membership) in `tests/TenancyHub.Application.UnitTests/`
- [ ] T088 [P] Extend `tests/TenancyHub.ApiService.UnitTests/` for ProblemDetails shape, validation 400 responses (T026), and `/api/v1/me` mapping
- [ ] T089 [P] Add `docs/platform-foundation-user-journeys.md` documenting quickstart §3–§8 paths (including §3.6–§3.13, §4.1, §7 notification matrix, §5.1–5.3 operator/settings/decline flows) for contributors and link it from `docs/README.md` (constitution Principle VI)
- [ ] T090 Create `tests/TenancyHub.E2E/` Playwright C# project with journeys mapped to `docs/platform-foundation-user-journeys.md` per `docs/testing.md` (**depends on T089**—Principle VI)
- [ ] T091 Run full [quickstart.md](./quickstart.md) validation and fix gaps in code or docs
- [ ] T092 [P] Final pass on [docs/operations-rebuild-runbook.md](../../docs/operations-rebuild-runbook.md) for Entra (client certificate parameters `EntraWebClientCertificatePfx` / `EntraWebClientCertificatePassword`, not client secrets), Postgres volume reset, migrations, and seed alignment with implemented parameter names
- [ ] T093 Verify `dotnet build TenancyHub.slnx` and `dotnet test TenancyHub.slnx --configuration Release` clean with warnings-as-errors
- [ ] T095 [P] Add XML documentation on public tenancy and security contracts in `src/TenancyHub.ApiService/Endpoints/MeEndpoints.cs`, `src/TenancyHub.ApiService/Middleware/TenancyContextMiddleware.cs`, and `src/TenancyHub.Web/Services/TenancyHubApiClient.cs` per constitution Principle II (non-obvious auth, agency header, and token-forwarding assumptions)
- [ ] T100 [P] Add XML documentation on all **public** types in `src/TenancyHub.Domain/` and `src/TenancyHub.Infrastructure/` (entities, DbContext surface, DI extensions) per constitution Principle II and `docs/build-quality.md`—build must pass with warnings-as-errors

---

## Dependencies & execution order

### Phase dependencies

- **Phase 1** → **Phase 2** → **User story phases (3–8)** → **Phase 9**
- **US1 (Phase 3)** and **US2 (Phase 4)** are both P1; US2 depends on foundational middleware but can overlap late US1 API client work after T024
- **US3** depends on US1 identity and US2 authorization patterns
- **US4** depends on audit writer (T023) and US2 authorization
- **US6** can start after US1 shell (T041) in parallel with US3/US4
- **US5** depends on membership/agency event hooks from US3 handlers (T056, T060–T062)
- **T096** depends on T036 `/me`, T041 shell, T053 invitation APIs, and T064 `Invitations.razor` (banner may ship before full invitation page if linked route is stubbed)
- **T101** depends on T036 `/me`, T053 invitation APIs, and T064 `Invitations.razor` (invite-only gate; stub route allowed until T064)
- **T090** depends on **T089** (`docs/platform-foundation-user-journeys.md` must exist before E2E journeys)
- **T075** nav for agency settings depends on **T074** `ShellNavigationPolicy` and **T098** page (build page in US3; nav link in US6 per FR-011)
- **T102** depends on T020 (migration table names)
- **T097–T099** depend on T063/T060 APIs; **T096** should follow T064 when possible

### User story completion order (priority)

| Order | Story | Priority | Suggested MVP slice |
|-------|-------|----------|---------------------|
| 1 | US1 Sign-in & agency context | P1 | **Yes — MVP** |
| 2 | US2 Tenant isolation | P1 | With MVP for safe multi-tenant POC |
| 3 | US3 Membership & roles | P2 | |
| 4 | US4 Audit & diagnostics | P2 | |
| 5 | US6 Application shell | P2 | |
| 6 | US5 Notifications | P3 | |

### Parallel opportunities

- Phase 1: T004, T005, T008, T009 in parallel after T001–T003
- Phase 2: T010–T016 entity tasks in parallel; T021–T022, T026–T027, T029 in parallel after T025
- After Phase 2: US1 Web auth (T031) and API JWT (T030) in parallel; T032 research in parallel with T030–T031
- US3: T064–T066, T097–T099 UI pages in parallel after API endpoints exist
- Phase 9: T086–T089, T095, T100 in parallel; **T090 after T089**

### Parallel example: User Story 1

```bash
# After T024 middleware exists:
# Developer A: T030 API JWT + T036–T037 /me endpoints
# Developer B: T031 Web sign-in + T038–T040 API client and agency state
```

### Parallel example: User Story 3

```bash
# After T059 membership endpoints:
# Developer A: T064 Invitations.razor
# Developer B: T065 Members.razor
# Developer C: T066 Operator agencies UI
# Developer D (after T063): T097 Platform operators UI
```

---

## Implementation strategy

### MVP first (User Stories 1 + 2)

1. Complete Phase 1 and Phase 2.
2. Complete Phase 3 (US1) and validate [quickstart.md](./quickstart.md) §3.1–§3.5 and §3.8–§3.11 (§3.6–§3.7 with US3; optional §3.12–§3.13).
3. Complete Phase 4 (US2) and validate §4.
4. Stop for demo when cross-tenant safety is proven.

### Incremental delivery

1. Add US3 membership → demo partner onboarding.
2. Add US4 audit → compliance demo.
3. Add US6 shell polish → roadmap placeholder UX.
4. Add US5 notifications → operating-platform feel.

### Suggested MVP scope

**Minimum**: Phases 1–2 + Phase 3 (US1) + Phase 4 (US2) — trustworthy sign-in, agency context, and isolation.

---

## Notes

- AppHost must stay orchestration-only; no business rules in `TenancyHub.AppHost/`.
- Blazor must not reference PostgreSQL; all data via `TenancyHubApiClient`.
- Duplicate agency display names allowed; use internal `AgencyId` everywhere in APIs.
- Invite expiry: **30 days**; session idle **30 minutes**; absolute cap **12 hours** per session from initial sign-in.
- Format validation: all tasks use `- [ ] Tnnn` with file paths and story labels where required.
- Remediation (2026-10-01 `/speckit-analyze` pass 1): T094 sign-out; T095 Web/API XML; expanded T046–T047, T052, T059–T060, T065, T087; research/plan/spec/quickstart aligned for archived settings, roster, concurrent sign-out, and performance non-gate.
- Remediation (2026-10-01 `/speckit-analyze` pass 2): T096 active+invited shell routing; T097 operator platform UI; T098 agency settings UI; T099 Blazor validation; T100 Domain/Infrastructure XML; canonical settings route in contract + T060/T063 wording; quickstart §3.6, §5.1–5.3; spec edge-case pointer; plan performance note.
- Remediation (2026-10-01 `/speckit-analyze` pass 3): T101 invite-only routing; T102 seed script; FR-010/T080/T087 invite vs bell; T068/T070 audit access rules; T041/T084 bell visibility; research disabled-account decision; T007 resource names; spec Notification entity + assumption dedup; quickstart §3.7.
- Remediation (2026-10-01 `/speckit-analyze` pass 4): FR-008 successful sign-in + explicit non-audit agency switch; T044/T087/§3.8 SC-004 sign-in audit; T067 FR-008 checklist; T037/T102/api-v1 contract alignment; clarifications intro (normative FR precedence).
- Remediation (2026-10-01 `/speckit-analyze` pass 5): US1 checkpoint vs quickstart §3.6–§3.7 (joint US1+US3); T053 deps on T096/T101; T075/T098 nav via ShellNavigationPolicy; T090→T089; FR-003 tags on US2 tasks; spec FR-001 disabled-account POC bound + step-up out of scope.
- Remediation (2026-10-01 `/speckit-analyze` pass 6): quickstart §3.9–§3.12, §4.1 lifecycle gates, §7 FR-010 matrix; T049 retagged FR-013; plan.md pass 6 note.
- Remediation (2026-10-01 `/speckit-analyze` pass 7): §3.9 disabled-directory step (renumber §3.10–§3.13); US1 checkpoint §3.8–§3.11; quickstart §9 ↔ T089; api-v1 audit auth table (FR-009).
- Remediation (2026-10-01 `/speckit-analyze` pass 8): `GET /operator/agencies/{agencyId}/audit` in contract + T068/T072; quickstart §3.11 T043-only; plan pass 8 note.
