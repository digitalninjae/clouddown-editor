using CloudDown.Editor.Models;
using Markdig;

namespace CloudDown.Editor.Services;

/// <summary>
/// Default <see cref="IMarkdownService"/> backed by Markdig, configured for
/// CommonMark + common GitHub Flavored Markdown extensions.
/// </summary>
public sealed class MarkdownService : IMarkdownService
{
    private readonly MarkdownPipeline _pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();

    /// <inheritdoc />
    public string ToHtml(string markdown) =>
        Markdown.ToHtml(markdown ?? string.Empty, _pipeline);

    /// <inheritdoc />
    public string ApplyFormatting(string content, MarkdownFormat format, int selectionStart, int selectionLength)
    {
        content ??= string.Empty;

        if (selectionStart < 0 || selectionLength < 0 || selectionStart + selectionLength > content.Length)
            throw new ArgumentOutOfRangeException(nameof(selectionStart), "Selection falls outside the content bounds.");

        var selected = content.Substring(selectionStart, selectionLength);
        var replacement = format switch
        {
            MarkdownFormat.Bold => Wrap(selected, "**"),
            MarkdownFormat.Italic => Wrap(selected, "*"),
            MarkdownFormat.Strikethrough => Wrap(selected, "~~"),
            MarkdownFormat.InlineCode => Wrap(selected, "`"),
            MarkdownFormat.Header1 => LinePrefix(selected, "# "),
            MarkdownFormat.Header2 => LinePrefix(selected, "## "),
            MarkdownFormat.Header3 => LinePrefix(selected, "### "),
            MarkdownFormat.Blockquote => LinePrefix(selected, "> "),
            MarkdownFormat.BulletList => LinePrefix(selected, "- "),
            _ => throw new NotSupportedException($"Formatting '{format}' is not yet implemented.")
        };

        return string.Concat(
            content.AsSpan(0, selectionStart),
            replacement,
            content.AsSpan(selectionStart + selectionLength));
    }

    private static string Wrap(string text, string marker) => $"{marker}{text}{marker}";

    private static string LinePrefix(string text, string prefix) => $"{prefix}{text}";
}
