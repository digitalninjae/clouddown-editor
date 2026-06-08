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

    [Test]
    public void ToHtml_NullInput_ReturnsEmpty() =>
        _service.ToHtml(null!).Should().BeEmpty();
}
