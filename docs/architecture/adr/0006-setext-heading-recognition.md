# ADR-0006: Recognize Setext headings in the tokenizer; insert ATX only

|              |                                           |
| ------------ | ----------------------------------------- |
| **Status**   | Proposed                                  |
| **Date**     | 2026-06-15                                |
| **Deciders** | Brian Cupples                             |
| **Relates**  | [ADR-0001](0001-native-controls-no-webview.md), [ADR-0003](0003-separate-core-project.md) |

## Context

Markdown has two heading syntaxes. **ATX** headings prefix a line with one to six `#` characters
(`## Title`). **Setext** headings underline a line of text with `=` (level 1) or `-` (level 2):

```markdown
Title
=====
```

The two interact through a CommonMark precedence rule that already bit us. A line of `-`
characters can be **either** a Setext H2 underline **or** a thematic break (`<hr />`); when both
readings are possible, CommonMark gives the **Setext heading** precedence. So `text\n---` renders
as `<h2>`, while `text\n\n---` (blank line between) renders as `<hr />`. Feature #10's horizontal-rule
op emitted `---` directly under the preceding line, so its output was silently reinterpreted as a
Setext underline and rendered the text as a heading. #11 fixed that op to blank-line-separate its
rules; this ADR settles how the **tokenizer** (#12) and **`ApplyFormatting`** should treat Setext
going forward, so the question is decided once rather than re-litigated per feature.

Two separate concerns are easy to conflate:

- **Recognition** — when the editor highlights existing/imported text, does a Setext heading get
  colored as a heading?
- **Insertion** — does the formatting toolbar offer a "make this a Setext heading" command?

Markdig parses Setext headings into the same `HeadingBlock` node as ATX, exposing `IsSetext` and
`Level`. So recognition costs essentially nothing: the construct already arrives classified.

## Decision

1. **Recognize Setext H1/H2 in the tokenizer.** A `HeadingBlock` — whether ATX or Setext — emits a
   `Heading` token spanning the whole construct (for Setext, the text line plus its underline). This
   means imported documents highlight correctly and a `---` underneath a line of text is colored as
   a heading, never mistaken for a thematic break. No Setext-specific code is required beyond
   walking `HeadingBlock`; `IsSetext` need not even be inspected.

2. **`ApplyFormatting` inserts ATX only.** The heading commands (`Header1`–`Header6`) continue to
   produce `#`-prefixed lines. We do **not** add a Setext-insertion command.

The rationale for (2): ATX covers all six heading levels, is unambiguous, dominates real-world
Markdown, and is what every existing heading toggle/round-trip in Feature #5 already produces.
Setext expresses only levels 1–2 and reintroduces exactly the `---` precedence hazard above. Adding
a second way to author headings would expand the public formatting surface and its test matrix for
no practical gain.

## Options Considered

### Option A: Recognize Setext, insert ATX only (chosen)

| Dimension     | Assessment                                                                     |
| ------------- | ------------------------------------------------------------------------------ |
| Recognition   | Full — imported Setext headings highlight; no `---`-as-heading misclassification |
| Authoring     | One consistent heading syntax (ATX, all six levels)                            |
| Cost          | ~Zero — `HeadingBlock` already covers both; no extra insertion path or tests   |

### Option B: Recognize and also insert Setext (for H1/H2)

| Dimension     | Assessment                                                              |
| ------------- | ----------------------------------------------------------------------- |
| Recognition   | Same as A                                                               |
| Authoring     | Two heading syntaxes; toolbar/level mapping must choose between them     |
| Cost          | New insertion op, level-1/2-only special-casing, the `---` hazard, more tests |

### Option C: Ignore Setext entirely (treat as paragraph + thematic break)

| Dimension     | Assessment                                                                  |
| ------------- | --------------------------------------------------------------------------- |
| Recognition   | Wrong — Setext headings render as `<h1>`/`<h2>` but would highlight as body text, and `text\n---` would be miscolored as a rule, diverging from the rendered output |
| Authoring     | n/a                                                                         |
| Cost          | Would require *suppressing* information Markdig already provides            |

## Trade-off Analysis

Recognition is non-negotiable: the tokenizer's job is to match how the document actually renders,
and Markdig renders Setext as a heading. Option C would make highlighting diverge from output and
revive the `---` misclassification, so it is rejected. Between A and B, the only difference is
whether we *author* Setext. B buys a rarely-used alternate syntax at the price of a larger API,
level-restricted special-casing, and the precedence hazard — while A keeps one canonical authoring
path (ATX) with no loss of expressivity. A is the minimal decision that makes highlighting correct.

## Consequences

- **Easier:** the tokenizer treats all headings uniformly; imported docs with Setext headings
  highlight; the `text\n---` vs `text\n\n---` distinction is reflected in highlighting, consistent
  with rendering and with #11's HR fix.
- **Harder:** nothing materially. Authors who type a Setext heading by hand still get correct
  highlighting; they simply won't find a toolbar button that *produces* one.
- **Revisit if:** users ask to author Setext headings, or a downstream consumer needs to distinguish
  Setext from ATX in the token stream (at which point a token sub-kind or a flag could be added — it
  is available on `HeadingBlock.IsSetext`).

## Action Items

1. [x] Tokenizer emits a `Heading` token for every `HeadingBlock` (ATX and Setext), covered by tests
   asserting `Title\n---` → heading and `Title\n\n---` → no token.
2. [ ] Keep `ApplyFormatting` heading commands ATX-only (no Setext-insertion op) — status quo; no change.
