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
