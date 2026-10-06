# Tenancy Hub: Milestone strategy

<!-- AI collaborator instructions: This is the canonical milestone policy. When creating or updating GitHub issues and pull requests, assign the delivery-phase milestone defined here. Do not invent ad-hoc milestone names (for example "MVP", "Sprint 3") unless the maintainer adds them in this file first. -->

GitHub **milestones** group issues and pull requests by **delivery phase** from [product-vision.md](./product-vision.md#delivery-phases). They are separate from:

- **`type/epic`** labels (roadmap themes such as R1: not a milestone substitute; see [label-strategy.md](./label-strategy.md)).
- **Roadmap rows** **R1–R15** (scope and dependencies; see [roadmap.md](../specs/tenancy-hub-platform/roadmap.md)).

Related artefacts:

- Pull requests: [pull-request-policy.md](./pull-request-policy.md).
- Labels: [label-strategy.md](./label-strategy.md).
- Agent entry point: [AGENTS.md](../AGENTS.md).

---

## Canonical milestones (GitHub)

Create and maintain **exactly these** open milestones on `markheydon/tenancy-hub` unless this document is updated first.

| Milestone title | Delivery phase | Roadmap rows (primary) | Meaning |
|-----------------|----------------|------------------------|---------|
| **POC** | POC | **R1**, **R2**, **R3**, **R8** | Demo viability: multi-tenant shell, core CRM/properties, M365/SharePoint integration. |
| **Go-live** | Go-live | **R4**, **R5**, **R6**, **R13**, **R14**, **R15**; **R7** when compliance tracking is in scope for launch | Partner cutover: operational modules plus website/syndication paths. |
| **Later** | Later | **R9**, **R10**, **R11**, **R12** | Post go-live expansion (reporting, finance, portals, AI). |

Row-to-phase mapping matches the **Phase** column in [roadmap.md](../specs/tenancy-hub-platform/roadmap.md). If the roadmap phase changes, update this table and move open issues/PRs to the correct milestone.

### POC vs “MVP” in conversation

The repository uses **POC**, **Go-live**, and **Later** as milestone names: not a separate **MVP** milestone. Informal “MVP” usually means either:

- **POC complete**: all POC rows shipped (demo-ready), or  
- A **Go-live minimum** agreed with a design partner (subset of Go-live rows).

When opening issues, use the **roadmap Phase column**, not informal MVP wording.

---

## How to choose a milestone

| Work type | Milestone rule |
|-----------|----------------|
| Issue tied to one roadmap row **R#** | Use the phase for that row (**POC**, **Go-live**, or **Later**). |
| Epic spanning multiple rows in one phase | Same phase milestone (for example R1–R3 work → **POC**). |
| Epic spanning phases | Use the **earliest** phase milestone that must be satisfied for the epic to be considered done for the current programme goal (usually **POC** until POC rows are complete, then **Go-live**). |
| Spec Kit feature under `specs/00N-*` | Milestone from the parent roadmap row cited in the spec (for example `001-platform-foundation` → **R1** → **POC**). |
| `area/infrastructure` (CI, Dependabot, AppHost only) | **No milestone** if the change is not tied to a roadmap row; otherwise milestone of the feature it unblocks (for example R1 foundation → **POC**). |
| `area/docs` only | **No milestone** unless the doc change is release-gating for a phased deliverable. |
| Dependabot PRs | **No milestone**. |
| Bugs in production (future) | Phase of the affected capability’s roadmap row, or **Go-live** if no row is obvious and the bug blocks partner use. |

When in doubt, prefer the milestone of the **primary** `area/*` / **R#** in the issue title or Spec Kit path.

---

## Issues

- Set **milestone** when creating or grooming an issue if the delivery phase is known.
- Keep **milestone** aligned with the linked roadmap row when reprioritising (update [roadmap.md](../specs/tenancy-hub-platform/roadmap.md) first, then issues).
- **`type/epic`** issues for a roadmap row (for example “R1 Platform foundation”) use that row’s phase milestone (**POC** for R1).
- Do not use milestones as a substitute for `status/*` or `priority/*` labels.

---

## Pull requests

Follow [pull-request-policy.md](./pull-request-policy.md):

- **Default:** copy **milestone** from the linked tracking issue.
- **No issue:** set milestone from the **Roadmap / Spec Kit** section (cited **R#** → phase table above).
- **Dependabot:** leave milestone unset.
- When the PR opens, ensure the **issue** milestone matches if both exist (issue is source of truth once set).

GitHub CLI examples:

```bash
gh issue edit 42 --milestone "POC"
gh pr edit 10 --milestone "POC"
```

---

## Maintenance

| Event | Action |
|-------|--------|
| New roadmap row or phase change | Update [roadmap.md](../specs/tenancy-hub-platform/roadmap.md), then this file’s table, then re-milestone open issues. |
| Phase complete (for example POC rows done) | Close the **POC** milestone on GitHub when all POC-scope issues for that programme goal are closed; open a new **POC** milestone only if the maintainer resets scope (document in this file). |
| New milestone name needed | Add it here first, create it on GitHub, then use it: do not create orphan milestones. |

---

## AI collaborator checklist

When creating or updating an issue or PR:

1. Identify primary **R#** or Spec Kit path → lookup **Phase** in [roadmap.md](../specs/tenancy-hub-platform/roadmap.md).
2. Set milestone to **POC**, **Go-live**, or **Later** per the table above (or leave unset per rules).
3. On PRs, copy milestone from the issue when linked.
4. Do not create milestones named MVP, Sprint, or version numbers unless this document is updated.
