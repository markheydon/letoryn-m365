# Contributing to Letoryn

Thank you for your interest in **Letoryn for Microsoft 365**. This document explains how to propose changes.

## Before you start

- Read [README.md](README.md) for product context and [AGENTS.md](AGENTS.md) for engineering conventions.
- Follow [docs/local-development.md](docs/local-development.md) to build, test, and run the Aspire app locally.
- Large or cross-cutting product work should follow Spec Kit under `specs/` (see [platform roadmap](specs/letoryn-platform/roadmap.md)).

## Pull requests

Open pull requests against `main` using the process in [docs/pull-request-policy.md](docs/pull-request-policy.md):

- UK English, template headings from [.github/pull_request_template.md](.github/pull_request_template.md).
- At least one `type/` and one `priority/` label, plus `status/in-review` and `area/*` when known.
- Run `dotnet format Letoryn.slnx`, `dotnet build Letoryn.slnx`, and `dotnet test Letoryn.slnx` before requesting review.

## Code of conduct

This project follows [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md). By participating, you agree to uphold it.

## Security

Do not open public issues for security vulnerabilities. See [.github/SECURITY.md](.github/SECURITY.md).

## Trademark

The name **Letoryn** and any official logos are not licensed under the MIT license. Forks and unofficial deployments must not imply they are the official Letoryn hosted service.
