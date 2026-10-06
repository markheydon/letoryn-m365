# Roadmap: Letoryn platform

Letoryn is a multi-tenant Microsoft 365–centric operating platform for small UK letting agencies; the epic is too large for one Spec Kit cycle, so work is split into independently specifiable slices ([spec of specs](https://github.github.com/spec-kit/concepts/spec-of-specs.html)). Each slice is a normal feature (`spec.md`, `plan.md`, `tasks.md`); product context and delivery phases live in [docs/product-vision.md](../../docs/product-vision.md).

**Status legend:** planned · in-progress · done

Build rows in **dependency order**; POC / go-live / later grouping is in [product-vision.md](../../docs/product-vision.md#delivery-phases). GitHub milestones for those phases: [milestone-strategy.md](../../docs/milestone-strategy.md).

| ID | Sub-feature | Intent | Scope boundary | Depends on | Phase | Status | Sub-spec |
|----|-------------|--------|----------------|------------|-------|--------|----------|
| R1 | Platform foundation | Multi-tenant SaaS shell: PostgreSQL, Entra ID, agency isolation, roles, audit, notifications | Per-agency tenancy from day one; shared UI chrome; domain modules plug in behind stable tenancy APIs | n/a | **POC** | in-progress | n/a |
| R2 | CRM & contacts | Tenants, landlords, contractors, applicants, companies, activities, communication history | Contact-centric CRM; property links deepen with R3/R4 | R1 | **POC** | planned | n/a |
| R3 | Properties & units | Property records, units, lifecycle/preparation, media **metadata** tied to properties | Operational property data in PostgreSQL; binary media in SharePoint via R8: not portal syndication (see R14) | R1, R2 | **POC** | planned | n/a |
| R4 | Tenancies & lettings flow | Applications, tenancy creation, move-in, renewals, arrears **tracking** (not collection) | Lightweight marketing/viewings; no payment execution; complaints in R15 | R2, R3 | **Go-live** | planned | n/a |
| R5 | Operational dashboard | Visibility: properties, tasks, work orders, compliance, calendar summaries | Read-only aggregation | R2, R3, R4 | **Go-live** | planned | n/a |
| R6 | Maintenance & work orders | Repairs, quotes, contractors, job tracking | No contractor portal; telephony deferred | R2, R3 | **Go-live** | planned | n/a |
| R7 | Compliance | EPC, gas, EICR, inspections, expiry and renewal workflows | Certificate metadata in-app; documents/media in SharePoint where applicable | R3, R8 | **Go-live** (optional) | planned | n/a |
| R8 | Graph, Outlook & SharePoint | Mail/calendar in context; SharePoint for **documents and media library** (property photos, repair evidence, cleaning proof, etc.) | Email capture to enquiries/tasks (work orders when R6 exists); Teams/To Do/SMS deferred; not a full SharePoint CMS replacement | R1, R3 | **POC** | planned | n/a |
| R9 | Reporting | KPIs, occupancy, arrears, compliance dashboards | Not full BI/custom report builder | R3–R7, R15 | **Later** | planned | n/a |
| R10 | Finance & payments | Rent schedules, statements, Stripe/GoCardless, later open banking | Does not replace full accounting; reconciliation in sub-spec | R4 | **Later** | planned | n/a |
| R11 | Customer portals | Tenant, landlord, contractor self-service | Per-portal scope in sub-spec | R4, R6, R10 | **Later** | planned | n/a |
| R12 | AI-assisted operations | Document Q&A, email triage, assisted workflows | No autonomous legal decisions; builds on R7/R8 | R7, R8 | **Later** | planned | n/a |
| R13 | WordPress property showcase | Showcase lettings on an **existing agency WordPress site**: prefer a **plugin** exposing a property custom post type for theme/builder layouts | Does not replace the marketing site platform; may evolve toward a fuller website offering later | R3 | **Go-live** | planned | n/a |
| R14 | Property feed for aggregators | Export property data for **[Data Export](https://dataexport.co.uk/)** (or equivalent): same role as today’s CRM feed to syndication | **Does not** replace Data Export or portal relationships; delivers a compatible **outbound property feed** from Letoryn | R3 | **Go-live** | planned | n/a |
| R15 | Complaints & case management | Track tenant/landlord complaints with escalation, audit trail, and links to tenancies, properties, and contacts | In-app case workflow; not formal redress/ombudsman replacement; telephony/SMS channels deferred | R2, R4 | **Go-live** | planned | n/a |

Sub-specs link back: `**Input**: Parent roadmap: specs/letoryn-platform/roadmap.md → entry **R#**.` See [Spec Kit spec-of-specs](https://github.github.com/spec-kit/concepts/spec-of-specs.html). When a sub-spec directory exists, set the **Sub-spec** column (for example `specs/001-platform-foundation/`).

If a row is still too large after `/speckit.specify`, add `specs/<slice-slug>/roadmap.md` and decompose that slice only.

Do not commit customer-specific requirements, internal process maps, or commercial figures: see [product-vision.md](../../docs/product-vision.md#confidentiality-and-private-assets).
