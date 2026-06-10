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
        Markdown.ToHtml(markdown, _pipeline);

    /// <inheritdoc />
    public string ApplyFormatting(string content, MarkdownFormat format, int selectionStart, int selectionLength)
    {
        if (selectionStart < 0 || selectionLength < 0 || selectionStart + selectionLength > content.Length)
            throw new ArgumentOutOfRangeException(nameof(selectionStart),
                "Selection falls outside the content bounds.");

        var selected = content.Substring(selectionStart, selectionLength);

        return format switch
        {
            // Emphasis markers toggle: re-applying to already-wrapped text removes them.
            MarkdownFormat.Bold => ToggleInline(content, selectionStart, selectionLength, "**"),
            MarkdownFormat.Italic => ToggleInline(content, selectionStart, selectionLength, "*"),
            MarkdownFormat.Strikethrough => ToggleInline(content, selectionStart, selectionLength, "~~"),
            MarkdownFormat.InlineCode => Splice(content, selectionStart, selectionLength, Wrap(selected, "`")),
            // Headings toggle per line and switch level rather than stack.
            MarkdownFormat.Header1 => ToggleHeading(content, selectionStart, selectionLength, 1),
            MarkdownFormat.Header2 => ToggleHeading(content, selectionStart, selectionLength, 2),
            MarkdownFormat.Header3 => ToggleHeading(content, selectionStart, selectionLength, 3),
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
        var markerLength = marker.Length;
        var replacement = selected;

        // Markers are part of the selection, e.g., selecting "**cat**".
        if (length >= 2 * markerLength &&
            selected.StartsWith(marker, StringComparison.Ordinal) &&
            selected.EndsWith(marker, StringComparison.Ordinal))
        {
            replacement = selected.Substring(markerLength, length - 2 * markerLength);
        }
        // Markers sit just outside the selection, e.g., selecting "cat" within "**cat**":
        // replace the whole "**cat**" span (selection + both markers) with just the selection.
        else if (start >= markerLength &&
                 start + length + markerLength <= content.Length &&
                 content.Substring(start - markerLength, markerLength) == marker &&
                 content.Substring(start + length, markerLength) == marker)
        {
            start -= markerLength;
            length += 2 * markerLength;
        }
        else
        {
            replacement = Wrap(selected, marker);
        }

        return Splice(content, start, length, replacement);
    }

    /// <summary>
    /// Applies an ATX heading of <paramref name="level"/> (1–3) to every line the selection
    /// touches, as a uniform toggle: if all touched lines already carry that exact heading it is
    /// removed; otherwise each line is set to it, replacing any existing heading prefix (so the
    /// level switches rather than stacks). Works on a partial-line selection — the heading
    /// always applies at the start of the line.
    /// </summary>
    private static string ToggleHeading(string content, int start, int length, int level)
    {
        var prefix = new string('#', level) + " ";

        // Expand the selection to the full lines it touches.
        var spanStart = LineStart(content, start);
        var spanEnd = LineEnd(content, length > 0 ? start + length - 1 : start);
        var lines = content.Substring(spanStart, spanEnd - spanStart).Split('\n');

        var toggled = lines.All(line => line.StartsWith(prefix, StringComparison.Ordinal))
            ? lines.Select(line => line[prefix.Length..])
            : lines.Select(line => prefix + StripHeading(line));

        return Splice(content, spanStart, spanEnd - spanStart, string.Join('\n', toggled));
    }

    // Start of the line containing index (just after the previous newline, or 0).
    private static int LineStart(string content, int index)
    {
        var i = Math.Min(index, content.Length);
        while (i > 0 && content[i - 1] != '\n')
            i--;
        return i;
    }

    // End of the line containing index (the next newline, or the content length).
    private static int LineEnd(string content, int index)
    {
        var i = Math.Min(index, content.Length);
        while (i < content.Length && content[i] != '\n')
            i++;
        return i;
    }

    // Removes a leading ATX heading marker ("#"…"# ") from a line, if present.
    private static string StripHeading(string line)
    {
        var hashes = 0;
        while (hashes < line.Length && line[hashes] == '#')
            hashes++;
        return hashes > 0 && hashes < line.Length && line[hashes] == ' ' ? line[(hashes + 1)..] : line;
    }

    private static string Splice(string content, int start, int length, string replacement) =>
        string.Concat(content.AsSpan(0, start), replacement, content.AsSpan(start + length));

    private static string Wrap(string text, string marker) => $"{marker}{text}{marker}";

    private static string LinePrefix(string text, string prefix) => $"{prefix}{text}";
}
