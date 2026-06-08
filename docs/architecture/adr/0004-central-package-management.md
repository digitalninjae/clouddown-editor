# ADR-0004: Central Package Management for NuGet versions

**Status:** Accepted
**Date:** 2026-06-08
**Deciders:** Brian Cupples
**Note:** Documented retroactively.

## Context

The solution has three projects — the library, the sample app, and the test project — that
share several NuGet dependencies (MAUI, test packages, etc.). Without a single source of
truth for versions, the same package can drift to different versions across projects, causing
subtle build/runtime inconsistencies and noisy upgrades (every csproj edited on each bump).

## Decision

Enable **Central Package Management (CPM)** via a root `Directory.Packages.props` with
`ManagePackageVersionsCentrally=true`. All versions are declared once as `PackageVersion`
entries; project `PackageReference` items carry **no inline `Version`**. The MAUI version is
pinned through the SDK-provided `$(MauiVersion)` property.

## Options Considered

### Option A: Per-project versions on each `PackageReference` (default)
| Dimension | Assessment |
|-----------|------------|
| Complexity | Low (template default) |
| Drift risk | High — versions diverge across projects |
| Upgrade effort | High — edit every csproj per bump |

**Pros:** No extra files; familiar.
**Cons:** Version drift; tedious, error-prone upgrades; no single source of truth.

### Option B: Central Package Management (chosen)
| Dimension | Assessment |
|-----------|------------|
| Complexity | Low–Medium — one root file; version-less references |
| Drift risk | Eliminated — versions centralized |
| Upgrade effort | Low — change one line |

**Pros:** Single source of truth; no drift; clean diffs on upgrades; modern .NET standard.
**Cons:** Every package needs a `PackageVersion` entry; references must omit versions
(small ramp-up); occasional friction with properties like `$(MauiVersion)`.

### Option C: Shared MSBuild properties in `Directory.Build.props`
**Rejected:** approximates CPM with custom `$(…Version)` properties but without NuGet's
first-class support, validation, or transitive-pinning features. CPM is the purpose-built
mechanism.

## Trade-off Analysis

The default (A) is simplest only until the second project needs the same package; after that,
drift and upgrade toil grow. CPM (B) is the purpose-built, low-overhead solution and the
current .NET standard. Option C reinvents it with less tooling support. The minor ramp-up
(version-less references, one entry per package) is a one-time cost.

## Consequences

- **Easier:** consistent versions everywhere; one-line upgrades; clean, reviewable diffs.
- **Harder:** adding a package is two steps (a version-less `PackageReference` plus a
  `PackageVersion` entry); contributors must know not to add inline versions.
- **Revisit if:** the solution needs per-project version overrides at scale (CPM supports
  `VersionOverride`, but heavy use would signal a different need).

## Action Items

1. [x] Add root `Directory.Packages.props` with `ManagePackageVersionsCentrally=true`.
2. [x] Move all versions to `PackageVersion`; strip inline versions from project files.
3. [ ] Note the "no inline versions; add a `PackageVersion` entry" rule in the contributing guide.
