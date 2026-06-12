Feature: Selection formatting
    As a user editing Markdown
    I want to apply formatting to the text I have selected
    So that I can mark up my document without typing the syntax by hand

Scenario: Applying bold to a selected word
    Given the editor contains "hello world"
    And the text "world" is selected
    When I apply Bold formatting
    Then the content should be "hello **world**"

Scenario: Applying italic to a selected word
    Given the editor contains "hello world"
    And the text "hello" is selected
    When I apply Italic formatting
    Then the content should be "*hello* world"

Scenario Outline: Applying inline formatting wraps the selection
    Given the editor contains "<content>"
    And the text "<selection>" is selected
    When I apply <format> formatting
    Then the content should be "<expected>"

    Examples:
        | content     | selection | format        | expected          |
        | the cat sat | cat       | Bold          | the **cat** sat   |
        | the cat sat | cat       | Italic        | the *cat* sat     |
        | the cat sat | cat       | Strikethrough | the ~~cat~~ sat   |
        | the cat sat | cat       | InlineCode    | the `cat` sat     |

Scenario Outline: Re-applying inline code toggles it off
    Given the editor contains "<content>"
    And the text "<selection>" is selected
    When I apply InlineCode formatting
    Then the content should be "<expected>"

    Examples:
        | content       | selection | expected    |
        | the `cat` sat | cat       | the cat sat |
        | the `cat` sat | `cat`     | the cat sat |

Scenario Outline: Re-applying inline emphasis toggles it off
    Given the editor contains "<content>"
    And the text "<selection>" is selected
    When I apply <format> formatting
    Then the content should be "<expected>"

    Examples:
        | content         | selection | format        | expected    |
        | the **cat** sat | cat       | Bold          | the cat sat |
        | the **cat** sat | **cat**   | Bold          | the cat sat |
        | the *cat* sat   | cat       | Italic        | the cat sat |
        | the ~~cat~~ sat | cat       | Strikethrough | the cat sat |

Scenario Outline: Applying a heading toggles the line and switches level
    Given the editor contains "<content>"
    And the text "<selection>" is selected
    When I apply <format> formatting
    Then the content should be "<expected>"

    Examples:
        | content       | selection | format  | expected      |
        | hello         | hello     | Header1 | # hello       |
        | hello         | hello     | Header2 | ## hello      |
        | hello         | hello     | Header6 | ###### hello  |
        | ## hello      | hello     | Header2 | hello         |
        | ## hello      | hello     | Header3 | ### hello     |
        | #### hello    | hello     | Header2 | ## hello      |
        | hello world   | world     | Header1 | # hello world |

Scenario Outline: Applying a list toggles the line and switches type
    Given the editor contains "<content>"
    And the text "<selection>" is selected
    When I apply <format> formatting
    Then the content should be "<expected>"

    Examples:
        | content    | selection | format       | expected   |
        | milk       | milk      | BulletList   | - milk     |
        | milk       | milk      | NumberedList | 1. milk    |
        | milk       | milk      | TaskList     | - [ ] milk |
        | - milk     | milk      | BulletList   | milk       |
        | - [ ] milk | milk      | TaskList     | milk       |
        | - milk     | milk      | NumberedList | 1. milk    |
        | 1. milk    | milk      | TaskList     | - [ ] milk |
        | - [ ] milk | milk      | BulletList   | - milk     |

Scenario Outline: Applying a link or image wraps the selection and inserts a url placeholder
    Given the editor contains "<content>"
    And the text "<selection>" is selected
    When I apply <format> formatting
    Then the content should be "<expected>"

    Examples:
        | content            | selection | format | expected                  |
        | see Anthropic here | Anthropic | Link   | see [Anthropic](url) here |
        | see logo here      | logo      | Image  | see ![logo](url) here     |

Scenario: Applying a link selects the url placeholder for the user to type
    Given the editor contains "see Anthropic here"
    And the text "Anthropic" is selected
    When I apply Link formatting
    Then the content should be "see [Anthropic](url) here"
    And the selected text should be "url"

Scenario: The text selection preference selects the link label instead of the url
    Given the link selection preference is Text
    And the editor contains "see Anthropic here"
    And the text "Anthropic" is selected
    When I apply Link formatting
    Then the content should be "see [Anthropic](url) here"
    And the selected text should be "Anthropic"

Scenario Outline: Applying a blockquote toggles the line at its start
    Given the editor contains "<content>"
    And the text "<selection>" is selected
    When I apply Blockquote formatting
    Then the content should be "<expected>"

    Examples:
        | content       | selection | expected      |
        | hello         | hello     | > hello       |
        | > hello       | hello     | hello         |
        | hello world   | world     | > hello world |

Scenario: Applying a code block wraps the selection in fences and selects the language
    Given the line ending is LF
    And the editor contains "print()"
    And the text "print()" is selected
    When I apply CodeBlock formatting
    Then the content should be "```lang\nprint()\n```"
    And the selected text should be "lang"

Scenario: Applying a horizontal rule to a selection wraps it between rules
    Given the line ending is LF
    And the editor contains "the cat sat"
    And the text "the cat sat" is selected
    When I apply HorizontalRule formatting
    Then the content should be "---\n\nthe cat sat\n\n---"

Scenario: Operations preserve the document's own line ending
    Given the editor contains "alpha\r\nbeta"
    And the text "alpha\r\nbeta" is selected
    When I apply BulletList formatting
    Then the content should be "- alpha\r\n- beta"

Scenario: The line-ending override normalizes regardless of the content
    Given the line ending is LF
    And the editor contains "alpha\r\nbeta"
    And the text "alpha\r\nbeta" is selected
    When I apply BulletList formatting
    Then the content should be "- alpha\n- beta"

Scenario: Applying emphasis with nothing selected inserts empty markers
    Given the editor contains ""
    And the caret is at the start
    When I apply Bold formatting
    Then the content should be "****"
    And the selected text should be "****"

Scenario: Toggling a format off restores the original text
    Given the editor contains "the cat sat"
    And the text "cat" is selected
    When I apply Bold formatting
    And I apply Bold formatting
    Then the content should be "the cat sat"
