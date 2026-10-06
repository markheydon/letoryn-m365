# Letoryn Constitution

## Core Principles

### I. Security First (NON-NEGOTIABLE)

Letoryn is a **security-first** application. Threat modeling, least privilege, and safe defaults
MUST guide every feature and integration.

- All **user-supplied and external input** MUST be validated at trust boundaries (HTTP APIs, forms,
  webhooks, file uploads, and background job payloads) before use in business logic, persistence, or
  outbound calls.
- Authentication, authorization, tenant isolation, and secrets handling MUST be explicit in design
  and review—not assumed from framework defaults alone.
- Sensitive data MUST NOT appear in logs, error pages, or client-visible messages unless required
  and approved.

**Rationale:** The product handles tenancy and operational data; a single missed boundary undermines
trust for every customer.

### II. Documentation Completeness

Code MUST be understandable without oral tradition. Documentation is part of the deliverable, not
an afterthought.

- **Class libraries** under `src/` MUST document public types and members with XML comments; the
  solution build enforces this (see `docs/build-quality.md`).
- **Application projects** (Web, API, AppHost) MUST document non-obvious public contracts, security
  assumptions, and integration behavior where a maintainer would otherwise guess.
- Feature specs, plans, and tasks under `specs/` MUST stay aligned with shipped behavior for
  governance work; user-facing guides MUST stay aligned with E2E coverage (Principle VI).

**Rationale:** Fully documented code reduces security and quality regressions and speeds onboarding
for any modern contributor.

### III. Clean Architecture & Familiar Modern Practices

Solution structure MUST follow **clean architecture** boundaries: domain and application rules stay
independent of UI, hosting, and infrastructure; dependencies point inward.

- Use patterns documented in `docs/csharp-patterns.md` and `docs/tech-stack.md`: dependency
  injection, async/`CancellationToken`, typed HTTP clients, `IOptions<T>` in apps, records for
  immutable DTOs, nullable reference types, and centralized package/MSBuild policy.
- New code MUST match existing project conventions before introducing new abstractions; complexity
  MUST be justified in review.
- Aspire orchestration stays in AppHost; cross-cutting defaults in `Letoryn.ServiceDefaults`;
  product logic MUST NOT leak into orchestration-only projects.

**Rationale:** Predictable structure lets contributors apply standard .NET and Aspire skills without
relearning a bespoke layout.

### IV. Testing Standards

Automated tests MUST prove behavior that matters; style and tooling MUST stay consistent.

- **Unit tests:** xUnit v3, NSubstitute, xUnit assertions only—no FluentAssertions, Moq, NUnit, or
  MSTest (see `docs/testing.md`).
- Tests MUST cover meaningful behavior and regressions; trivial assertions that duplicate the
  compiler are discouraged.
- **AppHost** resource graphs are not unit-tested; validate orchestration by running the distributed
  app locally and through application-level tests.
- CI MUST pass Release lint (`dotnet format --verify-no-changes`), build, and test before merge.

**Rationale:** One testing stack keeps reviews fast and avoids incompatible mock/assert ecosystems.

### V. User Experience Consistency

The web UI MUST feel like one product, not a collection of one-off pages.

- **Fluent UI Blazor** components and parameters are the default for layout, spacing, and
  appearance (see `docs/web-ui-and-css.md`).
- Custom CSS is limited to Blazor platform chrome, approved shell hooks, and future centralized
  branding— not per-page overrides of Fluent internals.
- Error, empty, and loading states MUST use consistent patterns across journeys so users are not
  surprised by divergent behavior.

**Rationale:** Consistency reduces support burden and reinforces trust in a security-first product.

### VI. E2E Journeys Aligned with Public Documentation

End-to-end tests MUST reflect what we tell users the product does.

- **Playwright C#** covers high-value user journeys—not TypeScript Playwright or micro UI tests
  (see `docs/testing.md`).
- Each E2E journey MUST map to a documented path in a **user guide, help topic, or equivalent
  public-facing documentation**; when docs or product behavior changes, tests or docs MUST update
  in the same change set or a tracked follow-up before release.
- Prefer a small set of stable journey tests over brittle element-level coverage.

**Rationale:** Misaligned docs and tests hide broken workflows and undermine security and UX
promises we make publicly.

## Engineering Standards

Technology choices and non-negotiable build rules for this repository:

| Area | Requirement |
|------|-------------|
| Runtime & orchestration | .NET 10, Aspire 13.6+ AppHost with Project v2 (`AddDotnetProject`) |
| Front end | Blazor + Fluent UI Blazor v5 (`Letoryn.Web`) |
| Quality bar | `TreatWarningsAsErrors`, nullable enabled, EditorConfig enforced in build |
| Packages | Central versions in `Directory.Packages.props` |
| Product planning | Spec Kit SDD under `specs/`; keep public-repo specs free of confidential detail |
| Runtime guidance | `AGENTS.md` and `docs/` elaborate this constitution; they MUST NOT contradict it |

## Quality Gates & Workflow

- Pull requests MUST demonstrate compliance with Core Principles; reviewers MUST block merges that
  skip input validation, drop required documentation, or diverge from testing/UI standards without
  explicit amendment to this constitution.
- Features SHOULD flow specify → plan → tasks → implement using Spec Kit workflows when scope is
  non-trivial.
- Local verification before push: `dotnet build Letoryn.slnx`, `dotnet test Letoryn.slnx`, and
  format as needed (`dotnet format Letoryn.slnx`).

## Governance

This constitution is the highest-level engineering governance for Letoryn. When `AGENTS.md`,
`docs/`, or team habit conflict with a principle here, **this document wins** until amended.

**Amendments:** Propose changes via `/speckit-constitution` (or equivalent constitution workflow)
with rationale, version bump, and updated `LAST_AMENDED_DATE`. Material new principles or
redefinitions require MINOR or MAJOR version increments per semantic versioning below.

**Version policy:**

- **MAJOR:** Backward-incompatible removal or redefinition of principles.
- **MINOR:** New principle or materially expanded section.
- **PATCH:** Clarifications, wording, typo fixes without changing intent.

**Compliance review:** Periodic review SHOULD occur when platform direction changes (see
`docs/tech-stack.md` and `specs/letoryn-platform/roadmap.md`) or after significant security
incidents.

**Version**: 1.0.0 | **Ratified**: 2026-09-30 | **Last Amended**: 2026-09-30
