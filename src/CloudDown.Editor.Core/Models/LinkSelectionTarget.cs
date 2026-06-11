namespace CloudDown.Editor.Models;

/// <summary>
/// Which placeholder a link or image insertion selects afterward, so the caret lands where the
/// user will most likely type next.
/// </summary>
public enum LinkSelectionTarget
{
    /// <summary>Select the <c>url</c> placeholder (the default) — caret ready to type the address.</summary>
    Url,

    /// <summary>Select the link text / image alt — caret ready to type or edit the label.</summary>
    Text
}
