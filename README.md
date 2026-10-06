# Letoryn

**Letoryn for Microsoft 365.** A **multi-tenant**, **Microsoft 365–centric** operating platform for small UK letting agencies (typically 1–25 staff). The goal is one integrated product: CRM, property and tenancy operations, maintenance, compliance, communications, and (over time) finance and portals: while **staying native to Entra ID and Microsoft Graph**, not replacing Microsoft 365.

The repository is an early **.NET Aspire** scaffold ([tech stack](docs/tech-stack.md)). All product work is **Spec Kit** SDD: a [platform roadmap](specs/letoryn-platform/roadmap.md) decomposes the epic; each slice runs specify → plan → tasks → implement ([Spec Kit](https://github.com/github/spec-kit), [spec of specs](https://github.github.com/spec-kit/concepts/spec-of-specs.html)).

An initial **design-partner agency** provides domain expertise and early testing; **mid–long term** the intent is a **commercial SaaS** for any qualifying agency. Delivery phases and module map: [product vision](docs/product-vision.md); slice index: [roadmap](specs/letoryn-platform/roadmap.md).

## Problem

Many agencies stitch together a CRM, referencing tools, rent and compliance products, Outlook, SharePoint, telephony, spreadsheets, and accounting. That fragmentation drives duplicate entry, manual handoffs, weak reporting, brittle integrations, and high total cost. Letoryn aims to **consolidate operational software** around a single platform that still **integrates with M365** rather than fighting it.

## Product direction

| Area | Direction |
|------|-----------|
| Tenancy | **Multi-tenant SaaS** from day one (agency isolation, shared deployment) |
| Identity | Entra ID / Microsoft 365 accounts |
| UX | Blazor + Fluent UI |
| Integrations | Microsoft Graph (mail, calendar, SharePoint documents **and media library**); see roadmap phases |
| System of record | PostgreSQL |
| Hosting | Azure (Aspire deployment flows when introduced) |
| Go-live integrations | WordPress property showcase plugin (R13), property feed for aggregators such as [Data Export](https://dataexport.co.uk/) (R14): not replacing those platforms |
| Later | Reporting, finance, portals, AI: roadmap **Later** phase |

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
| POC (R1, R2, R3, R8) and go-live slices | Planned ([roadmap](specs/letoryn-platform/roadmap.md)) |

## Spec-driven work

POC, go-live, and later delivery phases (including **R15** complaints): [product vision: delivery phases](docs/product-vision.md#delivery-phases). GitHub milestone rules: [milestone-strategy.md](docs/milestone-strategy.md). Pick the next slice from the [roadmap](specs/letoryn-platform/roadmap.md) by dependency and status.

CI: Release build, format check, and tests on push/PR to `main` (`.github/workflows/ci.yml`).

## License

[MIT](LICENSE). Copyright (c) Mark Heydon.
