using AwesomeAssertions;
using CloudDown.Editor.Models;
using CloudDown.Editor.Services;
using Reqnroll;

namespace CloudDown.Editor.Tests.Steps;

[Binding]
public sealed class FormattingSteps
{
    private readonly IMarkdownService _service = new MarkdownService();

    private string _content = string.Empty;
    private int _selectionStart;
    private int _selectionLength;
    private int _resultSelectionStart;
    private int _resultSelectionLength;

    [Given("the editor contains \"(.*)\"")]
    public void GivenTheEditorContains(string content) => _content = content;

    [Given("the text \"(.*)\" is selected")]
    public void GivenTheTextIsSelected(string selection)
    {
        _selectionStart = _content.IndexOf(selection, StringComparison.Ordinal);
        _selectionStart.Should().BeGreaterThanOrEqualTo(0, "the selection must exist within the content");
        _selectionLength = selection.Length;
    }

    [When("I apply (.*) formatting")]
    public void WhenIApplyFormatting(MarkdownFormat format)
    {
        var result = _service.ApplyFormatting(_content, format, _selectionStart, _selectionLength);
        _content = result.Content;
        _resultSelectionStart = result.SelectionStart;
        _resultSelectionLength = result.SelectionLength;
    }

    [Then("the content should be \"(.*)\"")]
    public void ThenTheContentShouldBe(string expected) => _content.Should().Be(expected);

    [Then("the selected text should be \"(.*)\"")]
    public void ThenTheSelectedTextShouldBe(string expected) =>
        _content.Substring(_resultSelectionStart, _resultSelectionLength).Should().Be(expected);
}
