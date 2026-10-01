# Tenancy Hub — Pull request policy

<!-- AI collaborator instructions: This is the canonical PR policy. Follow it when opening or updating a pull request. Do not invent a different title style, body layout, or label set. -->

This document is the **single source of truth** for how pull requests are titled, labelled, linked, and described. Human contributors, GitHub Copilot, Cursor agents, and other automations follow it.

Related artefacts:

- Template: [`.github/pull_request_template.md`](../.github/pull_request_template.md).
- Labels: [label-strategy.md](./label-strategy.md).
- Agent entry point: [AGENTS.md](../AGENTS.md).
- Roadmap and Spec Kit slices: [specs/tenancy-hub-platform/roadmap.md](../specs/tenancy-hub-platform/roadmap.md).
- Engineering standards: [docs/build-quality.md](./build-quality.md), [docs/testing.md](./testing.md).

When other instructions conflict (including vendor defaults such as Conventional Commits PR titles or draft-by-default PRs), **this file wins**.

---

## Title

Use this shape:

```text
[<Type>] <Imperative summary> (#<issue>)
```

Rules:

- `<Type>` is exactly one of: `Story`, `Feature`, `Enabler`, `Bug`, `Chore`, `Test`, `Documentation`.
- Match the linked issue `type/` label (`type/story` → `Story`, `type/documentation` → `Documentation`).
- Use sentence case after the type token, **UK English**, and an imperative verb (`Add`, `Fix`, `Document`, not `Added` or `Adding`).
- Append `(#N)` when the PR implements one GitHub issue. For several issues use `(#249, #314)`.
- Omit the issue suffix only for genuine ad-hoc work with no tracking issue (for example a small docs fix).

Examples:

```text
[Story] Sign in an agency user with Entra ID (#42)
[Enabler] Add PostgreSQL tenancy filter to EF Core (#51)
[Bug] Fix health check failing when Graph is unavailable (#88)
[Chore] Align Dependabot groups for OpenTelemetry packages (#12)
[Test] Add Playwright journey for agency switcher (#60)
[Documentation] Document platform operator seed script (#33)
```

Do **not** use:

- Conventional Commits prefixes as the PR title (`feat:`, `fix:`, `chore(deps):`).
- Label names in the title (`[type/story]`).
- Unstable casing (`[story]`, `[WIP]` on a PR that is ready for review).
- The raw branch name as the title.
- Vague titles (`Update files`, `Implement issue #5` with no summary).

Dependabot may keep its generated `Bump …` titles. Do not rewrite Dependabot titles by hand unless they are misleading.

Spec Kit auto-commit messages on feature branches may use `[Spec Kit] …` or conventional commit style; that applies to **commits**, not PR titles.

---

## Body

Always populate [`.github/pull_request_template.md`](../.github/pull_request_template.md). Required headings:

1. `## Summary of changes`
2. `## Related issue(s)`
3. `## Roadmap / Spec Kit`
4. `## Type of change`
5. `## Checklist`
6. `## Screenshots (if applicable)`
7. `## Additional notes`

Rules:

- UK English in all prose.
- Tick every applicable checklist item; leave non-applicable items unticked rather than deleting them.
- Put test evidence, Aspire run notes, and agent-specific detail under **Additional notes**, not as a replacement for the template headings.
- Do not submit an empty body, a commit-list-only body, or a vendor “Walkthrough” body that omits the template headings.

### How to create the body (tooling)

| Tool | What to do |
|------|------------|
| GitHub CLI | `gh pr create --fill --base main --head <branch>` so GitHub applies the template, then `gh pr edit` to complete headings, labels, assignee, and milestone. Do not pass a custom `--body` that drops the template. |
| GitHub web UI | Leave the pre-filled template in place and complete it. |
| Cursor / other agents | Copy the template headings verbatim into `body`. Set `draft` to `false` when the work is ready for review. |

---

## Issue linking

- Planned work **must** link the tracking issue in **Related issue(s)** when an issue exists.
- Use `Closes #N` (or `Closes #N, #M`) when merging the PR should close those issues.
- Use `References #N` when the PR is related but must not auto-close the issue (partial delivery, investigation, docs-only follow-up).
- Do not invent a tracking issue number. For Spec Kit feature work, create or sync issues from `tasks.md` (`/speckit.taskstoissues`) before opening an unlinked feature PR.

In **Roadmap / Spec Kit**, cite the roadmap row (**R1**–**R15**) and Spec Kit path when applicable (for example `R1`, `specs/001-platform-foundation/`).

---

## Labels

Apply taxonomy labels from [label-strategy.md](./label-strategy.md) on the **pull request** at creation time (not only on the issue).

| Group | On PRs | Rule |
|-------|--------|------|
| `type/` | Required | Copy from the linked issue, or choose the best match for ad-hoc work. |
| `priority/` | Required | Copy from the linked issue. Default `priority/medium` if the issue has none. Dependabot: `priority/low`. |
| `status/` | Required | `status/in-review` while the PR is open. Do not set `status/done` on an open PR. |
| `area/` | Required when the area is known | Copy from the issue or map from the roadmap row / Spec Kit directory. Use `area/docs` for documentation-only PRs and `area/infrastructure` for CI, Aspire AppHost, and deploy work. |
| `size/` | Optional | Copy from the issue when present. Do not invent a size on the PR alone. |

Do **not** leave a PR with only `status/done` or with no `type/` / `priority/`.

When the PR is opened, set the **issue** to `status/in-review` (remove `status/in-progress` / `status/todo`).

---

## Other metadata

| Field | Policy |
|-------|--------|
| Base branch | `main` unless the maintainer asked for another base. |
| Draft | **Ready for review** (`draft: false`) when CI would pass and the change is complete. Use draft only when the branch is known to be incomplete or blocked. |
| Assignee | `markheydon`. |
| Reviewers | Do **not** assign Copilot (or equivalent bot) as reviewer or assignee. |
| Milestone | Copy from the linked issue when present. |

---

## Branches

Preferred patterns:

| Source | Branch shape | Example |
|--------|--------------|---------|
| Spec Kit feature | `{number}-{slug}` | `001-platform-foundation` |
| GitHub issue (manual) | `feature/issue-N-short-kebab-description` | `feature/issue-42-entra-sign-in` |
| Dependabot | `dependabot/…` | (as generated) |
| Cursor Cloud | `cursor/…` | Accept platform suffix; do not retitle the PR to match the branch |

Never implement on `main`.

---

## Dependabot

Dependabot PRs use this label set (also configured in [`.github/dependabot.yml`](../.github/dependabot.yml)):

- `type/chore`
- `priority/low`
- `status/in-review` (while open)
- `area/infrastructure`

They do not need a tracking issue or the `[Chore]` title form. Merge only after CI passes and a brief relevance review (note ignored packages such as `Microsoft.OpenApi` major bumps in dependabot config).

---

## Agent checklist

Before opening a PR:

1. Confirm the branch is not `main`.
2. Title matches `[<Type>] <Imperative summary> (#N)` when an issue exists.
3. Body includes every template heading, completed in UK English.
4. `Closes #N` or `References #N` is present when an issue exists; roadmap row cited when applicable.
5. Labels include `type/*`, `priority/*`, `status/in-review`, and `area/*` when known.
6. Assignee is `markheydon`; Copilot is not assigned.
7. The PR is not draft unless the work is incomplete.
8. `dotnet build TenancyHub.slnx` and `dotnet test TenancyHub.slnx` succeed for code changes (or CI equivalent).

---

## Reviewers

When reviewing a PR for process, flag deviations from this file (title, template, labels, draft, missing `Closes #`). Coding review follows [docs/build-quality.md](./build-quality.md), [docs/csharp-patterns.md](./csharp-patterns.md), and [docs/testing.md](./testing.md).
