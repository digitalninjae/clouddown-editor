using AwesomeAssertions;
using CloudDown.Editor.Models;
using CloudDown.Editor.Services;

namespace CloudDown.Editor.Tests;

/// <summary>
/// Granular, data-driven cases for the code &amp; block formats: inline code (an inline toggle like
/// the other emphases), blockquotes (a per-line toggle like headings/lists), fenced code blocks, and
/// horizontal rules (block insertions that emit their own lines). The line ending is forced to LF so
/// the expectations are host-independent; line-ending detection has its own fixture. Behavioral
/// coverage lives in <c>Formatting.feature</c>.
/// </summary>
[TestFixture]
public class ApplyFormattingCodeAndBlockTests
{
    private MarkdownService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new MarkdownService();
        _service.FormattingOptions.LineEnding = LineEndingMode.Lf;
    }

    private FormattingResult Apply(string content, MarkdownFormat format, string selection)
    {
        var start = content.IndexOf(selection, StringComparison.Ordinal);
        return _service.ApplyFormatting(content, format, start, selection.Length);
    }

    // Inline code wraps the selection and, like the other emphases, toggles back off — whether the
    // backticks are part of the selection or sit just outside it.
    [TestCase("the cat sat", "cat", "the `cat` sat")]
    [TestCase("the `cat` sat", "cat", "the cat sat")]
    [TestCase("the `cat` sat", "`cat`", "the cat sat")]
    public void ApplyFormatting_InlineCode_TogglesAroundSelection(string content, string selection, string expected) =>
        Apply(content, MarkdownFormat.InlineCode, selection).Content.Should().Be(expected);

    // Blockquotes apply "> " at the start of every touched line and toggle off when all are quoted,
    // mirroring headings/lists — and work on a partial-line selection.
    [TestCase("hello", "hello", "> hello")]
    [TestCase("> hello", "hello", "hello")]
    [TestCase("hello world", "world", "> hello world")]
    [TestCase("> hello world", "world", "hello world")]
    // A ">" without the optional space is still recognized when stripping.
    [TestCase(">hello", "hello", "hello")]
    public void ApplyFormatting_Blockquote_TogglesPerLine(string content, string selection, string expected) =>
        Apply(content, MarkdownFormat.Blockquote, selection).Content.Should().Be(expected);

    [Test]
    public void ApplyFormatting_Blockquote_MultiLine_QuotesEveryLine()
    {
        const string content = "alpha\nbeta";
        _service.ApplyFormatting(content, MarkdownFormat.Blockquote, 0, content.Length)
            .Content.Should().Be("> alpha\n> beta");
    }

    [Test]
    public void ApplyFormatting_Blockquote_MultiLine_TogglesOffWhenAllQuoted()
    {
        const string content = "> alpha\n> beta";
        _service.ApplyFormatting(content, MarkdownFormat.Blockquote, 0, content.Length)
            .Content.Should().Be("alpha\nbeta");
    }

    [Test]
    public void ApplyFormatting_CodeBlock_WrapsSelectionInFences()
    {
        var result = Apply("hello", MarkdownFormat.CodeBlock, "hello");
        result.Content.Should().Be("```lang\nhello\n```");
    }

    [Test]
    public void ApplyFormatting_CodeBlock_SelectsTheLangPlaceholder()
    {
        var result = Apply("hello", MarkdownFormat.CodeBlock, "hello");
        result.Content.Substring(result.SelectionStart, result.SelectionLength).Should().Be("lang");
    }

    [Test]
    public void ApplyFormatting_CodeBlock_MultiLine_WrapsAllLines()
    {
        const string content = "first\nsecond";
        _service.ApplyFormatting(content, MarkdownFormat.CodeBlock, 0, content.Length)
            .Content.Should().Be("```lang\nfirst\nsecond\n```");
    }

    [Test]
    public void ApplyFormatting_CodeBlock_OnFencedBlock_StripsTheFences()
    {
        // Selecting the whole block (fences included) toggles it back to plain text.
        const string content = "```lang\nhello\n```";
        _service.ApplyFormatting(content, MarkdownFormat.CodeBlock, 0, content.Length)
            .Content.Should().Be("hello");
    }

    [Test]
    public void ApplyFormatting_HorizontalRule_WithSelection_WrapsTextBetweenRules()
    {
        var result = Apply("the cat sat", MarkdownFormat.HorizontalRule, "the cat sat");
        // Blank lines around each rule keep them thematic breaks (not a Setext heading underline).
        result.Content.Should().Be("---\n\nthe cat sat\n\n---");
        // The wrapped text stays selected.
        result.Content.Substring(result.SelectionStart, result.SelectionLength).Should().Be("the cat sat");
    }

    [Test]
    public void ApplyFormatting_HorizontalRule_EmptySelection_InsertsRuleOnItsOwnLine()
    {
        // Caret between "a" and "b": the rule lands on its own blank-line-separated line.
        var result = _service.ApplyFormatting("ab", MarkdownFormat.HorizontalRule, 1, 0);
        result.Content.Should().Be("a\n\n---\n\nb");
        result.Content.Substring(result.SelectionStart, result.SelectionLength).Should().Be("---");
    }

    [Test]
    public void ApplyFormatting_HorizontalRule_EmptySelection_AtLineStart_OmitsLeadingBreak()
    {
        // Caret already at a line boundary needs no extra surrounding break.
        var result = _service.ApplyFormatting(string.Empty, MarkdownFormat.HorizontalRule, 0, 0);
        result.Content.Should().Be("---");
    }
}
