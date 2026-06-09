namespace CloudDown.Editor.Models;

/// <summary>
/// The editing experience presented by the <c>MarkdownEditor</c> control.
/// </summary>
public enum EditorMode
{
    /// <summary>Distraction-free writing with a rich inline preview (syntax hidden).</summary>
    Writer,

    /// <summary>Plain text with syntax highlighting (full Markdown syntax visible).</summary>
    Editor,

    /// <summary>Side-by-side editing and live preview.</summary>
    Split
}
