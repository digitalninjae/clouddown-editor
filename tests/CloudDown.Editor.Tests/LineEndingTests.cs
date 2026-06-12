using AwesomeAssertions;
using CloudDown.Editor.Models;
using CloudDown.Editor.Services;

namespace CloudDown.Editor.Tests;

/// <summary>
/// Line-ending handling (#25): by default the document's own ending is detected and preserved for
/// any newline an operation emits — the per-line rejoin of headings/lists/blockquotes and the
/// separators of code blocks / horizontal rules. An override forces a specific ending, normalizing
/// regardless of the content.
/// </summary>
[TestFixture]
public class LineEndingTests
{
    private MarkdownService _service = null!;

    [SetUp]
    public void SetUp() => _service = new MarkdownService();

    [Test]
    public void Preserve_CrlfContent_RoundTripsThroughHeading()
    {
        const string content = "alpha\r\nbeta";
        _service.ApplyFormatting(content, MarkdownFormat.Header2, 0, content.Length)
            .Content.Should().Be("## alpha\r\n## beta");
    }

    [Test]
    public void Preserve_CrlfContent_RoundTripsThroughList()
    {
        const string content = "milk\r\neggs";
        _service.ApplyFormatting(content, MarkdownFormat.BulletList, 0, content.Length)
            .Content.Should().Be("- milk\r\n- eggs");
    }

    [Test]
    public void Preserve_SingleLineCrlfSpan_KeepsItsEnding()
    {
        // Toggling just the first line of a CRLF document must not strip that line's '\r'.
        const string content = "alpha\r\nbeta";
        _service.ApplyFormatting(content, MarkdownFormat.Header1, 0, 5)
            .Content.Should().Be("# alpha\r\nbeta");
    }

    [Test]
    public void Preserve_CrlfContent_CodeBlockEmitsCrlfFences()
    {
        const string content = "alpha\r\nbeta";
        _service.ApplyFormatting(content, MarkdownFormat.CodeBlock, 0, content.Length)
            .Content.Should().Be("```lang\r\nalpha\r\nbeta\r\n```");
    }

    [Test]
    public void Preserve_CrlfContent_HorizontalRuleEmitsCrlf()
    {
        const string content = "x\r\ncat\r\ny";
        var start = content.IndexOf("cat", StringComparison.Ordinal);
        _service.ApplyFormatting(content, MarkdownFormat.HorizontalRule, start, 3)
            .Content.Should().Be("x\r\n---\r\ncat\r\n---\r\ny");
    }

    [Test]
    public void Preserve_DetectsLfFromContent_NotTheHostNewline()
    {
        // The content already uses LF, so the emitted fences must be LF regardless of the host OS.
        const string content = "a\nb";
        _service.ApplyFormatting(content, MarkdownFormat.CodeBlock, 0, content.Length)
            .Content.Should().Be("```lang\na\nb\n```");
    }

    [Test]
    public void Preserve_NoLineBreakInContent_FallsBackToTheHostNewline()
    {
        var nl = Environment.NewLine;
        _service.ApplyFormatting("hello", MarkdownFormat.CodeBlock, 0, 5)
            .Content.Should().Be($"```lang{nl}hello{nl}```");
    }

    [Test]
    public void Override_Lf_NormalizesCrlfContentOnRejoin()
    {
        const string content = "alpha\r\nbeta";
        _service.FormattingOptions.LineEnding = LineEndingMode.Lf;
        _service.ApplyFormatting(content, MarkdownFormat.Header2, 0, content.Length)
            .Content.Should().Be("## alpha\n## beta");
    }

    [Test]
    public void Override_CrLf_NormalizesLfContentOnRejoin()
    {
        const string content = "alpha\nbeta";
        _service.FormattingOptions.LineEnding = LineEndingMode.CrLf;
        _service.ApplyFormatting(content, MarkdownFormat.Header2, 0, content.Length)
            .Content.Should().Be("## alpha\r\n## beta");
    }

    [Test]
    public void Override_CrLf_OnSingleLineContent_EmitsCrlfFences()
    {
        _service.FormattingOptions.LineEnding = LineEndingMode.CrLf;
        _service.ApplyFormatting("hello", MarkdownFormat.CodeBlock, 0, 5)
            .Content.Should().Be("```lang\r\nhello\r\n```");
    }
}
