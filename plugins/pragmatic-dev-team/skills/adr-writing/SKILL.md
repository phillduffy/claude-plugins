---
name: ADR Writing
description: This skill should be used when creating Architecture Decision Records, documenting decisions, capturing architectural rationale, or discussing ADR format. Based on Michael Nygard's ADR format and best practices.
version: 0.1.0
---

# Architecture Decision Records (ADRs)

Lightweight documents capturing architecturally significant decisions, based on Michael Nygard's format.

## Why ADRs?

- **Future understanding** - Why was this decision made?
- **Onboarding** - New team members understand context
- **Avoid re-litigating** - Decision already made and documented
- **Track evolution** - See how architecture evolved

## ADR Format

```markdown
# ADR-XXXX: [Short Title in Noun Phrase Form]

## Status
[Proposed | Accepted | Deprecated | Superseded by ADR-YYYY]

## Context
[Describe the forces at play, including technical, political, social, and project constraints. This is the "why" - what situation led to this decision?]

## Decision
[State the decision clearly and concisely. Start with "We will..." This is the "what".]

## Consequences
[Describe the resulting context after applying the decision. Include positive, negative, and neutral consequences. This is the "so what".]
```

## Example ADR

```markdown
# ADR-0012: Use Result Pattern Instead of Exceptions for Domain Errors

## Status
Accepted

## Context
Our current codebase uses exceptions for both exceptional circumstances (database down, network failure) and expected business rule violations (invalid email, insufficient funds). This creates several problems:

- Exception handling is expensive
- Exception flow is hard to follow
- Callers don't know if a method might throw
- We lose specific error information in generic catch blocks

We need a way to distinguish expected failures from exceptional circumstances while maintaining type safety.

## Decision
We will use the Result<T, Error> pattern from CSharpFunctionalExtensions for all expected business failures.

- Domain methods return `Result<T, Error>` instead of throwing
- Exceptions remain only for truly exceptional circumstances
- Error types are defined in static `DomainErrors` class
- Railway-oriented programming for composing operations

## Consequences

### Positive
- Explicit error handling - callers must handle failures
- Type-safe errors with specific information
- Easier to test error paths
- No hidden control flow via exceptions
- Better performance for expected failures

### Negative
- Learning curve for team
- More verbose than throwing exceptions
- Requires consistent adoption across codebase
- Need to update existing code gradually

### Neutral
- Adds CSharpFunctionalExtensions dependency
- Changes coding style significantly
```

## Best Practices

### Do
- Use noun phrases for titles ("Use X" not "Should we use X?")
- Write full sentences, not bullet fragments
- Include all consequences (positive, negative, neutral)
- Link to related ADRs
- Keep each ADR focused on one decision

### Don't
- Delete old ADRs - mark as deprecated/superseded
- Make ADRs too long - aim for 1-2 pages
- Include implementation details - that's for docs
- Reuse ADR numbers - sequential and permanent
- Skip the context - future readers need it

## ADR Lifecycle

```
Proposed → Accepted → [Deprecated | Superseded]
    ↑          ↓
    └── Rejected

Status transitions:
- Proposed: Under discussion
- Accepted: Decision made
- Deprecated: No longer applies
- Superseded: Replaced by another ADR
```

## When to Write an ADR

| Write ADR | Skip ADR |
|-----------|----------|
| Significant architectural choice | Implementation detail |
| Technology selection | Library version update |
| Pattern adoption | Bug fix |
| Breaking change | Minor refactor |
| Will affect future decisions | Easily reversible |

## File Naming

```
docs/adr/
├── 0001-record-architecture-decisions.md
├── 0002-use-postgresql-for-persistence.md
├── 0003-use-result-pattern-for-errors.md
└── 0004-adopt-vertical-slice-architecture.md
```

## Template Variations

### MADR (More Detailed)
Adds: Options Considered, Pros/Cons of each option

### Y-Statements
"In the context of [context], facing [concern], we decided [decision], to achieve [goal], accepting [consequences]."

## Quick Reference

| Section | Purpose | Tips |
|---------|---------|------|
| Status | Current state | Keep updated |
| Context | Why deciding | Include forces/constraints |
| Decision | What decided | "We will..." |
| Consequences | Impact | Good, bad, and neutral |
