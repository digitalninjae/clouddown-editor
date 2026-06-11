using AwesomeAssertions;
using CloudDown.Editor.Models;
using CloudDown.Editor.Services;

namespace CloudDown.Editor.Tests;

/// <summary>
/// Granular, data-driven cases for the list formats (bullet, numbered, task), which apply at the
/// start of the line(s) the selection touches and toggle / switch type rather than stack. Numbered
/// lists count sequentially from a start value (default 1). Behavioral coverage lives in
/// <c>Formatting.feature</c>.
/// </summary>
[TestFixture]
public class ApplyFormattingListTests
{
    private MarkdownService _service = null!;

    [SetUp]
    public void SetUp() => _service = new MarkdownService();

    // Adding a list marker to a plain line.
    [TestCase("milk", "milk", MarkdownFormat.BulletList, "- milk")]
    [TestCase("milk", "milk", MarkdownFormat.NumberedList, "1. milk")]
    [TestCase("milk", "milk", MarkdownFormat.TaskList, "- [ ] milk")]
    // Re-applying the same kind toggles it off.
    [TestCase("- milk", "milk", MarkdownFormat.BulletList, "milk")]
    [TestCase("1. milk", "milk", MarkdownFormat.NumberedList, "milk")]
    [TestCase("- [ ] milk", "milk", MarkdownFormat.TaskList, "milk")]
    // Applying a different kind switches the marker (does not stack).
    [TestCase("- milk", "milk", MarkdownFormat.NumberedList, "1. milk")]
    [TestCase("- milk", "milk", MarkdownFormat.TaskList, "- [ ] milk")]
    [TestCase("1. milk", "milk", MarkdownFormat.BulletList, "- milk")]
    [TestCase("1. milk", "milk", MarkdownFormat.TaskList, "- [ ] milk")]
    [TestCase("- [ ] milk", "milk", MarkdownFormat.BulletList, "- milk")]
    [TestCase("- [ ] milk", "milk", MarkdownFormat.NumberedList, "1. milk")]
    // A partial-line selection still toggles the whole line, at its start.
    [TestCase("buy milk", "milk", MarkdownFormat.BulletList, "- buy milk")]
    [TestCase("- buy milk", "milk", MarkdownFormat.BulletList, "buy milk")]
    // Recognizes the other CommonMark bullet chars and the ')' / checked-task variants when
    // stripping or switching, even though we always emit "- ", "N. ", and "- [ ] ".
    [TestCase("* milk", "milk", MarkdownFormat.BulletList, "milk")]
    [TestCase("+ milk", "milk", MarkdownFormat.BulletList, "milk")]
    [TestCase("1) milk", "milk", MarkdownFormat.NumberedList, "milk")]
    [TestCase("- [x] milk", "milk", MarkdownFormat.TaskList, "milk")]
    [TestCase("- [X] milk", "milk", MarkdownFormat.BulletList, "- milk")]
    public void ApplyFormatting_List_TogglesAndSwitchesPerLine(
        string content,
        string selection,
        MarkdownFormat format,
        string expected)
    {
        var start = content.IndexOf(selection, StringComparison.Ordinal);
        _service.ApplyFormatting(content, format, start, selection.Length).Content.Should().Be(expected);
    }

    [Test]
    public void ApplyFormatting_BulletList_MultiLine_AppliesToEveryLine()
    {
        const string content = "milk\neggs";
        _service.ApplyFormatting(content, MarkdownFormat.BulletList, 0, content.Length)
            .Content.Should().Be("- milk\n- eggs");
    }

    [Test]
    public void ApplyFormatting_NumberedList_MultiLine_NumbersSequentially()
    {
        const string content = "milk\neggs\nbread";
        _service.ApplyFormatting(content, MarkdownFormat.NumberedList, 0, content.Length)
            .Content.Should().Be("1. milk\n2. eggs\n3. bread");
    }

    [Test]
    public void ApplyFormatting_NumberedList_StartValue_NumbersFromThatValue()
    {
        // A custom start lets the list continue a preceding one (here, picking up at 4).
        const string content = "milk\neggs\nbread";
        _service.ApplyFormatting(content, MarkdownFormat.NumberedList, 0, content.Length, numberedListStart: 4)
            .Content.Should().Be("4. milk\n5. eggs\n6. bread");
    }

    [Test]
    public void ApplyFormatting_NumberedList_DefaultsToStartingAtOne()
    {
        _service.ApplyFormatting("milk", MarkdownFormat.NumberedList, 0, 4)
            .Content.Should().Be("1. milk");
    }

    [Test]
    public void ApplyFormatting_NumberedList_NegativeStart_Throws()
    {
        var act = () => _service.ApplyFormatting("milk", MarkdownFormat.NumberedList, 0, 4, numberedListStart: -1);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void ApplyFormatting_List_MultiLine_TogglesOffWhenAllPresent()
    {
        const string content = "- milk\n- eggs";
        _service.ApplyFormatting(content, MarkdownFormat.BulletList, 0, content.Length)
            .Content.Should().Be("milk\neggs");
    }

    [Test]
    public void ApplyFormatting_NumberedList_MultiLine_TogglesOffRegardlessOfNumbers()
    {
        const string content = "1. milk\n2. eggs\n3. bread";
        _service.ApplyFormatting(content, MarkdownFormat.NumberedList, 0, content.Length)
            .Content.Should().Be("milk\neggs\nbread");
    }

    [Test]
    public void ApplyFormatting_List_MultiLine_UnifiesMixedKinds()
    {
        // Not all lines are bullets, so every line is set to bullet (switching the numbered one).
        const string content = "- milk\n1. eggs";
        _service.ApplyFormatting(content, MarkdownFormat.BulletList, 0, content.Length)
            .Content.Should().Be("- milk\n- eggs");
    }

    [Test]
    public void ApplyFormatting_BulletToNumbered_MultiLine_RenumbersSequentially()
    {
        const string content = "- milk\n- eggs\n- bread";
        _service.ApplyFormatting(content, MarkdownFormat.NumberedList, 0, content.Length)
            .Content.Should().Be("1. milk\n2. eggs\n3. bread");
    }
}
