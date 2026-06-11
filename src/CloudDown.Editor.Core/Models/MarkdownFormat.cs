namespace CloudDown.Editor.Models;

/// <summary>
/// A Markdown formatting action that can be applied to the editor's current selection.
/// </summary>
public enum MarkdownFormat
{
    Bold,
    Italic,
    Strikethrough,
    Header1,
    Header2,
    Header3,
    Header4,
    Header5,
    Header6,
    BulletList,
    NumberedList,
    TaskList,
    Link,
    Image,
    InlineCode,
    CodeBlock,
    Blockquote,
    HorizontalRule
}
