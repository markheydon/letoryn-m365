# Operations rebuild runbook

**Authoritative** step-by-step procedures to restore Tenancy Hub after environment loss, credential rotation, database volume corruption, or a major incident. Maintainers and automation should treat **`docs/operations-rebuild-runbook.md`** as the single source of truth—not copies under `specs/`.

| Also useful | Purpose |
|-------------|---------|
| [local-development.md](./local-development.md) | Day-to-day build, test, Aspire |
| [tech-stack.md](./tech-stack.md) | Stack and integration boundaries |
| [specs/001-platform-foundation/quickstart.md](../specs/001-platform-foundation/quickstart.md) | Feature acceptance validation (after R1 ships) |

**Scope (R1+)**: Microsoft Entra app registration, local Aspire secrets, PostgreSQL via Aspire, initial platform operator seed, smoke verification. Production Azure steps are pointers until deploy documentation lands.

**Maintenance**: When rebuild steps change (new parameters, seed scripts, Azure paths), update **this file** in the same PR as the code or config change. Feature specs may link here but must not duplicate procedural steps.

---

## 1. Prerequisites checklist

- [ ] .NET SDK 10.0.300+ (`global.json`)
- [ ] Aspire CLI 13.6+ (`aspire --version`)
- [ ] Docker running (PostgreSQL container via Aspire)
- [ ] Access to Microsoft Entra admin center for your dev tenant
- [ ] Permissions to create app registrations (Application Developer or higher)

---

## 2. Microsoft Entra app registration (dev)

Repeat when the registration is deleted or client secrets expire.

### 2.1 Register the web application

1. Open [Microsoft Entra admin center](https://entra.microsoft.com/) → **Applications** → **App registrations** → **New registration**.
2. **Name**: `Tenancy Hub (Dev)` (or environment-specific).
3. **Supported account types**: *Accounts in this organizational directory only* (single tenant) unless POC requires multitenant—match product tenant strategy.
4. **Redirect URI**: Web → `https://localhost:{PORT}/signin-oidc`  
   - `{PORT}` comes from `aspire describe webfrontend` after first run (Aspire assigns dynamic HTTPS port). For stable dev, note the port from the dashboard and update the registration when it changes, or configure a fixed launch profile when the app supports it.
5. Register. Record **Application (client) ID** and **Directory (tenant) ID**.

### 2.2 Configure authentication

1. **Authentication** → prefer authorization code with PKCE (Microsoft.Identity.Web default).
2. Add additional redirect URIs for each developer machine port if needed.
3. **Front-channel logout URL** (optional R1): matching `/signout-callback-oidc`.

### 2.3 Expose API (resource) for `TenancyHub.ApiService`

Either **single app** or **two registrations** (recommended for production parity):

**Option A – dedicated API registration**

1. New registration: `Tenancy Hub API (Dev)`.
2. **Expose an API** → Set **Application ID URI** → `api://{api-client-id}`.
3. Add scope e.g. `access_as_user`.
4. On the **web** registration, **API permissions** → add permission to the API scope → **Grant admin consent** for dev tenant.

### 2.4 Client secret (confidential client)

1. **Certificates & secrets** → **New client secret** → record **Value** immediately (shown once).
2. Store in AppHost user secrets (section 3)—never commit.

### 2.5 Product roles vs Entra

Agency roles (administrator, standard member, read-only member) and platform operator status live in **PostgreSQL** for R1, not Entra app roles. Entra supplies authentication only unless a future change documents optional `groups` claims.

---

## 3. Local configuration (Aspire parameters)

From repository root, set secrets on the **AppHost** project (or per-service if configuration is split):

```bash
cd src/TenancyHub.AppHost
dotnet user-secrets init  # once per machine
dotnet user-secrets set "Parameters:EntraTenantId" "<tenant-id>"
dotnet user-secrets set "Parameters:EntraWebClientId" "<web-client-id>"
dotnet user-secrets set "Parameters:EntraWebClientSecret" "<secret>"
dotnet user-secrets set "Parameters:EntraApiClientId" "<api-client-id>"
dotnet user-secrets set "Parameters:EntraApiAudience" "api://<api-client-id>"
```

The AppHost should wire these with `AddParameter(..., secret: true)` and `WithEnvironment` / configuration mapping on `webfrontend` and `apiservice`. **Do not** paste secrets into `appsettings.json`.

Parameter names and mapping must match what `TenancyHub.AppHost` declares after implement—if they differ, update this section in the same PR as the AppHost change.

---

## 4. PostgreSQL via Aspire (reset / rebuild)

### 4.1 Normal first run

```bash
aspire run
```

AppHost adds PostgreSQL with generated credentials; the API receives connection settings through `WithReference` and the Aspire Npgsql EF client integration.

### 4.2 Apply schema

After EF migrations exist:

```bash
dotnet ef database update --project src/TenancyHub.Infrastructure --startup-project src/TenancyHub.ApiService
```

Adjust project paths if the repository layout changes; keep this command aligned with [local-development.md](./local-development.md).

### 4.3 Fix password / volume mismatch

Symptoms: `password authentication failed` for Postgres in Aspire logs after AppHost recreation.

1. Stop Aspire: `aspire stop`
2. Remove the Postgres container **and** its volume (Docker Desktop → Volumes, or `docker volume ls` / `docker volume rm` for the Aspire postgres volume name shown in the dashboard).
3. Start again: `aspire run` — the integration generates fresh credentials.

See `.agents/skills/aspireify/references/apphost-wiring.md` (stale volume section).

---

## 5. Seed initial platform operator

R1 allows seeding **outside** the product before the “at least one operator” guard applies.

After database is migrated:

1. Identify your Entra **object id** (`oid` claim) after first sign-in attempt or from Entra user profile.
2. Run the documented seed script/SQL (location noted in implement—typically under `docs/` or `scripts/`) **or** use a one-time dev-only bootstrap endpoint if provided.
3. Verify: sign in → operator global flows visible per platform foundation spec User Story 1.

Document the chosen seed mechanism in the repo when implement lands; this section links to that script by name once added.

---

## 6. Verification smoke test

1. `aspire describe` — `postgres`, `apiservice`, `webfrontend` healthy.
2. Browse web URL → redirect to Entra → sign in.
3. `GET /api/v1/me` (browser devtools or curl with token) returns identity payload.
4. Run cross-agency isolation checks from [specs/001-platform-foundation/quickstart.md](../specs/001-platform-foundation/quickstart.md) when R1 is implemented.

---

## 7. Production / Azure (pointer)

Production Entra apps, Key Vault, and Azure Database for PostgreSQL use Aspire publish and Azure provisioning integrations when deployment is documented. Mirror the Entra steps in section 2 for production redirect URIs, managed identities, and secret storage in Key Vault. Follow `.agents/skills/aspire-deployment/` when publishing; extend **this runbook** with environment-specific sections rather than scattering steps in feature specs.

---

## 8. Incident-oriented order of operations

Use this sequence after a major loss (new tenant, wiped laptop, corrupted Docker volume):

1. Section 1 — prerequisites  
2. Section 2 — Entra (web + API registrations)  
3. Section 3 — AppHost user secrets  
4. Section 4 — Postgres (include 4.3 if auth errors persist)  
5. Section 4.2 — migrations  
6. Section 5 — platform operator seed  
7. Section 6 — smoke test  

For Aspire diagnostics: `.agents/skills/aspire-monitoring/` (`aspire logs`, `aspire describe`, `aspire otel traces`).
