# Build quality

MSBuild and analyzer settings applied solution-wide via `Directory.Build.props` (and `.editorconfig` for style).

## Warnings as errors

`<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` applies to **every** project. Any compiler or analyzer warning fails the build. Fix warnings; do not disable rules locally without team agreement.

## Code style in build

`<EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>` runs EditorConfig-based .NET analyzers during `dotnet build`. Style rules live in `.editorconfig` at the repository root.

## Nullable reference types

`<Nullable>enable</Nullable>` is enabled for all projects. Treat nullability warnings seriously: they are errors under WAE.

## XML documentation

| Setting | Effect |
|---------|--------|
| `<GenerateDocumentationFile>true</GenerateDocumentationFile>` | Emits `.xml` doc files for assemblies |
| CS1591 suppressed for **Exe** / **WinExe** | AppHost, Web, and ApiService are not required to have `///` on every public member |
| CS1591 suppressed for **test projects** | Test assemblies are not required to document test types |
| CS1591 **not** suppressed for other class libraries | `ServiceDefaults`, `Letoryn.Domain`, and `Letoryn.Infrastructure` must document public API or the build fails (see `.editorconfig`) |

When adding a new **class library** under `src/`, assume XML docs are required on public types and members.

## Central packages

NuGet versions are pinned in `Directory.Packages.props`. Add new package versions there; reference packages in projects without a `Version` attribute.

## CI

GitHub Actions runs Release **lint** (`dotnet format --verify-no-changes`), **build**, and **test** on push and pull requests (see `.github/workflows/ci.yml`).

Fix local formatting before push:

```bash
dotnet format Letoryn.slnx
```
