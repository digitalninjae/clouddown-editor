# CloudDown.Editor

A drop-in, fully native Markdown editor control for .NET MAUI.

[![Build Main](https://github.com/digitalninjae/clouddown-editor/actions/workflows/build.yml/badge.svg?branch=main)](https://github.com/digitalninjae/clouddown-editor/actions/workflows/build.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
![Status: pre-1.0](https://img.shields.io/badge/status-pre--1.0%20·%20in%20development-orange)

> 🚧 **Status: pre-1.0, under active development.** The Markdown editor control is **not yet
> implemented**. Today the repository contains the UI-free core (Markdown service, models, theme
> model), a working test harness, and the project's design documentation — but no editor control,
> toolbar, or platform handlers yet. The API shown under [Planned usage](#planned-usage) is the
> **target** design and will change. Track progress in the **[roadmap](docs/roadmap.md)**.

## Overview

CloudDown.Editor is a reusable Markdown editor control for .NET MAUI, built on each platform's
**native text controls — no WebView** — for a fast, accessible, native editing experience. It is
being built to power the CloudDown app, but is designed as a standalone package any MAUI app can
adopt.

For the full problem statement, goals, scope, and non-goals, see the
**[Vision & Scope](docs/vision.md)**.

## Planned features

The target product (see the [roadmap](docs/roadmap.md) for sequencing):

- **Three editing modes** — _Writer_ (distraction-free, syntax hidden), _Editor_ (full syntax with
  highlighting), and _Split_ (side-by-side editing + live preview).
- **Native on every platform** — Android, iOS, Windows, and macOS (Mac Catalyst), each via its
  native text control.
- **Formatting toolbar** — headers, bold/italic/strikethrough, lists, links, code, blockquotes.
- **MVVM-friendly API** — bindable properties and commands; no business logic in code-behind.
- **Theming** — light / dark / system, with customizable syntax colors.
- **Host-driven decoration hooks** — an API for the host app to decorate text ranges (e.g.
  spell-check squiggles); the library provides the hook, the host provides the feature.

## Project status

What exists in the repository today:

- ✅ **UI-free core** (`CloudDown.Editor.Core`, `net10.0`) — `MarkdownService` (Markdig-backed
  HTML rendering + selection formatting), the `EditorMode` / `EditorThemeMode` / `MarkdownFormat`
  enums, and the `EditorTheme` / `SyntaxColors` theme model.
- ✅ **Solution & build** — all projects build across the platform TFMs; the sample builds on
  Windows.
- ✅ **Test harness** — NUnit + Reqnroll (BDD) + AwesomeAssertions, running on `net10.0`.
- ✅ **Documentation** — vision, roadmap, architecture overview, conventions, and ADRs.
- ❌ **Not yet built** — the `MarkdownEditor` control, `FormattingToolbar`, the native platform
  handlers, and theme/decoration rendering.

The **[roadmap](docs/roadmap.md)** breaks the path to v1.0 into phases (P0–P7) with explicit
deliverables and "done when" criteria.

## Supported platforms

Android, iOS, Windows, and macOS (via Mac Catalyst) — the officially supported .NET MAUI heads.
Native handlers for each are in progress (roadmap P2–P3). Target minimum OS versions are tracked
in the [vision](docs/vision.md#8-constraints).

## Building from source

Prerequisites: the **.NET 10 SDK** and the **.NET MAUI workload** (`dotnet workload install maui`).

```bash
git clone https://github.com/digitalninjae/clouddown-editor.git
cd clouddown-editor

# Build everything (platform TFMs build on a machine with the MAUI workload)
dotnet build CloudDown.Editor.slnx

# Run the unit/behavior tests (Core is net10.0 — no emulator or device needed)
dotnet test tests/CloudDown.Editor.Tests/CloudDown.Editor.Tests.csproj
```

The sample app lives in [`src/CloudDown.Editor.Sample`](src/CloudDown.Editor.Sample) and
demonstrates wiring the library into a MAUI app via `ConfigureCloudDownEditor()`.

## Planned usage

> ⚠️ **Target API — not yet available.** This shows the intended shape of the library so the
> direction is clear; it does not work today and may change before 1.0.

Register the editor in your MAUI app's `MauiProgram` (the host-builder extension already exists):

```csharp
using CloudDown.Editor;

builder
    .UseMauiApp<App>()
    .ConfigureCloudDownEditor();
```

Then use the control in XAML (planned):

```xml
<clouddown:MarkdownEditor
    Content="{Binding MarkdownContent}"
    Mode="Writer"
    ThemeMode="Auto" />
```

## Documentation

- [Vision & Scope](docs/vision.md) — why this exists, who it's for, what's in and out of scope.
- [Roadmap](docs/roadmap.md) — the phased delivery plan to v1.0 and beyond.
- [Architecture overview](docs/architecture/overview.md) — projects, dependency rules, the handler model.
- [Architecture Decision Records](docs/architecture/adr/) — the significant decisions and their rationale.
- [Conventions](docs/conventions.md) — documentation, formatting, code, git, and process conventions.
- [Contributing](CONTRIBUTING.md) — how to get set up and submit changes.

## Contributing

Contributions are welcome — see **[CONTRIBUTING.md](CONTRIBUTING.md)** for setup and guidelines,
and the [roadmap](docs/roadmap.md) for where help is most useful right now (the core editing
engine and the first platform handler). Please also read the
[Code of Conduct](CODE_OF_CONDUCT.md).

## Security

To report a vulnerability, please follow the process in [SECURITY.md](SECURITY.md) — do not open a
public issue for security problems.

## License

MIT — see [LICENSE](LICENSE).

## Credits

**Author:** Brian Cupples ([@digitalninjae](https://github.com/digitalninjae))

**Inspired by:** [Typora](https://typora.io/), [Bear](https://bear.app/), and
[iA Writer](https://ia.net/writer).

**Built with:** [.NET MAUI](https://github.com/dotnet/maui),
[Markdig](https://github.com/xoofx/markdig), and
[CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet).
