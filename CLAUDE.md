# CloudDown.Editor - AI Coding Assistant Guide

## Project Overview

**CloudDown.Editor** is a reusable .NET MAUI NuGet package providing a cross-platform Markdown editor control with three editing modes (Writer, Editor, Split). This library powers the CloudDown application but is designed for any MAUI app needing Markdown editing capabilities.

## Technology Stack

- **Framework**: .NET MAUI
- **Language**: C#
- **Markdown Processing**: Markdig
- **Architecture**: MVVM
- **Platforms**: Android, iOS, Windows, macOS
- **UI Design**: Material 3 Expressive (Android)

## Project Structure

```
CloudDown.Editor/
├── src/
│   ├── CloudDown.Editor.Core/          # UI-free logic (net10.0; no MAUI dependency) — see ADR-0003
│   │   ├── Models/                     # Data models, enums
│   │   ├── ViewModels/                 # MVVM view models (CommunityToolkit.Mvvm)
│   │   └── Services/                   # Markdown processing services (Markdig)
│   ├── CloudDown.Editor/               # MAUI control library (platform TFMs) — references Core
│   │   ├── Controls/                   # UI controls
│   │   │   ├── MarkdownEditor.cs      # Main editor control
│   │   │   └── FormattingToolbar.cs   # Formatting toolbar
│   │   └── Platforms/                  # Platform-specific implementations
│   │       ├── Android/               # Native Android TextView/handlers
│   │       ├── iOS/                   # Native UITextView/handlers
│   │       ├── Windows/               # Native RichEditBox/handlers
│   │       └── MacCatalyst/           # Native NSTextView/handlers
│   └── CloudDown.Editor.Sample/        # Sample MAUI app demonstrating usage
└── tests/
    └── CloudDown.Editor.Tests/         # Tests (net10.0; reference Core directly)
```

## Key Components

### MarkdownEditor Control

**Location**: `src/CloudDown.Editor/Controls/MarkdownEditor.cs`

Three editing modes:

- **Writer Mode**: Distraction-free writing (hides syntax)
- **Editor Mode**: Full syntax visible
- **Split Mode**: Side-by-side editing and preview

Platform-specific native implementations for optimal performance.

### FormattingToolbar Control

**Location**: `src/CloudDown.Editor/Controls/FormattingToolbar.cs`

Provides formatting shortcuts for:

- Headers (H1-H6)
- Bold, italic, strikethrough
- Lists (ordered, unordered)
- Links, images
- Code blocks, inline code
- Blockquotes

### Platform Handlers

**Location**: `src/CloudDown.Editor/Platforms/{Platform}/`

Each platform implements native text editing controls:

- Android: Custom TextView with Markdown syntax highlighting
- iOS: UITextView with attributed string rendering
- Windows: RichEditBox with syntax highlighting
- macOS: NSTextView with attributed string rendering

## Git Workflow

**All commits must be made on a dedicated work/feature/improvement branch — never commit
directly to `main` or `dev`.**

- Before starting any change, create a branch off the appropriate base (typically `dev`),
  e.g. `feature/markdown-editor-control`, `fix/cursor-jump-android`, `improvement/ci-matrix`.
- Use a `type/short-description` naming convention: `feature/`, `fix/`, or `improvement/`.
- Open a pull request to merge the branch back; do not push commits straight to `main`/`dev`.

## Architecture Decision Records (ADRs)

- ADRs live in `docs/architecture/adr/` in a MADR-style format; see that folder's `README.md`.
- **Do not mark an ADR `Accepted` until Brian has reviewed the actual document and explicitly
  stated it is approved.** Until then it stays `Proposed` — even if the underlying decision has
  been discussed or already implemented. Authoring or editing an ADR is **not** approval; only
  an explicit approval from the maintainer is.
- New decisions start at `Proposed`; the maintainer moves them to `Accepted`.

## Conventions

Project conventions (documentation, Markdown formatting, code, git, process) live in
[docs/conventions.md](docs/conventions.md) — that is the single source of truth; **record new
conventions there**. Key points:

- **Diagrams**: Mermaid fenced blocks (` ```mermaid `) only, never ASCII art; validate
  they render before committing.
- **Tables**: align the column dividers in the source — pad cells so the `|` separators line up
  vertically and the separator row's dashes fill each column.
- **Doc metadata**: each doc/ADR opens with a metadata table whose header row is left blank
  (the table form keeps each field on its own rendered line).
- **Solution items**: when creating a new documentation file (under `docs/` or a root-level
  `.md` meta file), add it to the `Documentation` folder in `CloudDown.Editor.slnx` as a
  `<File Path="…" />` entry — `.slnx` lists files individually, so each new doc must be
  registered so it appears in the solution view.

## Coding Guidelines

### C# Code Style

- Use PascalCase for public members, camelCase for private
- Prefer expression-bodied members where appropriate
- Use `var` only when type is obvious from right-hand side
- Async methods must end with `Async` suffix
- Null-conditional operators (`?.`, `??`) preferred over explicit null checks

### MVVM Pattern

- ViewModels inherit from `ObservableObject` (CommunityToolkit.Mvvm)
- Commands use `ICommand` or `IAsyncRelayCommand`
- Property changes use `OnPropertyChanged` or `[ObservableProperty]` attribute
- No business logic in code-behind

### Platform-Specific Code

```csharp
// Use conditional compilation for platform-specific code
#if ANDROID
    // Android-specific implementation
#elif IOS
    // iOS-specific implementation
#elif WINDOWS
    // Windows-specific implementation
#elif MACCATALYST
    // macOS-specific implementation
#endif
```

### Markdown Processing

- Use Markdig pipeline for all Markdown parsing
- Support CommonMark + GitHub Flavored Markdown extensions
- Handle syntax highlighting in platform-specific renderers
- Cache parsed Markdown to avoid redundant processing

## Development Priorities

### Performance

- Native controls for text editing (no cross-platform text boxes)
- Efficient Markdown parsing (cache AST, incremental updates)
- Lazy rendering for large documents
- Platform-specific optimizations

### Reliability

- Robust error handling for file operations
- Validate Markdown syntax without breaking user input
- Graceful degradation for unsupported syntax
- Preserve user content during crashes

### Testability

- Unit tests for all Markdown processing logic
- Platform-specific handler tests where possible
- MVVM pattern enables easy view model testing
- Integration tests via sample app

## Build Configuration

### Debug

- Enable detailed logging
- Include debug symbols
- Fast deployment for iterative testing

### Release

- Code optimization enabled
- Trim unused assemblies
- AOT compilation for supported platforms
- Signed NuGet package

## Dependencies

**Core**:

- .NET MAUI (latest stable)
- Markdig (latest stable)
- CommunityToolkit.Mvvm (latest stable)

**Testing** (hybrid strategy — see below):

- NUnit (test runner; data-driven `[TestCase]`/`[TestCaseSource]` for granular cases)
- Reqnroll (BDD/Gherkin `.feature` specs for editor behaviors; runs on NUnit via `Reqnroll.NUnit`)
- AwesomeAssertions (MIT fork of FluentAssertions) for readable assertions
- NSubstitute for mocking (when needed)

**Testing strategy**: BDD-led hybrid. Reqnroll `.feature` scenarios describe public editor
_behaviors_ (formatting, mode switching, toolbar, undo/redo) and double as living documentation;
NUnit data-driven tests cover the high-volume, low-level cases (e.g. Markdig parsing edge cases).
Cross-platform logic lives in the UI-free `CloudDown.Editor.Core` project (`net10.0`, no MAUI
dependency), so it is testable on a plain `net10.0` host and the test project references Core
directly with no MAUI dependency (see [ADR-0003](docs/architecture/adr/0003-separate-core-project.md)).

## Common Tasks

### Adding New Markdown Syntax Support

1. Update Markdig pipeline configuration
2. Implement platform-specific rendering (if needed)
3. Add formatting toolbar button (if applicable)
4. Update tests and sample app
5. Document in README

### Creating New Editor Feature

1. Add property/command to ViewModel
2. Update MarkdownEditor control API
3. Implement in platform handlers
4. Add UI controls (toolbar buttons, etc.)
5. Write unit tests
6. Update sample app to demonstrate

### Platform-Specific Bug Fix

1. Identify affected platform
2. Locate handler implementation
3. Fix within `#if` directive or handler class
4. Test on affected platform
5. Verify other platforms unaffected

## Testing Strategy

- **Unit tests**: Markdown parsing, view models, business logic
- **Platform tests**: Handler behavior, native control integration
- **Integration tests**: Full editing workflows via sample app
- **Manual testing**: Real devices for touch/pen input, accessibility

## Future Roadmap

### v1.1

- Table editing UI
- Image paste support
- Custom syntax extensions
- Export to PDF/HTML

### v2.0

- Plugin system
- Language Server Protocol support
- Advanced theming engine
- Split pane customization

## Common Patterns

### Creating Bindable Properties

```csharp
public static readonly BindableProperty TextProperty =
    BindableProperty.Create(
        nameof(Text),
        typeof(string),
        typeof(MarkdownEditor),
        string.Empty,
        propertyChanged: OnTextChanged);

public string Text
{
    get => (string)GetValue(TextProperty);
    set => SetValue(TextProperty, value);
}
```

### Platform-Specific Handler

```csharp
public partial class MarkdownEditorHandler
{
#if ANDROID
    protected override AndroidTextView CreatePlatformView() =>
        new MarkdownTextView(Context);
#elif IOS
    protected override UITextView CreatePlatformView() =>
        new MarkdownUITextView();
#endif
}
```

### Observable ViewModel

```csharp
public partial class EditorViewModel : ObservableObject
{
    [ObservableProperty]
    private string markdownText;

    [RelayCommand]
    private async Task FormatBoldAsync()
    {
        // Implementation
    }
}
```

## AI Assistant Expectations

When providing coding assistance:

1. **Always use C#** for this project
2. **Follow MVVM** - separate concerns properly
3. **Platform-aware** - consider cross-platform implications
4. **Performance-conscious** - native controls, efficient parsing
5. **Testable** - write code that can be unit tested
6. **Document** - XML comments for public APIs
7. **Consistent** - match existing code style

### Code Quality Checklist

- [ ] Follows C# naming conventions
- [ ] Uses appropriate MVVM pattern
- [ ] Platform-specific code properly isolated
- [ ] Error handling implemented
- [ ] XML documentation for public APIs
- [ ] Unit tests included (where applicable)
- [ ] Performance considerations addressed

## Questions to Ask Before Implementing

1. Does this belong in the library or the CloudDown app?
2. Is this cross-platform or platform-specific?
3. What's the performance impact on large documents?
4. How does this affect the public API?
5. What tests are needed?
6. Are there breaking changes to existing consumers?

## Known Constraints

- **No cloud storage** in this library (that's CloudDown app's job)
- **No file management** - library only handles editor controls
- **Minimum platform versions**: Android 8.0, iOS 13.0, Windows 10 1809
- **NuGet package size**: Keep dependencies minimal
- **Breaking changes**: Avoid in minor versions

## Session Management

### Purpose

Session summaries preserve context between coding sessions and prevent knowledge loss during long conversations. They enable continuity when starting fresh chats or before context window compaction.

### Session Summary Location

**Path**: `/.sessions/`

**Naming**: `YYYY-MM-DD-HHmm-session-summary.md`

**Example**: `2026-01-03-1430-session-summary.md`

### Starting a New Session

**ALWAYS start by reading the latest session summary:**

```bash
# Check for existing sessions
ls -lt /.sessions/*.md | head -1

# Read the most recent summary
view /.sessions/2026-01-03-1430-session-summary.md
```

This ensures continuity from the previous session and awareness of:

- What was accomplished
- Current blockers or issues
- Next steps planned
- Decisions made
- Open questions

### Creating Session Summaries

**When to create:**

- End of significant coding session (>30 minutes)
- Before context window fills (proactive compaction)
- After completing a major feature or milestone
- When switching between different project areas
- Before taking a break from the project (>1 day)

**What to include:**

```markdown
# Session Summary - [Date] [Time]

## Session Overview
- Duration: [X hours]
- Focus: [Primary goal/feature worked on]
- Status: [In Progress | Completed | Blocked]

## Completed Work
- [Specific task 1]
- [Specific task 2]
- [Code files created/modified]

## Key Decisions
- [Decision 1 and rationale]
- [Decision 2 and rationale]
- [Architecture choices]

## Current State
- **What's working**: [Features/components completed]
- **What's broken**: [Known issues, bugs]
- **What's in progress**: [Partially completed work]

## Technical Notes
- [Important implementation details]
- [Gotchas discovered]
- [Performance considerations]
- [Platform-specific quirks encountered]

## Blockers & Issues
- [Issue 1: description and impact]
- [Issue 2: description and impact]

## Next Steps
1. [Immediate next task - highest priority]
2. [Follow-up task]
3. [Future consideration]

## Open Questions
- [Question 1 needing resolution]
- [Question 2 for future discussion]

## Code Locations
- Modified: [List of files changed]
- Created: [List of new files]
- Deleted: [List of removed files]

## Dependencies Added/Changed
- [Package name]: [version] - [reason]

## Testing Status
- Unit tests: [passed/failed/not run]
- Platform tests: [which platforms tested]
- Known test gaps: [what needs testing]

## References
- [Links to docs consulted]
- [Stack Overflow solutions used]
- [GitHub issues referenced]
```

### Session Summary Best Practices

**Be specific**:

- ❌ "Worked on the editor"
- ✅ "Implemented MarkdownEditorHandler for Android with syntax highlighting using SpannableString"

**Include context**:

- Why decisions were made
- What alternatives were considered
- What didn't work and why

**Track mental state**:

- Current understanding of the codebase
- Areas of confusion or uncertainty
- Patterns emerging

**Future-proof**:

- Write for your future self (or another developer)
- Assume knowledge gaps after time away
- Document the "why" not just the "what"

### Example Session Summary

```markdown
# Session Summary - 2026-01-03 14:30

## Session Overview
- Duration: 2.5 hours
- Focus: Android platform handler for MarkdownEditor
- Status: In Progress (80% complete)

## Completed Work
- Created `MarkdownEditorHandler.Android.cs` in `/Platforms/Android/`
- Implemented custom `MarkdownTextView` extending `AppCompatEditText`
- Added syntax highlighting using `SpannableString` with `ForegroundColorSpan`
- Set up Markdown detection for headers, bold, italic, code blocks
- Basic text change event handling

## Key Decisions
- **Custom TextView over WebView**: Better performance, native feel, offline support
- **SpannableString for highlighting**: Real-time syntax coloring without re-parsing entire document
- **Incremental parsing**: Only re-highlight changed lines, not entire document

## Current State
- **What's working**:
  - Headers (H1-H6) highlight correctly
  - Bold/italic syntax detection
  - Basic text input and editing
- **What's broken**:
  - Link syntax highlighting incomplete (regex needs refinement)
  - Code block detection fails with nested backticks
  - Cursor position jumps when highlighting updates
- **What's in progress**:
  - Fixing cursor position preservation
  - Implementing list detection (ordered/unordered)

## Technical Notes
- Android `TextWatcher` fires before/during/after text changes - need all three
- `Selection.setSelection()` must be called AFTER `setText()` to preserve cursor
- Regex patterns compiled once and cached for performance
- Material 3 color tokens accessed via `MaterialColors.getColor()`

## Blockers & Issues
- **Cursor jumping**: When applying spans, cursor moves to end of text. Need to save/restore selection range.
- **Performance**: Large documents (>10KB) lag on input. May need debouncing or background parsing.

## Next Steps
1. Fix cursor position preservation (highest priority - breaks UX)
2. Add list syntax highlighting (bullets, numbers)
3. Implement link/image syntax detection
4. Add unit tests for regex patterns
5. Performance testing with large Markdown files

## Open Questions
- Should we use WorkManager for background parsing on large files?
- How to handle real-time collaboration conflicts (future feature)?
- Best approach for custom Markdown extensions?

## Code Locations
- Created:
  - `/Platforms/Android/Handlers/MarkdownEditorHandler.Android.cs`
  - `/Platforms/Android/Controls/MarkdownTextView.cs`
- Modified:
  - `/Controls/MarkdownEditor.cs` (added Android-specific properties)

## Dependencies Added/Changed
- None this session

## Testing Status
- Unit tests: Not yet written
- Platform tests: Manual testing on Pixel 7 (Android 14)
- Known test gaps: Need automated UI tests for text input

## References
- Android TextWatcher docs: https://developer.android.com/reference/android/text/TextWatcher
- SpannableString guide: https://developer.android.com/guide/topics/text/spans
- Material 3 colors: https://m3.material.io/styles/color/system/overview
```

### Using Session Summaries

**At session start:**

```bash
# Read the latest session
view /.sessions/$(ls -t /.sessions/*.md | head -1)
```

**Before context compaction:**

```bash
# Create summary to preserve current state
create_file /.sessions/2026-01-03-1645-session-summary.md "..."
```

**When resuming after break:**

- Read latest summary first
- Review "Current State" and "Next Steps"
- Check "Blockers & Issues" for resolved/new items
- Continue from "Next Steps" priority list

### Session Directory Structure

```
/.sessions/
├── 2026-01-03-1430-session-summary.md
├── 2026-01-03-1645-session-summary.md
├── 2026-01-04-0900-session-summary.md
└── README.md  # Explains session management system
```

### Maintenance

- **Keep recent sessions** (last 30 days)
- **Archive old sessions** (move to `/.sessions/archive/YYYY-MM/`)
- **Index major milestones** (create `/.sessions/MILESTONES.md` with links)

## License

MIT License - See LICENSE file for details

---

**Author**: Brian Cupples  
**Repository**: github.com/yourusername/clouddown-editor  
**NuGet**: CloudDown.Editor
