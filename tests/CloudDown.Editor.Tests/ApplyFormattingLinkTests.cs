using AwesomeAssertions;
using CloudDown.Editor.Models;
using CloudDown.Editor.Services;

namespace CloudDown.Editor.Tests;

/// <summary>
/// Granular cases for the link and image formats, which insert <c>[text](url)</c> / <c>![alt](url)</c>
/// using the selection as the link text / image alt and return a selection on the <c>url</c>
/// placeholder. Behavioral coverage lives in <c>Formatting.feature</c>.
/// </summary>
[TestFixture]
public class ApplyFormattingLinkTests
{
    private MarkdownService _service = null!;

    [SetUp]
    public void SetUp() => _service = new MarkdownService();

    [Test]
    public void ApplyFormatting_Link_WrapsSelectionAsLinkText()
    {
        const string content = "see Anthropic here";
        var start = content.IndexOf("Anthropic", StringComparison.Ordinal);

        var result = _service.ApplyFormatting(content, MarkdownFormat.Link, start, "Anthropic".Length);

        result.Content.Should().Be("see [Anthropic](url) here");
    }

    [Test]
    public void ApplyFormatting_Image_WrapsSelectionAsAltText()
    {
        const string content = "see logo here";
        var start = content.IndexOf("logo", StringComparison.Ordinal);

        var result = _service.ApplyFormatting(content, MarkdownFormat.Image, start, "logo".Length);

        result.Content.Should().Be("see ![logo](url) here");
    }

    [Test]
    public void ApplyFormatting_Link_EmptySelection_InsertsTextPlaceholder()
    {
        var result = _service.ApplyFormatting("see  here", MarkdownFormat.Link, 4, 0);
        result.Content.Should().Be("see [text](url) here");
    }

    [Test]
    public void ApplyFormatting_Image_EmptySelection_InsertsAltPlaceholder()
    {
        var result = _service.ApplyFormatting("see  here", MarkdownFormat.Image, 4, 0);
        result.Content.Should().Be("see ![alt](url) here");
    }

    [TestCase("see Anthropic here", "Anthropic", MarkdownFormat.Link)]
    [TestCase("see logo here", "logo", MarkdownFormat.Image)]
    public void ApplyFormatting_LinkOrImage_SelectsUrlPlaceholder(
        string content,
        string selection,
        MarkdownFormat format)
    {
        var start = content.IndexOf(selection, StringComparison.Ordinal);

        var result = _service.ApplyFormatting(content, format, start, selection.Length);

        // The returned selection lands on the url placeholder, ready to overtype.
        result.Content.Substring(result.SelectionStart, result.SelectionLength).Should().Be("url");
    }
}
