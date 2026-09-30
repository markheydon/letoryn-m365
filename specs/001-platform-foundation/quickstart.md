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

Apply platform operator seed per [docs/operations-rebuild-runbook.md §5](../../docs/operations-rebuild-runbook.md#5-seed-initial-platform-operator).

**Expected**: API health `/health` returns healthy; no connection errors in `aspire logs apiservice`.

---

## 3. Sign-in and agency context (User Story 1)

| Step | Action | Expected |
|------|--------|----------|
| 3.1 | Open `webfrontend` URL | Redirect to Microsoft sign-in |
| 3.2 | Sign in as User A (Agency 1 member) | Shell shows Agency 1 name; home loads |
| 3.3 | Call `GET /api/v1/me` with token | Membership for Agency 1 only in scope |
| 3.4 | Sign in as User B (Agency 2) in another browser | Agency 2 context; no Agency 1 data in UI |

Multi-agency user: verify last-used agency default and agency switch clears stale UI state (spec scenario 3).

---

## 4. Tenant isolation (User Story 2)

With User A signed in to Agency 1:

1. Attempt `GET /api/v1/agencies/{agency2Id}/audit` (or any Agency 2 id).
2. **Expected**: 403 or 404 with no Agency 2 payload fields.

Repeat with forged `X-TenancyHub-Agency-Id` header.

---

## 5. Membership and roles (User Story 3)

As agency administrator or assigned operator:

1. Invite user with **read-only** role → membership **Invited**.
2. Invitee signs in → invitation acceptance UI → accept → **Active**.
3. Read-only user attempts `POST .../notifications/{id}/read` → **403**.
4. Standard member marks notification read → **200**.

Verify last-administrator guard by attempting to demote sole admin → blocked.

---

## 6. Audit (User Story 4)

1. Perform role change.
2. As agency administrator, open audit view → entry with actor, target, summary within 1 minute (SC-004).
3. Standard member navigates to audit URL → denied; nav hidden.

---

## 7. Notifications (User Story 5)

Trigger role change or lifecycle suspend → affected user sees in-app notification; no email.

---

## 8. Shell (User Story 6)

Administrator/standard member on active agency: home shows coming-soon module summary. Read-only member: no module summary. Forbidden nav entries hidden.

---

## 9. Automated tests

```bash
dotnet test TenancyHub.slnx --configuration Release
```

Playwright C# journeys (when added) map to sections 3–7; update this quickstart if journey names differ.

---

## 10. Rebuild drill (annual / new machine)

1. Follow [docs/operations-rebuild-runbook.md](../../docs/operations-rebuild-runbook.md) from §2 (Entra) and §4 (Postgres volume if needed).
2. Re-run sections 1–2 of this quickstart.
3. **Expected**: Same functional outcomes without code changes—proves documentation accuracy.

---

## Failure triage

| Symptom | Check |
|---------|--------|
| Redirect URI mismatch | Entra registration vs `aspire describe webfrontend` URL |
| API 401 | API audience/client id in secrets; token scope |
| Postgres auth failed | [operations-rebuild-runbook.md §4.3](../../docs/operations-rebuild-runbook.md#43-fix-password--volume-mismatch) volume reset |
| Empty operator flows | [operations-rebuild-runbook.md §5](../../docs/operations-rebuild-runbook.md#5-seed-initial-platform-operator) seed |

Aspire investigation: `.agents/skills/aspire-monitoring/` — `aspire logs`, `aspire otel traces`.
