# Platform foundation user journeys (R1)

Contributor and E2E reference for **platform foundation** (roadmap **R1**). Each journey maps 1:1 to a step or subsection in [specs/001-platform-foundation/quickstart.md](../specs/001-platform-foundation/quickstart.md) **§3–§8**. When quickstart step numbers change, update this document in the same change set (constitution **Principle VI**).

**Prerequisites**: [operations-rebuild-runbook.md](./operations-rebuild-runbook.md) (Entra, secrets, Postgres, platform operator seed). **Contracts**: [contracts/api-v1.md](../specs/001-platform-foundation/contracts/api-v1.md).

**Automation**: [testing.md](./testing.md): Playwright C# E2E in `tests/Letoryn.E2E/` cites journey IDs from the index below (`[Trait("Journey", "R1-J…")]`). Prefer stable API + UI paths over brittle selectors.

---

## Journey index

| ID | Quickstart | User story | Summary | Typical automation |
|----|------------|------------|---------|-------------------|
| R1-J3.1 | §3.1 | US1 | Anonymous hit → Microsoft sign-in | E2E |
| R1-J3.2 | §3.2 | US1 | Agency member sign-in → agency shell | E2E |
| R1-J3.3 | §3.3 | US1 | `GET /api/v1/me` scoped to active agency | API / E2E |
| R1-J3.4 | §3.4 | US1 | Second agency user: no cross-agency UI data | E2E |
| R1-J3.5 | §3.5 | US1 | Sign-out one browser; other session remains | E2E |
| R1-J3.6 | §3.6 | US1+US3 | Active + pending invite → last-used active shell; invite prompt | E2E |
| R1-J3.7 | §3.7 | US1+US3 | Invite-only user → invitation-acceptance only | E2E |
| R1-J3.8 | §3.8 | US1 | Successful sign-in → sign-in audit within 1 min | API / E2E |
| R1-J3.9 | §3.9 | US1 | Disabled Entra account → denied; failed sign-in audit | Manual |
| R1-J3.10 | §3.10 | US1 | No membership → access not configured | E2E |
| R1-J3.11 | §3.11 | US1 | Platform operator, no agencies → operator home | E2E |
| R1-J3.12 | §3.12 | US1 | Idle 30 minutes → session ends | Manual |
| R1-J3.13 | §3.13 | US1 | Absolute 12 hours → session ends | Manual |
| R1-J4.0 | §4 (intro) | US2 | Cross-tenant API / forged agency header denied | API |
| R1-J4.1 | §4.1 | US2 | Agency/membership lifecycle gates + isolation | E2E / API |
| R1-J5.1 | §5.1 | US3 | Operator create agency, assign operators, last-operator guard | E2E |
| R1-J5.2 | §5.2 | US3 | Invite accept/decline, provision, agency settings | E2E |
| R1-J5.3 | §5.3 | US3 | Role guards, membership suspend, roster visibility | E2E / API |
| R1-J6 | §6 | US4 | Audit visibility, role change, agency switch, member denied | E2E |
| R1-J7 | §7 | US5 | In-app notification matrix (no email/push in R1) | E2E |
| R1-J8 | §8 | US6 | Shell modules, nav hiding, per-browser sign-out | E2E |

Steps **§3.6–§3.7** need invitation APIs and UI (US3). **§3.12–§3.13** and **§3.9** are documented for completeness but are **manual** in R1 (timing or Entra directory changes).

---

## §3 Sign-in and agency context (US1)

### R1-J3.1: Redirect to Microsoft sign-in

| | |
|--|--|
| **Actor** | Unauthenticated visitor |
| **Path** | Browser → `webfrontend` base URL |
| **Expected** | Redirect to Microsoft sign-in; no agency data |

### R1-J3.2: Member lands in agency shell

| | |
|--|--|
| **Actor** | User A: **Active** member of Agency 1 |
| **Path** | Complete Entra sign-in after §3.1 |
| **Expected** | Shell shows Agency 1 name; home loads |

### R1-J3.3: `/me` reflects active membership scope

| | |
|--|--|
| **Actor** | User A (Agency 1) with bearer token |
| **Path** | `GET /api/v1/me` |
| **Expected** | Response membership scope is Agency 1 only (matches active context) |

### R1-J3.4: Tenant-scoped UI for second agency

| | |
|--|--|
| **Actor** | User B: **Active** member of Agency 2 (separate browser/profile) |
| **Path** | Sign in → browse shell |
| **Expected** | Agency 2 context; no Agency 1 data in UI |

**Multi-agency same user**: Last-used agency is the default after sign-in; switching agency clears stale UI state (spec scenario 3): exercise when validating **R1-J3.6** or dedicated switch flows.

### R1-J3.5: Sign-out is per browser session

| | |
|--|--|
| **Actor** | Same user signed in on browser A and B |
| **Path** | Sign out in browser A only |
| **Expected** | Browser A requires sign-in again; browser B remains signed in (FR-001 concurrent sessions) |

### R1-J3.6: Active agency + pending invitation

| | |
|--|--|
| **Actor** | User with **Active** membership in Agency A and **Invited** in Agency B |
| **Path** | Sign in |
| **Expected** | Shell opens on last-used **Active** Agency A; banner or nav prompts invitation acceptance; Agency B **Invited** is not auto-activated on sign-in alone |

**Depends on**: US3 invite flow ([quickstart §5.2](../specs/001-platform-foundation/quickstart.md)).

### R1-J3.7: Invite-only sign-in

| | |
|--|--|
| **Actor** | User with **only** **Invited** membership(s), not a platform operator with shell path |
| **Path** | Sign in |
| **Expected** | **Invitation-acceptance** experience only (not routine agency shell); no agency notification bell; accept or decline required per invite |

### R1-J3.8: Sign-in audit (success and intentional failure)

| | |
|--|--|
| **Actor** | User A after successful sign-in; optionally wrong-tenant or revoked session |
| **Path** | Complete §3.2; query authorized audit API/UI within 1 minute |
| **Expected** | **Successful sign-in** audit for User A (`AgencyId` may be Agency 1 when shell context is established). Intentional failed sign-in → **failed sign-in** audit without agency data leakage (FR-008) |

### R1-J3.9: Disabled directory account

| | |
|--|--|
| **Actor** | Work account disabled in Entra (or token refresh rejected) while Letoryn membership remains |
| **Path** | Sign-in or next API call after disable |
| **Expected** | Access denied; no agency data; **failed sign-in** or terminated-session audit (FR-001; POC access token lifetime ≤ 60 minutes: [runbook](./operations-rebuild-runbook.md)) |

### R1-J3.10: Access not configured

| | |
|--|--|
| **Actor** | Valid Entra user with no Letoryn membership and no platform-operator global path |
| **Path** | Sign in |
| **Expected** | **Access not configured** page; no agency data in UI or API |

### R1-J3.11: Operator without agency assignments

| | |
|--|--|
| **Actor** | **Platform operator** with zero agency assignments and no agency memberships |
| **Path** | Sign in |
| **Expected** | **Operator home** / operator-only flows (create agency, manage operators); no active agency shell until assigned |

### R1-J3.12: Idle session timeout (optional)

| | |
|--|--|
| **Actor** | Signed-in user |
| **Path** | No activity that resets idle timer for **30 minutes** |
| **Expected** | Session ends; sign-in required; unsaved client state discarded (FR-001) |

### R1-J3.13: Absolute session cap (optional)

| | |
|--|--|
| **Actor** | Signed-in user |
| **Path** | **12 hours** from initial sign-in |
| **Expected** | Session ends even if recently active; sign-in required (FR-001) |

---

## §4 Tenant isolation (US2)

### R1-J4.0: Cross-tenant API denial

| | |
|--|--|
| **Actor** | User A signed in to Agency 1 |
| **Path** | `GET /api/v1/agencies/{agency2Id}/audit` (or any Agency 2 resource id); repeat with forged `X-Letoryn-Agency-Id` |
| **Expected** | **403** or **404** with no Agency 2 payload fields |

### R1-J4.1: Lifecycle and membership gates

Validate US2 scenarios 3–5; after each state change, **R1-J4.0** still holds.

| Sub-step | Condition | Actor | Expected |
|----------|-----------|-------|----------|
| 4.1a | Agency **Suspended** | Member of that agency | Sign-in or active context denied: clear **agency suspended** message; assigned platform operator may still use operator flows for that agency (invite/provision/roles) |
| 4.1b | Agency **Archived** | Member | Cannot operate; assigned operator can read audit/diagnostics; operator invite/provision/role change denied until reactivation to **Active** or **Suspended** |
| 4.1c | Membership **Suspended** (agency **Active**) | Affected user | Blocked for that agency only; may access other valid agencies; message distinct from agency-level suspension |
| 4.1d | Regression | Any prior actor | Cross-tenant checks: no Agency B payload leakage |

---

## §5 Membership and roles (US3)

### R1-J5.1: Operator provisioning

| | |
|--|--|
| **Actor** | **Platform operator** |
| **Path** | |
| 1 | Operator **Agencies** UI or `POST /api/v1/operator/agencies` → new agency **Active**; creator **auto-assigned** (auditable) |
| 2 | **Platform operators** UI or `PUT .../operators/{userId}/assignments` with agency ids → unassigned agencies inaccessible in operator flows |
| 3 | Attempt to revoke the **last** platform operator |
| **Expected** | Steps 1–2 succeed with audit trail; step 3 **blocked** with clear message (FR-016, FR-006) |

### R1-J5.2: Invites, provision, settings, decline

| | |
|--|--|
| **Actor** | Agency administrator or assigned platform operator |
| **Path** | |
| 1 | Invite user with **read-only** role → membership **Invited** |
| 2 | Invitee signs in → acceptance UI → **accept** → **Active** |
| 3 | Invite another user → invitee **declines** → membership **Removed**; accept blocked until re-invited (audit only for decline; no inviter in-app notification in R1) |
| 4 | **Provision** user with **standard member** role → **Active** immediately (no accept step) |
| 5 | Agency administrator on **active** agency: **Agency settings** UI or `PATCH /api/v1/agencies/{agencyId}/settings` → saved and auditable |
| 6 | **Suspended** agency: agency admin cannot sign in; assigned operator may still update settings via same API |
| **Expected** | States and permissions match each step; ties to **R1-J3.6**, **R1-J3.7**, **R1-J7** |

### R1-J5.3: Permissions and guards

| | |
|--|--|
| **Actor** | Read-only, standard member, agency administrator |
| **Path** | |
| 1 | Read-only: `POST .../notifications/{id}/read` → **403** |
| 2 | Standard member: mark notification read → **200** (after **R1-J7** triggers exist) |
| 3 | Attempt to demote sole agency administrator → **blocked** |
| 4 | Suspend user’s agency membership (agency **active**) → user cannot use that agency as active context; other agencies still work if applicable |
| 5 | Read-only: `GET .../memberships` or member list URL → **403** / nav hidden; standard member → roster **200** |
| 6 | Change member role → within **one minute** permitted actions reflect new role without full logout (second browser or wait ≤ 60s; SC-003) |
| **Expected** | Authorization matches role; distinct suspension messaging (see **R1-J7** for membership-suspended notice) |

---

## §6 Audit (US4): R1-J6

| Step | Path | Expected |
|------|------|----------|
| 6.1 | Successful member sign-in (**R1-J3.2** / **R1-J3.8**) | **Successful sign-in** visible to authorized viewer within 1 minute (SC-004; FR-008) |
| 6.2 | Perform role change (**R1-J5.3**) | Agency administrator audit view → role-change entry (actor, target, summary) within 1 minute |
| 6.3 | Multi-agency user switches active agency in shell | **No** new audit row for switch alone; subsequent auditable action (e.g. settings update) under new agency |
| 6.4 | Standard member opens audit URL | Denied; nav hidden |

Operator agency audit: `GET /operator/agencies/{agencyId}/audit` per [api-v1](../specs/001-platform-foundation/contracts/api-v1.md) when validating operator read paths (**R1-J4.1** archived agency).

---

## §7 Notifications (US5): R1-J7

**Prerequisite**: User has **Active** membership and routine shell agency context. No agency notification bell on invite-only layout (**R1-J3.7**).

| Trigger | Expected in-app (FR-010) |
|---------|---------------------------|
| Role changed | Affected user sees unread item in agency notification list; standard member may mark read; read-only cannot |
| Membership activated (provision or accept) | User sees notice in agency list once in shell context |
| Membership suspended / reactivated / removed | Affected user sees in-app notice (users with **Active** or **Suspended** membership where applicable for lifecycle-related agency events) |
| Agency display name or contact change | Affected users + agency administrators (where applicable) see notice |
| Agency lifecycle → **Suspended**, **Archived**, or reactivated to **Active** / **Suspended** | Users with **Active** or **Suspended** membership in that agency see notice; **Invited-only** and **Removed** do not |
| Pending **Invited** membership (invite sent) | Invitee sees pending invite in **invitation-acceptance** flow only: not in agency notification bell until **Active** |

**R1 bounds**: No email or push. No dismiss/remove from list: mark read only for standard members.

**E2E hint**: Pair each row with the mutating journey (**R1-J5.2**, **R1-J5.3**, **R1-J4.1**, settings patch) then assert list + read permission via **R1-J5.3** step 1–2.

---

## §8 Shell (US6): R1-J8

| | |
|--|--|
| **Actor** | Administrator or standard member on **active** agency; read-only member |
| **Path** | Sign in → home and nav |
| **Expected** | Admin/standard: home shows coming-soon module summary. Read-only: no module summary. Forbidden nav entries hidden. Sign-out ends **current browser session** only (**R1-J3.5**) |

---

## Maintaining this document

1. Run acceptance from [quickstart §1–§2](../specs/001-platform-foundation/quickstart.md) before journey validation.
2. When adding or renaming a quickstart step in §3–§8, add or renumber the matching **R1-J*** row in the journey index and section body.
3. When adding Playwright tests, name test classes or traits after journey IDs (for example `R1_J5_2_InviteAccept`) and keep [quickstart §9](../specs/001-platform-foundation/quickstart.md) pointed at this file.
