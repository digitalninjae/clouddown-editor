using CloudDown.Editor.Models;

namespace CloudDown.Editor.Services;

/// <summary>
/// Maps Markdown source text to a flat list of styled ranges (<see cref="MarkdownToken"/>) that
/// platform handlers render as native text spans. The output is theme-agnostic — token
/// <em>kinds</em>, not colors — so a host can apply any <see cref="SyntaxColors"/> at render time.
/// Contains no UI dependencies so it can be unit-tested on a plain .NET host.
/// </summary>
public interface ISyntaxTokenizer
{
    /// <summary>
    /// Tokenizes <paramref name="content"/> into styled ranges, in document order (pre-order: an
    /// enclosing construct precedes the constructs nested inside it). The returned ranges may nest
    /// and overlap; see <see cref="MarkdownToken"/>. Returns an empty list for <c>null</c> or empty
    /// input and for content with no recognized syntax.
    /// </summary>
    IReadOnlyList<MarkdownToken> Tokenize(string content);
}
