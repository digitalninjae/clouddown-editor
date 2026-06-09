# ADR-0001: Native platform text controls instead of a WebView

|  |  |
|---|---|
| **Status** | Accepted |
| **Date** | 2026-06-08 |
| **Deciders** | Brian Cupples |
| **Note** | Documented retroactively; this decision predates the formal ADR log. |

## Context

CloudDown.Editor must provide Markdown editing with live preview across Android, iOS,
Windows, and macOS (Mac Catalyst). The core technical choice is *what renders and edits
the text*. Two broad approaches exist:

- A **WebView** hosting a JavaScript Markdown editor (e.g., a CodeMirror/ProseMirror-style
  component), shared across platforms.
- **Native platform text controls** per platform, surfaced through MAUI handlers.

Forces at play: native look-and-feel, input/typing performance, accessibility (screen
readers, assistive tech), offline operation, IME/keyboard behavior, and the maintenance
cost of platform-specific code. As a *library* (not an app), the quality of the editing
experience is the product — integrators adopt it precisely so they don't have to build this.

## Decision

Use **native platform text controls**, wired through MAUI handlers, with **no WebView**:

- Android — `EditText` with `Span`s
- iOS — `UITextView` with `NSAttributedString`
- Windows — `RichEditBox`
- macOS — `NSTextView` with attributed strings

## Options Considered

### Option A: WebView + JavaScript editor
| Dimension     | Assessment                                                        |
|---------------|-------------------------------------------------------------------|
| Complexity    | Medium (one editor, but JS ↔ native interop)                      |
| Performance   | Poor for large docs; input latency, memory overhead               |
| Native feel   | Poor — non-native selection, IME, scrolling                       |
| Accessibility | Weak — DOM a11y inside a WebView is inconsistent across platforms |
| Maintenance   | Low per-platform code, but heavy JS/runtime surface               |

**Pros:** Maximum code reuse; rich existing JS editors.
**Cons:** Non-native UX; accessibility gaps; performance/memory cost; brittle interop; large footprint.

### Option B: Native platform controls (chosen)
| Dimension     | Assessment                                                   |
|---------------|--------------------------------------------------------------|
| Complexity    | High — per-platform handler implementations                  |
| Performance   | Excellent — platform-optimized text rendering                |
| Native feel   | Excellent — native selection, IME, scrolling, gestures       |
| Accessibility | Strong — inherits platform assistive technology              |
| Maintenance   | Higher — four implementations, behavior divergence to manage |

**Pros:** Best UX, performance, and accessibility; small footprint; truly native.
**Cons:** Most per-platform code; risk of behavioral divergence across platforms.

### Option C: Cross-platform MAUI `Editor`/`Entry`
**Rejected:** the built-in controls cannot do rich syntax highlighting / inline preview;
insufficient for Writer mode.

## Trade-off Analysis

The decisive forces are **accessibility, native feel, and performance** — all core to the
product's value proposition and all areas where WebView is structurally weak. We accept the
main cost of Option B (four platform handlers and the need to keep their behavior aligned)
because it buys exactly the qualities integrators cannot easily get otherwise. The
divergence risk is mitigated by a testing strategy (shared behavior specs and per-platform
handler tests — see [ADR-0002](0002-test-stack.md)).

## Consequences

- **Easier:** native UX, accessibility, performance, and a minimal dependency footprint.
- **Harder:** four native handler implementations to build and maintain; cross-platform
  behavior must be verified, not assumed.
- **Revisit if:** per-platform maintenance cost becomes unsustainable, or a platform's
  native control proves unable to meet a required feature.

## Action Items

1. [ ] Implement `MarkdownEditor` handlers per platform under `Platforms/{Platform}/`.
2. [ ] Define shared behavior specs so platforms stay aligned.
3. [ ] Add per-platform handler tests where feasible.
