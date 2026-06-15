using AwesomeAssertions;
using CloudDown.Editor.Models;
using CloudDown.Editor.Services;

namespace CloudDown.Editor.Tests;

/// <summary>
/// Data-driven coverage for <see cref="SyntaxTokenizer"/> (#15): each representative Markdown
/// sample maps to the exact token sequence the tokenizer should emit, in document (pre-order)
/// order. Spans index into the source text and include the construct's markers, matching what
/// <see cref="SyntaxColors"/> colors. Constructs without a color (thematic breaks) produce nothing.
/// </summary>
[TestFixture]
public class SyntaxTokenizerTests
{
    private SyntaxTokenizer _tokenizer = null!;

    [SetUp]
    public void SetUp() => _tokenizer = new SyntaxTokenizer();

    private static MarkdownToken T(TokenKind kind, int start, int length) => new(kind, start, length);

    private static readonly MarkdownToken[] None = [];

    public static IEnumerable<TestCaseData> Cases()
    {
        // No syntax → no tokens.
        yield return new TestCaseData("", None).SetName("Empty string → no tokens");
        yield return new TestCaseData("plain text", None).SetName("Plain paragraph → no tokens");

        // Headings (ATX): the whole "# …" line, including the markers.
        yield return new TestCaseData("# H1", new[] { T(TokenKind.Heading, 0, 4) }).SetName("ATX H1");
        yield return new TestCaseData("## H2", new[] { T(TokenKind.Heading, 0, 5) }).SetName("ATX H2");
        yield return new TestCaseData("###### H6", new[] { T(TokenKind.Heading, 0, 9) }).SetName("ATX H6");

        // Setext headings are recognized as headings (ADR-0006), spanning the text + underline.
        yield return new TestCaseData("Title\n===", new[] { T(TokenKind.Heading, 0, 9) }).SetName("Setext H1");
        yield return new TestCaseData("Title\n---", new[] { T(TokenKind.Heading, 0, 9) }).SetName("Setext H2 is a heading, not a thematic break");

        // A blank-separated "---" is a real thematic break — no color category, so no token.
        yield return new TestCaseData("Title\n\n---", None).SetName("Blank-separated --- is a thematic break → no token");
        yield return new TestCaseData("---", None).SetName("Bare --- thematic break → no token");

        // Emphasis: delimiter character/count decide kind; span includes the delimiters.
        yield return new TestCaseData("**b**", new[] { T(TokenKind.Bold, 0, 5) }).SetName("Bold **");
        yield return new TestCaseData("*i*", new[] { T(TokenKind.Italic, 0, 3) }).SetName("Italic *");
        yield return new TestCaseData("_i_", new[] { T(TokenKind.Italic, 0, 3) }).SetName("Italic _");
        yield return new TestCaseData("~~s~~", new[] { T(TokenKind.Strikethrough, 0, 5) }).SetName("Strikethrough ~~");
        yield return new TestCaseData("***bi***", new[] { T(TokenKind.Italic, 0, 8), T(TokenKind.Bold, 1, 6) }).SetName("Nested bold+italic *** → outer then inner");

        // Code: inline and fenced both map to Code, markers included.
        yield return new TestCaseData("`code`", new[] { T(TokenKind.Code, 0, 6) }).SetName("Inline code");
        yield return new TestCaseData("```cs\nx\n```", new[] { T(TokenKind.Code, 0, 11) }).SetName("Fenced code block");

        // Links and images both map to Link, spanning the whole construct.
        yield return new TestCaseData("[t](u)", new[] { T(TokenKind.Link, 0, 6) }).SetName("Link");
        yield return new TestCaseData("![a](u)", new[] { T(TokenKind.Link, 0, 7) }).SetName("Image");

        // Blockquote: whole quote including the '>' marker.
        yield return new TestCaseData("> quote", new[] { T(TokenKind.Blockquote, 0, 7) }).SetName("Blockquote");

        // Lists: only the marker prefix is tokenized, never the item content.
        yield return new TestCaseData("- item", new[] { T(TokenKind.ListMarker, 0, 2) }).SetName("Bullet marker only");
        yield return new TestCaseData("1. item", new[] { T(TokenKind.ListMarker, 0, 3) }).SetName("Ordered marker only");
        yield return new TestCaseData("- a\n- b", new[] { T(TokenKind.ListMarker, 0, 2), T(TokenKind.ListMarker, 4, 2) }).SetName("Two bullet markers");

        // Nesting: an enclosing construct precedes the one nested inside it.
        yield return new TestCaseData("# A **b**", new[] { T(TokenKind.Heading, 0, 9), T(TokenKind.Bold, 4, 5) }).SetName("Bold nested in heading → outer then inner");

        // Empty list items have no child block, so the marker span is derived by scanning the
        // prefix. These exercise every branch: each bullet character, ordered '.'/')' delimiters,
        // and the marker with/without a trailing space.
        yield return new TestCaseData("- ", new[] { T(TokenKind.ListMarker, 0, 2) }).SetName("Empty '-' item → marker via scan");
        yield return new TestCaseData("-", new[] { T(TokenKind.ListMarker, 0, 1) }).SetName("Bare '-' item → marker without trailing space");
        yield return new TestCaseData("* ", new[] { T(TokenKind.ListMarker, 0, 2) }).SetName("Empty '*' item → marker via scan");
        yield return new TestCaseData("+ ", new[] { T(TokenKind.ListMarker, 0, 2) }).SetName("Empty '+' item → marker via scan");
        yield return new TestCaseData("1. ", new[] { T(TokenKind.ListMarker, 0, 3) }).SetName("Empty '1.' item → marker via scan");
        yield return new TestCaseData("1) ", new[] { T(TokenKind.ListMarker, 0, 3) }).SetName("Empty '1)' item → marker via scan");
    }

    [TestCaseSource(nameof(Cases))]
    public void Tokenize_ProducesExpectedTokens(string markdown, MarkdownToken[] expected) =>
        _tokenizer.Tokenize(markdown).Should().Equal(expected);

    [Test]
    public void Tokenize_Null_ReturnsEmpty() =>
        _tokenizer.Tokenize(null!).Should().BeEmpty();

    [Test]
    public void MarkdownToken_End_IsStartPlusLength()
    {
        var token = new MarkdownToken(TokenKind.Heading, 2, 3);
        token.End.Should().Be(5);
    }
}
