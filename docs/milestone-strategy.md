# Letoryn: Milestone strategy

<!-- AI collaborator instructions: This is the canonical milestone policy. When creating or updating GitHub issues and pull requests, assign the delivery-phase milestone defined here. Do not invent ad-hoc milestone names (for example "MVP", "Sprint 3") unless the maintainer adds them in this file first. -->

GitHub **milestones** group issues and pull requests by **delivery phase** from [product-vision.md](./product-vision.md#delivery-phases). They are separate from:

- **`type/epic`** labels (roadmap themes such as R1: not a milestone substitute; see [label-strategy.md](./label-strategy.md)).
- **Roadmap rows** **R1–R15** (scope and dependencies; see [roadmap.md](../specs/letoryn-platform/roadmap.md)).

Related artefacts:

- Pull requests: [pull-request-policy.md](./pull-request-policy.md).
- Labels: [label-strategy.md](./label-strategy.md).
- Agent entry point: [AGENTS.md](../AGENTS.md).

---

## Canonical milestones (GitHub)

Create and maintain **exactly these** open milestones on `markheydon/letoryn-m365` unless this document is updated first. Milestone titles match roadmap **phase section headings** one to one.

| Milestone title | Delivery phase | Roadmap rows (primary) | Meaning |
|-----------------|----------------|------------------------|---------|
| **Catalogue** | Catalogue | **R1**, **R3**, **R8**, **R13**, **R14** | Property catalogue: foundation (R1 done), listings, SharePoint media, one listing feed for WordPress and syndication |
| **Operations** | Operations | **R2**, **R5**, **R6** | Light contacts, repairs/work orders, dashboard without full tenancies |
| **Vision** | Vision | **R4**, **R7**, **R9**, **R10**, **R11**, **R12**, **R15** | Long-term modules; may not ship without domain expert input |

Row-to-phase mapping matches the roadmap **Catalogue**, **Operations**, and **Vision** sections in [roadmap.md](../specs/letoryn-platform/roadmap.md). If the roadmap phase changes, update this table and re-milestone **all** issues and PRs (open and closed) per [Migration from legacy milestones](#migration-from-legacy-milestones).

### Informal “first release”

Informal “MVP” or “v1” usually means **Catalogue** complete (remaining R3, R8, R13/R14 with one shared feed), not the full R1–R15 map.

---

## How to choose a milestone

| Work type | Milestone rule |
|-----------|----------------|
| Issue tied to one roadmap row **R#** | Use the phase for that row (**Catalogue**, **Operations**, or **Vision**). |
| Epic spanning multiple rows in one phase | Same phase milestone (for example R3 and R8 work → **Catalogue**). |
| Epic spanning phases | Use the **earliest** phase milestone that must be satisfied for the epic to be considered done for the current programme goal (usually **Catalogue** until catalogue rows are complete). |
| Spec Kit feature under `specs/00N-*` | Milestone from the parent roadmap row cited in the spec (for example `001-platform-foundation` → **R1** → **Catalogue**). |
| `area/infrastructure` (CI, Dependabot, AppHost only) | **No milestone** if the change is not tied to a roadmap row; otherwise milestone of the feature it unblocks (for example R1 foundation → **Catalogue**). |
| `area/docs` only | **No milestone** unless the doc change is release-gating for a phased deliverable. |
| Dependabot PRs | **No milestone**. |
| Bugs in production (future) | Phase of the affected capability’s roadmap row, or **Catalogue** if no row is obvious and the bug blocks catalogue use. |

When in doubt, prefer the milestone of the **primary** `area/*` / **R#** in the issue title or Spec Kit path.

### Row → milestone quick reference

| R# | Milestone |
|----|-----------|
| R1, R3, R8, R13, R14 | **Catalogue** |
| R2, R5, R6 | **Operations** |
| R4, R7, R9, R10, R11, R12, R15 | **Vision** |

---

## Issues

- Set **milestone** when creating or grooming an issue if the delivery phase is known.
- Keep **milestone** aligned with the linked roadmap row when reprioritising (update [roadmap.md](../specs/letoryn-platform/roadmap.md) first, then issues).
- **`type/epic`** issues for a roadmap row (for example “R1 Platform foundation”) use that row’s phase milestone (**Catalogue** for R1).
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
gh issue edit 42 --milestone "Catalogue"
gh pr edit 10 --milestone "Operations"
```

---

## Migration from legacy milestones

Legacy milestone names **POC**, **Go-live**, and **Later** are retired. After a roadmap phase change:

1. Ensure GitHub has open milestones **Catalogue**, **Operations**, and **Vision** (create or rename; close empty legacy milestones).
2. Remap **every issue and every pull request**, **open and closed**, except items that policy leaves unset (Dependabot, untracked infra chores).
3. Prefer the current **R#** in title, body, labels (`area/*`), or Spec Kit path; fall back to legacy milestone mapping:

| Legacy milestone | Remap to |
|------------------|----------|
| **POC** | **Catalogue** for R1, R3, R8, R13, R14; **Operations** for R2 if the issue was CRM-only POC work; otherwise use **R#** table above |
| **Go-live** | Per **R#** table (for example R13/R14 → **Catalogue**; R4/R6/R15 → **Vision** or **Operations** per current roadmap) |
| **Later** | **Operations** for R2, R5, R6; **Vision** for all other Later rows |

Example bulk pass (adjust numbers after listing):

```bash
# List all issues (paginate as needed)
gh issue list --repo markheydon/letoryn-m365 --state all --limit 500 --json number,milestone,title

# List all PRs
gh pr list --repo markheydon/letoryn-m365 --state all --limit 500 --json number,milestone,title

# Edit one item
gh issue edit <n> --repo markheydon/letoryn-m365 --milestone "Catalogue"
gh pr edit <n> --repo markheydon/letoryn-m365 --milestone "Vision"
```

Re-run after reprioritising the roadmap so closed history stays consistent with the current phase model.

### Migration status (2026-10-06)

GitHub milestones **Catalogue**, **Operations**, and **Vision** are open with descriptions aligned to this file. Legacy **Go-live** and **Later** milestones are closed with zero items (former **POC** was remapped to **Catalogue**). All tracked issues and PRs with a milestone use **Catalogue** for R1 work; chores without a roadmap row (for example rebrand #8) correctly have no milestone.

---

## Maintenance

| Event | Action |
|-------|--------|
| New roadmap row or phase change | Update [roadmap.md](../specs/letoryn-platform/roadmap.md), then this file’s table, then re-milestone all issues and PRs. |
| Catalogue phase complete | Close the **Catalogue** milestone on GitHub when all catalogue-scope issues for that programme goal are closed; open a new **Catalogue** milestone only if the maintainer resets scope (document in this file). |
| New milestone name needed | Add it here first, create it on GitHub, then use it: do not create orphan milestones. |

---

## AI collaborator checklist

When creating or updating an issue or PR:

1. Identify primary **R#** or Spec Kit path → lookup phase in [roadmap.md](../specs/letoryn-platform/roadmap.md) (section heading or row → milestone table above).
2. Set milestone to **Catalogue**, **Operations**, or **Vision** per the table above (or leave unset per rules).
3. On PRs, copy milestone from the issue when linked.
4. Do not create milestones named POC, Go-live, MVP, Sprint, or version numbers unless this document is updated.
