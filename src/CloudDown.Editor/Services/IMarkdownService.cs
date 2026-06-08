using CloudDown.Editor.Models;

namespace CloudDown.Editor.Services;

/// <summary>
/// Cross-platform Markdown processing: rendering and selection-based formatting.
/// Contains no UI dependencies so it can be unit-tested on a plain .NET host.
/// </summary>
public interface IMarkdownService
{
    /// <summary>Renders Markdown <paramref name="markdown"/> to an HTML fragment.</summary>
    string ToHtml(string markdown);

    /// <summary>
    /// Applies <paramref name="format"/> to the substring of <paramref name="content"/>
    /// described by <paramref name="selectionStart"/> and <paramref name="selectionLength"/>,
    /// returning the new content.
    /// </summary>
    string ApplyFormatting(string content, MarkdownFormat format, int selectionStart, int selectionLength);
}
