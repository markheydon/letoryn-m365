# Data model: Platform foundation (R1)

**Feature**: `001-platform-foundation`  
**Storage**: PostgreSQL via EF Core (`TenancyHubDbContext`)  
**Canonical spec**: [spec.md](./spec.md)

All tables are **agency-scoped** where noted. Internal **`AgencyId`** (UUID) is the tenancy key; display name is not unique (FR-015).

---

## Agency

| Field | Type | Notes |
|-------|------|--------|
| Id | uuid | Primary key, canonical tenant identity |
| DisplayName | string | Required; duplicates allowed |
| PrimaryContactEmail | string | Required |
| PrimaryContactPhone | string | Required |
| LifecycleStatus | enum | `Active`, `Suspended`, `Archived` |
| CreatedAt | timestamptz | UTC |
| UpdatedAt | timestamptz | UTC |
| LastLifecycleChangeAt | timestamptz | For operator diagnostics summary |

**Validation**: Lifecycle transitions only via platform operator flows (FR-004). Archived blocks membership mutations (FR-007).

**State transitions** (operator-only):

```text
Active <-> Suspended
Active -> Archived
Suspended -> Archived
Archived -> Suspended | Active
```

---

## UserIdentity

Maps Entra **object id** and **email** (UPN/preferred username) to product identity.

| Field | Type | Notes |
|-------|------|--------|
| Id | uuid | Internal |
| EntraObjectId | string | Unique, from token `oid` |
| Email | string | Normalized; sign-in match for invites |
| IsPlatformOperator | bool | Separate from agency roles (FR-006) |
| LastUsedAgencyId | uuid? | Nullable; for default shell context (FR-002) |
| CreatedAt | timestamptz | |

**Rules**: At least one `IsPlatformOperator = true` always (FR-006). Operator status grant/revoke auditable.

---

## AgencyMembership

| Field | Type | Notes |
|-------|------|--------|
| Id | uuid | |
| AgencyId | uuid | FK → Agency |
| UserIdentityId | uuid | FK → UserIdentity |
| Status | enum | `Invited`, `Active`, `Suspended`, `Removed` |
| AgencyRole | enum | Exactly one: `Administrator`, `StandardMember`, `ReadOnlyMember` |
| InvitedAt | timestamptz? | Set on invite |
| InvitedRoleSnapshot | enum? | Role at invite (same as AgencyRole while invited) |
| ExpiresAt | timestamptz? | Invite: Created + 30 days |
| ActivatedAt | timestamptz? | |
| UpdatedAt | timestamptz | |

**Uniqueness**: At most one **pending** `Invited` row per (AgencyId, Email/User) — idempotent invites (FR-007).

**Invariants**:

- Cannot leave agency with zero **active** administrators once ≥1 active admin exists (FR-007).
- `Removed` ends access; `Suspended` blocks agency access but retains role for reactivation.
- Archived agency: no new invites/provisions/role changes.

---

## PlatformOperatorAssignment

| Field | Type | Notes |
|-------|------|--------|
| Id | uuid | |
| UserIdentityId | uuid | Operator |
| AgencyId | uuid | |
| AssignedAt | timestamptz | |
| AssignedByUserIdentityId | uuid | |

**Uniqueness**: (UserIdentityId, AgencyId). Auto-created when operator creates agency (FR-016).

---

## AuditEvent

Append-only.

| Field | Type | Notes |
|-------|------|--------|
| Id | uuid | |
| AgencyId | uuid? | Null for global operator-only events if needed |
| OccurredAt | timestamptz | |
| ActorUserIdentityId | uuid? | Null for system |
| ActionType | string | Stable code, e.g. `membership.role_changed` |
| Summary | string | Human-readable English |
| TargetUserIdentityId | uuid? | |
| PayloadJson | jsonb? | Structured details; no secrets |

**Retention**: Full agency lifetime; no purge (FR-008).

---

## Notification

Agency-scoped in-app shell (FR-010).

| Field | Type | Notes |
|-------|------|--------|
| Id | uuid | |
| AgencyId | uuid | |
| UserIdentityId | uuid | Recipient |
| CreatedAt | timestamptz | |
| Category | string | Maps to trigger types in FR-010 |
| Title | string | English |
| Body | string | English |
| IsRead | bool | Default false |
| ReadAt | timestamptz? | Standard members only may set |

**Rules**: No dismiss/delete in R1. Read-only members cannot update `IsRead`.

---

## UserSession (optional metadata table)

Tracks per-session absolute sign-in time for 12-hour cap and audit correlation; cookie auth remains primary.

| Field | Type | Notes |
|-------|------|--------|
| Id | uuid | |
| UserIdentityId | uuid | |
| StartedAt | timestamptz | Initial sign-in |
| LastActivityAt | timestamptz | Idle reset |
| EndedAt | timestamptz? | Sign-out or expiry |

Concurrent sessions = multiple rows (FR-001).

---

## Relationships (ER overview)

```text
UserIdentity 1---* AgencyMembership *---1 Agency
UserIdentity 1---* PlatformOperatorAssignment *---1 Agency
Agency 1---* AuditEvent
Agency 1---* Notification
UserIdentity 1---* Notification
```

---

## Indexes (implementation hints)

- `AgencyMembership (AgencyId, Status)`
- `AgencyMembership (UserIdentityId, Status)`
- `AuditEvent (AgencyId, OccurredAt DESC)`
- `Notification (UserIdentityId, AgencyId, CreatedAt DESC)`
- `UserIdentity (EntraObjectId)` unique
- `UserIdentity (Email)` for invite lookup
