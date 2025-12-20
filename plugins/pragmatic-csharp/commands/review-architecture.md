---
name: review-architecture
description: Review C# code for adherence to pragmatic architecture principles
argument-hint: "[scope: file path, 'diff', or 'all']"
allowed-tools:
  - Read
  - Grep
  - Glob
  - Bash
  - Task
---

# Review Architecture Command

Review C# code for adherence to 10 pragmatic architecture principles.

## Arguments

- **No argument or `diff`**: Review unstaged git changes (default)
- **File path**: Review specific file (e.g., `src/Handlers/CreateUser.cs`)
- **Feature name**: Review feature folder (e.g., `Word` or `AboutInformation`)
- **`all`**: Full codebase scan (warning: may be slow for large codebases)

## Process

1. **Determine scope** from arguments:
   - If no args or `diff`: Run `git diff --name-only` to get changed `.cs` files
   - If file path: Use that file
   - If `all`: Glob for all `.cs` files in `src/`

2. **Load skill**: Reference csharp-pragmatic-architecture skill for patterns

3. **Launch architecture-reviewer agent** with the identified scope

4. **Report findings** organized by severity:
   - Critical issues (must fix)
   - Important issues (should fix)
   - Suggestions (nice to have)

5. **Offer next steps**:
   - "Want me to fix any of these?"
   - "Should I explain [specific issue] in more detail?"

## Output Format

```
## Architecture Review: [scope]

### Critical Issues (X)
[List of critical violations with file:line references]

### Important Issues (X)
[List of important violations]

### Suggestions (X)
[List of optional improvements]

### Good Patterns Found
[Highlight code that follows principles well]

---
Total: X critical, Y important, Z suggestions
```

## Tips

- Run before commits to catch issues early
- Use file path for focused review during development
- Use `diff` for pre-commit checks
- Use `all` sparingly (generates lots of output)

## Examples

```
/review-architecture
/review-architecture src/Core/Core.Application/Word/CreateDocument.cs
/review-architecture diff
/review-architecture all
```
