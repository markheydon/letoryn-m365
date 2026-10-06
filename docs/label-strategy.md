# Letoryn: Label strategy

<!-- AI collaborator instructions: See the "AI collaborator instructions" section at the bottom. When creating issues or PRs, apply at least one label from each of the `type/` and `priority/` groups. -->

This document defines the canonical label taxonomy for the **letoryn-m365** GitHub repository. Create or update labels using the definitions below so humans, Cursor agents, and other automations stay consistent.

Related artefacts:

- Pull requests: [pull-request-policy.md](./pull-request-policy.md) and [`.github/pull_request_template.md`](../.github/pull_request_template.md).
- Milestones (delivery phases): [milestone-strategy.md](./milestone-strategy.md).
- Product slices (R1–R15): [specs/letoryn-platform/roadmap.md](../specs/letoryn-platform/roadmap.md).

GitHub label definitions are synced from the maintainer’s cross-repo taxonomy process; this file is the **Letoryn–specific** catalogue (especially `area/*` for R1–R15).

---

## Label taxonomy

### Type labels

Describes the nature of the issue or PR.

| Label | Colour | Description |
|-------|--------|-------------|
| `type/epic` | `#6f42c1` | A named product theme spanning multiple features or a major roadmap increment (for example platform foundation): not a GitHub milestone; use [milestone-strategy.md](./milestone-strategy.md) for **POC** / **Go-live** / **Later** |
| `type/feature` | `#0075ca` | A Feature: groups related stories under an epic or roadmap slice |
| `type/story` | `#1d76db` | A user-facing Story delivering a discrete piece of value |
| `type/enabler` | `#e4e669` | An Enabler: technical prerequisite that unblocks stories |
| `type/test` | `#bfd4f2` | A Test issue whose primary deliverable is test coverage (unit, integration, E2E) |
| `type/bug` | `#d73a4a` | A bug or unexpected behaviour |
| `type/chore` | `#fef2c0` | Maintenance, dependency updates, or technical debt with no user-facing change |
| `type/documentation` | `#0052cc` | Documentation additions or improvements only |

---

### Priority labels

Describes urgency and importance.

| Label | Colour | Description |
|-------|--------|-------------|
| `priority/critical` | `#b60205` | Blocking: all progress or production affected |
| `priority/high` | `#d93f0b` | Should be addressed in the current slice, sprint, or release cut |
| `priority/medium` | `#fbca04` | Default for new feature work; should be addressed soon but is not blocking |
| `priority/low` | `#c2e0c6` | Nice to have; can be deferred |

---

### Status labels

Describes workflow state on the issue or PR.

| Label | Colour | Description |
|-------|--------|-------------|
| `status/todo` | `#ffffff` | Ready to be worked on; not yet started |
| `status/in-progress` | `#0e8a16` | Currently being worked on |
| `status/blocked` | `#e11d48` | Cannot proceed; waiting on something external |
| `status/ice-box` | `#8b949e` | Shelved for later; not in the active delivery queue |
| `status/in-review` | `#1d76db` | Pull request open; awaiting code review |
| `status/done` | `#cfd3d7` | Completed and closed |

---

### Feature area labels (`area/*`)

Maps work to [platform roadmap](../specs/letoryn-platform/roadmap.md) slices **R1–R15** or cross-cutting engineering areas. Apply the **primary** slice when work spans modules (for example Graph integration for properties uses `area/graph-m365` when the PR is mainly M365 wiring, `area/properties-units` when it is mainly property domain logic).

| Label | Roadmap | Colour | Description |
|-------|---------|--------|-------------|
| `area/platform-foundation` | R1 | `#e4e669` | Multi-tenant shell: PostgreSQL, Entra ID, agency isolation, roles, audit, notifications, shared UI chrome |
| `area/crm-contacts` | R2 | `#c5def5` | CRM: tenants, landlords, contractors, applicants, companies, activities, communication history |
| `area/properties-units` | R3 | `#c5def5` | Property records, units, lifecycle/preparation, media **metadata** (binaries via R8) |
| `area/tenancies-lettings` | R4 | `#bfd4f2` | Applications, tenancy lifecycle, move-in, renewals, arrears **tracking** (not collection) |
| `area/dashboard` | R5 | `#bfd4f2` | Operational dashboard and read-only aggregation |
| `area/maintenance` | R6 | `#f9d0c4` | Maintenance, repairs, quotes, contractors, job tracking |
| `area/compliance` | R7 | `#fef2c0` | EPC, gas, EICR, inspections, expiry and renewal workflows |
| `area/graph-m365` | R8 | `#d4c5f9` | Microsoft Graph, Outlook, SharePoint: mail, calendar, documents and media library |
| `area/reporting` | R9 | `#96d8c9` | KPIs, occupancy, compliance dashboards: not full BI |
| `area/finance-payments` | R10 | `#f9d0c4` | Rent schedules, statements, Stripe/GoCardless, open banking direction |
| `area/portals` | R11 | `#c5def5` | Tenant, landlord, contractor self-service portals |
| `area/ai-operations` | R12 | `#d4c5f9` | AI-assisted document Q&A, email triage, assisted workflows |
| `area/wordpress` | R13 | `#c5def5` | WordPress property showcase / plugin and custom post type integration |
| `area/property-feed` | R14 | `#c5def5` | Outbound property feed for aggregators (for example Data Export) |
| `area/complaints` | R15 | `#f9d0c4` | Complaints and case management with audit trail |
| `area/infrastructure` | n/a | `#e4e669` | Aspire AppHost, CI/CD, Azure deployment, Dependabot, build and release tooling |
| `area/docs` | n/a | `#0052cc` | Repository documentation, specs, runbooks, and agent guidance (no product code) |

Multiple `area/*` labels are allowed when the change genuinely spans slices; prefer one primary label for board filtering.

---

### Size labels

Effort estimate. Added during planning: not required when an issue is first created.

| Label | Colour | Description |
|-------|--------|-------------|
| `size/xs` | `#dde8c9` | Trivial: under one hour (typo, config tweak) |
| `size/s` | `#c5def5` | Small: under half a day |
| `size/m` | `#fef2c0` | Medium: half a day to one day |
| `size/l` | `#f9d0c4` | Large: two to three days |
| `size/xl` | `#d4c5f9` | Extra-large: more than three days; consider splitting |

---

## AI collaborator instructions

### When to apply each label

#### `type/`: always required on issues and PRs

- Apply `type/epic` to issues that name a shippable roadmap theme (for example **R1 Platform foundation**) spanning multiple Spec Kit features: not a generic milestone.
- Apply `type/feature` when **two or more** related stories or enablers deliver one user-facing capability under an epic or roadmap row.
- Apply `type/story` to user-facing delivery issues (for example `[Story] Agency admin invites a user`).
- Apply `type/enabler` to technical prerequisites (for example `[Enabler] EF Core multi-tenant filter`).
- Apply `type/test` when the primary deliverable is test coverage.
- Apply `type/bug` for unexpected or broken behaviour.
- Apply `type/chore` for maintenance, refactors, or dependency updates with no user-facing change.
- Apply `type/documentation` when the change only touches documentation or specs prose.

#### `priority/`: always required on issues and PRs

- Apply `priority/critical` only when blocking all progress or affecting production.
- Apply `priority/high` for the active roadmap slice or imminent release cut (for example POC items on **R1** while it is in progress).
- Apply `priority/medium` as the default for new feature requests.
- Apply `priority/low` for nice-to-have improvements or minor chores.

#### `status/`: updated as work progresses

- Apply `status/todo` when an issue is ready to start (acceptance criteria known).
- Change to `status/in-progress` when implementation begins.
- Change to `status/blocked` when an external dependency prevents progress.
- Change to `status/ice-box` when shelved (not the same as blocked).
- Change to `status/in-review` when a PR is opened for the issue.
- Change to `status/done` when the issue is closed.

#### `area/`: required when scope is known

- Map Spec Kit work under `specs/00N-*` to the matching roadmap row (for example `specs/001-platform-foundation/` → `area/platform-foundation` / **R1**).
- Apply `area/infrastructure` for AppHost wiring, `.github/workflows`, Dependabot, Azure/Aspire deployment, and shared build quality: not domain features.
- Apply `area/docs` for changes that only touch `docs/`, `AGENTS.md`, `specs/**/spec.md`, runbooks, or label/PR policy.
- Apply `area/graph-m365` for Entra, Graph SDK, Outlook, or SharePoint integration even when triggered from another module.

#### `size/`: apply during planning

- Size labels are optional at issue creation; add them when estimating.
- If an issue is `size/xl`, split it before starting implementation.

#### Milestones: delivery phase (not labels)

- GitHub milestones **POC**, **Go-live**, and **Later** are defined in [milestone-strategy.md](./milestone-strategy.md).
- Set milestone from the roadmap **Phase** column for the issue’s primary **R#**; do not duplicate phase as a label.
- `type/epic` describes theme; milestone describes when the work is intended to ship in the programme.
