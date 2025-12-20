---
name: test-plan
description: Create test plan for a feature. Uses BDD strategist and test coverage analyst.
argument-hint: "[feature description] - What to create tests for"
allowed-tools:
  - Read
  - Grep
  - Glob
  - Bash
  - Task
---

# Test Plan Command

Create comprehensive test plan using BDD strategist and test coverage analyst.

## Process

1. **Understand the feature** from argument or context

2. **Launch bdd-strategist agent** to:
   - Identify scenarios (happy path, errors, edge cases)
   - Write Gherkin scenarios
   - Suggest Three Amigos questions

3. **Launch test-coverage-analyst agent** to:
   - Recommend test pyramid distribution
   - Identify coverage gaps
   - Suggest edge cases

4. **Output complete test plan**

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
