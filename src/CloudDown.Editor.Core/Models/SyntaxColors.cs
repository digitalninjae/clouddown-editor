using Microsoft.Maui.Graphics;

namespace CloudDown.Editor.Models;

/// <summary>
/// The colors used to highlight individual Markdown syntax tokens within an
/// <see cref="EditorTheme"/>. Each value colors one category of token (e.g. headings,
/// emphasis, links). Platform handlers map these to native text spans when rendering.
/// </summary>
public sealed class SyntaxColors
{
    /// <summary>Color for heading text and markers (<c>#</c>).</summary>
    public Color Heading { get; init; } = Colors.Purple;

    /// <summary>Color for bold text and its markers (<c>**</c>).</summary>
    public Color Bold { get; init; } = Colors.RoyalBlue;

    /// <summary>Color for italic text and its markers (<c>_</c> / <c>*</c>).</summary>
    public Color Italic { get; init; } = Colors.SeaGreen;

    /// <summary>Color for strikethrough text and its markers (<c>~~</c>).</summary>
    public Color Strikethrough { get; init; } = Colors.Gray;

    /// <summary>Color for link text, URLs, and image references.</summary>
    public Color Link { get; init; } = Colors.DodgerBlue;

    /// <summary>Color for inline code and fenced code blocks.</summary>
    public Color Code { get; init; } = Colors.IndianRed;

    /// <summary>Color for blockquote text and the <c>&gt;</c> marker.</summary>
    public Color Blockquote { get; init; } = Colors.SlateGray;

    /// <summary>Color for list markers (bullets and ordered-list numbers).</summary>
    public Color ListMarker { get; init; } = Colors.DarkOrange;
}
