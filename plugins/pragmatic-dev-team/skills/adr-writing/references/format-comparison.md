# ADR Format Comparison

Choose the right format based on team needs and decision complexity.

## Format Overview

| Format | Best For | Complexity | Key Feature |
|--------|----------|------------|-------------|
| **Nygard** | Teams starting with ADRs | Low | Simplest, agile-friendly |
| **MADR** | Structured option comparison | Medium | Explicit alternatives |
| **Y-Statements** | Concise single-sentence | Low | Forces clarity |
| **RFC Style** | Cross-team governance | High | Formal review process |

## Nygard (Original ADR)

**Creator:** Michael Nygard (2011)

**When to use:**
- Teams new to ADRs
- Minimal overhead needed
- Decision context is straightforward

```markdown
# ADR-0012: Use Result Pattern for Domain Errors

## Status
Accepted

## Context
Our codebase uses exceptions for both exceptional circumstances and expected
business rule violations. This obscures intent and makes error handling costly.

## Decision
We will use Result<T, Error> for expected business failures. Exceptions remain
only for truly exceptional circumstances.

## Consequences
- Callers must explicitly handle failures
- More verbose than throwing exceptions
- Learning curve for team
```

**Pros:**
- Minimal structure, quick to write
- Pioneered the ADR concept
- Great for getting started

**Cons:**
- No "options considered" section
- Harder to understand why alternatives were rejected

---

## MADR (Markdown Architectural Decision Records)

**When to use:**
- Need structured comparison of options
- Complex decisions with multiple viable approaches
- Future reviewers need to understand trade-offs

```markdown
# Use PostgreSQL for Transactional Data

## Status
Accepted

## Context and Problem Statement
We need a database for our order management system. Must support ACID transactions,
complex queries, and scale to 10M rows.

## Decision Drivers
* ACID compliance for financial transactions
* Team familiarity
* Operational cost
* Query complexity support

## Considered Options
1. PostgreSQL
2. SQL Server
3. MySQL
4. CockroachDB

## Decision Outcome
Chosen option: **PostgreSQL**, because it provides ACID compliance, excellent
JSON support for flexible schemas, and the team has production experience.

### Consequences
**Good:**
- Strong community, extensive documentation
- Free and open source
- Excellent performance for read-heavy workloads

**Bad:**
- Manual sharding for horizontal scaling
- Slightly more complex replication than MySQL

**Neutral:**
- Need to manage our own backups (vs managed SQL Server)
```

**Pros:**
- Explicit options comparison
- Captures decision drivers
- Good for retrospective analysis

**Cons:**
- More verbose than Nygard
- Can be overkill for simple decisions

---

## Y-Statements

**When to use:**
- Very concise documentation needed
- Decision fits in single sentence
- Communicating decisions to executives

**Format:**
> In the context of `<use case>`, facing `<concern>`, we decided for `<option>` to achieve `<quality>`, accepting `<downside>`.

**Examples:**

```markdown
In the context of **user authentication**, facing **security vs convenience trade-offs**,
we decided for **OAuth2 with JWT tokens** to achieve **stateless authentication**,
accepting **token revocation complexity**.

In the context of **API versioning**, facing **breaking changes management**,
we decided for **URI versioning (/v2/orders)** to achieve **explicit version visibility**,
accepting **URL proliferation over time**.
```

**Pros:**
- Extremely concise
- Forces clarity of thought
- Easy to scan

**Cons:**
- Can become unwieldy for complex decisions
- Limited space for nuance

---

## RFC Style

**When to use:**
- Cross-team or organization-wide decisions
- Need formal review and approval process
- Decisions affect multiple bounded contexts

**Structure:**
```markdown
# RFC-0023: Adopt OpenTelemetry for Observability

## Metadata
- **Authors:** Platform Team
- **Status:** Under Review
- **Reviewers:** @alice, @bob, @charlie
- **Due Date:** 2025-02-01

## Summary
Adopt OpenTelemetry as the standard observability framework across all services.

## Motivation
Current fragmented approach using multiple vendor SDKs creates inconsistency...

## Detailed Design
[Technical implementation details]

## Alternatives Considered
[Other approaches evaluated]

## Migration Plan
[Rollout strategy]

## Open Questions
[Unresolved issues for reviewers]
```

**Pros:**
- Formal governance process
- Distributed review via comments
- Good for org-wide standards

**Cons:**
- Heavy process overhead
- Slower decision-making
- May discourage smaller decisions

---

## Decision Matrix

| Scenario | Recommended Format |
|----------|-------------------|
| First ADR for the team | Nygard |
| Database technology choice | MADR |
| Executive communication | Y-Statement |
| Company-wide standard | RFC |
| Simple library choice | Nygard |
| Architecture pattern adoption | MADR |
| Quick design review | Y-Statement |

## Key Insight

> **Format matters less than adoption.** Pick one, make it easy, get team buy-in.

Start with Nygard. Graduate to MADR when you need structured comparisons. Reserve RFC for cross-team governance.

## Sources

- [ADR Templates](https://adr.github.io/adr-templates/)
- [MADR GitHub](https://github.com/adr/madr)
- [Michael Nygard's Original Post](https://cognitect.com/blog/2011/11/15/documenting-architecture-decisions)
