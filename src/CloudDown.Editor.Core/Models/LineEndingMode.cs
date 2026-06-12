namespace CloudDown.Editor.Models;

/// <summary>
/// How <see cref="Services.IMarkdownService"/> chooses the line ending for any newline it
/// <em>emits</em> (fenced code blocks, horizontal rules, and the rejoining of per-line operations).
/// </summary>
public enum LineEndingMode
{
    /// <summary>
    /// Detect the document's existing ending from the content and reuse it, so a file keeps its own
    /// convention. Falls back to <see cref="System.Environment.NewLine"/> when the content has no
    /// line break to detect (empty or single-line).
    /// </summary>
    Preserve,

    /// <summary>Always emit a Unix line feed (<c>\n</c>), normalizing regardless of the content.</summary>
    Lf,

    /// <summary>Always emit a Windows carriage-return + line feed (<c>\r\n</c>), normalizing regardless of the content.</summary>
    CrLf
}
