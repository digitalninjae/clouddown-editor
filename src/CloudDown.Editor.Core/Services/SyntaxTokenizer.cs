using CloudDown.Editor.Models;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace CloudDown.Editor.Services;

/// <summary>
/// Default <see cref="ISyntaxTokenizer"/> backed by Markdig. Parses the source with precise
/// inline source locations enabled, then walks the syntax tree emitting one
/// <see cref="MarkdownToken"/> per recognized construct, using each node's source span.
/// </summary>
public sealed class SyntaxTokenizer : ISyntaxTokenizer
{
    // Precise source locations are enabled so that inline nodes (emphasis, code, links) carry
    // source spans, not just blocks. This pipeline is kept independent of MarkdownService's render
    // pipeline because tokenizing and rendering have different needs.
    private readonly MarkdownPipeline _pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .UsePreciseSourceLocation()
        .Build();

    /// <inheritdoc />
    public IReadOnlyList<MarkdownToken> Tokenize(string content)
    {
        if (string.IsNullOrEmpty(content))
            return Array.Empty<MarkdownToken>();

        var document = Markdown.Parse(content, _pipeline);
        var tokens = new List<MarkdownToken>();

        // Descendants() yields nodes in pre-order, so an enclosing construct precedes the ones
        // nested inside it — the natural order for consumers layering native text spans.
        foreach (var node in document.Descendants())
        {
            if (node.Span.IsEmpty)
                continue;

            MarkdownToken? token = node switch
            {
                HeadingBlock        => Token(TokenKind.Heading, node.Span),
                QuoteBlock          => Token(TokenKind.Blockquote, node.Span),
                // FencedCodeBlock derives from CodeBlock, so this catches both fenced and indented.
                CodeBlock           => Token(TokenKind.Code, node.Span),
                ListItemBlock item  => ListMarkerToken(content, item),
                EmphasisInline emph => Token(EmphasisKind(emph), node.Span),
                CodeInline          => Token(TokenKind.Code, node.Span),
                LinkInline          => Token(TokenKind.Link, node.Span),
                _                   => null,
            };

            if (token is { } t)
                tokens.Add(t);
        }

        return tokens;
    }

    // Markdig models bold/italic/strikethrough as a single EmphasisInline distinguished by its
    // delimiter: '~' is strikethrough; a doubled delimiter (**/__) is bold; a single one is italic.
    private static TokenKind EmphasisKind(EmphasisInline emphasis) => emphasis switch
    {
        { DelimiterChar: '~' } => TokenKind.Strikethrough,
        { DelimiterCount: >= 2 } => TokenKind.Bold,
        _ => TokenKind.Italic,
    };

    private static MarkdownToken? Token(TokenKind kind, SourceSpan span) =>
        span.Length > 0 ? new MarkdownToken(kind, span.Start, span.Length) : null;

    // A list item's source span covers the whole item, but SyntaxColors.ListMarker colors only the
    // marker. The marker runs from the item start up to where its first child block's content
    // begins (e.g. "- " or "1. "); fall back to scanning the prefix if the item has no child block.
    private static MarkdownToken? ListMarkerToken(string content, ListItemBlock item)
    {
        var markerStart = item.Span.Start;
        var contentStart = item.Count > 0 ? item[0].Span.Start : -1;
        if (contentStart <= markerStart)
            contentStart = ScanMarkerEnd(content, item.Span);

        var length = contentStart - markerStart;
        return length > 0 ? new MarkdownToken(TokenKind.ListMarker, markerStart, length) : null;
    }

    // Defensive fallback for an item with no child block: walk the bullet ('-', '+', '*') or the
    // ordered number plus its '.'/')' delimiter, then one trailing space.
    private static int ScanMarkerEnd(string content, SourceSpan span)
    {
        var i = span.Start;
        var end = Math.Min(content.Length, span.End + 1); // SourceSpan.End is inclusive.
        if (i >= end)
            return i;

        if (content[i] is '-' or '+' or '*')
        {
            i++;
        }
        else
        {
            while (i < end && char.IsDigit(content[i]))
                i++;
            if (i < end && content[i] is '.' or ')')
                i++;
        }

        if (i < end && content[i] == ' ')
            i++;

        return i;
    }
}
