# Contributing to Letoryn

Thank you for your interest in **Letoryn for Microsoft 365**. This document explains how to propose changes.

## Before you start

- Read [README.md](README.md) for product context and [AGENTS.md](AGENTS.md) for engineering conventions.
- Follow [docs/local-development.md](docs/local-development.md) to build, test, and run the Aspire app locally.
- Large or cross-cutting product work should follow Spec Kit under `specs/` (see [platform roadmap](specs/letoryn-platform/roadmap.md)).

## UK lettings domain expertise

The maintainer is **actively looking for a UK lettings domain expert to partner** on Vision-phase roadmap work (tenancies, compliance programmes, finance, complaints, and similar). Catalogue and Operations phases are scoped to be buildable without that depth.

If you have domain expertise or want to co-develop those areas, open a [GitHub issue](https://github.com/markheydon/letoryn-m365/issues/new) with a short description of your background and what you would like to contribute. Use title prefix `Partnership:` so it is easy to find. For security-sensitive contact, see [.github/SECURITY.md](.github/SECURITY.md).

## Pull requests

Open pull requests against `main` using the process in [docs/pull-request-policy.md](docs/pull-request-policy.md):

- UK English, template headings from [.github/pull_request_template.md](.github/pull_request_template.md).
- At least one `type/` and one `priority/` label, plus `status/in-review` and `area/*` when known.
- Set the GitHub milestone from the roadmap row (**Catalogue**, **Operations**, or **Vision**); see [docs/milestone-strategy.md](docs/milestone-strategy.md).
- Run `dotnet format Letoryn.slnx`, `dotnet build Letoryn.slnx`, and `dotnet test Letoryn.slnx` before requesting review.

## Code of conduct

This project follows [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md). By participating, you agree to uphold it.

## Security

Do not open public issues for security vulnerabilities. See [.github/SECURITY.md](.github/SECURITY.md).

## Trademark

The name **Letoryn** and any official logos are not licensed under the MIT license. Forks and unofficial deployments must not imply they are the official Letoryn hosted service.
