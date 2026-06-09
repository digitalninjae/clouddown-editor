# Architecture Decision Records

This log captures significant architecture decisions for CloudDown.Editor, one decision per
record, using a [MADR](https://adr.github.io/madr/)-style format. Records are immutable once
**Accepted** — to change a decision, add a new ADR that supersedes the old one (and update the
old record's status to **Superseded by ADR-XXXX**).

## Status values

`Proposed` · `Accepted` · `Deprecated` · `Superseded`

## Log

| ADR                                        | Title                                                                | Status   |
| ------------------------------------------ | -------------------------------------------------------------------- | -------- |
| [0001](0001-native-controls-no-webview.md) | Native platform text controls instead of a WebView                   | Accepted |
| [0002](0002-test-stack.md)                 | Test stack — NUnit + Reqnroll + AwesomeAssertions (hybrid)           | Accepted |
| [0003](0003-separate-core-project.md)      | Separate `CloudDown.Editor.Core` project for testable, UI-free logic | Accepted |
| [0004](0004-central-package-management.md) | Central Package Management for NuGet versions                        | Accepted |

## Adding a new ADR

1. Copy the format of an existing record; number it sequentially (next: **0005**).
2. Start at `Proposed`. An ADR is moved to `Accepted` **only after the maintainer (Brian) has
   reviewed the document and explicitly approved it** — authoring/editing an ADR is not approval.
3. Add a row to the table above.
4. Cross-link related ADRs and the [vision doc](../../vision.md).
