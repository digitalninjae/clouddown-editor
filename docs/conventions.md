# CloudDown.Editor — Project Conventions

|                  |               |
| ---------------- | ------------- |
| **Status**       | Living        |
| **Owner**        | Brian Cupples |
| **Last updated** | 2026-06-08    |

> The single home for this project's conventions — documentation, formatting, code, git, and
> process. [CLAUDE.md](../CLAUDE.md) references this file. When we agree on a new convention,
> record it **here**. For the _why_ behind architectural choices, see the
> [ADRs](architecture/adr/).

---

## Documentation

- All project docs live in `docs/`.
- Every doc and ADR opens with a **metadata table** whose **header row is left blank**, holding
  fields such as Status, Owner/Deciders, Last updated/Date, Related/Note. The table form keeps
  each field on its own rendered line (consecutive `**bold:**` lines otherwise collapse into a
  single paragraph).
- **Register new docs in the solution.** When you add a doc under `docs/` (or a root-level `.md`
  meta file such as a new community-health file), add it as a solution item under the
  `Documentation` folder in `CloudDown.Editor.slnx` so it shows up in the solution view. The
  `.slnx` format lists files individually (no folder glob), so each new file needs its own
  `<File Path="…" />` entry; nest it under the matching virtual subfolder (e.g. `Architecture`,
  `ADR`).

## Markdown formatting

### Tables

- **Align the column dividers in the source.** Pad every cell with trailing spaces so the `|`
  separators line up vertically down the whole table, and the separator row's dashes fill each
  column's width. Aligned source is far easier to read and to diff than ragged source.
- Leave the **metadata** table's header row blank (see [Documentation](#documentation)).
- **Prettier is the canonical Markdown formatter** for this repo (`npm run format`), and CI
  enforces it (`npm run format:check`). Its table style — padded cells and `| --- |` separators —
  is intentional and **not configurable**. Don't fight it with another tool: if your IDE flags
  Prettier's tables (e.g. Rider's Markdown table inspection, which prefers compact `|---|`),
  disable that inspection. See [CONTRIBUTING.md](../CONTRIBUTING.md) → _Editor setup_.

```markdown
<!-- Avoid: ragged source -->
| Dimension | Assessment |
|---|---|
| Complexity | Low |
| Test speed | Fast — plain .NET host |

<!-- Prefer: aligned source -->
| Dimension  | Assessment             |
|------------|------------------------|
| Complexity | Low                    |
| Test speed | Fast — plain .NET host |
```

### Diagrams

- Use **Mermaid** fenced code blocks (` ```mermaid `) for all diagrams.
- **Never use ASCII art** — it does not align reliably across fonts and renderers.
- **Validate** that a diagram renders before committing it.

## Code

- **C# style** and **MVVM**: see the _Coding Guidelines_ and _MVVM Pattern_ sections in
  [CLAUDE.md](../CLAUDE.md).
- **Architecture boundary**: UI-free logic lives in `CloudDown.Editor.Core`; UI lives in
  `CloudDown.Editor` ([ADR-0003](architecture/adr/0003-separate-core-project.md)).

## Git & process

- **Branch per change** (`feature/`, `fix/`, `improvement/`), merged via PR — see CLAUDE.md.
- **ADRs** stay `Proposed` until the maintainer reviews and approves — see CLAUDE.md and the
  [ADR log](architecture/adr/README.md).
- **Testing**: NUnit + Reqnroll + AwesomeAssertions, BDD-led hybrid
  ([ADR-0002](architecture/adr/0002-test-stack.md)).

## Dependencies & repo

- **Central Package Management**: declare versions only in `Directory.Packages.props`; project
  `PackageReference`s carry no inline version
  ([ADR-0004](architecture/adr/0004-central-package-management.md)).
- **Line endings** are normalized to LF via `.gitattributes`.
