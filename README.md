# Letoryn

**Letoryn for Microsoft 365.** A **multi-tenant**, **Microsoft 365–centric** property catalogue for small UK letting agencies (typically 1–25 staff). It stays native to **Entra ID and Microsoft Graph**, stores listing media in **SharePoint**, and publishes listings to the agency's existing **WordPress** site. It is not a replacement for the CRM, compliance, referencing, accounting, or other systems an agency already runs.

The repository is an early **.NET Aspire** scaffold ([tech stack](docs/tech-stack.md)). All product work is **Spec Kit** SDD: a [platform roadmap](specs/letoryn-platform/roadmap.md) decomposes the epic; each slice runs specify → plan → tasks → implement ([Spec Kit](https://github.com/github/spec-kit), [spec of specs](https://github.github.com/spec-kit/concepts/spec-of-specs.html)).

The first feature set is that catalogue: listings in PostgreSQL, photos and files in **SharePoint**, published to WordPress. Further ideas on the roadmap are optional and are not a plan to retire the third-party tools around the product. An initial **design-partner agency** provides domain expertise and early testing; **mid–long term** the intent is a **commercial SaaS** for any qualifying agency. Delivery phases and module map: [product vision](docs/product-vision.md); slice index: [roadmap](specs/letoryn-platform/roadmap.md).

## Problem

An agency already has a website, Microsoft 365, and specialist tools for CRM, referencing, compliance, and accounting. Letoryn takes the property list those tools do not present well: one catalogue, photos in SharePoint, and a WordPress showcase on the site the agency already has. It does not aim to become the only system on the desk.

## Product direction

| Area | Direction |
|------|-----------|
| Tenancy | **Multi-tenant SaaS** from day one (agency isolation, shared deployment) |
| Identity | Entra ID / Microsoft 365 accounts |
| UX | Blazor + Fluent UI |
| Integrations | Microsoft Graph (mail, calendar, SharePoint documents **and media library**); see roadmap phases |
| System of record | PostgreSQL |
| Hosting | Azure (Aspire deployment flows when introduced) |
| First feature set | Property catalogue, SharePoint media library, WordPress showcase plugin (R1, R3, R8, R13) |
| Next | Property feed for aggregators such as [Data Export](https://dataexport.co.uk/) (R14): an export into that existing path, not a replacement for it |
| Later ideas | CRM, tenancies, maintenance, compliance, dashboard, reporting, finance, portals, AI: recorded only; not a commitment to replace those third-party systems |

Full module and process context: [product vision](docs/product-vision.md). Capability slices: [roadmap](specs/letoryn-platform/roadmap.md). There is no separate PRD process.

## Repository hygiene

The repo is intended as **public open source** (MIT). Do not commit customer-confidential material, internal process maps, or private M365 links. See [product vision: confidentiality](docs/product-vision.md#confidentiality-and-private-assets).

## Trademark

**Letoryn** is the project name. The MIT license does not grant rights to use the name or any official branding to imply you operate the official Letoryn hosted service. See [CONTRIBUTING.md](CONTRIBUTING.md).

## Repository layout

```
src/          Letoryn.AppHost, ApiService, Web, ServiceDefaults
tests/        Unit tests (xUnit v3, NSubstitute)
docs/         Engineering standards (see docs/README.md)
specs/        Spec Kit features + platform roadmap
.agents/skills/  Aspire, Fluent UI, Playwright workflows
```

| Project | Role |
|---------|------|
| `Letoryn.AppHost` | Aspire orchestration |
| `Letoryn.ApiService` | Backend API |
| `Letoryn.Web` | Blazor web UI |
| `Letoryn.ServiceDefaults` | OpenTelemetry, health, service discovery |

Contributors and agents: [AGENTS.md](AGENTS.md), [CONTRIBUTING.md](CONTRIBUTING.md), [docs/README.md](docs/README.md).

## Prerequisites

- [.NET SDK 10](https://dotnet.microsoft.com/download) (`global.json`)
- [Aspire CLI 13.6+](https://aspire.dev/get-started/install-cli/)

## Quick start

```bash
git clone https://github.com/markheydon/letoryn-m365.git
cd letoryn-m365
dotnet restore Letoryn.slnx
aspire update --migrate   # first time / after package bumps
dotnet build Letoryn.slnx
dotnet test Letoryn.slnx
aspire run                # from repo root
```

Details: [docs/local-development.md](docs/local-development.md).

## Development status

| Layer | Status |
|-------|--------|
| Aspire AppHost + API + Blazor shell | Scaffolded |
| Catalogue (R1, R3, R8, R13) and later slices | Planned ([roadmap](specs/letoryn-platform/roadmap.md)) |

## Spec-driven work

**Catalogue:** **R1 → R3 → R8 → R13** (multi-tenant foundation, property listings, SharePoint media, WordPress showcase).

**Next:** **R14** (property feed for aggregators). **Later ideas, not a replacement plan:** R2 and R4–R15.

See [platform roadmap](specs/letoryn-platform/roadmap.md) for Spec Kit workflow and phase definitions. GitHub milestone rules: [milestone-strategy.md](docs/milestone-strategy.md).

CI: Release build, format check, and tests on push/PR to `main` (`.github/workflows/ci.yml`).

## License

[MIT](LICENSE). Copyright (c) Mark Heydon.
