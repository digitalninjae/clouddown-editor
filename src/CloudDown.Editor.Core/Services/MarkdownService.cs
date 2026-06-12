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
    public MarkdownFormattingOptions FormattingOptions { get; set; } = new();

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

        // The line ending used for any separator an operation emits (block ops, per-line rejoins),
        // resolved once from the options / content so the whole call is consistent.
        var eol = ResolveLineEnding(content);

        return format switch
        {
            // Emphasis markers toggle: re-applying to already-wrapped text removes them.
            MarkdownFormat.Bold => ToggleInline(content, selectionStart, selectionLength, "**"),
            MarkdownFormat.Italic => ToggleInline(content, selectionStart, selectionLength, "*"),
            MarkdownFormat.Strikethrough => ToggleInline(content, selectionStart, selectionLength, "~~"),
            MarkdownFormat.InlineCode => ToggleInline(content, selectionStart, selectionLength, "`"),
            // Headings toggle per line and switch level rather than stack.
            MarkdownFormat.Header1 => ToggleHeading(content, selectionStart, selectionLength, 1, eol),
            MarkdownFormat.Header2 => ToggleHeading(content, selectionStart, selectionLength, 2, eol),
            MarkdownFormat.Header3 => ToggleHeading(content, selectionStart, selectionLength, 3, eol),
            MarkdownFormat.Header4 => ToggleHeading(content, selectionStart, selectionLength, 4, eol),
            MarkdownFormat.Header5 => ToggleHeading(content, selectionStart, selectionLength, 5, eol),
            MarkdownFormat.Header6 => ToggleHeading(content, selectionStart, selectionLength, 6, eol),
            MarkdownFormat.Blockquote => ToggleBlockquote(content, selectionStart, selectionLength, eol),
            // Lists toggle per line and switch type rather than stack, mirroring headings.
            MarkdownFormat.BulletList => ToggleBulletList(content, selectionStart, selectionLength, eol),
            MarkdownFormat.NumberedList => ToggleNumberedList(content, selectionStart, selectionLength, numberedListStart, eol),
            MarkdownFormat.TaskList => ToggleTaskList(content, selectionStart, selectionLength, eol),
            // Code block fences and horizontal rules sit on their own lines, so they emit eol.
            MarkdownFormat.CodeBlock => ToggleCodeBlock(content, selectionStart, selectionLength, eol),
            MarkdownFormat.HorizontalRule => InsertHorizontalRule(content, selectionStart, selectionLength, eol),
            // Links/images insert [text](url) / ![alt](url); the configured target picks which
            // placeholder is selected (url by default).
            MarkdownFormat.Link => InsertLink(content, selectionStart, selectionLength, isImage: false, FormattingOptions.LinkSelectionTarget),
            MarkdownFormat.Image => InsertLink(content, selectionStart, selectionLength, isImage: true, FormattingOptions.LinkSelectionTarget),
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
    private static FormattingResult ToggleHeading(string content, int start, int length, int level, string eol)
    {
        var prefix = new string('#', level) + " ";
        var (spanStart, spanLength, lines) = GetSelectionFullLines(content, start, length);

        var toggled = lines.All(line => HeadingLevel(line) == level)
            ? lines.Select(StripHeading)
            : lines.Select(line => prefix + StripHeading(line));

        return ReplaceAndSelect(content, spanStart, spanLength, string.Join(eol, toggled));
    }

    // Start of the line containing index (just after the previous newline, or 0).
    private static int LineStart(string content, int index)
    {
        var i = Math.Min(index, content.Length);
        while (i > 0 && content[i - 1] != '\n')
            i--;
        return i;
    }

    // End of the line containing index: the start of the next line break (CR or LF), or the content
    // length. Stopping before the break means a CRLF line's span never includes a trailing '\r',
    // so it cannot be lost when the line is transformed and rejoined.
    private static int LineEnd(string content, int index)
    {
        var i = Math.Min(index, content.Length);
        while (i < content.Length && content[i] is not ('\n' or '\r'))
            i++;
        return i;
    }

    // Splits text into lines on LF, dropping a trailing CR from each so CRLF and LF content yield
    // the same line content. Callers rejoin with the resolved line ending, so reading is universal
    // while writing honors the configured / detected ending.
    private static string[] SplitLines(string text)
    {
        var lines = text.Split('\n');
        for (var i = 0; i < lines.Length; i++)
            if (lines[i].EndsWith('\r'))
                lines[i] = lines[i][..^1];
        return lines;
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
    /// per-line formats (headings, lists, blockquotes).
    /// </summary>
    private static (int Start, int Length, string[] Lines) GetSelectionFullLines(string content, int start, int length)
    {
        var spanStart = LineStart(content, start);
        var spanEnd = LineEnd(content, length > 0 ? start + length - 1 : start);
        return (spanStart, spanEnd - spanStart, SplitLines(content[spanStart..spanEnd]));
    }

    // Each list toggle applies its marker to every line the selection touches, as a uniform toggle:
    // if all touched lines already carry that kind it is removed; otherwise each line is set to it,
    // replacing any existing list marker (so the type switches rather than stacks).

    private static FormattingResult ToggleBulletList(string content, int start, int length, string eol)
    {
        var (spanStart, spanLength, lines) = GetSelectionFullLines(content, start, length);
        var toggled = lines.All(IsBullet)
            ? lines.Select(StripList)
            : lines.Select(line => "- " + StripList(line));
        return ReplaceAndSelect(content, spanStart, spanLength, string.Join(eol, toggled));
    }

    private static FormattingResult ToggleTaskList(string content, int start, int length, string eol)
    {
        var (spanStart, spanLength, lines) = GetSelectionFullLines(content, start, length);
        var toggled = lines.All(IsTask)
            ? lines.Select(StripList)
            : lines.Select(line => "- [ ] " + StripList(line));
        return ReplaceAndSelect(content, spanStart, spanLength, string.Join(eol, toggled));
    }

    // Numbered lists count sequentially from numberStart so a list can continue a preceding one.
    private static FormattingResult ToggleNumberedList(string content, int start, int length, int numberStart, string eol)
    {
        var (spanStart, spanLength, lines) = GetSelectionFullLines(content, start, length);
        var toggled = lines.All(line => IsNumbered(line, out _))
            ? lines.Select(StripList)
            : lines.Select((line, i) => $"{numberStart + i}. " + StripList(line));
        return ReplaceAndSelect(content, spanStart, spanLength, string.Join(eol, toggled));
    }

    // Blockquotes apply a "> " marker to every line the selection touches, toggling per the same
    // uniform rule as headings/lists: if all touched lines are already quoted the marker is removed,
    // otherwise it is added (re-quoting an already-quoted line nests it, mirroring CommonMark).
    private static FormattingResult ToggleBlockquote(string content, int start, int length, string eol)
    {
        var (spanStart, spanLength, lines) = GetSelectionFullLines(content, start, length);
        var toggled = lines.All(IsBlockquote)
            ? lines.Select(StripBlockquote)
            : lines.Select(line => "> " + line);
        return ReplaceAndSelect(content, spanStart, spanLength, string.Join(eol, toggled));
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

    // A blockquote line: a '>' marker at the start (per CommonMark, the following space is optional).
    private static bool IsBlockquote(string line) => line.StartsWith('>');

    // Removes one level of blockquote marker ('>' and one optional following space) from a line.
    private static string StripBlockquote(string line)
    {
        if (!IsBlockquote(line))
            return line;
        var rest = line[1..];
        return rest.StartsWith(' ') ? rest[1..] : rest;
    }

    /// <summary>
    /// Inserts a link <c>[text](url)</c> or image <c>![alt](url)</c> at the selection, using the
    /// selected text as the link text / image alt (or a <c>text</c>/<c>alt</c> placeholder when the
    /// selection is empty). The returned selection lands on the placeholder named by
    /// <paramref name="selectionTarget"/> — the <c>url</c> (default) or the text/alt — so the caret
    /// is ready where the user types next.
    /// </summary>
    private static FormattingResult InsertLink(string content, int start, int length, bool isImage, LinkSelectionTarget selectionTarget)
    {
        const string url = "url";
        var prefix = isImage ? "![" : "[";
        var text = length > 0 ? content.Substring(start, length) : (isImage ? "alt" : "text");

        var spliced = Splice(content, start, length, $"{prefix}{text}]({url})");

        // The text starts just past the opening prefix; the url just past the text and "](".
        var textStart = start + prefix.Length;
        var urlStart = textStart + text.Length + 2;
        return selectionTarget == LinkSelectionTarget.Text
            ? new FormattingResult(spliced, textStart, text.Length)
            : new FormattingResult(spliced, urlStart, url.Length);
    }

    private static string Splice(string content, int start, int length, string replacement) =>
        string.Concat(content.AsSpan(0, start), replacement, content.AsSpan(start + length));

    // Splices replacement into [start, start+length) and selects the whole inserted replacement,
    // so the affected text stays highlighted after the operation.
    private static FormattingResult ReplaceAndSelect(string content, int start, int length, string replacement) =>
        new(Splice(content, start, length, replacement), start, replacement.Length);

    private static string Wrap(string text, string marker) => $"{marker}{text}{marker}";

    /// <summary>
    /// Wraps the selected line(s) in a fenced code block — an opening <c>```lang</c> fence and a
    /// closing <c>```</c> fence, each on its own line — and lands the selection on the <c>lang</c>
    /// placeholder so the user can name the language. Toggles: when the touched lines already form
    /// a fenced block, the fences are stripped instead and the inner content re-selected.
    /// </summary>
    private static FormattingResult ToggleCodeBlock(string content, int start, int length, string eol)
    {
        const string lang = "lang";
        const string fence = "```";
        var spanStart = LineStart(content, start);
        var spanEnd = LineEnd(content, length > 0 ? start + length - 1 : start);
        var span = content[spanStart..spanEnd];
        var lines = SplitLines(span);

        // Already fenced (open + close fence lines around a body): strip the fences, keep the body.
        if (lines.Length >= 2 && IsFenceLine(lines[0]) && IsFenceLine(lines[^1]))
        {
            var inner = string.Join(eol, lines[1..^1]);
            return ReplaceAndSelect(content, spanStart, spanEnd - spanStart, inner);
        }

        var spliced = Splice(content, spanStart, spanEnd - spanStart, $"{fence}{lang}{eol}{span}{eol}{fence}");
        // The lang placeholder sits immediately after the opening fence on the first line.
        return new FormattingResult(spliced, spanStart + fence.Length, lang.Length);
    }

    // A fence line opens or closes a code block: three or more backticks at the line start.
    private static bool IsFenceLine(string line) => line.StartsWith("```", StringComparison.Ordinal);

    /// <summary>
    /// Inserts a horizontal rule (<c>---</c>). With a selection, the touched lines are wrapped with a
    /// rule before and after, each blank-line separated, and the wrapped text stays selected. With no
    /// selection, a single rule is inserted on its own line at the caret. A <c>---</c> directly under
    /// a paragraph line is a Setext heading underline, not a thematic break, so a blank line is always
    /// emitted before a rule (and after, for symmetry) unless the content already provides one.
    /// </summary>
    private static FormattingResult InsertHorizontalRule(string content, int start, int length, string eol)
    {
        const string rule = "---";

        if (length > 0)
        {
            var spanStart = LineStart(content, start);
            var spanEnd = LineEnd(content, start + length - 1);
            var span = content[spanStart..spanEnd];
            var lead = LeadingSeparator(content, spanStart, eol);
            var trail = TrailingSeparator(content, spanEnd, eol);
            // The span text is itself blank-line separated from each rule, so both rules parse as
            // thematic breaks rather than turning the text into a Setext heading.
            var replacement = $"{lead}{rule}{eol}{eol}{span}{eol}{eol}{rule}{trail}";
            var spliced = Splice(content, spanStart, spanEnd - spanStart, replacement);
            // Re-select the original text, now sitting between the two rules.
            return new FormattingResult(spliced, spanStart + lead.Length + rule.Length + 2 * eol.Length, span.Length);
        }

        var before = LeadingSeparator(content, start, eol);
        var after = TrailingSeparator(content, start, eol);
        var inserted = Splice(content, start, 0, $"{before}{rule}{after}");
        return new FormattingResult(inserted, start + before.Length, rule.Length);
    }

    // The break(s) to emit before a rule inserted at pos so it sits on its own line with a blank line
    // above it: nothing if a blank line is already there, one EOL if pos is at a line start (the line
    // above is non-blank text), otherwise two (break out of the current line, then the blank line).
    private static string LeadingSeparator(string content, int pos, string eol) =>
        HasBlankLineBefore(content, pos) ? string.Empty
        : pos > 0 && content[pos - 1] == '\n' ? eol
        : eol + eol;

    // Mirror of LeadingSeparator for the content after a rule: nothing at the document end or when a
    // blank line already follows, one EOL when pos is at a line end, otherwise two.
    private static string TrailingSeparator(string content, int pos, string eol) =>
        HasBlankLineAfter(content, pos) ? string.Empty
        : pos == content.Length || content[pos] is '\n' or '\r' ? eol
        : eol + eol;

    // A blank line sits immediately before pos (or pos is the document start), handling LF and CRLF.
    private static bool HasBlankLineBefore(string content, int pos)
    {
        var before = content[..pos];
        return before.Length == 0 || before.EndsWith("\n\n", StringComparison.Ordinal)
            || before.EndsWith("\r\n\r\n", StringComparison.Ordinal);
    }

    // A blank line sits immediately after pos (or pos is the document end), handling LF and CRLF.
    private static bool HasBlankLineAfter(string content, int pos)
    {
        var after = content[pos..];
        return after.Length == 0 || after.StartsWith("\n\n", StringComparison.Ordinal)
            || after.StartsWith("\r\n\r\n", StringComparison.Ordinal);
    }

    // Resolves the line ending to emit: the configured override, or the document's own ending.
    private string ResolveLineEnding(string content) =>
        FormattingOptions.LineEnding switch
        {
            LineEndingMode.Lf => "\n",
            LineEndingMode.CrLf => "\r\n",
            _ => DetectLineEnding(content)
        };

    // Detects the document's line ending from its first break (CRLF vs LF), falling back to the
    // host's newline when there is none to detect (empty or single-line content).
    private static string DetectLineEnding(string content)
    {
        var newline = content.IndexOf('\n');
        if (newline < 0)
            return Environment.NewLine;
        return newline > 0 && content[newline - 1] == '\r' ? "\r\n" : "\n";
    }
}
