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

Repeat when the registration is deleted or client certificates are rotated.

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

### 2.4 Client certificate (confidential client)

Tenancy Hub uses a **client certificate** for the web app (auth-code exchange and token refresh). Do **not** use client secrets—many Entra tenants block secret creation by admin policy.

1. Generate a dev certificate (see [scripts/r1/create-entra-web-client-certificate.sh](../scripts/r1/create-entra-web-client-certificate.sh) or your own PKI process). You need:
   - **`.cer`** (public key only) for Entra
   - **`.pfx`** (private key + cert) for the app—never commit; `*.pfx` is gitignored
2. Entra admin center → web app registration → **Certificates & secrets** → **Upload certificate** → select the **`.cer`** file.
3. Store the Base64-encoded `.pfx` and its password in AppHost secrets (section 3)—never commit.
4. **Rotation**: Entra allows multiple certificates. Upload the new `.cer`, update AppHost secrets, verify sign-in, then remove the old certificate from Entra.

**Troubleshooting**: If sign-in fails at the token step with client authentication errors, confirm the thumbprint of the cert in Entra matches the `.pfx` you encoded (Entra portal shows thumbprints for uploaded certs). Wrong password on the PFX or stale Base64 in user secrets are the other common causes.

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
dotnet user-secrets set "Parameters:EntraWebClientCertificatePfx" "$(base64 -w0 ~/.local/share/tenancy-hub/certs/entra-web-dev.pfx)"
dotnet user-secrets set "Parameters:EntraWebClientCertificatePassword" "<pfx-password>"
dotnet user-secrets set "Parameters:EntraApiClientId" "<api-client-id>"
dotnet user-secrets set "Parameters:EntraApiAudience" "api://<api-client-id>"
dotnet user-secrets set "Parameters:TenancyHubInternalSignInAuditKey" "<random-shared-secret>"
```

Use any long random string for `TenancyHubInternalSignInAuditKey` (same value on **apiservice** and **webfrontend** via AppHost). The Web app sends it when establishing an API session and reporting sign-in audit events; the API rejects those calls without a matching key.

Alternatively, from the repository root: `aspire secret set "Parameters:EntraWebClientCertificatePfx" "$(base64 -w0 /path/to/entra-web-dev.pfx)"` (and the same for other parameters).

On macOS, use `base64 -i entra-web-dev.pfx` (no `-w0`) to produce a single line for the PFX parameter.

The AppHost should wire these with `AddParameter(..., secret: true)` and `WithEnvironment` / configuration mapping on `webfrontend` and `apiservice`. **Do not** paste secrets into `appsettings.json`.

AppHost parameter names (secret parameters via `AddParameter`):

| Parameter | Mapped to `apiservice` | Mapped to `webfrontend` |
|-----------|------------------------|-------------------------|
| `EntraTenantId` | `AzureAd__TenantId` | `AzureAd__TenantId` |
| `EntraApiClientId` | `AzureAd__ClientId` | — |
| `EntraApiAudience` | `AzureAd__Audience` | — |
| `EntraWebClientId` | — | `AzureAd__ClientId` |
| `EntraWebClientCertificatePfx` | — | `AzureAd__ClientCredentials__0__Base64EncodedValue` |
| `EntraWebClientCertificatePassword` | — | `AzureAd__ClientCredentials__0__CertificatePassword` |
| `EntraApiAudience` | — | `TenancyHub__ApiScope` = `{EntraApiAudience}/access_as_user` (AppHost expression) |
| `TenancyHubInternalSignInAuditKey` | `TenancyHub__InternalSignInAuditKey` | `TenancyHub__InternalSignInAuditKey` |

`webfrontend` also receives `AzureAd__ClientCredentials__0__SourceType` = `Base64Encoded` (fixed in AppHost).

Both apps also receive `AzureAd__Instance` = `https://login.microsoftonline.com/`.

**Migration from client secrets**: Remove `Parameters:EntraWebClientSecret` from AppHost user secrets after setting the certificate parameters above and uploading the matching `.cer` to Entra.

**Web OIDC paths** (Microsoft.Identity.Web defaults; confirm in `src/TenancyHub.Web/appsettings.json`):

| Setting | Value |
|---------|--------|
| Sign-in callback | `/signin-oidc` |
| Sign-out callback | `/signout-callback-oidc` |
| Microsoft Identity UI sign-in | `/MicrosoftIdentity/Account/SignIn` |
| Microsoft Identity UI sign-out | `/MicrosoftIdentity/Account/SignOut` |

Register redirect URIs in Entra for each `webfrontend` HTTPS port from `aspire describe webfrontend` (section 2.1).

**API scope for Web → API calls**: configure `TenancyHub:ApiScope` (for example `api://{api-client-id}/access_as_user`) on `webfrontend` to match the exposed API scope granted to the web app registration.

PostgreSQL resource: AppHost `AddPostgres("postgres").WithDataVolume("pg-data").AddDatabase("tenancyhub")`; API uses `AddNpgsqlDbContext<TenancyHubDbContext>(connectionName: "tenancyhub")`. The named Docker volume **`pg-data`** keeps database files across AppHost restarts (schema and data survive; you do not need to re-apply migrations after every stop/start when the volume is intact).

**EF migrations (local Aspire)**: AppHost wires `AddEFMigrations` on `apiservice` with `WithMigrationsProject` → `TenancyHub.Infrastructure`, `RunDatabaseUpdateOnStart()`, and `apiservice.WaitForCompletion(migrations)` after `migrations.WaitFor(postgres)`. Pending migrations apply automatically on `aspire run` before the API serves traffic.

**Disabled directory accounts (FR-001)**: In Entra, keep **access token lifetime** ≤ 60 minutes for POC so revoked/disabled users lose API access within a refresh cycle without Microsoft Graph integration (see [research.md](../specs/001-platform-foundation/research.md)).

---

## 4. PostgreSQL via Aspire (reset / rebuild)

### 4.1 Normal first run

```bash
aspire run
```

AppHost adds PostgreSQL with generated credentials and a persistent data volume (`pg-data`); the API receives connection settings through `WithReference` and the Aspire Npgsql EF client integration.

To **wipe local Postgres data** (fresh database, re-run migrations and seed): `aspire stop`, then `docker volume rm pg-data` (or remove the volume in Docker Desktop). On the next `aspire run`, Aspire recreates the volume and credentials.

### 4.2 Apply schema

**Local Aspire (`aspire run`)**: schema is applied automatically by the `tenancyhub-migrations` resource (see section 3). No separate step is required for day-to-day development after a fresh volume or new migration merge.

**CI, scripts, or offline** (no AppHost): apply pending migrations explicitly:

```bash
dotnet ef database update --project src/TenancyHub.Infrastructure --startup-project src/TenancyHub.ApiService
```

Creating new migrations is unchanged: `dotnet ef migrations add <Name> --project src/TenancyHub.Infrastructure --startup-project src/TenancyHub.ApiService` (requires `DesignTimeDbContextFactory` in Infrastructure). Adjust project paths if the repository layout changes; keep this command aligned with [local-development.md](./local-development.md).

### 4.3 Fix password / volume mismatch

Symptoms: `password authentication failed` for Postgres in Aspire logs after AppHost recreation.

1. Stop Aspire: `aspire stop`
2. Remove the Postgres container **and** its volume (Docker Desktop → Volumes, or `docker volume rm pg-data` — see §4.1 if the dashboard shows a different generated name on older runs).
3. Start again: `aspire run` — the integration generates fresh credentials.

See `.agents/skills/aspireify/references/apphost-wiring.md` (stale volume section).

---

## 5. Seed initial platform operator

R1 allows seeding **outside** the product before the “at least one operator” guard applies.

After database is migrated:

1. Identify your Entra **object id** (`oid` claim) after first sign-in attempt or from Entra user profile.
2. Run [scripts/r1/seed-platform-operator.sql](../scripts/r1/seed-platform-operator.sql) against the Aspire Postgres database:

   ```bash
   aspire describe postgres   # connection string for psql
   psql "<connection-string>" -f scripts/r1/seed-platform-operator.sql
   ```

   Edit the script placeholders `REPLACE_WITH_ENTRA_OID` and `REPLACE_WITH_EMAIL` before running. The script upserts into `"UserIdentities"` on unique `"EntraObjectId"` and sets `"IsPlatformOperator" = TRUE`.
3. Verify: sign in → operator global flows visible per platform foundation spec User Story 1.

**Access token lifetime (FR-001)**: For POC, set Entra access token lifetime to ≤ 60 minutes so disabled directory accounts lose API access within a refresh cycle without Microsoft Graph polling (see [specs/001-platform-foundation/research.md](../specs/001-platform-foundation/research.md)).

---

## 6. Verification smoke test

1. `aspire describe` — `postgres`, `apiservice`, `webfrontend` healthy.
2. Browse web URL → redirect to Entra → sign in.
3. `GET /api/v1/me` (browser devtools or curl with token) returns identity payload.
4. Run cross-agency isolation checks from [specs/001-platform-foundation/quickstart.md](../specs/001-platform-foundation/quickstart.md) when R1 is implemented.

---

## 7. Production / Azure (pointer)

Production Entra apps, Key Vault, and Azure Database for PostgreSQL use Aspire publish and Azure provisioning integrations when deployment is documented. Mirror the Entra steps in section 2 for production redirect URIs (including **client certificates**, not secrets). For hosted environments, prefer Microsoft.Identity.Web **`ClientCredentials`** with **`SourceType: KeyVault`** or certificateless **`SignedAssertionFromManagedIdentity`**—not Base64 PFX in app settings. Follow `.agents/skills/aspire-deployment/` when publishing; extend **this runbook** with environment-specific sections rather than scattering steps in feature specs.

---

## 8. Incident-oriented order of operations

Use this sequence after a major loss (new tenant, wiped laptop, corrupted Docker volume):

1. Section 1 — prerequisites  
2. Section 2 — Entra (web + API registrations)  
3. Section 3 — AppHost user secrets  
4. Section 4 — Postgres (include 4.3 if auth errors persist)  
5. Section 4.2 — migrations (automatic on `aspire run`; use manual `dotnet ef database update` only when not using AppHost)  
6. Section 5 — platform operator seed  
7. Section 6 — smoke test  

For Aspire diagnostics: `.agents/skills/aspire-monitoring/` (`aspire logs`, `aspire describe`, `aspire otel traces`).
