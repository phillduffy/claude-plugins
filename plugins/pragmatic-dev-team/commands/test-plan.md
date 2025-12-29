---
name: test-plan
description: Create test plan for a feature. Uses BDD strategist for scenarios, coverage, and exploratory testing.
argument-hint: "[feature description] - What to create tests for"
allowed-tools:
  - Read
  - Grep
  - Glob
  - Bash
  - Task
  - TodoWrite
  - AskUserQuestion
---

# Test Plan Command

Create comprehensive test plan using bdd-strategist agent.

## Process

1. **Understand the feature** from argument or context

2. **Launch bdd-strategist agent** to:
   - Define user story with acceptance criteria
   - Write Gherkin scenarios
   - Analyze test coverage gaps
   - Identify edge cases using testing heuristics
   - Suggest exploratory testing charters

3. **Output complete test plan**

## Examples

```
/test-plan password reset feature
/test-plan the new checkout flow
/test-plan OrderService.ProcessOrder method
```

## Output

- User story with acceptance criteria
- Gherkin scenarios
- Test pyramid recommendation
- Edge cases to cover
- Questions to clarify
