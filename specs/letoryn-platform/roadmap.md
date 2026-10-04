# Roadmap: Letoryn platform

Letoryn is a **multi-tenant** Microsoft 365–centric **property catalogue** for small UK letting agencies; the epic is too large for one Spec Kit cycle, so work is split into independently specifiable slices ([spec of specs](https://github.github.com/spec-kit/concepts/spec-of-specs.html)). Each slice is a normal feature (`spec.md`, `plan.md`, `tasks.md`); product context and delivery phases live in [docs/product-vision.md](../../docs/product-vision.md).

The product **sits beside** the agency's existing website, syndication path, and specialist systems. It does not aim to be the single application that makes a CRM, tenancy platform, compliance product, or accounting package unnecessary. The **first feature set** is the catalogue an agency can publish to its existing WordPress site. Rows below **Catalogue** are recorded ideas. Scheduling one is not a decision to replace the third-party system that already covers that job.

**Status legend:** planned · in-progress · done

Build rows in **dependency order**; **Catalogue**, **Next**, and **Later** grouping is summarized below and in [product-vision.md](../../docs/product-vision.md#delivery-phases). GitHub milestones for delivery phases: [milestone-strategy.md](../../docs/milestone-strategy.md).

| Phase | Meaning |
|-------|---------|
| **Catalogue** | First usable slice: sign in as an agency, maintain property listings, store photos and files in SharePoint, publish listings to an existing WordPress site |
| **Next** | The next integration that still assumes surrounding tools stay: an outbound property feed |
| **Later** | Recorded ideas (CRM, tenancies, maintenance, compliance, finance, and others). Not a plan to retire third-party systems |

| ID | Sub-feature | Intent | Scope boundary | Depends on | Phase | Status | Sub-spec |
|----|-------------|--------|----------------|------------|-------|--------|----------|
| R1 | Platform foundation | Multi-tenant SaaS shell: PostgreSQL, Entra ID, agency isolation, roles, audit, notifications | Per-agency tenancy from day one; shared UI chrome; domain modules plug in behind stable tenancy APIs | n/a | **Catalogue** | in-progress | n/a |
| R2 | CRM & contacts | Tenants, landlords, contractors, applicants, companies, activities, communication history | Optional contact records. Not a replacement for an agency CRM. Property links can deepen here; a listing does not require them | R1 | **Later** | planned | n/a |
| R3 | Properties & units | Property records, units, lifecycle/preparation, media **metadata** tied to properties | Catalogue slice is the marketing listing (address, type, rent, availability, description, features, media metadata). Operational property data in PostgreSQL; binary media in SharePoint via R8. No tenancy, deposit, notice, or arrears data (see R4). Not portal syndication (see R14). Contact links arrive with R2 and are not required to publish a listing | R1 | **Catalogue** | planned | n/a |
| R4 | Tenancies & lettings flow | Applications, tenancy creation, move-in, renewals, arrears **tracking** (not collection) | Lightweight marketing/viewings; no payment execution; complaints in R15 | R2, R3 | **Later** | planned | n/a |
| R5 | Operational dashboard | Visibility: properties, tasks, work orders, compliance, calendar summaries | Read-only aggregation | R2, R3, R4 | **Later** | planned | n/a |
| R6 | Maintenance & work orders | Repairs, quotes, contractors, job tracking | No contractor portal; telephony deferred | R2, R3 | **Later** | planned | n/a |
| R7 | Compliance | EPC, gas, EICR, inspections, expiry and renewal workflows | Certificate metadata in-app; documents/media in SharePoint where applicable. A displayed EPC rating on a listing belongs to R3; expiry chasing belongs here | R3, R8 | **Later** | planned | n/a |
| R8 | Graph, Outlook & SharePoint | Mail/calendar in context; SharePoint for **documents and media library** (property photos, repair evidence, cleaning proof, etc.) | Catalogue milestone delivers the property photo and document library in SharePoint. Mail and calendar context stay in this row and follow once that library works. Email capture to enquiries/tasks (work orders when R6 exists); Teams/To Do/SMS deferred; not a full SharePoint CMS replacement | R1, R3 | **Catalogue** | planned | n/a |
| R9 | Reporting | KPIs, occupancy, arrears, compliance dashboards | Not full BI/custom report builder | R3–R7, R15 | **Later** | planned | n/a |
| R10 | Finance & payments | Rent schedules, statements, Stripe/GoCardless, later open banking | Does not replace full accounting; reconciliation in sub-spec | R4 | **Later** | planned | n/a |
| R11 | Customer portals | Tenant, landlord, contractor self-service | Per-portal scope in sub-spec | R4, R6, R10 | **Later** | planned | n/a |
| R12 | AI-assisted operations | Document Q&A, email triage, assisted workflows | No autonomous legal decisions; builds on R7/R8 | R7, R8 | **Later** | planned | n/a |
| R13 | WordPress property showcase | Showcase lettings on an **existing agency WordPress site**: prefer a **plugin** exposing a property custom post type for theme/builder layouts | Does not replace the marketing site. Letoryn remains the system of record for listings; the plugin publishes a copy, including images stored in SharePoint | R3, R8 | **Catalogue** | planned | n/a |
| R14 | Property feed for aggregators | Export property data for **[Data Export](https://dataexport.co.uk/)** (or equivalent): same role as today's CRM feed to syndication | **Does not** replace Data Export or portal relationships; delivers a compatible **outbound property feed** from Letoryn | R3 | **Next** | planned | n/a |
| R15 | Complaints & case management | Track tenant/landlord complaints with escalation, audit trail, and links to tenancies, properties, and contacts | In-app case workflow; not formal redress/ombudsman replacement; telephony/SMS channels deferred | R2, R4 | **Later** | planned | n/a |

**Catalogue:** **R1 → R3 → R8 → R13** — log in as an agency, manage property listings, upload and browse property photos in SharePoint, and publish available properties to an existing WordPress site as a custom post type. Within R8, ship the SharePoint media library before mail and calendar.

**Next:** **R14** (aggregator feed into the syndication path the agency already uses).

**Later (ideas, not a replacement programme):** **R2** and **R4–R7**, **R9–R12**, and **R15**.

Implement within each phase in dependency order (Catalogue: R1 before R3; R3 before R8; R3 and R8 before R13).

Sub-specs link back: `**Input**: Parent roadmap: specs/letoryn-platform/roadmap.md → entry **R#**.` See [Spec Kit spec-of-specs](https://github.github.com/spec-kit/concepts/spec-of-specs.html). When a sub-spec directory exists, set the **Sub-spec** column (for example `specs/001-platform-foundation/`).

If a row is still too large after `/speckit.specify`, add `specs/<slice-slug>/roadmap.md` and decompose that slice only.

Do not commit customer-specific requirements, internal process maps, or commercial figures: see [product-vision.md](../../docs/product-vision.md#confidentiality-and-private-assets).
