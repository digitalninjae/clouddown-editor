# ADR-0002: Test stack — NUnit + Reqnroll + AwesomeAssertions (hybrid)

|              |               |
| ------------ | ------------- |
| **Status**   | Accepted      |
| **Date**     | 2026-06-08    |
| **Deciders** | Brian Cupples |

## Context

The library needs an automated test approach covering two distinct kinds of logic:

- **Editor behaviors** — applying formatting, mode switching, toolbar actions, undo/redo.
  These read well as scenarios and benefit from doubling as living documentation.
- **Granular Markdown processing** — many "input Markdown → expected output" cases
  (Markdig rendering, formatting transforms), where volume matters.

Constraints and forces: the maintainer is fluent in **NUnit** and values its data-driven
attributes; **FluentAssertions** moved to a commercial license at v8, which is undesirable
for an MIT library; a **BDD** style is preferred for behavior specs. The template
default was xUnit + FluentAssertions.

## Decision

Adopt a **hybrid, BDD-led** stack:

- **NUnit** as the test runner (and `[TestCase]`/`[TestCaseSource]` for data-driven cases).
- **Reqnroll** for BDD `.feature` specs, hosted on NUnit via `Reqnroll.NUnit`.
- **AwesomeAssertions** (MIT fork of FluentAssertions) for assertions.

Split: Reqnroll scenarios for **behaviors** (living documentation); NUnit data-driven tests
for the **high-volume granular** cases.

## Options Considered

### Option A: xUnit + FluentAssertions (template default)

| Dimension        | Assessment                           |
| ---------------- | ------------------------------------ |
| Complexity       | Low — ecosystem default              |
| Licensing        | FluentAssertions v8+ is commercial   |
| Data-driven      | Clunkier (`[Theory]`/`[MemberData]`) |
| Team familiarity | Lower (NUnit preferred)              |
| BDD support      | None built-in                        |

**Pros:** De-facto .NET default; strong isolation (instance per test).
**Cons:** Commercial assertion library; weaker data-driven ergonomics; no BDD; not the preferred runner.

### Option B: NUnit + Reqnroll + AwesomeAssertions, hybrid (chosen)

| Dimension        | Assessment                                  |
| ---------------- | ------------------------------------------- |
| Complexity       | Medium — two test styles in one project     |
| Licensing        | All MIT / open-source                       |
| Data-driven      | Excellent (`[TestCase]`/`[TestCaseSource]`) |
| Team familiarity | High (NUnit)                                |
| BDD support      | First-class (Reqnroll → living docs)        |

**Pros:** Open-source throughout; great data-driven ergonomics; BDD specs as documentation; preferred runner.
**Cons:** Two paradigms to maintain; Reqnroll codegen step; NUnit shares fixture instances (state hygiene needed).

### Option C: Pure BDD (every test a Reqnroll scenario)

**Rejected:** Gherkin boilerplate is excessive for the hundreds of low-level parsing cases;
NUnit data-driven tests express those far more concisely.

## Trade-off Analysis

Option A's only real edge — being the ecosystem default — is outweighed by the commercial
assertion licensing and weaker data-driven support, both of which matter for an MIT library
heavy on parsing tests. Pure BDD (C) over-applies a good tool. The **hybrid** uses each tool
where it is strongest: Reqnroll for human-readable behavior specs, NUnit for bulk cases.
AwesomeAssertions neutralizes xUnit's "better assertions" argument while staying MIT.

## Consequences

- **Easier:** behavior specs double as documentation; concise data-driven parsing tests;
  no commercial dependency.
- **Harder:** contributors must understand two test styles; Reqnroll adds a generation step
  (`*.feature.cs`, git-ignored); NUnit's shared fixture instance requires resetting mutable
  state in `[SetUp]`.
- **Revisit if:** the BDD layer sees little use, or Reqnroll/NUnit integration becomes a burden.

## Action Items

1. [x] Add `Reqnroll.NUnit` and `AwesomeAssertions`; remove xUnit/FluentAssertions.
2. [x] Establish hybrid example: a `.feature` behavior spec + NUnit data-driven rendering tests.
3. [x] Update the CLAUDE.md test section to this stack.
4. [ ] Document the test conventions in a CONTRIBUTING / testing guide.
