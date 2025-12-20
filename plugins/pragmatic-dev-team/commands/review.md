---
name: review
description: Run code review on specified scope. Reviews code quality, patterns, and best practices.
argument-hint: "[file|diff|all] - Scope to review (default: diff)"
allowed-tools:
  - Read
  - Grep
  - Glob
  - Bash
  - Task
---

# Code Review Command

On-demand code review using the code-reviewer agent.

## Process

1. **Determine scope** from argument:
   - No argument or "diff": Review uncommitted changes (`git diff`)
   - File path: Review specific file(s)
   - "all": Review entire codebase (warn: slow)

2. **Launch code-reviewer agent** with appropriate scope

3. **Report findings** organized by severity:
   - Critical: Must fix before merge
   - Important: Should fix
   - Suggestion: Consider

4. **Offer next steps**:
   - "Want me to fix any of these?"
   - "Should I review related files?"

## Examples

```
/review                    # Review uncommitted changes
/review src/Handlers/      # Review specific directory
/review OrderService.cs    # Review specific file
/review all                # Full codebase review (slow)
```

## Tips

- Run before committing for best results
- Focus on smaller scopes for actionable feedback
- Use "all" sparingly - it can be overwhelming
