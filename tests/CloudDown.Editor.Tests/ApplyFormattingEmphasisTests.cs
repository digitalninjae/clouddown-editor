using AwesomeAssertions;
using CloudDown.Editor.Models;
using CloudDown.Editor.Services;

namespace CloudDown.Editor.Tests;

/// <summary>
/// Granular, data-driven cases for the emphasis formats (Bold / Italic / Strikethrough),
/// which toggle: wrapping an unformatted selection and removing the markers when the
/// selection is already wrapped. Behavioral coverage lives in <c>Formatting.feature</c>.
/// </summary>
[TestFixture]
public class ApplyFormattingEmphasisTests
{
    private MarkdownService _service = null!;

    [SetUp]
    public void SetUp() => _service = new MarkdownService();

    // Wrapping an unformatted selection adds the markers.
    [TestCase("the cat sat", "cat", MarkdownFormat.Bold, "the **cat** sat")]
    [TestCase("the cat sat", "cat", MarkdownFormat.Italic, "the *cat* sat")]
    [TestCase("the cat sat", "cat", MarkdownFormat.Strikethrough, "the ~~cat~~ sat")]
    // Re-applying to the inner text (markers just outside the selection) removes them.
    [TestCase("the **cat** sat", "cat", MarkdownFormat.Bold, "the cat sat")]
    [TestCase("the *cat* sat", "cat", MarkdownFormat.Italic, "the cat sat")]
    [TestCase("the ~~cat~~ sat", "cat", MarkdownFormat.Strikethrough, "the cat sat")]
    // Selecting the markers along with the text also removes them.
    [TestCase("the **cat** sat", "**cat**", MarkdownFormat.Bold, "the cat sat")]
    [TestCase("the *cat* sat", "*cat*", MarkdownFormat.Italic, "the cat sat")]
    [TestCase("the ~~cat~~ sat", "~~cat~~", MarkdownFormat.Strikethrough, "the cat sat")]
    public void ApplyFormatting_Emphasis_TogglesMarkers(
        string content,
        string selection,
        MarkdownFormat format,
        string expected)
    {
        var start = content.IndexOf(selection, StringComparison.Ordinal);
        _service.ApplyFormatting(content, format, start, selection.Length).Should().Be(expected);
    }

    [TestCase(MarkdownFormat.Bold, "the **cat** sat")]
    [TestCase(MarkdownFormat.Italic, "the *cat* sat")]
    [TestCase(MarkdownFormat.Strikethrough, "the ~~cat~~ sat")]
    public void ApplyFormatting_Emphasis_IsRoundTrip(MarkdownFormat format, string wrapped)
    {
        const string original = "the cat sat";
        var start = original.IndexOf("cat", StringComparison.Ordinal);

        var applied = _service.ApplyFormatting(original, format, start, "cat".Length);
        applied.Should().Be(wrapped);

        // Re-select the inner word in the wrapped content and toggle the emphasis back off.
        var innerStart = applied.IndexOf("cat", StringComparison.Ordinal);
        _service.ApplyFormatting(applied, format, innerStart, "cat".Length).Should().Be(original);
    }

    // Degenerate selections — a bare marker, or a span with only a leading marker — must not
    // throw: the bounds guards in ToggleInline fall back to wrapping rather than over-running
    // the string (each of these would throw ArgumentOutOfRangeException without the guards).
    [TestCase("**", "**", MarkdownFormat.Bold, "******")]
    [TestCase("*", "*", MarkdownFormat.Italic, "***")]
    [TestCase("**cat", "cat", MarkdownFormat.Bold, "****cat**")]
    public void ApplyFormatting_Emphasis_DegenerateSelection_FallsBackToWrapping(
        string content,
        string selection,
        MarkdownFormat format,
        string expected)
    {
        var start = content.IndexOf(selection, StringComparison.Ordinal);
        _service.ApplyFormatting(content, format, start, selection.Length).Should().Be(expected);
    }
}
