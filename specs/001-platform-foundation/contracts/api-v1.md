# API contracts: Platform foundation (R1)

**Base URL (local Aspire)**: Resolved via service discovery as `https+http://apiservice` from `TenancyHub.Web`; external testing uses URL from `aspire describe apiservice`.

**Authentication**: `Authorization: Bearer {access_token}` from Entra (API app registration). All endpoints require authenticated caller unless noted.

**Agency context**: Routine agency-scoped routes require header **`X-TenancyHub-Agency-Id: {uuid}`** matching caller's active agency (FR-002). Operator cross-agency routes use explicit **`/operator/agencies/{agencyId}/...`** paths; server verifies platform operator assignment.

**Error shape**: ASP.NET Core ProblemDetails (`application/problem+json`). No tenant leakage in titles or detail (FR-013).

---

## Conventions

| HTTP | Usage |
|------|--------|
| 200 | Success with body |
| 201 | Created |
| 204 | Success no body |
| 400 | Validation |
| 401 | Unauthenticated |
| 403 | Forbidden (generic message) |
| 404 | Not found or forbidden (same outward shape for cross-tenant) |

Dates: ISO 8601 UTC in JSON. Copy: UK English in user-visible `title`/`detail` fields from API when returned to UI.

---

## Session and context

### GET `/api/v1/me`

Returns signed-in identity, platform operator flag, agency memberships summary, operator assignments, last-used agency id, pending invites count.

**Response 200** (abbreviated):

```json
{
  "userId": "uuid",
  "email": "user@agency.example",
  "isPlatformOperator": false,
  "lastUsedAgencyId": "uuid",
  "memberships": [
    {
      "agencyId": "uuid",
      "displayName": "Example Lettings",
      "status": "Active",
      "role": "StandardMember"
    }
  ],
  "operatorAssignments": [],
  "pendingInvites": []
}
```

### PUT `/api/v1/me/active-agency`

**Body**: `{ "agencyId": "uuid" }`

**Effects**: Sets active agency for session; discards incompatible in-flight server state. **Not** a separate FR-008 audit event in R1 (agency switch is context only; auditable actions occur under the new agency).

**403**: Agency not active, membership not active, or not assigned (operators).

### POST `/api/v1/me/sign-out`

Ends the **current** authenticated session only (FR-001): invalidates the server-side `UserSession` row for this session and clears the Web auth cookie. Other concurrent sessions for the same user remain valid.

**Response 204**: No content.

---

## Invitations (invitee)

### GET `/api/v1/invitations/pending`

Lists **Invited** memberships for caller's email.

### POST `/api/v1/invitations/{membershipId}/accept`

Matching identity required; transitions to **Active** (FR-007).

### POST `/api/v1/invitations/{membershipId}/decline`

Transitions to **Removed**.

---

## Agencies (platform operator)

### POST `/api/v1/operator/agencies`

Creates agency (FR-016); auto-assigns creator.

**Body**:

```json
{
  "displayName": "New Agency Ltd",
  "primaryContactEmail": "office@example.com",
  "primaryContactPhone": "+441234567890"
}
```

### POST `/api/v1/operator/agencies/{agencyId}/lifecycle`

**Body**: `{ "targetStatus": "Suspended" | "Active" | "Archived" }`

---

## Membership (agency admin or operator on assigned agency)

Base: `/api/v1/agencies/{agencyId}/memberships` with assignment/admin checks.

| Method | Path | Action |
|--------|------|--------|
| GET | `/memberships` | Roster (admin, operator; standard read-only list per FR) |
| POST | `/memberships/invite` | Body: `{ "email", "role" }` → **Invited** |
| POST | `/memberships/provision` | Body: `{ "email", "role" }` → **Active** |
| PATCH | `/memberships/{id}/role` | Role change |
| POST | `/memberships/{id}/suspend` | |
| POST | `/memberships/{id}/reactivate` | |
| DELETE | `/memberships/{id}` | **Removed** |
| DELETE | `/memberships/{id}/invitation` | Revoke pending invite |

---

## Agency settings (admin active agency / operator)

### PATCH `/api/v1/agencies/{agencyId}/settings`

**Canonical** settings update for R1 (FR-015). One route; authorization differs by caller:

| Caller | Allowed when |
|--------|----------------|
| Agency **administrator** with **active** membership | Agency lifecycle is **Active** |
| **Platform operator** assigned to `{agencyId}` | Agency is **Active** or **Suspended** (not **Archived**) |

**Body**: `{ "displayName", "primaryContactEmail", "primaryContactPhone" }` (partial allowed).

Operators MUST use this path (not a separate `/operator/agencies/{agencyId}` settings route). Implement shared handler logic in `AgencySettingsEndpoints` per [tasks.md](./tasks.md) T060.

---

## Audit

Shared response shape for both routes below: `{ "items": [ { "occurredAt", "actorEmail", "actionType", "summary" } ], "nextCursor": "..." }` with query `?cursor=&limit=50`.

### GET `/api/v1/agencies/{agencyId}/audit`

**Agency administrators only (FR-009, T068, T071)**:

| Caller | Allowed when |
|--------|----------------|
| Agency **administrator** | Agency lifecycle is **Active** and caller has **Active** membership in that agency |

Requires routine agency context: header **`X-TenancyHub-Agency-Id`** MUST match `{agencyId}` (FR-002, T050). Standard and read-only members: **403** (tenant-safe).

### GET `/api/v1/operator/agencies/{agencyId}/audit`

**Platform operators only (FR-009, T068, T072)**:

| Caller | Allowed when |
|--------|----------------|
| **Platform operator** assigned to `{agencyId}` | Any lifecycle (**Active**, **Suspended**, or **Archived**) for that assigned agency |

Operator cross-agency route: **does not** require `X-TenancyHub-Agency-Id` to match `{agencyId}` or agency to be **Active**; server verifies operator assignment only (same pattern as `GET /operator/agencies/{agencyId}/summary`). Operator access is auditable (T073). Agency administrators MUST use the agency route above, not this path.

---

## Operator diagnostics

### GET `/api/v1/operator/agencies/{agencyId}/summary`

FR-009 read-only summary + link to **`GET /api/v1/operator/agencies/{agencyId}/audit`** for operators.

---

## Notifications

### GET `/api/v1/agencies/{agencyId}/notifications`

Scoped to active agency context.

### POST `/api/v1/agencies/{agencyId}/notifications/{id}/read`

Standard members only (FR-010).

---

## Operator platform management

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/v1/operator/platform/operators` | List operators |
| POST | `/api/v1/operator/platform/operators/{userId}/grant` | Grant operator status |
| POST | `/api/v1/operator/platform/operators/{userId}/revoke` | Revoke (blocked if last operator) |
| PUT | `/api/v1/operator/platform/operators/{userId}/assignments` | Body: `{ "agencyIds": ["uuid"] }` |

---

## OpenAPI

During implement, expose OpenAPI 3.1 from `TenancyHub.ApiService` (`MapOpenApi` in Development) generated from minimal APIs or controllers; this document is the semantic contract until OpenAPI is checked in as `contracts/openapi.yaml`.
