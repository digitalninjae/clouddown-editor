namespace CloudDown.Editor.Models;

/// <summary>
/// The outcome of a formatting operation: the new document <see cref="Content"/> together with the
/// selection the editor should apply afterward. The selection lets a host place the caret where the
/// user will continue typing — e.g. on the <c>url</c> placeholder after inserting a link — or keep
/// the affected text highlighted after a wrap or toggle.
/// </summary>
/// <param name="Content">The full document content after the operation.</param>
/// <param name="SelectionStart">The caret/selection start, as an index into <see cref="Content"/>.</param>
/// <param name="SelectionLength">The selection length; <c>0</c> for a collapsed caret.</param>
public readonly record struct FormattingResult(string Content, int SelectionStart, int SelectionLength);
