# CloudDown.Editor

A professional cross-platform Markdown editor control for .NET MAUI

[![Build Status](https://github.com/digitalninjae/clouddown-editor/workflows/Build/badge.svg)](https://github.com/digitalninjae/clouddown-editor/actions)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## Features

**Three Editing Modes:**
- **Writer Mode** - Rich preview with inline editing (native implementation)
- **Editor Mode** - Plain text with syntax highlighting
- **Split Mode** - Side-by-side editing and preview

**Fully Native:**
- Platform-specific implementations (not WebView)
- Uses EditText/Spans (Android), UITextView/NSAttributedString (iOS), RichEditBox (Windows)
- Excellent performance and native feel

**Easy Integration:**
- Drop-in control for any MAUI app
- Customizable toolbar
- Full MVVM support
- Extensive API

## Installation
```bash
dotnet add package CloudDown.Editor
```

## Quick Start

### 1. Register the handlers
```csharp
// MauiProgram.cs
using CloudDown.Editor;

public static MauiApp CreateMauiApp()
{
    var builder = MauiApp.CreateBuilder();
    builder
        .UseMauiApp<App>()
        .ConfigureCloudDownEditor(); // Register editor handlers
        
    return builder.Build();
}
```

### 2. Add to your XAML
```xml
xmlns:clouddown="clr-namespace:CloudDown.Editor.Controls;assembly=CloudDown.Editor"

<Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="*"/>
    </Grid.RowDefinitions>
    
    <clouddown:FormattingToolbar 
        Grid.Row="0"
        TargetEditor="{x:Reference Editor}" />
    
    <clouddown:MarkdownEditor 
        x:Name="Editor"
        Grid.Row="1"
        Content="{Binding MarkdownContent}"
        Mode="Writer"
        ContentChanged="OnContentChanged" />
</Grid>
```

### 3. Use in code-behind
```csharp
private void OnContentChanged(object sender, ContentChangedEventArgs e)
{
    // e.NewContent has the markdown
    Debug.WriteLine($"Content: {e.NewContent}");
}

// Or bind to ViewModel
public class EditorViewModel : ObservableObject
{
    private string _markdownContent;
    public string MarkdownContent
    {
        get => _markdownContent;
        set => SetProperty(ref _markdownContent, value);
    }
}
```

## Documentation

- [Getting Started](docs/getting-started.md)
- [API Reference](docs/api-reference.md)
- [Writer Mode](docs/writer-mode.md) - Native rich text implementation
- [Editor Mode](docs/editor-mode.md) - Syntax highlighting
- [Split Mode](docs/split-mode.md) - Side-by-side
- [Customization](docs/customization.md) - Themes, fonts, colors

## Sample App

See the [CloudDown](https://github.com/digitalninjae/clouddown) app for a complete example.

## Platforms

- ✅ Android 8.0+ (API 26+)
- ✅ iOS 13+
- ✅ macOS 11+ (Mac Catalyst)
- ✅ Windows 10+ (10.0.19041.0+)

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md)

## Used By

- [CloudDown](https://github.com/digitalninjae/clouddown) - Full-featured markdown editor app

## License

MIT - See [LICENSE](LICENSE)

## Credits

Built by [Brian Cupples](https://github.com/digitalninjae)

Inspired by wanting to edit Markdown files stored on my OneDrive, but not being able to easily.
