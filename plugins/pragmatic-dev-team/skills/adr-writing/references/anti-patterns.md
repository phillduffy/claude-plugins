# ADR Anti-Patterns

Common mistakes in ADR creation, review, and maintenance.

## Decision-Making Anti-Patterns

### No Decision

**Symptom:** Endless discussion, fear of wrong choice.

**Example:**
```markdown
## Decision
We should probably use PostgreSQL, but we're not sure. Maybe we should
wait for more information. Let's discuss in the next sprint.
```

**Fix:** Set a deadline. Make the decision with confidence level:

```markdown
## Decision
We will use PostgreSQL.

**Confidence:** Medium - may revisit if we exceed 100M rows.
```

---

### Groundhog Day

**Symptom:** Same discussion repeats every few months because decision wasn't captured.

**Fix:** Write ADR immediately after decision. Link in future discussions.

```markdown
See [ADR-0012](./0012-use-postgresql.md) for prior decision.
If context has changed, propose superseding ADR.
```

---

### Sprint/Rush

**Symptom:** Only one option considered. Short-term focus, no long-term consequences.

**Example:**
```markdown
## Considered Options
1. PostgreSQL (team knows it)

## Decision
PostgreSQL because we know it.
```

**Fix:** Research at least 2 alternatives. Document mid/long-term consequences.

```markdown
## Considered Options
1. PostgreSQL - team expertise, proven at scale
2. CockroachDB - distributed by default, higher learning curve
3. SQL Server - managed Azure option, licensing cost

## Decision
PostgreSQL, because...
```

---

## Content Anti-Patterns

### Fairy Tale

**Symptom:** Only pros listed. No downsides acknowledged.

**Example:**
```markdown
## Consequences
- Great performance
- Easy to use
- Team loves it
```

**Fix:** Be honest. Disclose trade-offs and risks:

```markdown
## Consequences
### Good
- Strong query performance for our access patterns
- Team has production experience

### Bad
- Manual sharding if we exceed single-node capacity
- Requires operational expertise for replication

### Risks
- If write volume 10x, may need to revisit
```

---

### Tunnel Vision

**Symptom:** Only considers developer perspective. Ignores ops, security, maintenance.

**Example:**
```markdown
## Context
We need a fast framework. Express.js is fast.

## Decision
Use Express.js.
```

**Fix:** Consider full lifecycle:

```markdown
## Context
We need a web framework. Considerations:
- Development speed
- Team experience
- Security patches cadence
- Long-term maintenance
- Monitoring/debugging story
```

---

### Wishful Thinking (Tautology)

**Symptom:** Circular reasoning. "We use X because X is good."

**Example:**
```markdown
## Decision
We will use microservices because microservices are the modern approach.
```

**Fix:** Trace to actual requirements:

```markdown
## Decision
We will use microservices because:
- Teams can deploy independently (shipping speed)
- Different services have different scaling profiles
- Isolation of failure domains

This is NOT a good fit if: single team, <10 devs, or shared data model.
```

---

## Communication Anti-Patterns

### Email-Driven Architecture

**Symptom:** Decision buried in email thread. Multiple conflicting "sources of truth."

**Fix:**
- ADRs live in Git/wiki ONLY
- Email/Slack just links to ADR
- Reply with "decision captured in ADR-0012" and link

---

### Analysis Paralysis

**Symptom:** Months of discussion, no decision. Perfect is enemy of good.

**Fix:** Set review deadline:

```markdown
## Timeline
- Proposed: 2025-01-15
- Review deadline: 2025-01-29
- If no consensus: Owner decides by 2025-01-31
```

---

### Power Game

**Symptom:** "I'm senior, so I'm right." Hierarchy trumps technical argument.

**Example:**
> "I've been doing this for 20 years. Trust me, we need Oracle."

**Fix:** Focus on technical arguments and context:

```markdown
## Discussion Points
1. What are our actual requirements?
2. What evidence supports each option?
3. What is the cost of being wrong?
```

---

## Review Anti-Patterns

### Offended Reaction

**Symptom:** Defensive pushback when ADR is questioned. Takes critique personally.

**Fix:**
- Stay factual, focus on requirements
- "How does this address [requirement]?" not "This is wrong"
- Separate ego from architecture

---

### I Told You So

**Symptom:** Retrospective bragging. "I knew microservices wouldn't work."

**Fix:**
- Context matters - different projects, different requirements
- Update ADR with learnings, not blame
- Focus on "what we learned" not "who was right"

---

## Maintenance Anti-Patterns

### ADR Graveyard

**Symptom:** ADRs written but never updated. Outdated decisions confuse new team members.

**Fix:**
- Quarterly ADR review
- Deprecate/supersede when context changes
- Link ADRs to related code (in comments)

---

### ADR #0001 is Missing

**Symptom:** No ADR explaining the ADR format itself.

**Fix:** First ADR should be meta:

```markdown
# ADR-0001: Record Architecture Decisions

## Status
Accepted

## Decision
We will use ADRs to document significant architecture decisions.
Format: Nygard (Status, Context, Decision, Consequences).
Storage: docs/adr/ in each repository.
```

---

### Orphan ADRs

**Symptom:** ADRs not linked from code. Developers don't know they exist.

**Fix:**
```csharp
// Architecture: See ADR-0012 for Result pattern decision
// https://github.com/company/repo/docs/adr/0012-result-pattern.md
public class Result<T>
{
    // ...
}
```

---

## Quick Reference

| Anti-Pattern | Symptom | One-Line Fix |
|--------------|---------|--------------|
| No Decision | Endless discussion | Set deadline |
| Groundhog Day | Repeated debates | Capture in ADR immediately |
| Sprint/Rush | One option only | Require 2+ alternatives |
| Fairy Tale | Only pros | Require good AND bad |
| Tunnel Vision | Dev-only view | Consider full lifecycle |
| Wishful Thinking | Circular logic | Trace to requirements |
| Email-Driven | Multiple sources | Git/wiki only |
| Analysis Paralysis | Months of debate | Review deadline |
| Power Game | Hierarchy wins | Focus on evidence |
| ADR Graveyard | Stale content | Quarterly review |

## Sources

- [How to Create ADRs (and How Not To)](https://ozimmer.ch/practices/2023/04/03/ADRCreation.html)
- [ADR Review Anti-Patterns](https://ozimmer.ch/practices/2023/04/05/ADRReview.html)
- [Architecture Decision Records Governance](https://dellenny.com/architecture-decision-records-adrs-a-lightweight-governance-model-for-software-architecture/)
