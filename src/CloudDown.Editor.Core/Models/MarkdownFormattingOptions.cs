namespace CloudDown.Editor.Models;

/// <summary>
/// Global, host-configurable options that influence how formatting is applied. A consuming editor
/// sets these once — or changes them at runtime — to match the user's preference. Additional global
/// formatting preferences are expected to live here over time.
/// </summary>
public sealed class MarkdownFormattingOptions
{
    /// <summary>
    /// Which placeholder a link/image insertion selects afterward. Defaults to
    /// <see cref="LinkSelectionTarget.Url"/>.
    /// </summary>
    public LinkSelectionTarget LinkSelectionTarget { get; set; } = LinkSelectionTarget.Url;

    /// <summary>
    /// The line ending used for any newline a formatting operation emits. Defaults to
    /// <see cref="LineEndingMode.Preserve"/>, which keeps the document's existing convention.
    /// </summary>
    public LineEndingMode LineEnding { get; set; } = LineEndingMode.Preserve;
}
