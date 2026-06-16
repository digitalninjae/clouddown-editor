namespace CloudDown.Editor.Models;

/// <summary>
/// A single styled range produced by <see cref="Services.ISyntaxTokenizer"/>: a
/// <see cref="Kind"/> together with the span of source text it covers. Platform handlers turn these
/// into native text spans (e.g. Android <c>SpannableString</c>, iOS/macOS <c>NSAttributedString</c>),
/// coloring each by mapping <see cref="Kind"/> through <see cref="SyntaxColors"/>.
/// </summary>
/// <remarks>
/// <see cref="Start"/> and <see cref="Length"/> index into the original Markdown <em>source</em>
/// (not rendered output). Tokens may <strong>nest and overlap</strong> — e.g. a <c>Bold</c> token
/// inside the <c>Heading</c> token of <c># A **b**</c> — so consumers should layer them rather than
/// assume a flat partition.
/// </remarks>
/// <param name="Kind">The category of syntax this range represents.</param>
/// <param name="Start">The zero-based start index into the source text.</param>
/// <param name="Length">The number of characters the token covers.</param>
public readonly record struct MarkdownToken(TokenKind Kind, int Start, int Length)
{
    /// <summary>The exclusive end index of the token (<see cref="Start"/> + <see cref="Length"/>).</summary>
    public int End => Start + Length;
}
