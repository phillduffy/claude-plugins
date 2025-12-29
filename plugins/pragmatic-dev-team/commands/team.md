---
name: team
description: Get comprehensive team review. Coordinates multiple specialists for thorough assessment.
argument-hint: "[scope or 'summary'] - What to review, or 'summary' for session wrap-up"
allowed-tools:
  - Read
  - Grep
  - Glob
  - Bash
  - Task
  - TodoWrite
---

# Team Review Command

Comprehensive review using the team-coordinator to orchestrate multiple specialists.

## Process

1. **Determine mode** from argument:
   - "summary" or no argument: End-of-session summary
   - Scope specified: Comprehensive review of that area

2. **Launch team-coordinator agent** which:
   - Assesses what specialists are needed
   - Coordinates their work
   - Deduplicates findings
   - Presents unified report

3. **Output coordinated team assessment**

## Examples

```
/team                          # Session summary
/team summary                  # Same as above
/team the authentication module  # Full team review of auth
/team ready for PR             # Pre-PR comprehensive check
```

## Consolidated Specialists

Depending on context:
- **code-reviewer** - Quality, performance, observability, accessibility
- **architecture-reviewer** - Design, refactoring, legacy code
- **security-reviewer** - OWASP, vulnerabilities
- **bdd-strategist** - Requirements, test coverage, exploratory testing
- **issue-specialist** - Issues, research, build-vs-buy

## Output

```markdown
## Team Review Summary

### Critical (must fix)
- [Findings]

### Important (should fix)
- [Findings]

### Suggestions
- [Findings]

### Good Patterns Found
- [Praise]
```
