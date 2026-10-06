# Letoryn

**Letoryn for Microsoft 365.** A **multi-tenant**, **Microsoft 365–centric** property catalogue for small UK letting agencies (typically 1–25 staff) and very small portfolios. It stays native to **Entra ID and Microsoft Graph**, stores listing media in **SharePoint**, and exposes **one canonical listing feed** for the agency's **WordPress** site and syndication tools. It is not a replacement for the CRM, compliance, referencing, accounting, or other systems an agency already runs.

The repository is **open source** (MIT) and an early **.NET Aspire** scaffold ([tech stack](docs/tech-stack.md)). All product work is **Spec Kit** SDD: a [platform roadmap](specs/letoryn-platform/roadmap.md) decomposes the epic; each slice runs specify → plan → tasks → implement ([Spec Kit](https://github.com/github/spec-kit), [spec of specs](https://github.github.com/spec-kit/concepts/spec-of-specs.html)).

## Collaboration and project status

**Looking for a UK lettings domain expert to partner** on Vision-phase modules (tenancies, compliance depth, finance, and similar). Catalogue and Operations slices are intended to be buildable without that expertise; see [CONTRIBUTING.md](CONTRIBUTING.md) for how to get in touch.

**Origin (public-safe):** the project started as an idea to bring fragmented agency tooling into one M365-native platform. That full scope needs lettings domain knowledge the maintainer cannot supply alone, so the repo is open for use, contribution, and partnership while the **Catalogue** ships first.

Delivery phases and module map: [product vision](docs/product-vision.md); slice index: [roadmap](specs/letoryn-platform/roadmap.md).

## Problem

An agency already has a website, Microsoft 365, and specialist tools for CRM, referencing, compliance, and accounting. Letoryn takes the property list those tools do not present well: one catalogue, photos in SharePoint, and a single feed for WordPress and syndication. It does not aim to become the only system on the desk.

## Product direction

| Area | Direction |
|------|-----------|
| Tenancy | **Multi-tenant SaaS** from day one (agency isolation, shared deployment) |
| Identity | Entra ID / Microsoft 365 accounts |
| UX | Blazor + Fluent UI |
| Integrations | Microsoft Graph (SharePoint media library first; mail, calendar, To Do in later R8 tranches) |
| System of record | PostgreSQL |
| Hosting | Azure (Aspire deployment flows when introduced) |
| Catalogue (first release) | Listings (R3), SharePoint media (R8), one listing feed for WordPress (R13) and syndication (R14); foundation **R1 done** |
| Operations | Optional contacts, repairs/work orders, dashboard (R2, R5, R6) |
| Vision | Tenancies, compliance, finance, portals, reporting, AI, complaints (R4, R7, R9–R12, R15): may not ship without domain expert input |

Full module and process context: [product vision](docs/product-vision.md). Capability slices: [roadmap](specs/letoryn-platform/roadmap.md). There is no separate PRD process.

## Repository hygiene

The repo is **public open source** (MIT). Do not commit customer-confidential material, internal process maps, or private M365 links. See [product vision: confidentiality](docs/product-vision.md#confidentiality-and-private-assets).

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
| Platform foundation (R1) | **Done** ([specs/001-platform-foundation](specs/001-platform-foundation/)) |
| Catalogue (R3, R8, R13, R14) | Planned ([roadmap](specs/letoryn-platform/roadmap.md)) |
| Operations and Vision | Recorded on roadmap |

## Spec-driven work

**Catalogue:** **R1 (done) → R3 → R8 → R13/R14** (one shared listing feed for WordPress and syndication).

**Operations:** **R2**, **R5**, **R6**. **Vision:** **R4**, **R7**, **R9–R12**, **R15**.

See [platform roadmap](specs/letoryn-platform/roadmap.md) for Spec Kit workflow and phase definitions. GitHub milestone rules: [milestone-strategy.md](docs/milestone-strategy.md).

CI: Release build, format check, and tests on push/PR to `main` (`.github/workflows/ci.yml`).

## License

[MIT](LICENSE). Copyright (c) Mark Heydon.
