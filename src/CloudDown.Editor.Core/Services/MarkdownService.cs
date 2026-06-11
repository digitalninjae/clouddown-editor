using System.Text.RegularExpressions;
using CloudDown.Editor.Models;
using Markdig;

namespace CloudDown.Editor.Services;

/// <summary>
/// Default <see cref="IMarkdownService"/> backed by Markdig, configured for
/// CommonMark + common GitHub Flavored Markdown extensions.
/// </summary>
public sealed partial class MarkdownService : IMarkdownService
{
    private readonly MarkdownPipeline _pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();

    /// <inheritdoc />
    public string ToHtml(string markdown) =>
        Markdown.ToHtml(markdown, _pipeline);

    /// <inheritdoc />
    public FormattingResult ApplyFormatting(string content, MarkdownFormat format, int selectionStart, int selectionLength, int numberedListStart = 1)
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
            MarkdownFormat.InlineCode => ReplaceAndSelect(content, selectionStart, selectionLength, Wrap(selected, "`")),
            // Headings toggle per line and switch level rather than stack.
            MarkdownFormat.Header1 => ToggleHeading(content, selectionStart, selectionLength, 1),
            MarkdownFormat.Header2 => ToggleHeading(content, selectionStart, selectionLength, 2),
            MarkdownFormat.Header3 => ToggleHeading(content, selectionStart, selectionLength, 3),
            MarkdownFormat.Header4 => ToggleHeading(content, selectionStart, selectionLength, 4),
            MarkdownFormat.Header5 => ToggleHeading(content, selectionStart, selectionLength, 5),
            MarkdownFormat.Header6 => ToggleHeading(content, selectionStart, selectionLength, 6),
            MarkdownFormat.Blockquote => ReplaceAndSelect(content, selectionStart, selectionLength, LinePrefix(selected, "> ")),
            // Lists toggle per line and switch type rather than stack, mirroring headings.
            MarkdownFormat.BulletList => ToggleBulletList(content, selectionStart, selectionLength),
            MarkdownFormat.NumberedList => ToggleNumberedList(content, selectionStart, selectionLength, numberedListStart),
            MarkdownFormat.TaskList => ToggleTaskList(content, selectionStart, selectionLength),
            // Links/images insert [text](url) / ![alt](url), selecting the url placeholder.
            MarkdownFormat.Link => InsertLink(content, selectionStart, selectionLength, isImage: false),
            MarkdownFormat.Image => InsertLink(content, selectionStart, selectionLength, isImage: true),
            _ => throw new NotSupportedException($"Formatting '{format}' is not yet implemented.")
        };
    }

    /// <summary>
    /// Applies an inline emphasis <paramref name="marker"/> (e.g. <c>**</c>) to the selection,
    /// <em>toggling</em> it: if the selection is already wrapped — whether the markers are part
    /// of the selection (<c>**cat**</c>) or sit immediately outside it (<c>cat</c> within
    /// <c>**cat**</c>) — the markers are removed; otherwise the selection is wrapped.
    /// </summary>
    private static FormattingResult ToggleInline(string content, int start, int length, string marker)
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

        return ReplaceAndSelect(content, start, length, replacement);
    }

    /// <summary>
    /// Applies an ATX heading of <paramref name="level"/> (1–6) to every line the selection
    /// touches, as a uniform toggle: if all touched lines already carry that exact heading it is
    /// removed; otherwise each line is set to it, replacing any existing heading prefix (so the
    /// level switches rather than stacks). Works on a partial-line selection — the heading
    /// always applies at the start of the line.
    /// </summary>
    private static FormattingResult ToggleHeading(string content, int start, int length, int level)
    {
        var prefix = new string('#', level) + " ";
        var (spanStart, spanLength, lines) = GetSelectionFullLines(content, start, length);

        var toggled = lines.All(line => HeadingLevel(line) == level)
            ? lines.Select(StripHeading)
            : lines.Select(line => prefix + StripHeading(line));

        return ReplaceAndSelect(content, spanStart, spanLength, string.Join('\n', toggled));
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

    /// <summary>
    /// Expands the selection <c>[<paramref name="start"/>, start + <paramref name="length"/>)</c>
    /// to the full lines it touches and returns those lines together with the span they occupy,
    /// so a per-line transform can be spliced back into <paramref name="content"/>. Shared by the
    /// per-line formats (headings, lists).
    /// </summary>
    private static (int Start, int Length, string[] Lines) GetSelectionFullLines(string content, int start, int length)
    {
        var spanStart = LineStart(content, start);
        var spanEnd = LineEnd(content, length > 0 ? start + length - 1 : start);
        return (spanStart, spanEnd - spanStart, content[spanStart..spanEnd].Split('\n'));
    }

    // Each list toggle applies its marker to every line the selection touches, as a uniform toggle:
    // if all touched lines already carry that kind it is removed; otherwise each line is set to it,
    // replacing any existing list marker (so the type switches rather than stacks).

    private static FormattingResult ToggleBulletList(string content, int start, int length)
    {
        var (spanStart, spanLength, lines) = GetSelectionFullLines(content, start, length);
        var toggled = lines.All(IsBullet)
            ? lines.Select(StripList)
            : lines.Select(line => "- " + StripList(line));
        return ReplaceAndSelect(content, spanStart, spanLength, string.Join('\n', toggled));
    }

    private static FormattingResult ToggleTaskList(string content, int start, int length)
    {
        var (spanStart, spanLength, lines) = GetSelectionFullLines(content, start, length);
        var toggled = lines.All(IsTask)
            ? lines.Select(StripList)
            : lines.Select(line => "- [ ] " + StripList(line));
        return ReplaceAndSelect(content, spanStart, spanLength, string.Join('\n', toggled));
    }

    // Numbered lists count sequentially from numberStart so a list can continue a preceding one.
    private static FormattingResult ToggleNumberedList(string content, int start, int length, int numberStart)
    {
        var (spanStart, spanLength, lines) = GetSelectionFullLines(content, start, length);
        var toggled = lines.All(line => IsNumbered(line, out _))
            ? lines.Select(StripList)
            : lines.Select((line, i) => $"{numberStart + i}. " + StripList(line));
        return ReplaceAndSelect(content, spanStart, spanLength, string.Join('\n', toggled));
    }

    // A task item: a bullet char, "[ ]"/"[x]"/"[X]", then a space, e.g. "- [ ] ".
    [GeneratedRegex(@"^[*+-] \[[xX ]\] ")]
    private static partial Regex TaskMarker();

    private static bool IsTask(string line) => TaskMarker().IsMatch(line);

    // A bullet item: a bullet char followed by a space — but NOT a task, whose marker begins the
    // same way, so the check must exclude it (this encodes the task-before-bullet precedence).
    private static bool IsBullet(string line) =>
        line.Length >= 2 && line[0] is '-' or '*' or '+' && line[1] == ' ' && !IsTask(line);

    // A numbered item: one or more digits, a '.' or ')' delimiter, then a space (per CommonMark).
    [GeneratedRegex(@"^\d+[.)] ")]
    private static partial Regex NumberedMarker();

    private static bool IsNumbered(string line, out int markerLength)
    {
        var match = NumberedMarker().Match(line);
        markerLength = match.Success ? match.Length : 0;
        return match.Success;
    }

    // Removes a list marker (bullet, numbered, or task) from a line, if present. Task is checked
    // before bullet because a task marker ("- [ ] ") begins with a bullet one.
    private static string StripList(string line)
    {
        if (IsTask(line))
            return line[6..];
        if (IsBullet(line))
            return line[2..];
        if (IsNumbered(line, out var markerLength))
            return line[markerLength..];
        return line;
    }

    /// <summary>
    /// Inserts a link <c>[text](url)</c> or image <c>![alt](url)</c> at the selection, using the
    /// selected text as the link text / image alt (or a <c>text</c>/<c>alt</c> placeholder when the
    /// selection is empty), and selecting the <c>url</c> placeholder so the caret lands where the
    /// user types the address.
    /// </summary>
    private static FormattingResult InsertLink(string content, int start, int length, bool isImage)
    {
        const string url = "url";
        var prefix = isImage ? "![" : "[";
        var text = length > 0 ? content.Substring(start, length) : (isImage ? "alt" : "text");

        var replacement = $"{prefix}{text}]({url})";
        // The url placeholder sits just past the opening prefix, the text, and the "](" delimiter.
        var urlStart = start + prefix.Length + text.Length + 2;
        return new FormattingResult(Splice(content, start, length, replacement), urlStart, url.Length);
    }

    private static string Splice(string content, int start, int length, string replacement) =>
        string.Concat(content.AsSpan(0, start), replacement, content.AsSpan(start + length));

    // Splices replacement into [start, start+length) and selects the whole inserted replacement,
    // so the affected text stays highlighted after the operation.
    private static FormattingResult ReplaceAndSelect(string content, int start, int length, string replacement) =>
        new(Splice(content, start, length, replacement), start, replacement.Length);

    private static string Wrap(string text, string marker) => $"{marker}{text}{marker}";

    private static string LinePrefix(string text, string prefix) => $"{prefix}{text}";
}
