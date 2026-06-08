# CloudDown.Editor — Vision & Scope

| | |
|---|---|
| **Status** | Draft |
| **Owner** | Brian Cupples |
| **Last updated** | 2026-06-08 |
| **Related** | [Roadmap](roadmap.md) · [Architecture overview](architecture/overview.md) · [ADRs](architecture/adr/) |

> This is the top-level document for the project. It defines *why* CloudDown.Editor
> exists, *who* it is for, and *what is in and out of scope*. Everything else — the
> roadmap, requirements, architecture decisions, and feature specs — derives from
> and must stay consistent with this document.

---

## 1. Problem Statement

.NET MAUI has no first-class, reusable Markdown **editing** control. Apps that need
Markdown editing today must either:

- embed a **WebView** + JavaScript editor (heavy, non-native feel, poor accessibility,
  awkward interop), or
- hand-roll a text control per platform (duplicated effort, inconsistent behavior).

There is no drop-in NuGet package that gives a MAUI app a high-quality, native Markdown
editor with live preview across all four platforms.

## 2. Vision Statement

**CloudDown.Editor is a drop-in, fully native Markdown editor control for .NET MAUI —
the editing experience of Typora or iA Writer, delivered as a NuGet package any MAUI app
can add in minutes.**

## 3. Target Users

| Persona | Description | Primary need |
|---|---|---|
| **App developer (integrator)** | A .NET MAUI developer building an app that needs Markdown editing. The primary customer of the library. | A reliable, well-documented, drop-in control with a clean bindable API and MVVM support. |
| **End user (of the host app)** | The person actually typing Markdown inside an app that embeds the control. | A fast, native, accessible writing experience across modes. |
| **CloudDown app** | The first-party consumer; this library is extracted from it. | Feature-complete editor it can depend on without reinventing editing. |

## 4. Goals & Objectives

1. **Native everywhere** — use each platform's native text control; no WebView.
2. **Three editing modes** — Writer (distraction-free, syntax hidden), Editor (syntax
   visible with highlighting), Split (side-by-side edit + preview).
3. **Drop-in integration** — one host-builder call plus a XAML control; sensible defaults.
4. **Performance** — real-time highlighting on small/medium docs; viable on large docs
   via virtualization and incremental parsing.
5. **Accessibility** — full screen-reader / platform assistive-technology support.
6. **Testable & reliable** — UI-free core logic with strong automated coverage;
   never corrupt or lose user content.

## 5. Scope

### In scope
- The `MarkdownEditor` control and its three modes.
- The `FormattingToolbar` control.
- Markdown processing (CommonMark + GitHub Flavored Markdown) via Markdig.
- Native platform handlers for Android, iOS, Windows, macOS (Mac Catalyst).
- A public, bindable, MVVM-friendly API and an extensible theming surface.
- A sample app and automated test suite demonstrating usage.

### Out of scope (non-goals)
- **Cloud storage / sync** — the responsibility of the consuming app (e.g. CloudDown).
- **File management** (open/save/browse) — the library handles editor controls only.
- **A full document/word-processor** — this is a Markdown editor, not a rich-text suite.

> **Future candidates (not non-goals):** real-time collaboration is *not* ruled out — it
> is deferred to the [roadmap](roadmap.md) as a future candidate rather than a v1/v2 commitment.

## 6. Guiding Principles

- **Native over convenient** — prefer platform-native controls even when a cross-platform
  shortcut exists. (See [ADR 0001](architecture/adr/0001-native-controls-no-webview.md).)
- **Library, not application** — no concerns that belong to the host app leak in.
- **Testable core** — platform-independent logic lives behind UI-free services so it can
  be unit-tested on a plain .NET host.
- **Stable public API** — avoid breaking changes in minor versions; the integrator's
  trust is the product.
- **Performance is a feature** — measured against document size, not assumed.

## 7. Success Criteria

> **Note:** The quantitative targets below are **provisional** — initial figures to design
> against, to be validated and refined once we have real benchmarks and integration feedback.

- A MAUI app can add the package and have a working editor in **≤ 10 minutes** *(provisional)*
  following the README quick-start.
- All three modes function on **all four platforms**.
- Real-time highlighting with **no perceptible lag** on documents < 1,000 lines;
  **< 100 ms** update latency at 1,000–10,000 lines *(provisional, to be benchmarked)*.
- Public API documented with XML doc comments and a published API reference.
- Automated tests cover Markdown processing and editor behaviors; CI is green.
- Adopted by the CloudDown app as its editor.

## 8. Constraints

- **Minimum platform versions**: Android 8.0, iOS 13.0, Windows 10 (1809), macOS 11
  (via Mac Catalyst).
- **Dependencies kept minimal** to control NuGet package size (core: MAUI, Markdig,
  CommunityToolkit.Mvvm).
- **No breaking changes in minor releases** (semantic versioning).
- **MVVM architecture**; no business logic in code-behind.

## 9. Key Risks

| Risk | Impact | Mitigation |
|---|---|---|
| Native rich-text rendering differs per platform | Inconsistent UX | Shared behavior specs (Reqnroll) + per-platform handler tests |
| Large-document performance | Lag, poor UX | Incremental parsing, virtualization, debouncing |
| Cursor/selection handling in highlighted text | Data/UX bugs | Behavior specs + manual device testing |
| iOS/Mac build & signing require Apple hardware | CI/release friction | Library builds on Windows; app packaging deferred to a macOS runner |

## 10. Stakeholders

- **Maintainer / Author**: Brian Cupples
- **Primary consumer**: CloudDown application
- **Community**: open-source contributors (MIT licensed)
