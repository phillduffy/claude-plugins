# ADR Governance

Managing ADRs at scale: lifecycle, ownership, review, and tooling.

## Status Lifecycle

```
Proposed → Accepted → [Deprecated | Superseded]
    ↓
 Rejected
```

| Status | Meaning | Action |
|--------|---------|--------|
| **Proposed** | Under discussion | Gather feedback, set review deadline |
| **Accepted** | Decision made | Implement, reference in code |
| **Deprecated** | No longer relevant | Context changed, tech/protocol gone |
| **Superseded** | Replaced by newer ADR | Link to replacement ADR |
| **Rejected** | Decided against | Document why, valuable for future |

### Superseding Example

```markdown
# ADR-0003: Use SpecFlow for BDD Testing

## Status
Superseded by [ADR-0042](./0042-use-reqnroll-for-bdd.md)

## Context
[Original context...]

## Decision
[Original decision...]

## Supersession Note
SpecFlow announced end-of-life December 2024. We migrated to Reqnroll,
the community fork. See ADR-0042 for details.
```

---

## Ownership Model

### Distributed Ownership

**Priority order for decision authority:**
1. CEO/CTO (strategic direction)
2. Team Lead (team-level patterns)
3. Individual Expert (domain expertise)

**Best practices:**
- Each ADR has a named owner (not just "architecture team")
- Owner responsible for keeping ADR current
- Distribute ownership to foster buy-in

```markdown
## Metadata
- **Owner:** @jane-doe
- **Team:** Payments
- **Last Review:** 2025-01-15
```

### Change Tracking

- Use Git for versioning (single source of truth)
- PR-based reviews for proposed changes
- Changelog in ADR for significant updates

---

## Review Process

### For Greenfield Projects

- Review at daily standup during early sprints
- Stabilizes in 2-3 sprints as patterns emerge
- High cadence: decisions made quickly

### For Existing Projects

- Regular review meetings (weekly/biweekly)
- Backlog of proposed ADRs
- Prioritize by impact

### Review Deadline

**Set deadlines to avoid analysis paralysis:**

```markdown
## Review Timeline
- **Proposed:** 2025-01-10
- **Review Deadline:** 2025-01-24
- **Decision Required By:** 2025-01-31
```

If no consensus by deadline, owner makes the call.

### Distributed Reviews

- Use PR comments (not email threads)
- @mention relevant stakeholders
- Summarize discussion in ADR itself

---

## Storage Strategies

### Option 1: Git Repository (Recommended)

```
project/
├── docs/
│   └── adr/
│       ├── 0001-record-architecture-decisions.md
│       ├── 0002-use-postgresql-for-persistence.md
│       └── 0003-use-result-pattern-for-errors.md
└── src/
```

**Pros:**
- Versioned with code
- PR-based review workflow
- Traceability (link commits to ADRs)

**Cons:**
- Less accessible to non-technical stakeholders

### Option 2: Wiki

**Pros:**
- Accessible to everyone
- Easy search and navigation

**Cons:**
- No versioning
- Disconnected from code

### Option 3: Hybrid (Recommended for Large Orgs)

- Git as source of truth
- Auto-publish to searchable site (Log4brains)
- Best of both worlds

---

## Scaling Patterns

### Central Repository

For organization-wide decisions:

```
company-architecture/
├── adr/
│   ├── 0001-api-versioning-standard.md
│   ├── 0002-authentication-pattern.md
│   └── 0003-observability-standard.md
```

### Per-Project ADRs

For team-level decisions:

```
order-service/
├── docs/adr/
│   ├── 0001-use-event-sourcing.md
│   └── 0002-saga-for-payment-flow.md
```

### Cross-Reference

Link related ADRs across repos:

```markdown
## Related Decisions
- [Company ADR-0002: Authentication Pattern](https://github.com/company/architecture/adr/0002)
- [Order Service ADR-0001: Event Sourcing](./0001-use-event-sourcing.md)
```

---

## Modern Tooling

### Log4brains (Recommended)

Static site generator for ADRs with search and navigation.

```bash
# Initialize
npx log4brains init

# Preview locally
npx log4brains preview

# Build for deployment
npx log4brains build
```

**Features:**
- Hot reload during editing
- Auto-publish to GitHub Pages
- Searchable web interface
- Supports MADR template

### adr-tools (Classic CLI)

Bash scripts for Nygard format:

```bash
# Install
brew install adr-tools

# Create new ADR
adr new "Use PostgreSQL for persistence"

# List ADRs
adr list

# Generate table of contents
adr generate toc
```

### dotnet-adr (.NET Projects)

Cross-platform .NET global tool:

```bash
# Install
dotnet tool install -g dotnet-adr

# Create ADR
adr new "Use Result Pattern for Errors"

# Initialize templates
adr init
```

### Integration Options

| Tool | Best For |
|------|----------|
| **Log4brains** | Teams wanting searchable site |
| **adr-tools** | Minimalists, Unix users |
| **dotnet-adr** | .NET developers |
| **Backstage ADR plugin** | Teams using Spotify Backstage |

---

## Review Checklist

Before accepting an ADR:

- [ ] Clear problem statement in Context
- [ ] "We will..." statement in Decision
- [ ] At least 2 options considered
- [ ] Consequences include good AND bad
- [ ] Owner assigned
- [ ] Related ADRs linked
- [ ] Deadline for implementation (if applicable)

---

## Anti-Patterns

| Anti-Pattern | Problem | Fix |
|--------------|---------|-----|
| **Email-Driven Architecture** | Multiple sources of truth | All decisions in Git, email just links |
| **Analysis Paralysis** | Forever discussed | Set review deadline |
| **Single Option "Decision"** | No real choice made | Require 2+ options |
| **Missing Consequences** | Can't evaluate later | Include good AND bad |
| **Stale ADRs** | Outdated decisions confuse | Regular review, deprecate obsolete |

## Sources

- [AWS ADR Best Practices](https://docs.aws.amazon.com/prescriptive-guidance/latest/architectural-decision-records/best-practices.html)
- [How to Review ADRs](https://ozimmer.ch/practices/2023/04/05/ADRReview.html)
- [Log4brains](https://github.com/thomvaill/log4brains)
- [dotnet-adr](https://endjin.com/blog/2024/03/adr-a-dotnet-tool-for-creating-and-managing-architecture-decision-records)
