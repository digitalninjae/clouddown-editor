using AwesomeAssertions;
using CloudDown.Editor.Models;
using CloudDown.Editor.Services;

namespace CloudDown.Editor.Tests;

/// <summary>
/// Granular, data-driven cases for the heading formats (H1–H3), which apply at the start of
/// the line(s) the selection touches and toggle / switch level rather than stack.
/// Behavioral coverage lives in <c>Formatting.feature</c>.
/// </summary>
[TestFixture]
public class ApplyFormattingHeadingTests
{
    private MarkdownService _service = null!;

    [SetUp]
    public void SetUp() => _service = new MarkdownService();

    // Adding a heading to a plain line.
    [TestCase("hello", "hello", MarkdownFormat.Header1, "# hello")]
    [TestCase("hello", "hello", MarkdownFormat.Header2, "## hello")]
    [TestCase("hello", "hello", MarkdownFormat.Header3, "### hello")]
    // Re-applying the same level toggles it off.
    [TestCase("# hello", "hello", MarkdownFormat.Header1, "hello")]
    [TestCase("## hello", "hello", MarkdownFormat.Header2, "hello")]
    // Applying a different level switches the prefix (does not stack).
    [TestCase("# hello", "hello", MarkdownFormat.Header3, "### hello")]
    [TestCase("## hello", "hello", MarkdownFormat.Header3, "### hello")]
    [TestCase("### hello", "hello", MarkdownFormat.Header1, "# hello")]
    // A partial-line selection still toggles the whole line, at its start.
    [TestCase("hello world", "world", MarkdownFormat.Header1, "# hello world")]
    [TestCase("# hello world", "world", MarkdownFormat.Header1, "hello world")]
    // The full H1–H6 range is supported (per CommonMark).
    [TestCase("hello", "hello", MarkdownFormat.Header4, "#### hello")]
    [TestCase("hello", "hello", MarkdownFormat.Header5, "##### hello")]
    [TestCase("hello", "hello", MarkdownFormat.Header6, "###### hello")]
    [TestCase("#### hello", "hello", MarkdownFormat.Header4, "hello")]
    [TestCase("#### hello", "hello", MarkdownFormat.Header2, "## hello")]
    [TestCase("###### hello", "hello", MarkdownFormat.Header1, "# hello")]
    // A tab after the hashes also forms a heading (CommonMark), so it's recognized.
    [TestCase("##\tHello", "Hello", MarkdownFormat.Header2, "Hello")]
    [TestCase("##\tHello", "Hello", MarkdownFormat.Header3, "### Hello")]
    public void ApplyFormatting_Heading_TogglesAndSwitchesPerLine(
        string content,
        string selection,
        MarkdownFormat format,
        string expected)
    {
        var start = content.IndexOf(selection, StringComparison.Ordinal);
        _service.ApplyFormatting(content, format, start, selection.Length).Content.Should().Be(expected);
    }

    [Test]
    public void ApplyFormatting_Heading_MultiLine_AppliesToEveryLine()
    {
        const string content = "alpha\nbeta";
        _service.ApplyFormatting(content, MarkdownFormat.Header2, 0, content.Length)
            .Content.Should().Be("## alpha\n## beta");
    }

    [Test]
    public void ApplyFormatting_Heading_MultiLine_TogglesOffWhenAllPresent()
    {
        const string content = "## alpha\n## beta";
        _service.ApplyFormatting(content, MarkdownFormat.Header2, 0, content.Length)
            .Content.Should().Be("alpha\nbeta");
    }

    [Test]
    public void ApplyFormatting_Heading_MultiLine_UnifiesMixedLevels()
    {
        // Not all lines already carry H2, so every line is set to it (switching the existing one).
        const string content = "# alpha\nbeta";
        _service.ApplyFormatting(content, MarkdownFormat.Header2, 0, content.Length)
            .Content.Should().Be("## alpha\n## beta");
    }
}
