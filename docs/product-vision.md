# Product vision

Public-safe product context for **Tenancy Hub**. Implementation slices and status live in the [platform roadmap](../specs/tenancy-hub-platform/roadmap.md); engineering stack choices live in [tech-stack.md](./tech-stack.md).

## Summary

Tenancy Hub is a **multi-tenant**, **Microsoft 365–centric** operating platform for **small UK letting agencies** (typically 1–25 staff, Microsoft 365 Business customers). The product combines CRM, property and tenancy operations, maintenance, compliance, communications, and (over time) finance and customer portals—**integrated with Entra ID and Microsoft Graph**, not a replacement for Microsoft 365.

The idea evolved from replacing a fragmented legacy agency CRM toward a **vertical SaaS** that reduces tool sprawl while staying native to how agencies already work in Outlook, SharePoint, and Teams.

## Problem

Letting agencies often operate across many disconnected systems: agency CRM, rent and referencing products, Outlook, SharePoint, telephony, spreadsheets, maintenance tools, and accounting. That fragmentation drives duplicate data entry, manual handoffs, weak reporting, brittle integrations, high total cost, and inconsistent user experience.

Tenancy Hub aims to **consolidate operational software** in one platform that still **embraces M365** rather than fighting it.

## Target users and go-to-market shape

- **Primary market:** independent and small UK letting / estate agencies.
- **Early validation:** an initial **design-partner agency** supplies domain expertise and realistic workflows; **mid–long term** the product is **commercial SaaS** for any qualifying agency.
- **Delivery narrative:** a demonstrable **proof of concept (POC)** first, then a broader **go-live** cutover if the partner commits, then **later** capabilities (reporting, finance, portals, AI).

Detailed row-level planning uses Spec Kit under `specs/`—there is no separate PRD track in this repository.

## Product modules (conceptual map)

These areas align with roadmap slices (R1–R15); design detail appears in each sub-spec when written.

| Module | Roadmap | Notes |
|--------|---------|--------|
| Foundation (auth, users, permissions, audit, notifications) | R1 | Multi-tenant from day one |
| CRM (contacts, companies, activities, comms history) | R2 | Includes prospects/leads as CRM activities |
| Properties (records, units, lifecycle, preparation) | R3 | Media metadata in-app; binaries via SharePoint (R8) |
| Tenancies & lettings flow | R4 | Applications, move-in, renewals, viewings; arrears **tracking** only |
| Operational dashboard | R5 | Aggregated visibility |
| Maintenance & work orders | R6 | Contractor portal deferred |
| Compliance | R7 | Certificates, inspections, expiry workflows |
| Graph / Outlook / SharePoint | R8 | Mail, calendar, documents and media library |
| Reporting | R9 | KPIs—not a full BI builder |
| Finance & payments | R10 | Schedules, statements; Stripe / GoCardless / open banking direction |
| Customer portals | R11 | Tenant, landlord, contractor (later) |
| AI-assisted operations | R12 | Document Q&A, email triage, assisted workflows |
| WordPress property showcase | R13 | Plugin on **existing** agency site |
| Property feed for aggregators | R14 | Outbound feed (e.g. [Data Export](https://dataexport.co.uk/))—not direct portal APIs |
| Complaints & case management | R15 | UK lettings complaint tracking with tenancy context |

## Letting agency process areas

Real agencies run end-to-end processes (prospecting through compliance). **Step-by-step proprietary process maps are not stored in git**; they inform sub-specs privately. The table below maps common **process areas** to roadmap anchors so nothing is lost when consolidating notes.

| Process area | Primary roadmap anchor |
|--------------|-------------------------|
| Prospecting (landlords, properties, contractors, tenants) | R2 |
| Lead conversion / valuations | R4 (+ R2) |
| Property preparation | R3 |
| Marketing / advertising | R13, R14 |
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

Phases appear on the [roadmap](../specs/tenancy-hub-platform/roadmap.md) **Phase** column. Build in **dependency order** within each phase; re-prioritise by updating the roadmap first, then affected sub-specs.

| Phase | Meaning |
|-------|---------|
| **POC** | Demo viability: multi-tenant app, core CRM/properties, M365/SharePoint integration |
| **Go-live** | Partner cutover: operational modules plus website/syndication paths that no longer treat a legacy CRM as the property hub |
| **Later** | Full product vision items after go-live unless reprioritised |

**POC (demo):** **R1 → R2 → R3 → R8** — agency sign-in, contacts and properties, Outlook/calendar relevance, property-related media via SharePoint.

**Go-live (if partner proceeds):** **R4, R5, R6, R15**, optionally **R7**, plus **R13** (WordPress showcase) and **R14** (aggregator feed).

**Later:** **R9–R12** and expanded product surface beyond the WordPress plugin.

**Suggested sequence:**

1. Complete all **POC** rows (demo milestone).
2. If the design partner commits, complete **Go-live** rows; treat **R7** as optional unless compliance tracking is a launch blocker.
3. Pick **Later** rows via `/speckit.specify` when priority allows.

## Microsoft 365 first

Users sign in with **Entra ID / Microsoft 365 work accounts**. The product should feel like an extension of M365:

- **In initial Graph slice (R8):** mail and calendar in context; SharePoint for documents and operational media; email capture to enquiries/tasks (and work orders when R6 exists).
- **Planned Graph direction (see tech-stack):** Teams, SharePoint, To Do, and broader comms—scoped in future slices or R8 follow-on, not all in the first R8 delivery.
- **Not chosen as platform core:** Dataverse as the primary datastore—Graph integration is required either way; **PostgreSQL** keeps long-term SaaS flexibility ([tech-stack.md](./tech-stack.md)).

## AI strategy (later)

Roadmap **R12** covers, at a high level:

- **Knowledge assistant:** answers from operational data and documents stored in SharePoint (e.g. certificate expiry questions)—no autonomous legal decisions.
- **Ticket triage:** email → enquiries, work orders, follow-up tasks.
- **Workflow assistance:** tenancy, inspections, communications, document generation support.

## Finance and portals (later)

- **R10:** rent schedules and statements; payment provider integrations (Stripe, GoCardless, open banking direction); does **not** replace full accounting—reconciliation and export details belong in the R10 sub-spec.
- **R11:** tenant portal (repairs, statements, payments over time), landlord and contractor portals—scoped per portal in sub-specs; depends on R4, R6, R10 as appropriate.

## Website and portal syndication (go-live)

- **R13:** property showcase on an **existing** WordPress site via a plugin (custom post type)—not a full website builder in v1.
- **R14:** **outbound property feed** compatible with syndication services (e.g. Data Export)—Tenancy Hub becomes the **source of property data**, not a replacement for aggregator relationships or Rightmove/Zoopla contracts.

## Deferred capabilities

Explicitly **out of scope** for early slices unless a roadmap row is added:

- **Telephony:** click-to-dial, call logging, incoming caller ID (noted as deferred from R6).
- **SMS** and broad **Teams / To Do** messaging workflows (future comms slice or R8 phase 2).
- **Contractor portal** (R11 later).
- **Direct** property portal API integrations (use R14 feed model instead).
- **Full accounting** replacement (finance integrates; accounting export TBD in R10).

## Confidentiality and private assets

This repository may become **public/OSS**. Do **not** commit:

- Customer- or partner-identifiable requirements
- Internal operational process maps (detailed workflows)
- Commercial figures or private chat/document links

Keep Spec Kit artifacts **public-safe** at the level needed to implement. Partner-specific cutover (exact WordPress stack, feed credentials) stays in private runbooks and conversations; specs describe integrations generically.

**Held outside git (recommended):** detailed business process library, design-partner commercial notes, and links to private M365 content—the **ideas** are captured here and in the roadmap; the **proprietary blueprint** stays private.

## Related documents

| Document | Role |
|----------|------|
| [README.md](../README.md) | Repository entry, problem, quick links |
| [specs/tenancy-hub-platform/roadmap.md](../specs/tenancy-hub-platform/roadmap.md) | Spec-of-specs index (R1–R15) |
| [tech-stack.md](./tech-stack.md) | .NET, Aspire, Azure, PostgreSQL, Graph boundaries |
| [AGENTS.md](../AGENTS.md) | Contributor and agent conventions |
