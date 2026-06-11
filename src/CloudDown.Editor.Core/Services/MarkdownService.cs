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
    public string ApplyFormatting(string content, MarkdownFormat format, int selectionStart, int selectionLength, int numberedListStart = 1)
    {
        if (selectionStart < 0 || selectionLength < 0 || selectionStart + selectionLength > content.Length)
            throw new ArgumentOutOfRangeException(nameof(selectionStart),
                "Selection falls outside the content bounds.");
        if (numberedListStart < 0)
            throw new ArgumentOutOfRangeException(nameof(numberedListStart),
                "A numbered list cannot start below zero.");

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
            MarkdownFormat.Header4 => ToggleHeading(content, selectionStart, selectionLength, 4),
            MarkdownFormat.Header5 => ToggleHeading(content, selectionStart, selectionLength, 5),
            MarkdownFormat.Header6 => ToggleHeading(content, selectionStart, selectionLength, 6),
            MarkdownFormat.Blockquote => Splice(content, selectionStart, selectionLength, LinePrefix(selected, "> ")),
            // Lists toggle per line and switch type rather than stack, mirroring headings.
            MarkdownFormat.BulletList => ToggleList(content, selectionStart, selectionLength, ListKind.Bullet, 1),
            MarkdownFormat.NumberedList => ToggleList(content, selectionStart, selectionLength, ListKind.Numbered, numberedListStart),
            MarkdownFormat.TaskList => ToggleList(content, selectionStart, selectionLength, ListKind.Task, 1),
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
    /// Applies an ATX heading of <paramref name="level"/> (1–6) to every line the selection
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

        var toggled = lines.All(line => HeadingLevel(line) == level)
            ? lines.Select(StripHeading)
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

    // ATX heading level (1–6) if the line is a heading — 1–6 '#' followed by a space, tab, or
    // end of line (per CommonMark) — otherwise 0.
    private static int HeadingLevel(string line)
    {
        var hashes = 0;
        while (hashes < line.Length && line[hashes] == '#')
            hashes++;
        if (hashes is < 1 or > 6)
            return 0;
        return hashes == line.Length || line[hashes] is ' ' or '\t' ? hashes : 0;
    }

    // Removes an ATX heading marker (the '#' run and its trailing space/tab) from a line, if present.
    private static string StripHeading(string line)
    {
        var level = HeadingLevel(line);
        if (level == 0)
            return line;
        return level < line.Length && line[level] is ' ' or '\t' ? line[(level + 1)..] : line[level..];
    }

    // A list marker carried by a single line.
    private enum ListKind { None, Bullet, Numbered, Task }

    /// <summary>
    /// Applies a list marker of <paramref name="kind"/> to every line the selection touches, as a
    /// uniform toggle: if all touched lines already carry that exact kind it is removed; otherwise
    /// each line is set to it, replacing any existing list marker (so the type switches rather than
    /// stacks). Numbered lists count sequentially from <paramref name="numberStart"/>.
    /// </summary>
    private static string ToggleList(string content, int start, int length, ListKind kind, int numberStart)
    {
        // Expand the selection to the full lines it touches.
        var spanStart = LineStart(content, start);
        var spanEnd = LineEnd(content, length > 0 ? start + length - 1 : start);
        var lines = content.Substring(spanStart, spanEnd - spanStart).Split('\n');

        var toggled = lines.All(line => ListKindOf(line, out _) == kind)
            ? lines.Select(StripList)
            : lines.Select((line, i) => kind switch
            {
                ListKind.Bullet => "- " + StripList(line),
                ListKind.Task => "- [ ] " + StripList(line),
                ListKind.Numbered => $"{numberStart + i}. " + StripList(line),
                _ => line
            });

        return Splice(content, spanStart, spanEnd - spanStart, string.Join('\n', toggled));
    }

    // The list marker a line carries, with the length of that marker (so it can be stripped).
    // Task is checked before Bullet because a task marker ("- [ ] ") begins with a bullet one.
    private static ListKind ListKindOf(string line, out int markerLength)
    {
        markerLength = 0;

        // Task: a bullet char, "[ ]"/"[x]"/"[X]", then a space, e.g. "- [ ] ".
        if (line.Length >= 6 &&
            line[0] is '-' or '*' or '+' && line[1] == ' ' &&
            line[2] == '[' && line[3] is ' ' or 'x' or 'X' && line[4] == ']' && line[5] == ' ')
        {
            markerLength = 6;
            return ListKind.Task;
        }

        // Bullet: a bullet char followed by a space.
        if (line.Length >= 2 && line[0] is '-' or '*' or '+' && line[1] == ' ')
        {
            markerLength = 2;
            return ListKind.Bullet;
        }

        // Numbered: one or more digits, a '.' or ')' delimiter, then a space (per CommonMark).
        var digits = 0;
        while (digits < line.Length && char.IsAsciiDigit(line[digits]))
            digits++;
        if (digits > 0 && digits + 1 < line.Length && line[digits] is '.' or ')' && line[digits + 1] == ' ')
        {
            markerLength = digits + 2;
            return ListKind.Numbered;
        }

        return ListKind.None;
    }

    // Removes a list marker (bullet, numbered, or task) from a line, if present.
    private static string StripList(string line) =>
        ListKindOf(line, out var markerLength) == ListKind.None ? line : line[markerLength..];

    private static string Splice(string content, int start, int length, string replacement) =>
        string.Concat(content.AsSpan(0, start), replacement, content.AsSpan(start + length));

    private static string Wrap(string text, string marker) => $"{marker}{text}{marker}";

    private static string LinePrefix(string text, string prefix) => $"{prefix}{text}";
}
