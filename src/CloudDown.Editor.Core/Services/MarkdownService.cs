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

        return format switch
        {
            // Emphasis markers toggle: re-applying to already-wrapped text removes them.
            MarkdownFormat.Bold => ToggleInline(content, selectionStart, selectionLength, "**"),
            MarkdownFormat.Italic => ToggleInline(content, selectionStart, selectionLength, "*"),
            MarkdownFormat.Strikethrough => ToggleInline(content, selectionStart, selectionLength, "~~"),
            MarkdownFormat.InlineCode => Splice(content, selectionStart, selectionLength, Wrap(selected, "`")),
            MarkdownFormat.Header1 => Splice(content, selectionStart, selectionLength, LinePrefix(selected, "# ")),
            MarkdownFormat.Header2 => Splice(content, selectionStart, selectionLength, LinePrefix(selected, "## ")),
            MarkdownFormat.Header3 => Splice(content, selectionStart, selectionLength, LinePrefix(selected, "### ")),
            MarkdownFormat.Blockquote => Splice(content, selectionStart, selectionLength, LinePrefix(selected, "> ")),
            MarkdownFormat.BulletList => Splice(content, selectionStart, selectionLength, LinePrefix(selected, "- ")),
            _ => throw new NotSupportedException($"Formatting '{format}' is not yet implemented.")
        };
    }

    /// <summary>
    /// Applies an inline emphasis <paramref name="marker"/> (e.g. <c>**</c>) to the selection,
    /// <em>toggling</em> it: if the selection is already wrapped — whether the markers are part
    /// of the selection (<c>**cat**</c>) or sit immediately outside it (<c>cat</c> within
    /// <c>**cat**</c>) — the markers are removed; otherwise the selection is wrapped.
    /// </summary>
    private static string ToggleInline(string content, int start, int length, string marker)
    {
        var selected = content.Substring(start, length);
        var m = marker.Length;

        // Markers are part of the selection, e.g. selecting "**cat**".
        if (length >= 2 * m
            && selected.StartsWith(marker, StringComparison.Ordinal)
            && selected.EndsWith(marker, StringComparison.Ordinal))
        {
            return Splice(content, start, length, selected.Substring(m, length - 2 * m));
        }

        // Markers sit just outside the selection, e.g. selecting "cat" within "**cat**".
        if (start >= m
            && start + length + m <= content.Length
            && content.Substring(start - m, m) == marker
            && content.Substring(start + length, m) == marker)
        {
            return string.Concat(
                content.AsSpan(0, start - m),
                selected.AsSpan(),
                content.AsSpan(start + length + m));
        }

        return Splice(content, start, length, Wrap(selected, marker));
    }

    private static string Splice(string content, int start, int length, string replacement) =>
        string.Concat(content.AsSpan(0, start), replacement, content.AsSpan(start + length));

    private static string Wrap(string text, string marker) => $"{marker}{text}{marker}";

    private static string LinePrefix(string text, string prefix) => $"{prefix}{text}";
}
