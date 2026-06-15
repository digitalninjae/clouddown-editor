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
    // UsePreciseSourceLocation is what extends source spans to inline nodes (emphasis, code, links);
    // block spans are tracked without it. This pipeline is independent of MarkdownService's render
    // pipeline — tokenizing and rendering have different needs.
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

            switch (node)
            {
                case HeadingBlock:
                    Add(tokens, TokenKind.Heading, node.Span);
                    break;
                case QuoteBlock:
                    Add(tokens, TokenKind.Blockquote, node.Span);
                    break;
                // FencedCodeBlock derives from CodeBlock, so this catches both fenced and indented.
                case CodeBlock:
                    Add(tokens, TokenKind.Code, node.Span);
                    break;
                case ListItemBlock item:
                    AddListMarker(tokens, content, item);
                    break;
                case EmphasisInline emphasis:
                    Add(tokens, EmphasisKind(emphasis), node.Span);
                    break;
                case CodeInline:
                    Add(tokens, TokenKind.Code, node.Span);
                    break;
                case LinkInline:
                    Add(tokens, TokenKind.Link, node.Span);
                    break;
            }
        }

        return tokens;
    }

    // Markdig models bold/italic/strikethrough as a single EmphasisInline distinguished by its
    // delimiter: '~' is strikethrough; a doubled delimiter (**/__) is bold; a single one is italic.
    private static TokenKind EmphasisKind(EmphasisInline emphasis) =>
        emphasis.DelimiterChar == '~' ? TokenKind.Strikethrough
        : emphasis.DelimiterCount >= 2 ? TokenKind.Bold
        : TokenKind.Italic;

    private static void Add(List<MarkdownToken> tokens, TokenKind kind, SourceSpan span)
    {
        if (span.Length > 0)
            tokens.Add(new MarkdownToken(kind, span.Start, span.Length));
    }

    // A list item's source span covers the whole item, but SyntaxColors.ListMarker colors only the
    // marker. The marker runs from the item start up to where its first child block's content
    // begins (e.g. "- " or "1. "); fall back to scanning the prefix if the item has no child block.
    private static void AddListMarker(List<MarkdownToken> tokens, string content, ListItemBlock item)
    {
        var markerStart = item.Span.Start;
        var contentStart = item.Count > 0 ? item[0].Span.Start : -1;
        if (contentStart <= markerStart)
            contentStart = ScanMarkerEnd(content, item.Span);

        var length = contentStart - markerStart;
        if (length > 0)
            tokens.Add(new MarkdownToken(TokenKind.ListMarker, markerStart, length));
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
