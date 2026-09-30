# Local development

How to build, test, and run Tenancy Hub on a developer machine. Product scope is under `specs/` via [Spec Kit](https://github.com/github/spec-kit) (see [platform roadmap](../specs/tenancy-hub-platform/roadmap.md)).

## Prerequisites

| Tool | Version / notes |
|------|-----------------|
| [.NET SDK](https://dotnet.microsoft.com/download) | **10.0.300+** (pinned in `global.json`) |
| [Aspire CLI](https://aspire.dev/get-started/install-cli/) | **13.6+** (`aspire --version`); update with `aspire update --self` |
| Git | LF line endings (see `.gitattributes`) |

Optional: VS Code with C# Dev Kit and recommended extensions from `.vscode/extensions.json`.

## First-time setup

```bash
git clone <repo-url>
cd tenancy-hub
dotnet restore TenancyHub.slnx
```

Keep Aspire packages aligned with the CLI from the repository root:

```bash
aspire update --migrate
```

## Build and test

```bash
dotnet build TenancyHub.slnx
dotnet test TenancyHub.slnx
```

Release configuration (matches CI):

```bash
dotnet build TenancyHub.slnx --configuration Release
dotnet test TenancyHub.slnx --configuration Release --no-build
```

Build policy (warnings as errors, analyzers, XML docs): [build-quality.md](./build-quality.md).

## Run the distributed app

From the repository root:

```bash
aspire run
```

Or target the AppHost explicitly:

```bash
aspire start --apphost src/TenancyHub.AppHost/TenancyHub.AppHost.csproj
```

Use `--isolated` when another Aspire session might be running or you are in a git worktree.

Stop:

```bash
aspire stop
```

Inspect resources and URLs:

```bash
aspire describe
aspire wait webfrontend --status healthy
```

VS Code tasks: **aspire start**, **build**, **test** (`.vscode/tasks.json`).

## Secrets and configuration

- **AppHost** has a `UserSecretsId` in `TenancyHub.AppHost.csproj` for local-only settings.
- Use `dotnet user-secrets` on the AppHost or individual service projects as integrations are added.
- Do **not** commit secrets, `.env` files with credentials, or connection strings in `appsettings*.json`.
- Prefer Aspire parameters and user secrets for local dev; Azure Key Vault / managed identity patterns for deployment (document when introduced).
- **Rebuild after loss or incident** (Entra, Postgres volumes, operator seed): [operations-rebuild-runbook.md](./operations-rebuild-runbook.md).

## Troubleshooting

| Issue | Action |
|-------|--------|
| SDK version mismatch | Install SDK from `global.json` or adjust `rollForward` |
| Stale Aspire packages | `aspire update` from repo root |
| Port / session conflicts | `aspire stop`, then `aspire start --isolated` |
| AppHost build vs run | `dotnet build` validates compile; `aspire start` runs orchestration and coordinated .NET project builds |

## Related docs

- [operations-rebuild-runbook.md](./operations-rebuild-runbook.md) — authoritative recovery after Entra/DB/environment loss
- [tech-stack.md](./tech-stack.md) — architecture and AppHost modelling
- [../AGENTS.md](../AGENTS.md) — agent entry point
