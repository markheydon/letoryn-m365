# Testing standards

Unless explicitly requested otherwise, all automated tests in this repository follow these rules.

## Unit testing

- **Framework:** [xUnit v3](https://xunit.net/) for all automated unit tests.
- **Mocks:** [NSubstitute](https://nsubstitute.github.io/) for mocks, stubs, and test doubles.
- **Assertions:** Built-in xUnit assert methods only.
- **Dependencies:** Keep test project dependencies minimal.

Do **not** introduce:

- FluentAssertions
- AwesomeAssertions
- Shouldly
- Moq
- NUnit
- MSTest

### Test runner

`global.json` configures **Microsoft.Testing.Platform** as the test runner (`test.runner`). New test projects under `tests/` should use the xUnit v3 and test SDK packages pinned in `Directory.Packages.props` (`xunit.v3`, `NSubstitute`, `Microsoft.NET.Test.Sdk`, `coverlet.collector`).

## AppHost testing (.NET Aspire)

AppHost modelling and orchestration are **not** tested. Validate AppHost changes by running the distributed application locally (for example `aspire start`) and through application-level tests: not dedicated AppHost unit tests.

## End-to-end testing

When end-to-end coverage is required:

- Use **Playwright via C#**, not TypeScript Playwright tests.
- Focus on key user journeys and business-critical workflows.
- Do not use Playwright as a substitute for unit tests.
- Prefer a small number of high-value E2E tests over large numbers of brittle UI tests.
- E2E tests should validate complete user workflows, not individual UI elements.

For local browser automation against a running Aspire app, discover endpoints from Aspire first (see `.agents/skills/aspire-monitoring/references/playwright-handoff.md` in this repo).

E2E project: `tests/Letoryn.E2E/` (Playwright C#, xUnit v3). Journeys cite IDs from [platform-foundation-user-journeys.md](./platform-foundation-user-journeys.md).

| Variable | Purpose |
|----------|---------|
| `LETORYN_E2E_BASE_URL` | Web frontend URL (required to run E2E; omit in CI to skip) |
| `LETORYN_E2E_MEMBER_STORAGE_STATE` | Playwright storage state for an active agency member |
| `LETORYN_E2E_NO_ACCESS_STORAGE_STATE` | Storage state for Entra user with no Letoryn membership |

After building the E2E project, install browsers once: `pwsh tests/Letoryn.E2E/bin/Release/net10.0/playwright.ps1 install chromium` (path matches your configuration).

## Layout

- Production code: `src/`
- Test projects: `tests/`: example: `tests/Letoryn.ApiService.UnitTests/` (xUnit v3, NSubstitute, `WebApplicationFactory` for API smoke tests); `tests/Letoryn.E2E/` (Playwright C# journeys). Add new test projects to `Letoryn.slnx`.
