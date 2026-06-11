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
    public void ApplyFormatting_LinkOrImage_SelectsUrlPlaceholderByDefault(
        string content,
        string selection,
        MarkdownFormat format)
    {
        var start = content.IndexOf(selection, StringComparison.Ordinal);

        var result = _service.ApplyFormatting(content, format, start, selection.Length);

        // The default LinkSelectionTarget.Url lands the selection on the url placeholder.
        result.Content.Substring(result.SelectionStart, result.SelectionLength).Should().Be("url");
    }

    [TestCase("see Anthropic here", "Anthropic", MarkdownFormat.Link, "Anthropic")]
    [TestCase("see logo here", "logo", MarkdownFormat.Image, "logo")]
    public void ApplyFormatting_LinkOrImage_TextPreference_SelectsTheLabel(
        string content,
        string selection,
        MarkdownFormat format,
        string expectedSelectedText)
    {
        _service.FormattingOptions.LinkSelectionTarget = LinkSelectionTarget.Text;
        var start = content.IndexOf(selection, StringComparison.Ordinal);

        var result = _service.ApplyFormatting(content, format, start, selection.Length);

        result.Content.Substring(result.SelectionStart, result.SelectionLength).Should().Be(expectedSelectedText);
    }

    [Test]
    public void ApplyFormatting_Link_TextPreference_EmptySelection_SelectsPlaceholderLabel()
    {
        _service.FormattingOptions.LinkSelectionTarget = LinkSelectionTarget.Text;

        var result = _service.ApplyFormatting("see  here", MarkdownFormat.Link, 4, 0);

        result.Content.Should().Be("see [text](url) here");
        result.Content.Substring(result.SelectionStart, result.SelectionLength).Should().Be("text");
    }
}
