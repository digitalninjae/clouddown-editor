namespace CloudDown.Editor.Models;

/// <summary>
/// Selects which <see cref="EditorTheme"/> the <c>MarkdownEditor</c> control renders with:
/// a fixed light or dark theme, or one that follows the system appearance.
/// </summary>
public enum EditorThemeMode
{
    /// <summary>Always use the light theme.</summary>
    Light,

    /// <summary>Always use the dark theme.</summary>
    Dark,

    /// <summary>Follow the system theme (light or dark).</summary>
    Auto
}
