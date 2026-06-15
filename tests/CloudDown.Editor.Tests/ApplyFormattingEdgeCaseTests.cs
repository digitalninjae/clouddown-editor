using AwesomeAssertions;
using CloudDown.Editor.Models;
using CloudDown.Editor.Services;

namespace CloudDown.Editor.Tests;

/// <summary>
/// Cross-format edge cases for <c>ApplyFormatting</c> (#11): selection bounds, empty selections,
/// idempotent round-trips, and the returned-selection contract. Format-specific behavior lives in
/// the per-format fixtures; this fixture pins the rules that hold across all of them.
/// </summary>
[TestFixture]
public class ApplyFormattingEdgeCaseTests
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

    // --- Selection bounds ---------------------------------------------------------------------

    // "hello" is length 5; each of these selections falls outside [0, 5].
    [TestCase(-1, 0)] // negative start
    [TestCase(0, -1)] // negative length
    [TestCase(0, 6)]  // runs past the end
    [TestCase(6, 0)]  // starts past the end
    [TestCase(3, 5)]  // start in range but start + length overruns
    public void ApplyFormatting_SelectionOutOfRange_Throws(int start, int length)
    {
        var act = () => _service.ApplyFormatting("hello", MarkdownFormat.Bold, start, length);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // The exact boundaries are valid: a caret at the very end, the whole content, and empty content.
    [TestCase("hello", 5, 0)]
    [TestCase("hello", 0, 5)]
    [TestCase("", 0, 0)]
    public void ApplyFormatting_SelectionAtBounds_DoesNotThrow(string content, int start, int length)
    {
        var act = () => _service.ApplyFormatting(content, MarkdownFormat.Bold, start, length);
        act.Should().NotThrow();
    }

    // --- Empty selection ----------------------------------------------------------------------

    // Inline emphases with nothing selected insert their (empty) markers at the caret.
    [TestCase(MarkdownFormat.Bold, "****")]
    [TestCase(MarkdownFormat.Italic, "**")]
    [TestCase(MarkdownFormat.Strikethrough, "~~~~")]
    [TestCase(MarkdownFormat.InlineCode, "``")]
    public void ApplyFormatting_InlineWithEmptySelection_InsertsEmptyMarkers(MarkdownFormat format, string expected)
    {
        var result = _service.ApplyFormatting(string.Empty, format, 0, 0);
        result.Content.Should().Be(expected);
        // The whole inserted marker pair is selected, so the caret is ready inside it.
        result.Content.Substring(result.SelectionStart, result.SelectionLength).Should().Be(expected);
    }

    // Per-line and block formats with nothing selected apply to the whole line the caret sits on.
    [TestCase(MarkdownFormat.Header2, "## abc")]
    [TestCase(MarkdownFormat.BulletList, "- abc")]
    [TestCase(MarkdownFormat.NumberedList, "1. abc")]
    [TestCase(MarkdownFormat.Blockquote, "> abc")]
    [TestCase(MarkdownFormat.CodeBlock, "```lang\nabc\n```")]
    public void ApplyFormatting_LineFormatWithEmptySelection_AppliesToTheWholeLine(MarkdownFormat format, string expected)
    {
        // Caret sits inside "abc" (offset 1) with nothing selected.
        _service.ApplyFormatting("abc", format, 1, 0).Content.Should().Be(expected);
    }

    // --- Idempotent round-trips ---------------------------------------------------------------

    // Applying a toggleable format and then re-applying it to the same word restores the original.
    [TestCase(MarkdownFormat.Bold, "cat", "**cat**")]
    [TestCase(MarkdownFormat.Italic, "cat", "*cat*")]
    [TestCase(MarkdownFormat.Strikethrough, "cat", "~~cat~~")]
    [TestCase(MarkdownFormat.InlineCode, "cat", "`cat`")]
    [TestCase(MarkdownFormat.Header1, "hello", "# hello")]
    [TestCase(MarkdownFormat.Header4, "hello", "#### hello")]
    [TestCase(MarkdownFormat.BulletList, "milk", "- milk")]
    [TestCase(MarkdownFormat.NumberedList, "milk", "1. milk")]
    [TestCase(MarkdownFormat.TaskList, "milk", "- [ ] milk")]
    [TestCase(MarkdownFormat.Blockquote, "hello", "> hello")]
    public void ApplyFormatting_ToggleTwice_RestoresOriginal(MarkdownFormat format, string word, string applied)
    {
        var first = _service.ApplyFormatting(word, format, 0, word.Length);
        first.Content.Should().Be(applied);

        // Re-select the word within the formatted content and toggle the format back off.
        var innerStart = first.Content.IndexOf(word, StringComparison.Ordinal);
        _service.ApplyFormatting(first.Content, format, innerStart, word.Length)
            .Content.Should().Be(word);
    }

    // --- Returned-selection contract ----------------------------------------------------------

    [Test]
    public void ApplyFormatting_Wrapping_SelectsTheWholeReplacement()
    {
        // Wrapping a word keeps the markers-and-word span selected.
        var result = Apply("the cat sat", MarkdownFormat.Bold, "cat");
        result.Content.Substring(result.SelectionStart, result.SelectionLength).Should().Be("**cat**");
    }

    [Test]
    public void ApplyFormatting_Unwrapping_SelectsTheBareWord()
    {
        // Toggling a wrapped word off leaves just the word selected.
        var result = Apply("the **cat** sat", MarkdownFormat.Bold, "cat");
        result.Content.Substring(result.SelectionStart, result.SelectionLength).Should().Be("cat");
    }

    [Test]
    public void ApplyFormatting_PerLineFormat_SelectsTheWholeLine()
    {
        var result = Apply("hello", MarkdownFormat.Header1, "hello");
        result.Content.Substring(result.SelectionStart, result.SelectionLength).Should().Be("# hello");
    }
}
