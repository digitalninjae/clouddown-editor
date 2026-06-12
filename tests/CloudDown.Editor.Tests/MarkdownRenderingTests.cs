using AwesomeAssertions;
using CloudDown.Editor.Services;

namespace CloudDown.Editor.Tests;

/// <summary>
/// Granular, high-volume rendering cases — the data-driven half of the hybrid test
/// strategy (behavioral specs live in the Reqnroll .feature files).
/// </summary>
[TestFixture]
public class MarkdownRenderingTests
{
    private MarkdownService _service = null!;

    // NUnit shares the fixture instance across tests, so reset state in SetUp.
    [SetUp]
    public void SetUp() => _service = new MarkdownService();

    // UseAdvancedExtensions enables auto heading IDs.
    [TestCase("# Heading", "<h1 id=\"heading\">Heading</h1>")]
    [TestCase("**bold**", "<p><strong>bold</strong></p>")]
    [TestCase("*italic*", "<p><em>italic</em></p>")]
    [TestCase("~~struck~~", "<p><del>struck</del></p>")]
    [TestCase("`code`", "<p><code>code</code></p>")]
    [TestCase("> quote", "<blockquote>\n<p>quote</p>\n</blockquote>")]
    public void ToHtml_RendersExpectedFragment(string markdown, string expectedHtml) =>
        _service.ToHtml(markdown).Trim().Should().Be(expectedHtml);

    // A '---' directly under a paragraph line is a Setext H2 underline, not a thematic break, so the
    // HorizontalRule operation must blank-line-separate its rules. These pin that its output renders
    // as rules (and the wrapped text as a paragraph), never as a heading.
    [TestCase("---\n\nthe cat sat\n\n---", "<hr />\n<p>the cat sat</p>\n<hr />")]
    [TestCase("abc\n\n---", "<p>abc</p>\n<hr />")]
    public void ToHtml_HorizontalRuleOutput_RendersAsThematicBreaks(string markdown, string expectedHtml) =>
        _service.ToHtml(markdown).Trim().Should().Be(expectedHtml);
}
