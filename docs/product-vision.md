# Product vision

Public-safe product context for **Letoryn**. Implementation slices and status live in the [platform roadmap](../specs/letoryn-platform/roadmap.md); engineering stack choices live in [tech-stack.md](./tech-stack.md).

## Summary

Letoryn is an **open-source**, **multi-tenant**, **Microsoft 365–centric** **property catalogue** for **small UK letting agencies** and very small portfolios (typically 1–25 staff, Microsoft 365 Business customers). The **first deliverable** is listings in PostgreSQL, media in **SharePoint**, and **one canonical outbound listing feed** for an agency WordPress site (R13) and syndication tools such as [Data Export](https://dataexport.co.uk/) (R14).

The project began as an idea to reduce tool sprawl into a single M365-native operating platform. That full breadth needs **UK lettings domain expertise** the maintainer cannot supply alone. The repository is public so others can use, contribute, or **partner on Vision-phase modules** while the Catalogue ships first.

The roadmap still records CRM, tenancies, maintenance, compliance, finance, portals, and AI (R2–R12, R15). Those are **not** a commitment to replace specialist products (referencing, rent collection, tenancy legal platforms, accounting) in the short to mid term.

## Problem

Letting agencies and small landlords often juggle a website, Microsoft 365, CRM, referencing, compliance, and accounting tools. **Marketing listings** are duplicated, hard to keep in sync, and poorly tied to where photos and documents already live in SharePoint.

Letoryn focuses first on **one catalogue beside existing systems**: authoritative listings, SharePoint media, and a single feed for the agency site and syndication path. It does not aim to become the only application on the desk.

## Target users and collaboration

- **Primary users:** independent and small UK letting / estate agencies; micro portfolios that mainly need their own site updated.
- **Open source:** MIT-licensed code; contributions welcome via [CONTRIBUTING.md](../CONTRIBUTING.md).
- **Domain expertise:** the project is **seeking a UK lettings domain expert to partner** on Vision-phase work (tenancies, compliance depth, finance, and similar). Contact paths are in README and CONTRIBUTING.
- **Hosting:** a community or maintainer-hosted offering may exist later; this document does not promise a commercial SaaS roadmap.

Detailed row-level planning uses Spec Kit under `specs/`: there is no separate PRD track in this repository.

## Product modules (conceptual map)

These areas align with roadmap slices (R1–R15); design detail appears in each sub-spec when written.

| Module | Roadmap | Notes |
|--------|---------|--------|
| Foundation (auth, users, permissions, audit, notifications) | R1 | **Done** via [specs/001-platform-foundation/](../specs/001-platform-foundation/) |
| CRM (contacts, companies, activities, comms history) | R2 | Operations phase; optional for catalogue |
| Properties (records, units, lifecycle, preparation) | R3 | Media metadata in-app; binaries via SharePoint (R8) |
| Tenancies & lettings flow | R4 | Vision |
| Operational dashboard | R5 | Operations |
| Maintenance & work orders | R6 | Operations; mini CRM for repairs |
| Compliance | R7 | Vision |
| Graph / Outlook / SharePoint | R8 | Catalogue: media library first; mail/calendar/To Do follow |
| Reporting | R9 | Vision |
| Finance & payments | R10 | Vision; defer vs specialist rent platforms |
| Customer portals | R11 | Vision |
| AI-assisted operations | R12 | Vision |
| WordPress property showcase | R13 | Catalogue; consumes shared listing feed |
| Property feed for aggregators | R14 | Catalogue; **same feed** as R13 |
| Complaints & case management | R15 | Vision |

## Letting agency process areas

Real agencies run end-to-end processes (prospecting through compliance). **Step-by-step proprietary process maps are not stored in git**; they inform sub-specs when written with domain input. The table below maps common **process areas** to roadmap anchors.

| Process area | Primary roadmap anchor |
|--------------|-------------------------|
| Prospecting (landlords, properties, contractors, tenants) | R2 |
| Lead conversion / valuations | R4 (+ R2) |
| Property preparation | R3 |
| Marketing / advertising | R13, R14 (one listing feed) |
| Viewings | R4 |
| Applications | R4 |
| Tenancy creation / move-in | R4 |
| Rent collection | R10 |
| Maintenance | R6 |
| Inspections | R7 |
| Renewals | R4 |
| Arrears (track vs collect) | R4 (track), R10 (collect) |
| **Complaints** | **R15** |
| Compliance (gas, EICR, EPC, legal obligations) | R7 |

## Delivery phases

Phases are **section headings** on the [roadmap](../specs/letoryn-platform/roadmap.md). Build in **dependency order** within each phase. Assign GitHub issues and PRs to the matching milestone (**Catalogue**, **Operations**, **Vision**) per [milestone-strategy.md](./milestone-strategy.md).

| Phase | Meaning |
|-------|---------|
| **Catalogue** | Multi-tenant foundation (R1 done), property listings, SharePoint media, **one listing feed** for WordPress (R13) and syndication (R14) |
| **Operations** | Optional contacts, repair/work-order tracking, simple dashboard: buildable without deep lettings law expertise |
| **Vision** | Tenancies, compliance programmes, finance, portals, reporting, AI, complaints: **may not ship** without a domain expert partner |

**Catalogue:** **R1 (done) → R3 → R8 → R13/R14**.

**Operations:** **R2**, **R5**, **R6** (after catalogue dependencies allow).

**Vision:** **R4**, **R7**, **R9–R12**, **R15**.

**Suggested sequence:**

1. Complete remaining **Catalogue** rows (R3, R8, R13/R14 with one feed implementation).
2. Pick **Operations** rows when catalogue is stable.
3. Specify **Vision** rows only with domain expert input or explicit maintainer decision.

## Microsoft 365 first

Users sign in with **Entra ID / Microsoft 365 work accounts**. The product should feel like an extension of M365:

- **Catalogue (R8):** SharePoint for property photos and documents; media URLs on the listing feed where needed.
- **Follow-on Graph (R8, Operations/Vision):** mail and calendar in context; email capture to enquiries/tasks; **Microsoft To Do** and Teams when scoped; not all in the first R8 delivery.
- **Not chosen as platform core:** Dataverse as the primary datastore. Graph integration is required either way; **PostgreSQL** keeps long-term flexibility ([tech-stack.md](./tech-stack.md)). Out-of-the-box To Do and SharePoint integration remains a key reason to stay Graph-native rather than forcing Dataverse as the hub.

## AI strategy (Vision)

Roadmap **R12** covers, at a high level:

- **Knowledge assistant:** answers from operational data and documents in SharePoint: no autonomous legal decisions.
- **Ticket triage:** email → enquiries, work orders, follow-up tasks.
- **Workflow assistance:** tenancy, inspections, communications, document generation support.

## Finance and portals (Vision)

- **R10:** rent schedules and statements; payment integrations; does **not** replace full accounting or products such as PayProp-style rent collection without a deliberate Vision-phase decision.
- **R11:** tenant, landlord, and contractor portals: scoped per portal in sub-specs.

## Website and syndication (Catalogue)

- **One canonical listing feed/API** from the catalogue (defined when specifying R3 or a dedicated export slice).
- **R13:** WordPress plugin on an **existing** site (custom post type): consumes that feed.
- **R14:** syndication services (e.g. Data Export) consume the **same** feed: not a second pipeline. Letoryn is the listing source of record; it does not replace aggregator relationships or portal contracts.

## Deferred capabilities and third-party products

Explicitly **out of scope** for Catalogue and usually deferred in Operations unless a roadmap row is added:

- **Referencing and tenancy legal workflows** (Vision / specialist tools such as Goodlord-class products).
- **Rent collection and full finance** (Vision / integrate with PayProp-class products rather than replace in the short to mid term).
- **Telephony** (deferred from R6).
- **SMS** and broad **Teams / To Do** workflows until R8 follow-on.
- **Contractor portal** (R11).
- **Direct** property portal API integrations (use the R14 syndication path on the shared feed).
- **Full accounting** replacement (finance integrates; export TBD in R10).

## Confidentiality and private assets

This repository is **public OSS**. Do **not** commit:

- Customer- or partner-identifiable requirements
- Internal operational process maps (detailed workflows)
- Commercial figures or private chat/document links

Keep Spec Kit artifacts **public-safe** at the level needed to implement. Deployment-specific cutover (exact WordPress stack, feed credentials) stays in private runbooks; specs describe integrations generically.

**Held outside git (recommended):** detailed business process library, private partnership or commercial notes, and links to private M365 content.

## Related documents

| Document | Role |
|----------|------|
| [README.md](../README.md) | Repository entry, problem, quick links |
| [specs/letoryn-platform/roadmap.md](../specs/letoryn-platform/roadmap.md) | Spec-of-specs index (R1–R15) |
| [tech-stack.md](./tech-stack.md) | .NET, Aspire, Azure, PostgreSQL, Graph boundaries |
| [AGENTS.md](../AGENTS.md) | Contributor and agent conventions |
