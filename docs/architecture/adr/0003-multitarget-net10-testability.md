# ADR-0003: Multi-target `net10.0` alongside platform TFMs for testability

**Status:** Accepted
**Date:** 2026-06-08
**Deciders:** Brian Cupples
**Note:** Documented retroactively.

## Context

The `mauilib` template produces a library targeting only platform-specific TFMs
(`net10.0-android;net10.0-ios;net10.0-maccatalyst`, plus `net10.0-windows…` on Windows).
A standard NUnit test project targets plain **`net10.0`**, which **cannot reference a
library that has no `net10.0`-compatible TFM** — there is no compatible asset to resolve.

This blocks fast, host-based unit testing of the library's cross-platform logic (Markdown
processing, formatting transforms, view-model logic), which is exactly the logic our
[test strategy](0002-test-stack.md) leans on most. We want to run those tests on a plain
.NET host (fast, no emulator/device), not only via slow platform/device test runs.

## Decision

Add a plain **`net10.0`** target to the library *alongside* the platform TFMs:

```
<TargetFrameworks>net10.0;net10.0-android;net10.0-ios;net10.0-maccatalyst</TargetFrameworks>
<!-- + net10.0-windows10.0.19041.0 on Windows -->
```

Keep platform-independent logic in **UI-free services** (e.g. `MarkdownService`) that compile
on the `net10.0` target; isolate platform code behind `#if` directives / handler classes so it
is excluded from that target. This is the same pattern the .NET MAUI Community Toolkit uses.

## Options Considered

### Option A: Platform-only TFMs; test via platform/device runners
| Dimension | Assessment |
|-----------|------------|
| Complexity | Low (no csproj change) |
| Test speed | Slow — emulator/device/platform host required |
| CI friction | High — needs platform runners for plain logic tests |

**Pros:** No project changes.
**Cons:** Slow feedback; can't unit-test logic on a plain host; heavier CI.

### Option B: Add a `net10.0` target to the library (chosen)
| Dimension | Assessment |
|-----------|------------|
| Complexity | Low — one TFM added; standard pattern |
| Test speed | Fast — plain .NET host |
| Constraint | Testable logic must stay UI-free to compile on `net10.0` |

**Pros:** Fast host-based unit tests; standard, well-understood; minimal change.
**Cons:** Imposes discipline (logic must be UI-free on that target); the `net10.0` build is a
shippable asset consumers could resolve, so its surface should be deliberate.

### Option C: Separate `CloudDown.Editor.Core` (net10.0) + MAUI library
| Dimension | Assessment |
|-----------|------------|
| Complexity | Higher — extra project, reference wiring, packaging |
| Separation | Strongest — compiler-enforced UI/logic split |

**Pros:** Hard boundary between UI-free core and MAUI control layer.
**Cons:** More projects and packaging overhead than warranted at this stage.

## Trade-off Analysis

Option A's simplicity is outweighed by the daily cost of slow, platform-bound tests for
plain logic. Option C's stronger separation isn't yet justified by the size of the core.
Option B delivers the key benefit — fast host-based testing — with a one-line TFM change and
an accepted convention (keep logic UI-free). If the UI-free core grows large or its
boundary blurs, C becomes the natural next step.

## Consequences

- **Easier:** unit-testing cross-platform logic on a plain host; fast CI without emulators.
- **Harder:** contributors must keep testable logic free of platform/UI types so it compiles
  on `net10.0`; the `net10.0` asset is part of the package and should be curated.
- **Revisit if:** UI-free logic grows enough to warrant extracting a dedicated core project
  (Option C).

## Action Items

1. [x] Add `net10.0` to the library `TargetFrameworks`.
2. [x] Keep cross-platform logic in UI-free services; test project references `net10.0`.
3. [ ] Note the "logic stays UI-free" convention in the contributing/testing guide.
