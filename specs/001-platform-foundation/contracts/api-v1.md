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

**Effects**: Sets active agency for session; discards incompatible in-flight server state; auditable if policy requires.

**403**: Agency not active, membership not active, or not assigned (operators).

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

### PATCH `/api/v1/operator/agencies/{agencyId}`

Update display name / contact fields (allowed on assigned agencies per lifecycle rules).

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

**Body**: `{ "displayName", "primaryContactEmail", "primaryContactPhone" }` (partial allowed).

---

## Audit

### GET `/api/v1/agencies/{agencyId}/audit`

Query: `?cursor=&limit=50` — agency administrators (active agency) or operators (assigned).

**Response**: `{ "items": [ { "occurredAt", "actorEmail", "actionType", "summary" } ], "nextCursor": "..." }`

---

## Operator diagnostics

### GET `/api/v1/operator/agencies/{agencyId}/summary`

FR-009 read-only summary + link to audit.

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
