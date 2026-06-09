using Microsoft.Maui.Graphics;

namespace CloudDown.Editor.Models;

/// <summary>
/// A concrete visual theme the <c>MarkdownEditor</c> control renders with: the surface and
/// text colors plus the per-token <see cref="SyntaxColors"/>. This is UI-free theme
/// <em>data</em> — platform handlers consume it to style native text controls (it does not
/// render anything itself).
/// </summary>
/// <remarks>
/// Use the built-in <see cref="Light"/> and <see cref="Dark"/> presets, customize one with a
/// <c>with</c>-style object initializer, or construct a bespoke theme. <see cref="EditorThemeMode"/>
/// selects which theme is applied at runtime; see <see cref="ForMode"/>.
/// </remarks>
public sealed class EditorTheme
{
    /// <summary>The editor surface (background) color.</summary>
    public Color Background { get; init; } = Colors.White;

    /// <summary>The default body-text (foreground) color.</summary>
    public Color Foreground { get; init; } = Colors.Black;

    /// <summary>Colors for individual Markdown syntax tokens.</summary>
    public SyntaxColors Syntax { get; init; } = new();

    /// <summary>The built-in light theme.</summary>
    public static EditorTheme Light { get; } = new();

    /// <summary>The built-in dark theme.</summary>
    public static EditorTheme Dark { get; } = new()
    {
        // A dark editor surface; there is no named color for this shade (≈ VS Code's "#1E1E1E").
        Background = Color.FromArgb("#1E1E1E"),
        Foreground = Colors.Gainsboro,
        Syntax = new SyntaxColors
        {
            Heading = Colors.Plum,
            Bold = Colors.CornflowerBlue,
            Italic = Colors.MediumAquamarine,
            Strikethrough = Colors.DarkGray,
            Link = Colors.LightSkyBlue,
            Code = Colors.LightCoral,
            Blockquote = Colors.LightSlateGray,
            ListMarker = Colors.SandyBrown,
        },
    };

    /// <summary>
    /// Resolves the concrete theme to render for a given <paramref name="mode"/>.
    /// <see cref="EditorThemeMode.Auto"/> follows <paramref name="systemPrefersDark"/>.
    /// </summary>
    public static EditorTheme ForMode(EditorThemeMode mode, bool systemPrefersDark = false) =>
        mode switch
        {
            EditorThemeMode.Light => Light,
            EditorThemeMode.Dark => Dark,
            _ => systemPrefersDark ? Dark : Light,
        };
}
