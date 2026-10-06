# Quickstart: Platform foundation (R1)

Runnable validation for the platform foundation feature after implementation. This guide proves end-to-end behavior; it does **not** replace [tasks.md](./tasks.md) implementation steps.

**Prerequisites**: [docs/operations-rebuild-runbook.md](../../docs/operations-rebuild-runbook.md) completed (Entra app, user secrets, Postgres healthy).

**Contracts**: [contracts/api-v1.md](./contracts/api-v1.md)  
**Data model**: [data-model.md](./data-model.md)

---

## 1. Start the distributed application

```bash
cd /path/to/tenancy-hub
dotnet build TenancyHub.slnx
aspire run
```

Wait for resources:

```bash
aspire wait postgres --status healthy
aspire wait apiservice --status healthy
aspire wait webfrontend --status healthy
aspire describe
```

Note HTTPS URLs for `webfrontend` and `apiservice`.

---

## 2. Database ready

When EF migrations ship with implement:

```bash
dotnet ef database update --project src/TenancyHub.Infrastructure --startup-project src/TenancyHub.ApiService
```

Apply platform operator seed per [docs/operations-rebuild-runbook.md §5](../../docs/operations-rebuild-runbook.md#5-seed-initial-platform-operator) using [scripts/r1/seed-platform-operator.sql](../../scripts/r1/seed-platform-operator.sql) (table names finalized in task T102 after first migration).

**Expected**: API health `/health` returns healthy; no connection errors in `aspire logs apiservice`.

---

## 3. Sign-in and agency context (User Story 1)

**Scope**: Steps **3.1–3.5** and **3.8–3.11** validate US1 alone (including disabled-directory denial, access-not-configured, operator-without-assignments). **3.12–3.13** are optional manual session-cap checks. Steps **3.6–3.7** require US3 invitation APIs and UI ([tasks.md](./tasks.md) T053, T064, T096, T101): run after membership work or as a joint US1+US3 gate.

| Step | Action | Expected |
|------|--------|----------|
| 3.1 | Open `webfrontend` URL | Redirect to Microsoft sign-in |
| 3.2 | Sign in as User A (Agency 1 member) | Shell shows Agency 1 name; home loads |
| 3.3 | Call `GET /api/v1/me` with token | Membership for Agency 1 only in scope |
| 3.4 | Sign in as User B (Agency 2) in another browser | Agency 2 context; no Agency 1 data in UI |

Multi-agency user: verify last-used agency default and agency switch clears stale UI state (spec scenario 3).

| Step | Action | Expected |
|------|--------|----------|
| 3.5 | Same user signed in on two browsers; sign out in browser A only | Browser A requires sign-in again; browser B remains signed in (FR-001 concurrent sessions) |
| 3.6 | User has **Active** membership in Agency A and a pending **Invited** membership in Agency B | Shell opens on last-used **Active** Agency A; banner or nav prompts to open invitation acceptance; **Invited** in Agency B is not auto-activated on sign-in alone |
| 3.7 | User has **only** **Invited** membership(s), no **Active** membership, not a platform operator with shell path | After sign-in → **invitation-acceptance** experience only (not routine agency shell); no agency notification bell; accept or decline required per invite (US1 scenario 5; tasks T101, T064) |
| 3.8 | After User A (Agency 1) completes sign-in in step 3.2 | Within 1 minute, authorized audit query shows a **successful sign-in** audit entry for that user (FR-008; `AgencyId` may be Agency 1 when shell context is established, per T044). Intentional failed sign-in (wrong tenant user or revoked session) produces a **failed sign-in** audit entry without agency data leakage |
| 3.9 | Work account **disabled in Entra** (or token refresh rejected) while Tenancy Hub membership remains | Sign-in or next API validation fails; no agency data; **failed sign-in** or terminated-session audit per T033/T044 (FR-001; use runbook access-token lifetime ≤ 60 minutes for POC) |
| 3.10 | Sign in as a work account with **no** Tenancy Hub membership and **no** platform-operator global path | **Access not configured** page (T043); no agency data in UI or API |
| 3.11 | Sign in as **platform operator** with **zero** agency assignments and no agency memberships | **Operator home** / operator-only global flows (create agency, manage operators); **no** active agency shell context until assigned (T043; US1 scenario 9) |
| 3.12 | *(Optional manual)* Signed-in user idle **30 minutes** without activity that resets the idle timer | Session ends; sign-in required; unsaved client state discarded (FR-001, T031/T034) |
| 3.13 | *(Optional manual)* Signed-in session reaches **12 hours** from initial sign-in | Session ends even if recently active; sign-in required (FR-001, T031/T034) |

**SC-002 (informal)**: With membership pre-provisioned, first sign-in to shell completes in under 3 minutes: manual check only, not a CI gate in R1 (see [plan.md](./plan.md): not a load-test or CI obligation).

---

## 4. Tenant isolation (User Story 2)

With User A signed in to Agency 1:

1. Attempt `GET /api/v1/agencies/{agency2Id}/audit` (or any Agency 2 id).
2. **Expected**: 403 or 404 with no Agency 2 payload fields.

Repeat with forged `X-TenancyHub-Agency-Id` header.

### 4.1 Lifecycle and membership gates (US2 scenarios 3–5)

1. **Agency suspended**: Member of that agency attempts sign-in or active context → denied with clear **agency suspended** message; assigned platform operator may still use operator flows for that agency (invite/provision/roles) while members cannot sign in until **Active** (T047).
2. **Agency archived**: Member cannot operate; assigned operator can read audit/diagnostics; operator **invite/provision/role change** denied until reactivation to **Active** or **Suspended** (T047).
3. **Membership suspended** (agency **Active**): User blocked for that agency only; may still access other valid agencies; distinct message from agency-level suspension (T047, T052).
4. Cross-tenant checks from §4 still pass after lifecycle changes (no Agency B payload leakage).

---

## 5. Membership and roles (User Story 3)

### 5.1 Operator provisioning (FR-016, FR-006)

As a **platform operator**:

1. Open operator **Agencies** UI (or `POST /api/v1/operator/agencies`) → new agency **Active**; creator **auto-assigned** (auditable).
2. Open **Platform operators** UI → grant operator status to a work-account identity, or `PUT .../operators/{userId}/assignments` with agency ids; verify unassigned agencies remain inaccessible in operator flows.
3. Attempt to revoke the last remaining platform operator → **blocked** with clear message.

### 5.2 Invites, provision, settings

As agency administrator or assigned operator:

1. Invite user with **read-only** role → membership **Invited**.
2. Invitee signs in → invitation acceptance UI → **accept** → **Active**.
3. Repeat invite for another user → invitee **declines** → membership **Removed**; accept blocked until re-invited (audit only for decline; no inviter in-app notification in R1).
4. **Provision** a user with **standard member** role → **Active** immediately (no accept step).
5. As agency administrator on an **active** agency, update display name or contact via **Agency settings** UI (`PATCH /api/v1/agencies/{agencyId}/settings`) → saved and auditable. On a **suspended** agency, agency admin cannot sign in; assigned operator may still update via the same API path.

### 5.3 Permissions and guards

1. Read-only user attempts `POST .../notifications/{id}/read` → **403**.
2. Standard member marks notification read → **200** (after §7 triggers exist).

Verify last-administrator guard by attempting to demote sole admin → blocked.

3. Suspend a user’s agency membership (agency **active**) → user cannot use that agency as active context; clear suspension message; other agencies still work if applicable (see §7 for membership-suspended notice).

4. **Read-only member** calls `GET /api/v1/agencies/{agencyId}/memberships` or opens member list URL → **403** / permission denied; nav hidden. **Standard member** → read-only roster **200**.

5. Change a member’s role → within **one minute** the member’s permitted actions reflect the new role without full logout (SC-003; authorization reads DB or cache TTL ≤ 1 minute per **T058**: re-test with a second browser session or wait ≤ 60s).

---

## 6. Audit (User Story 4)

1. Complete a successful member sign-in (see §3.2 / §3.8) → **successful sign-in** entry visible to authorized viewer within 1 minute (SC-004; FR-008).
2. Perform role change.
3. As agency administrator, open audit view → role-change entry with actor, target, summary within 1 minute (SC-004).
4. Multi-agency user switches active agency via shell → **no** new audit row for the switch alone (FR-008); a subsequent auditable action (for example settings update) appears under the new agency.
5. Standard member navigates to audit URL → denied; nav hidden.

---

## 7. Notifications (User Story 5)

Prerequisites: user has **Active** membership and routine shell agency context (no agency bell on invite-only layout per §3.7).

| Trigger | Expected (FR-010) |
|---------|-------------------|
| Role changed | Affected user sees unread item in agency notification list; standard member may mark read; read-only cannot |
| Membership activated (provision or accept) | User sees notice in agency list once in shell context |
| Membership suspended / reactivated / removed | Affected user sees in-app notice (users with **Active** or **Suspended** membership for lifecycle-related agency events where applicable) |
| Agency display name or contact change | Affected users + agency administrators (where applicable) see notice |
| Agency lifecycle → **Suspended**, **Archived**, or reactivated to **Active**/**Suspended** | Users with **Active** or **Suspended** membership in that agency see notice; **Invited-only** and **Removed** do not |
| Pending **Invited** membership (invite sent) | Invitee sees pending invite in **invitation-acceptance** flow only: not in agency notification bell until **Active** (T080/T087) |

No email or push in R1. No dismiss/remove from list: mark read only for standard members.

---

## 8. Shell (User Story 6)

Administrator/standard member on active agency: home shows coming-soon module summary. Read-only member: no module summary. Forbidden nav entries hidden. Sign-out control ends only the current browser session (see §3.5).

---

## 9. Automated tests

```bash
dotnet test TenancyHub.slnx --configuration Release
```

Playwright C# journeys (when added) map to [docs/platform-foundation-user-journeys.md](../../docs/platform-foundation-user-journeys.md) (T089), aligned with quickstart **§3.6–§3.13**, **§4.1**, **§5.1–§5.3**, **§7** matrix, and **§8**; update the journeys doc if step numbers change.

---

## 10. Rebuild drill (annual / new machine)

1. Follow [docs/operations-rebuild-runbook.md](../../docs/operations-rebuild-runbook.md) from §2 (Entra) and §4 (Postgres volume if needed).
2. Re-run sections 1–2 of this quickstart.
3. **Expected**: Same functional outcomes without code changes: proves documentation accuracy.

---

## Failure triage

| Symptom | Check |
|---------|--------|
| Redirect URI mismatch | Entra registration vs `aspire describe webfrontend` URL |
| API 401 | API audience/client id in secrets; token scope |
| Postgres auth failed | [operations-rebuild-runbook.md §4.3](../../docs/operations-rebuild-runbook.md#43-fix-password--volume-mismatch) volume reset |
| Empty operator flows | [operations-rebuild-runbook.md §5](../../docs/operations-rebuild-runbook.md#5-seed-initial-platform-operator) seed |

Aspire investigation: `.agents/skills/aspire-monitoring/`: `aspire logs`, `aspire otel traces`.
