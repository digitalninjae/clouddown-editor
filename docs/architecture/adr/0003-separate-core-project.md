# ADR-0003: Separate `CloudDown.Editor.Core` project for testable, UI-free logic

|                |                                                                                                                                                                  |
| -------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Status**     | Accepted                                                                                                                                                         |
| **Date**       | 2026-06-08                                                                                                                                                       |
| **Deciders**   | Brian Cupples                                                                                                                                                    |
| **Refined by** | [ADR-0005](0005-core-may-depend-on-maui-graphics.md) — the "no MAUI" boundary is narrowed to "no `Microsoft.Maui.Controls`/UI"; graphics primitives are allowed. |

## Context

The `mauilib` template produces a library targeting only platform-specific TFMs
(`net10.0-android;net10.0-ios;net10.0-maccatalyst`, plus `net10.0-windows…` on Windows).
A standard NUnit test project targets plain **`net10.0`**, which **cannot reference a
library that has no `net10.0`-compatible TFM** — there is no compatible asset to resolve.

This blocks fast, host-based unit testing of the library's cross-platform logic (Markdown
processing, formatting transforms, view-model logic), which is exactly the logic our
[test strategy](0002-test-stack.md) leans on most. We want to run those tests on a plain
.NET host (fast, no emulator/device), not only via slow platform/device test runs.

A second, deeper force: the boundary between "UI-free logic" and "MAUI UI" should be hard to
violate. If both live in one project, that boundary is upheld only by convention and `#if`
hygiene — easy to erode over time.

## Decision

Split the library into **two projects**:

- **`CloudDown.Editor.Core`** — a plain **`net10.0`** library containing all UI-free logic
  (models, Markdown services, view-models). It depends only on **Markdig** and
  **CommunityToolkit.Mvvm** and **has no reference to `Microsoft.Maui.Controls`**.
- **`CloudDown.Editor`** — the MAUI library (controls, native handlers, host-builder
  extension), targeting the platform TFMs only. It **references Core**.

The test project references **Core directly** and needs no MAUI dependency. The MAUI library
no longer needs a non-platform target. Because Core carries no MAUI reference, the "no UI
types in the logic layer" rule is **enforced by the compiler**, not by convention.

**Packaging:** ship a **single** `CloudDown.Editor` NuGet package that **bundles** the Core
assembly, rather than publishing Core separately. Core remains an internal implementation
detail (`IsPackable=false`). (Implementation deferred until `dotnet pack` is wired up.)

## Options Considered

### Option A: Platform-only TFMs; test it via platform/device runners

| Dimension  | Assessment                                    |
| ---------- | --------------------------------------------- |
| Complexity | Low (no project change)                       |
| Test speed | Slow — emulator/device/platform host required |
| Boundary   | Convention only                               |

**Pros:** No project changes.
**Cons:** Slow feedback; can't unit-test logic on a plain host; heavier CI.

### Option B: Add a `net10.0` target to the single MAUI library

| Dimension  | Assessment                      |
| ---------- | ------------------------------- |
| Complexity | Low — one TFM added             |
| Test speed | Fast — plain .NET host          |
| Boundary   | Convention + `#if` hygiene only |

**Pros:** Fast host-based tests with a one-line change; the .NET MAUI Community Toolkit uses
this pattern.
**Cons:** The UI-free boundary is not compiler-enforced; the `net10.0` slice of the MAUI
library is a shippable asset whose surface must be curated; mixing UI and logic in one
project blurs responsibilities as it grows.

### Option C: Separate `CloudDown.Editor.Core` (net10.0) + MAUI library — **chosen**

| Dimension  | Assessment                                                 |
| ---------- | ---------------------------------------------------------- |
| Complexity | Higher — two projects, reference wiring, bundled packaging |
| Test speed | Fast — tests reference a pure `net10.0` library            |
| Boundary   | **Strongest — compiler-enforced** (Core cannot see MAUI)   |

**Pros:** Hard, compiler-enforced separation of logic from UI; tests pull no MAUI; clean,
minimal dependency graph for the logic layer; logic layer is independently reusable.
**Cons:** An extra project to maintain; single-package shipping requires an MSBuild step to
bundle the Core DLL (otherwise it would become a package dependency on an unpublished project).

## Trade-off Analysis

A and B both leave the UI/logic boundary to discipline; given how central testability and a
clean core are to this project, a **compiler-enforced** boundary is worth a modest amount of
extra structure. B's "one-line change" is cheaper today, but the cost it defers — an eroding
boundary and a curated non-platform slice inside the UI library — grows with the codebase.
C's main cost is bundled-packaging setup, a one-time, well-understood MSBuild step. The
expected volume of UI-free logic here (Markdown engine, formatting, undo/redo, the
decoration/annotation model, view-models) makes the separation pay for itself.

## Consequences

- **Easier:** unit-testing logic on a plain host; the logic layer can't accidentally depend on
  MAUI; the test project carries no MAUI; clearer "where does this go?" guidance.
- **Harder:** one more project to manage; producing a single NuGet package requires an MSBuild
  target to embed the Core DLL and suppress the transitive package dependency.
- **Revisit if:** there is a demand to consume Core headlessly — it could be promoted to its own
  published package (a non-breaking addition).

## Action Items

1. [x] Create `CloudDown.Editor.Core` (`net10.0`); move models and services into it.
2. [x] Reference Core from the MAUI library; drop the MAUI library's non-platform target.
3. [x] Point the test project at Core; confirm tests run with no MAUI dependency.
4. [ ] At `dotnet pack` time, bundle the Core DLL into the single `CloudDown.Editor` package
       (`PrivateAssets="all"` + `TargetsForTfmSpecificBuildOutput`).
5. [ ] Note the "logic lives in Core; UI lives in the MAUI library" rule in the contributing guide.
