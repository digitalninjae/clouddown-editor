# Contributing to CloudDown.Editor

Thanks for your interest in contributing! This guide covers how to build the project, the
workflow we follow, and where the conventions live. By participating you agree to abide by our
[Code of Conduct](CODE_OF_CONDUCT.md).

## Ways to contribute

- Report bugs and request features via [GitHub Issues](https://github.com/digitalninjae/clouddown-editor/issues).
- Improve documentation.
- Submit code via pull requests (see the workflow below).
- For **security** issues, do **not** open a public issue — see [SECURITY.md](SECURITY.md).

## Prerequisites

- **.NET 10 SDK** (10.0.x).
- **.NET MAUI workloads** — install with `dotnet workload restore` (or `dotnet workload install maui android ios maccatalyst`).
- **Node.js 18+** — only needed if you edit Markdown (used to run the docs formatter).
- The **Windows** target framework only builds on Windows; producing signed iOS/macOS app
  packages requires a Mac. The library and its tests build on any of the supported OSes.

## Building and testing

```bash
# Restore and build the library across its target frameworks
dotnet build src/CloudDown.Editor/CloudDown.Editor.csproj

# Run the test suite (net10.0 — no emulator/device required)
dotnet test tests/CloudDown.Editor.Tests/CloudDown.Editor.Tests.csproj
```

The cross-platform logic lives in `CloudDown.Editor.Core` (`net10.0`, no MAUI dependency) and
is tested there, so the test suite runs fast on a plain .NET host. See the
[architecture overview](docs/architecture/overview.md) for the project layout.

## Branching and pull requests

- **Never commit directly to `main` or `dev`.** Branch off `dev` for every change.
- Name branches `type/short-description` using `feature/`, `fix/`, or `improvement/`
  (e.g. `feature/markdown-editor-control`, `fix/cursor-jump-android`).
- Open a pull request back into `dev`. Keep PRs focused and describe the change.
- Ensure the build, tests, and Markdown format check pass before requesting review.

## Commit messages

We loosely follow [Conventional Commits](https://www.conventionalcommits.org/): a
`type: summary` subject line, where _type_ is one of `feat`, `fix`, `docs`, `refactor`,
`test`, `build`, `chore`, etc. Example:

```
docs: add contributing guide
```

## Coding and documentation conventions

All project conventions — code style, Markdown formatting, diagrams, and process — live in
[docs/conventions.md](docs/conventions.md). Highlights:

- **Code style / MVVM**: see [docs/conventions.md](docs/conventions.md) and `CLAUDE.md`.
- **Architecture boundary**: UI-free logic goes in `CloudDown.Editor.Core`; UI goes in
  `CloudDown.Editor` ([ADR-0003](docs/architecture/adr/0003-separate-core-project.md)).
- **Tests**: NUnit + Reqnroll (BDD) + AwesomeAssertions, a BDD-led hybrid
  ([ADR-0002](docs/architecture/adr/0002-test-stack.md)). Add behavior specs as Reqnroll
  `.feature` scenarios and granular cases as NUnit data-driven tests.

### Markdown formatting

Markdown is formatted with **Prettier** and checked in CI. Before committing doc changes:

```bash
npm install      # first time only
npm run format   # auto-format Markdown (aligns tables, etc.)
npm run format:check   # verify (this is what CI runs)
```

Use **Mermaid** for diagrams (never ASCII art).

### Editor setup

**Prettier is the canonical Markdown formatter.** Its table style (aligned cells, `| --- |`
separators) is intentional and not configurable, so don't let another tool fight it.

- **Rider / other JetBrains IDEs**: Rider has **two separate** Markdown systems — _inspections_
  and _formatting_ — and the table warning comes from the **inspection**, not the formatter.
  - **To silence the warning** (the reliable fix): disable the **"Incorrect table formatting"**
    inspection (id `MarkdownIncorrectTableFormatting`) at
    Settings → Editor → Inspections → Markdown, or `Alt+Enter` on the warning → Configure
    inspection severity → "Do not show". Rider stores this in the gitignored `.idea/` folder, so
    each contributor sets it locally (it can't be committed). Enabling Prettier does **not**
    silence this inspection — they are independent.
  - _Optional convenience_: enable the Prettier integration so Rider auto-formats Markdown on
    save (Settings → Languages & Frameworks → JavaScript → Prettier; point it at the repo's
    `node_modules/prettier` and add `md` to "Run for files"). This only affects formatting.
- **VS Code**: install the Prettier extension and set it as the default formatter for Markdown.

## Architecture Decision Records (ADRs)

Significant decisions are recorded as ADRs under
[docs/architecture/adr/](docs/architecture/adr/). A new ADR starts as `Proposed` and is only
marked `Accepted` after the maintainer reviews and approves it. If you propose a notable
architectural change, add an ADR describing the context, options, and trade-offs.

## License

By contributing, you agree that your contributions will be licensed under the project's
[MIT License](LICENSE).
