# Roadmap: Letoryn platform

Letoryn is a **multi-tenant** Microsoft 365-centric **property catalogue** for small UK letting agencies; the epic is too large for one Spec Kit cycle, so work is split into independently specifiable slices ([spec of specs](https://github.github.com/spec-kit/concepts/spec-of-specs.html)). Each slice is a normal feature (`spec.md`, `plan.md`, `tasks.md`); product context and delivery phases live in [docs/product-vision.md](../../docs/product-vision.md).

The product **sits beside** the agency's existing website, syndication path, and specialist systems. It does not aim to be the single application that makes a CRM, tenancy platform, compliance product, or accounting package unnecessary. The **first feature set** is the catalogue an agency can publish to its existing WordPress site and syndication tools from **one outbound listing feed**. Rows in **Operations** and **Vision** are recorded ideas. Scheduling one is not a decision to replace the third-party system that already covers that job.

**Status legend:** planned · in-progress · done

Build rows in **dependency order** within each phase. Phase grouping is summarised below and in [product-vision.md](../../docs/product-vision.md#delivery-phases). GitHub milestones match these phases: [milestone-strategy.md](../../docs/milestone-strategy.md).

| Phase | Meaning |
|-------|---------|
| **Catalogue** | Sign in as an agency, maintain property listings, store photos and files in SharePoint, expose **one canonical listing feed** for the WordPress plugin (R13) and aggregator syndication (R14) |
| **Operations** | Light operational tooling buildable without deep lettings domain expertise: optional contacts, repair/work-order tracking, simple dashboard. Not full tenancies or compliance programmes |
| **Vision** | Long-term modules (tenancies, compliance depth, finance, portals, reporting, AI, complaints). **May not ship** without a UK lettings domain expert partner; not a plan to retire specialist third-party products |

## Catalogue

Foundation for the catalogue is **done** (R1). Remaining catalogue work: **R3 → R8 → R13/R14** (R13 and R14 can be specified in parallel once the listing feed contract exists).

| ID | Sub-feature | Intent | Scope boundary | Depends on | Status | Sub-spec |
|----|-------------|--------|----------------|------------|--------|----------|
| R1 | Platform foundation | Multi-tenant SaaS shell: PostgreSQL, Entra ID, agency isolation, roles, audit, notifications | Per-agency tenancy from day one; shared UI chrome; domain modules plug in behind stable tenancy APIs | n/a | done | [specs/001-platform-foundation/](../001-platform-foundation/) |
| R3 | Properties & units | Property records, units, lifecycle/preparation, media **metadata** tied to properties | Catalogue slice is the marketing listing (address, type, rent, availability, description, features, media metadata). Operational property data in PostgreSQL; binary media in SharePoint via R8. No tenancy, deposit, notice, or arrears data (see R4). Contact links arrive with R2 and are not required to publish a listing | R1 | planned | n/a |
| R8 | Graph, Outlook & SharePoint | Mail/calendar in context; SharePoint for **documents and media library** (property photos, repair evidence, cleaning proof, etc.) | Catalogue milestone delivers the property photo and document library in SharePoint and media URLs for the listing feed. Mail and calendar context follow once that library works. Email capture to enquiries/tasks (work orders when R6 exists); Teams/To Do/SMS deferred; not a full SharePoint CMS replacement | R1, R3 | planned | n/a |
| R13 | WordPress property showcase | Showcase lettings on an **existing agency WordPress site**: prefer a **plugin** exposing a property custom post type for theme/builder layouts | Consumes the **same canonical listing feed** as R14 (see R14). Does not replace the marketing site. Letoryn remains the system of record for listings; the plugin publishes a copy, including images from SharePoint where the feed exposes them | R3, R8 | planned | n/a |
| R14 | Property feed for aggregators | Export property data for **[Data Export](https://dataexport.co.uk/)** (or equivalent): same role as today's CRM feed to syndication | Consumes the **same canonical listing feed/API** as R13: one pipeline, not a second divergent export. **Does not** replace Data Export or portal relationships; delivers a compatible syndication-facing view of catalogue data, including SharePoint media URLs when the feed exposes them | R3, R8 | planned | n/a |

**Catalogue sequence:** **R1 (done) → R3 → R8 → R13/R14**. Within R8, ship the SharePoint media library before mail and calendar. Define the listing feed contract when specifying R3 (or a thin catalogue-export slice) so R13 and R14 do not duplicate ingestion logic.

## Operations

| ID | Sub-feature | Intent | Scope boundary | Depends on | Status | Sub-spec |
|----|-------------|--------|----------------|------------|--------|----------|
| R2 | CRM & contacts | Tenants, landlords, contractors, applicants, companies, activities, communication history | Optional contact records. Not a replacement for an agency CRM. Property links can deepen here; a listing does not require them | R1 | planned | n/a |
| R5 | Operational dashboard | Visibility: properties, tasks, work orders, compliance, calendar summaries | Read-only aggregation without tenancy or arrears until R4 exists | R2, R3, R6 | planned | n/a |
| R6 | Maintenance & work orders | Repairs, quotes, contractors, job tracking | Mini CRM for repair cases and landlord actions; no contractor portal; telephony deferred; not legal tenancy or compliance workflows | R2, R3 | planned | n/a |

## Vision

Recorded long-term intent. Rows here need lettings domain expertise and are **not committed** for delivery. Prefer integrating with or sitting beside products such as referencing, rent collection, and tenancy legal platforms (for example PayProp or Goodlord-class tools) rather than replacing them in the short to mid term.

| ID | Sub-feature | Intent | Scope boundary | Depends on | Status | Sub-spec |
|----|-------------|--------|----------------|------------|--------|----------|
| R4 | Tenancies & lettings flow | Applications, tenancy creation, move-in, renewals, arrears **tracking** (not collection) | Lightweight marketing/viewings; no payment execution; complaints in R15 | R2, R3 | planned | n/a |
| R7 | Compliance | EPC, gas, EICR, inspections, expiry and renewal workflows | Certificate metadata in-app; documents/media in SharePoint where applicable. A displayed EPC rating on a listing belongs to R3; expiry chasing belongs here | R3, R8 | planned | n/a |
| R9 | Reporting | KPIs, occupancy, arrears, compliance dashboards | Not full BI/custom report builder | R3-R7, R15 | planned | n/a |
| R10 | Finance & payments | Rent schedules, statements, Stripe/GoCardless, later open banking | Does not replace full accounting or specialist rent platforms; reconciliation in sub-spec | R4 | planned | n/a |
| R11 | Customer portals | Tenant, landlord, contractor self-service | Per-portal scope in sub-spec | R4, R6, R10 | planned | n/a |
| R12 | AI-assisted operations | Document Q&A, email triage, assisted workflows | No autonomous legal decisions; builds on R7/R8 | R7, R8 | planned | n/a |
| R15 | Complaints & case management | Track tenant/landlord complaints with escalation, audit trail, and links to tenancies, properties, and contacts | In-app case workflow; not formal redress/ombudsman replacement; telephony/SMS channels deferred | R2, R4 | planned | n/a |

Implement within each phase in dependency order (Catalogue: R3 before R8; R3 and R8 before R13 and R14; R13 and R14 share one feed implementation).

Sub-specs link back: `**Input**: Parent roadmap: specs/letoryn-platform/roadmap.md → entry **R#**.` See [Spec Kit spec-of-specs](https://github.github.com/spec-kit/concepts/spec-of-specs.html). When a sub-spec directory exists, set the **Sub-spec** column (for example `specs/001-platform-foundation/`).

If a row is still too large after `/speckit.specify`, add `specs/<slice-slug>/roadmap.md` and decompose that slice only.

Do not commit customer-specific requirements, internal process maps, or commercial figures: see [product-vision.md](../../docs/product-vision.md#confidentiality-and-private-assets).
