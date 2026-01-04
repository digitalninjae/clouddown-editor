# CloudDown.Editor

A professional cross-platform Markdown editor control for .NET MAUI

[![NuGet](https://img.shields.io/nuget/v/CloudDown.Editor.svg)](https://www.nuget.org/packages/CloudDown.Editor/)
[![Build Main](https://github.com/digitalninjae/clouddown-editor/actions/workflows/build.yml/badge.svg?branch=main)](https://github.com/digitalninjae/clouddown-editor/actions/workflows/build.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## Overview

CloudDown.Editor is a reusable, drop-in Markdown editor control for .NET MAUI applications. It provides three distinct editing modes with fully native implementations for the best performance and user experience on each platform.

## Features

### Three Editing Modes

**Writer Mode** ✨
- Rich preview with inline editing
- Native implementation using platform-specific controls
- EditText with Spans (Android), UITextView with NSAttributedString (iOS), RichEditBox (Windows)
- See formatting as you type
- Click anywhere to edit
- Perfect for focused writing

**Editor Mode** 💻
- Plain text with syntax highlighting
- Full control over Markdown syntax
- Color-coded formatting markers
- Keyboard shortcuts
- Fast and efficient for technical users

**Split Mode** 📱
- Side-by-side editing and preview
- Synchronized scrolling
- Live preview updates
- Great for learning Markdown
- Best of both worlds

### Fully Native

- **No WebView** - Pure native controls on each platform
- **High Performance** - Optimized platform-specific rendering
- **Native Feel** - Uses standard platform UI components
- **Accessibility** - Full support for screen readers and platform assistive technologies

### Easy Integration

- Drop-in control for any MAUI app
- Customizable formatting toolbar
- Full MVVM support
- Extensive public API
- Bindable properties for all settings
- Event-driven architecture

## Installation

Install via NuGet Package Manager:

```bash
dotnet add package CloudDown.Editor
```

Or via Package Manager Console:

```powershell
Install-Package CloudDown.Editor
```

## Quick Start

### 1. Register the Handlers

Add CloudDown.Editor to your MAUI app configuration:

```csharp
// MauiProgram.cs
using CloudDown.Editor;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureCloudDownEditor(); // Register editor handlers
            
        return builder.Build();
    }
}
```

### 2. Add to Your XAML

```xml
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:clouddown="clr-namespace:CloudDown.Editor.Controls;assembly=CloudDown.Editor"
             x:Class="MyApp.EditorPage">
    
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>
        
        <!-- Optional formatting toolbar -->
        <clouddown:FormattingToolbar 
            Grid.Row="0"
            TargetEditor="{x:Reference Editor}" />
        
        <!-- The markdown editor -->
        <clouddown:MarkdownEditor 
            x:Name="Editor"
            Grid.Row="1"
            Content="{Binding MarkdownContent}"
            Mode="Writer"
            Theme="Auto"
            ContentChanged="OnContentChanged" />
    </Grid>
</ContentPage>
```

### 3. Handle in Code-Behind

```csharp
using CloudDown.Editor.Controls;
using CloudDown.Editor.Models;

namespace MyApp;

public partial class EditorPage : ContentPage
{
    public EditorPage()
    {
        InitializeComponent();
    }

    private void OnContentChanged(object sender, ContentChangedEventArgs e)
    {
        // Access the markdown content
        string markdown = e.NewContent;
        Debug.WriteLine($"Content updated: {markdown}");
    }

    private void OnBoldClicked(object sender, EventArgs e)
    {
        // Programmatically apply formatting
        Editor.ApplyFormatting(MarkdownFormat.Bold);
    }
}
```

### 4. MVVM Pattern

```csharp
using CommunityToolkit.Mvvm.ComponentModel;

public partial class EditorViewModel : ObservableObject
{
    [ObservableProperty]
    private string markdownContent = "# Hello World\n\nStart editing...";

    [ObservableProperty]
    private EditorMode currentMode = EditorMode.Writer;

    partial void OnMarkdownContentChanged(string value)
    {
        // React to content changes
        Debug.WriteLine($"Content length: {value.Length}");
    }
}
```

```xml
<clouddown:MarkdownEditor 
    Content="{Binding MarkdownContent}"
    Mode="{Binding CurrentMode}" />
```

## API Reference

### MarkdownEditor Control

#### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Content` | `string` | `""` | The markdown content (bindable, two-way) |
| `Mode` | `EditorMode` | `Writer` | Current editing mode |
| `Theme` | `EditorTheme` | `Auto` | Editor theme (Light/Dark/Auto) |
| `ReadOnly` | `bool` | `false` | Whether the editor is read-only |
| `ShowLineNumbers` | `bool` | `false` | Show line numbers (Editor mode only) |
| `FontFamily` | `string` | System | Font family for editor text |
| `FontSize` | `double` | 16 | Font size in points |

#### Events

| Event | Args | Description |
|-------|------|-------------|
| `ContentChanged` | `ContentChangedEventArgs` | Fired when content changes |
| `ModeChanged` | `ModeChangedEventArgs` | Fired when editing mode changes |
| `SelectionChanged` | `SelectionChangedEventArgs` | Fired when text selection changes |

#### Methods

| Method | Parameters | Description |
|--------|------------|-------------|
| `ApplyFormatting` | `MarkdownFormat` | Apply markdown formatting to selection |
| `InsertText` | `string` | Insert text at cursor position |
| `GetMarkdownAsync` | - | Get current markdown content |
| `GetHtmlAsync` | - | Get rendered HTML |
| `SetContent` | `string` | Set markdown content programmatically |
| `Undo` | - | Undo last change |
| `Redo` | - | Redo last undone change |

### FormattingToolbar Control

#### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `TargetEditor` | `MarkdownEditor` | `null` | The editor to control |
| `ShowModeSwitch` | `bool` | `true` | Show mode switching buttons |
| `ShowFormatButtons` | `bool` | `true` | Show formatting buttons |
| `Orientation` | `StackOrientation` | `Horizontal` | Toolbar orientation |

### Enums

#### EditorMode

```csharp
public enum EditorMode
{
    Writer,      // Rich preview with inline editing
    Editor,      // Plain text with syntax highlighting
    Split        // Side-by-side view
}
```

#### MarkdownFormat

```csharp
public enum MarkdownFormat
{
    Bold,
    Italic,
    Strikethrough,
    Header1,
    Header2,
    Header3,
    BulletList,
    NumberedList,
    TaskList,
    Link,
    Image,
    InlineCode,
    CodeBlock,
    Blockquote,
    HorizontalRule
}
```

#### EditorTheme

```csharp
public enum EditorTheme
{
    Light,
    Dark,
    Auto        // Follow system theme
}
```

## Platform Support

| Platform | Minimum Version | Status |
|----------|----------------|--------|
| Android | 8.0 (API 26) | ✅ Fully Supported |
| iOS | 13.0 | ✅ Fully Supported |
| macOS | 11.0 (via Mac Catalyst) | ✅ Fully Supported |
| Windows | 10.0.19041.0 | ✅ Fully Supported |

## Documentation

- [Getting Started](docs/getting-started.md) - Detailed setup guide
- [API Reference](docs/api-reference.md) - Complete API documentation
- [Writer Mode](docs/writer-mode.md) - Native rich text implementation details
- [Editor Mode](docs/editor-mode.md) - Syntax highlighting configuration
- [Split Mode](docs/split-mode.md) - Side-by-side layout options
- [Customization](docs/customization.md) - Themes, fonts, colors, and styling
- [Performance](docs/performance.md) - Optimization tips for large documents
- [Accessibility](docs/accessibility.md) - Screen reader and assistive technology support

## Sample Application

A complete sample application demonstrating all features is available in the [samples](samples/CloudDown.Editor.Sample/) directory.

To run the sample:

```bash
git clone https://github.com/digitalninjae/clouddown-editor.git
cd clouddown-editor/samples/CloudDown.Editor.Sample
dotnet build
dotnet run
```

## Advanced Usage

### Custom Themes

```csharp
var customTheme = new EditorTheme
{
    BackgroundColor = Colors.White,
    TextColor = Colors.Black,
    SyntaxColors = new SyntaxColorScheme
    {
        Bold = Colors.DarkBlue,
        Italic = Colors.DarkGreen,
        Header = Colors.Purple,
        Link = Colors.Blue,
        Code = Colors.DarkRed
    }
};

Editor.CustomTheme = customTheme;
```

### Handling Large Documents

```csharp
// Enable virtualization for documents over 10,000 lines
Editor.EnableVirtualization = true;
Editor.VirtualizationThreshold = 10000;

// Debounce syntax highlighting for better performance
Editor.HighlightingDebounceMs = 300;
```

### Custom Formatting Toolbar

```csharp
// Create a custom toolbar with specific buttons
var toolbar = new FormattingToolbar
{
    TargetEditor = Editor,
    ShowModeSwitch = false,
    CustomButtons = new List<ToolbarButton>
    {
        new ToolbarButton("Bold", MarkdownFormat.Bold, "⌘B"),
        new ToolbarButton("Italic", MarkdownFormat.Italic, "⌘I"),
        new ToolbarButton("Link", MarkdownFormat.Link, "⌘K")
    }
};
```

## Performance

CloudDown.Editor is optimized for performance:

- **Incremental Parsing** - Only re-parses changed portions of the document
- **Lazy Rendering** - Only renders visible portions in large documents
- **Efficient Native Controls** - Platform-optimized text rendering
- **Debounced Updates** - Configurable debouncing for syntax highlighting
- **Memory Efficient** - Minimal allocations during editing

Benchmarks (on average hardware):
- **Small documents** (<1,000 lines): Real-time highlighting, no lag
- **Medium documents** (1,000-10,000 lines): <100ms update latency
- **Large documents** (>10,000 lines): Virtualization recommended

## Architecture

CloudDown.Editor uses .NET MAUI Handlers for cross-platform implementation:

```
┌─────────────────────────────────────────┐
│   MarkdownEditor (Cross-platform API)   │
└─────────────────┬───────────────────────┘
                  │
          ┌───────┴────────┐
          │  Handler Layer  │
          └───────┬─────────┘
                  │
    ┌─────────────┼─────────────┐
    │             │             │
┌───▼────┐  ┌────▼─────┐  ┌───▼─────┐
│Android │  │   iOS    │  │Windows  │
│EditText│  │UITextView│  │RichEdit │
└────────┘  └──────────┘  └─────────┘
```

Each platform uses native text editing controls for the best experience.

## Used By

CloudDown.Editor powers the following applications:

- [CloudDown](https://github.com/digitalninjae/clouddown) - Cross-platform markdown editor with cloud storage
- Add your project here! Submit a PR to showcase your app.

## Contributing

We welcome contributions! See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

Areas where we especially need help:
- Testing on different devices and OS versions
- Performance optimization for large documents
- Additional markdown syntax support
- Accessibility improvements
- Documentation and samples

## Roadmap

### v1.0 (Current)
- ✅ Three editing modes (Writer, Editor, Split)
- ✅ Native implementations for all platforms
- ✅ Basic markdown syntax support
- ✅ Formatting toolbar
- ✅ MVVM support

### v1.1 (Planned)
- [ ] Table editing UI
- [ ] Image paste support
- [ ] Custom syntax extensions
- [ ] Export to PDF/HTML
- [ ] Collaborative editing support

### v2.0 (Future)
- [ ] Plugin system
- [ ] Language server protocol support
- [ ] Advanced theming engine
- [ ] Split pane customization

## License

CloudDown.Editor is licensed under the MIT License. See [LICENSE](LICENSE) for details.

## Credits

**Author:** Brian Cupples ([@digitalninjae](https://github.com/digitalninjae))

**Inspired by:**
- [Typora](https://typora.io/) - Seamless live preview
- [Bear](https://bear.app/) - Beautiful writing experience
- [iA Writer](https://ia.net/writer) - Focus and clarity

**Built with:**
- [.NET MAUI](https://github.com/dotnet/maui) - Cross-platform framework
- [Markdig](https://github.com/xoofx/markdig) - Markdown processing

## Support

- 🐛 [Report bugs](https://github.com/digitalninjae/clouddown-editor/issues)
- 💡 [Request features](https://github.com/digitalninjae/clouddown-editor/issues)
- 💬 [Join discussions](https://github.com/digitalninjae/clouddown-editor/discussions)
- 📧 [Contact maintainer](mailto:your.email@example.com)

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for release history and version notes.

---

**Like CloudDown.Editor? Give us a star! ⭐**

Made with ❤️ by [Brian Cupples](https://github.com/digitalninjae)
