# ADR-0005: Core may depend on Microsoft.Maui.Graphics (primitives only)

|              |                                           |
| ------------ | ----------------------------------------- |
| **Status**   | Accepted                                  |
| **Date**     | 2026-06-09                                |
| **Deciders** | Brian Cupples                             |
| **Refines**  | [ADR-0003](0003-separate-core-project.md) |

## Context

[ADR-0003](0003-separate-core-project.md) split the library into a UI-free `CloudDown.Editor.Core`
(`net10.0`) and the MAUI control library, stating Core "has no reference to
`Microsoft.Maui.Controls`" so the UI/logic boundary is compiler-enforced.

Building out the theming model surfaced a question that decision did not explicitly settle. We
are introducing an `EditorTheme` data type (and `SyntaxColors`) so a host can define the visual
theme the editor renders with. A theme is fundamentally a description of **colors**, and the
natural type for a color is `Microsoft.Maui.Graphics.Color`. The same need will recur for the
planned decoration/annotation model (highlight colors, underline styles).

The tension: ADR-0003's headline ("no MAUI dependency") could be read as forbidding _any_
`Microsoft.Maui.*` package in Core. But the **intent** of ADR-0003 is to keep **UI** —
controls, handlers, rendering — out of the logic layer, so the layer stays headlessly testable
and can't accidentally reach into the view tree.

Key fact: **`Microsoft.Maui.Graphics` is a standalone package, not part of
`Microsoft.Maui.Controls`.** It provides drawing primitives (`Color`, `PointF`, `RectF`, …),
targets plain `net10.0`, and requires **no MAUI workload** — it restores and builds in a pure
`net10.0` project (verified). It is the graphics equivalent of depending on `System.Drawing`;
it pulls in no UI, no controls, and no platform heads.

Alternatives for representing color without it: hex `string` tokens, or a hand-rolled Core
color struct. Both keep Core free of any `Microsoft.Maui.*` reference, at the cost of a
stringly-typed or bespoke API and conversion glue in every handler.

## Decision

**Permit `Microsoft.Maui.Graphics` (graphics primitives only) as a Core dependency.** Refine the
ADR-0003 boundary from "no `Microsoft.Maui.*`" to its actual intent:

> Core must not reference **`Microsoft.Maui.Controls`** or any UI/rendering type. Graphics
> _primitives_ (`Microsoft.Maui.Graphics`: `Color`, geometry, etc.) are allowed as data.

`EditorTheme` and `SyntaxColors` therefore live in Core and expose `Color`-typed properties. The
compiler-enforced guarantee that matters — **Core cannot see `Microsoft.Maui.Controls`** — is
unchanged; only the narrower, non-UI graphics package is admitted.

Versioning: `Microsoft.Maui.Graphics` is pinned to a literal version in
`Directory.Packages.props` (not `$(MauiVersion)`, which is only defined for MAUI-targeting
projects), kept aligned with the MAUI release ([ADR-0004](0004-central-package-management.md)).

## Options Considered

### Option A: Hex-string color tokens in Core

| Dimension   | Assessment                                      |
| ----------- | ----------------------------------------------- |
| Purity      | Maximal — zero `Microsoft.Maui.*` in Core       |
| Ergonomics  | Poor — stringly-typed; every handler parses hex |
| Correctness | Validation pushed to runtime (malformed hex)    |

### Option B: Hand-rolled Core color struct

| Dimension  | Assessment                                                  |
| ---------- | ----------------------------------------------------------- |
| Purity     | Maximal — zero `Microsoft.Maui.*` in Core                   |
| Ergonomics | Fair — typed, but a bespoke type handlers must convert from |
| Cost       | Reinvents `Color` and its conversions; ongoing maintenance  |

### Option C: Depend on `Microsoft.Maui.Graphics` (chosen)

| Dimension  | Assessment                                                             |
| ---------- | ---------------------------------------------------------------------- |
| Purity     | High — admits primitives only; **no `Microsoft.Maui.Controls`**, no UI |
| Ergonomics | Best — `Color`/`Colors`; handlers consume it directly, no conversion   |
| Cost       | One small, standard dependency; a literal version to keep in step      |

## Trade-off Analysis

A and B preserve a stricter "no `Microsoft.Maui.*` at all" reading, but they buy that purity with
a worse public API and conversion code in every handler — without protecting the property that
actually matters (no UI in the logic layer), which Option C also fully preserves. The dependency
admitted by C is a leaf, UI-free, workload-free graphics primitives package; it does not erode
the compiler-enforced wall against `Microsoft.Maui.Controls`. The ergonomic and consistency gains
(one `Color` type across Core data and platform handlers) outweigh a strict-but-symbolic purity.

## Consequences

- **Easier:** theme and decoration data use a real `Color` type; no per-handler color conversion;
  a single color vocabulary across the codebase.
- **Harder:** Core is no longer literally free of every `Microsoft.Maui.*` package — the boundary
  is now "no `Microsoft.Maui.Controls`/UI," which must be stated precisely (done here and in the
  Core csproj comment). A literal `Microsoft.Maui.Graphics` version must be kept aligned with MAUI.
- **Revisit if:** Core ever needs to ship truly framework-agnostic (e.g. consumed outside the MAUI
  ecosystem) — at which point a bespoke color type (Option B) would be reconsidered.

## Action Items

1. [x] Add `Microsoft.Maui.Graphics` to central package management (pinned literal) and reference it from Core.
2. [x] Implement `EditorTheme` / `SyntaxColors` in Core using `Color`.
3. [x] Update the Core csproj comment and the architecture overview to state the refined boundary.
4. [ ] Apply the same "graphics primitives are allowed" rule to the planned decoration/annotation model.
