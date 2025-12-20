---
name: architect
description: Run architecture review. Checks dependency direction, boundaries, and DDD patterns.
argument-hint: "[file|module|diff] - Scope to review"
allowed-tools:
  - Read
  - Grep
  - Glob
  - Bash
  - Task
---

# Architecture Review Command

On-demand architecture review using the architecture-reviewer agent.

## Process

1. **Determine scope** from argument:
   - No argument or "diff": Review recent changes
   - Module/directory: Review specific area
   - "design": Focus on overall structure

2. **Load csharp-pragmatic-architecture skill** for patterns

3. **Launch architecture-reviewer agent** checking:
   - Dependency rule compliance
   - Bounded context boundaries
   - DDD patterns (aggregates, value objects)
   - SOLID principles

4. **Report findings** with educational context

## Examples

```
/architect                      # Review recent changes
/architect src/Domain/          # Review domain layer
/architect OrderAggregate.cs    # Review specific aggregate
/architect design               # Overall architecture assessment
```

## What Gets Checked

- Dependencies pointing correct direction
- No infrastructure in domain
- Aggregates properly bounded
- Value objects immutable
- Vertical slice organization
