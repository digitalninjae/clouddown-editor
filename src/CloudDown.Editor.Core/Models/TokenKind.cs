namespace CloudDown.Editor.Models;

/// <summary>
/// The category of a Markdown syntax token produced by
/// <see cref="Services.ISyntaxTokenizer"/>. Each value maps one-to-one to a property of
/// <see cref="SyntaxColors"/>, so a platform handler can color a token by looking up the matching
/// color. The set is theme-agnostic: it names <em>what</em> a span is, never how it looks.
/// </summary>
/// <remarks>
/// Constructs without a corresponding <see cref="SyntaxColors"/> color — notably thematic breaks
/// (<c>---</c>) — intentionally produce no token.
/// </remarks>
public enum TokenKind
{
    /// <summary>A heading and its markers — ATX (<c>#</c>) or Setext (underlined). Maps to <see cref="SyntaxColors.Heading"/>.</summary>
    Heading,

    /// <summary>Bold text and its markers (<c>**</c> / <c>__</c>). Maps to <see cref="SyntaxColors.Bold"/>.</summary>
    Bold,

    /// <summary>Italic text and its markers (<c>*</c> / <c>_</c>). Maps to <see cref="SyntaxColors.Italic"/>.</summary>
    Italic,

    /// <summary>Strikethrough text and its markers (<c>~~</c>). Maps to <see cref="SyntaxColors.Strikethrough"/>.</summary>
    Strikethrough,

    /// <summary>A link or image, including its markers, text, and URL. Maps to <see cref="SyntaxColors.Link"/>.</summary>
    Link,

    /// <summary>Inline code (<c>`…`</c>) or a fenced/indented code block, including its markers. Maps to <see cref="SyntaxColors.Code"/>.</summary>
    Code,

    /// <summary>A blockquote, including the <c>&gt;</c> marker. Maps to <see cref="SyntaxColors.Blockquote"/>.</summary>
    Blockquote,

    /// <summary>A list item's marker (bullet or ordered-list number), not its content. Maps to <see cref="SyntaxColors.ListMarker"/>.</summary>
    ListMarker,
}
