# Roadmap: Tenancy Hub platform

Tenancy Hub is a **multi-tenant** Microsoft 365–centric operating platform for small UK letting agencies: CRM, property and tenancy operations, maintenance, compliance, communications, and (over time) finance and customer portals. The epic is too large for one `/speckit.specify` → `/speckit.implement` cycle, so work is split into independently specifiable slices. Each slice is a normal Spec Kit feature (`spec.md`, `plan.md`, `tasks.md`); this file is the [spec of specs](https://github.github.com/spec-kit/concepts/spec-of-specs.html) index only—**not** a PRD layer.

An initial **design-partner agency** supplies domain expertise and early validation; the product target is a **commercial SaaS** for any qualifying agency. The roadmap is also the **planned delivery narrative** for partner conversations: a demonstrable **proof of concept (POC)** first, then a broader **go-live** cutover if the partner commits, then **later** capabilities.

**Status legend:** planned · in-progress · done

**Phase legend:**

| Phase | Meaning |
|-------|---------|
| **POC** | Build and demo to show the approach is viable (multi-tenant app + core CRM/properties + M365/SharePoint) |
| **Go-live** | Production cutover for the design partner—operational modules plus feeds that replace today’s CRM-as-hub integrations |
| **Later** | Important for the full product vision; scheduled after go-live unless reprioritised |

| ID | Sub-feature | Intent | Scope boundary | Depends on | Phase | Status | Sub-spec |
|----|-------------|--------|----------------|------------|-------|--------|----------|
| R1 | Platform foundation | Multi-tenant SaaS shell: PostgreSQL, Entra ID, agency isolation, roles, audit, notifications | Per-agency tenancy from day one; shared UI chrome; domain modules plug in behind stable tenancy APIs | — | **POC** | planned | `specs/001-platform-foundation/` |
| R2 | CRM & contacts | Tenants, landlords, contractors, applicants, companies, activities, communication history | Contact-centric CRM; property links deepen with R3/R4 | R1 | **POC** | planned | — |
| R3 | Properties & units | Property records, units, lifecycle/preparation, media **metadata** tied to properties | Operational property data in PostgreSQL; binary media in SharePoint via R8—not portal syndication (see R14) | R1, R2 | **POC** | planned | — |
| R4 | Tenancies & lettings flow | Applications, tenancy creation, move-in, renewals, arrears **tracking** (not collection) | Lightweight marketing/viewings; no payment execution | R2, R3 | **Go-live** | planned | — |
| R5 | Operational dashboard | Visibility: properties, tasks, work orders, compliance, calendar summaries | Read-only aggregation | R2, R3, R4 | **Go-live** | planned | — |
| R6 | Maintenance & work orders | Repairs, quotes, contractors, job tracking | No contractor portal; telephony deferred | R2, R3 | **Go-live** | planned | — |
| R7 | Compliance | EPC, gas, EICR, inspections, expiry and renewal workflows | Certificate metadata in-app; documents/media in SharePoint where applicable | R3, R8 | **Go-live** (optional) | planned | — |
| R8 | Graph, Outlook & SharePoint | Mail/calendar in context; SharePoint for **documents and media library** (property photos, repair evidence, cleaning proof, etc.) | Email capture to enquiries/tasks (work orders when R6 exists); not a full SharePoint CMS replacement | R1, R3 | **POC** | planned | — |
| R9 | Reporting | KPIs, occupancy, arrears, compliance dashboards | Not full BI/custom report builder | R3–R7 | **Later** | planned | — |
| R10 | Finance & payments | Rent schedules, statements, Stripe/GoCardless, later open banking | Does not replace full accounting; reconciliation in sub-spec | R4 | **Later** | planned | — |
| R11 | Customer portals | Tenant, landlord, contractor self-service | Per-portal scope in sub-spec | R4, R6, R10 | **Later** | planned | — |
| R12 | AI-assisted operations | Document Q&A, email triage, assisted workflows | No autonomous legal decisions; builds on R7/R8 | R7, R8 | **Later** | planned | — |
| R13 | WordPress property showcase | Showcase lettings on an **existing agency WordPress site**—prefer a **plugin** exposing a property custom post type for theme/builder layouts | Does not replace the marketing site platform; may evolve toward a fuller website offering later | R3 | **Go-live** | planned | — |
| R14 | Property feed for aggregators | Export property data for **[Data Export](https://dataexport.co.uk/)** (or equivalent)—same role as today’s CRM feed to syndication | **Does not** replace Data Export or portal relationships; delivers a compatible **outbound property feed** from Tenancy Hub | R3 | **Go-live** | planned | — |

## Delivery phases (summary)

**POC (demo):** **R1 → R2 → R3 → R8** — log in as an agency, manage contacts and properties, show Outlook/calendar relevance, upload and browse property-related media via SharePoint.

**Go-live (if partner proceeds):** **R4, R5, R6**, optionally **R7**, plus **R13** (WordPress showcase) and **R14** (aggregator feed)—so public website and portal syndication paths no longer depend on the previous CRM as the property hub.

**Later:** **R9–R12** and any expanded website/product surface beyond the WordPress plugin.

Implement within each phase in dependency order (for POC: R1 before R2/R3/R8; R3 before R8 for property-scoped media).

## Suggested build order

1. Complete all **POC** rows (sales/demo milestone).
2. If the design partner commits to cutover, complete **Go-live** rows; treat **R7** as optional unless compliance tracking is a launch blocker.
3. Pick **Later** rows via `/speckit.specify` when product priority allows.

Re-prioritise by updating **Phase** and this section—roadmap first, then affected sub-specs.

## Linking convention

When creating a sub-spec, set the **Input** line in `spec.md` to:

```markdown
**Input**: Parent roadmap: `specs/tenancy-hub-platform/roadmap.md` → entry **R#**. <one-line feature description>
```

When the sub-spec directory exists, set the **Sub-spec** column above to that path (for example `specs/001-platform-foundation/`).

## Recursion

If a row is still too large after `/speckit.specify`, add `specs/<slice-slug>/roadmap.md` and decompose that slice only.

## Confidentiality

Do not commit customer-specific requirements, internal process maps, or commercial figures into this repository. Domain detail belongs in Spec Kit artifacts only at the level needed for implementation, written for a **public-safe** audience. Partner-specific cutover steps (feeds, WordPress stack) can be discussed privately; specs describe integrations generically.
