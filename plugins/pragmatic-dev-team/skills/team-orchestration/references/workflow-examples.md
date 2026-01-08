# Team Orchestration Workflow Examples

## New Feature: Document Template Validation

```
Task: Add validation for template fields

1. BDD Strategist
   Dispatch: "Define BDD scenarios for template field validation"
   Output:
   - Given/When/Then scenarios
   - Acceptance criteria
   - Edge cases identified
   ✅ Approved

2. Architect
   Dispatch: "Review architecture for template validation"
   Output:
   - Validation belongs in Domain layer
   - Use Specification pattern
   Issues:
   - ❌ ValidationService in wrong layer
   Fix: Move to Domain layer
   Re-review: ✅ Approved

3. Implementation
   [Developer implements based on approved scenarios + architecture]

4. Code Review
   Dispatch: "Review template validation implementation"
   Issues:
   - ❌ Magic strings for errors
   - ❌ Missing null checks
   Fix: Extract constants, add guards
   Re-review: ✅ Approved

5. Test Coverage (optional)
   Dispatch: "Verify test coverage"
   Output: All scenarios covered
   ✅ Approved

Result: Feature ready for merge
```

## Bug Fix: Null Reference Exception

```
Task: Fix null reference in ribbon handler

1. Code Review
   Dispatch: "Analyze null reference bug"
   Output:
   - Root cause: Document accessed before null check
   - Fix: Add guard clause
   ✅ Analysis approved

2. Architect
   Skip: "Simple guard, no architectural impact"

3. Test Coverage
   Dispatch: "Add regression test"
   Output: Add RibbonCommandWithNoDocument test
   ✅ Approved

Result: Bug fix ready for merge
```

## Performance Optimization

```
Task: Improve document loading time

1. Code Review
   Dispatch: "Profile and identify bottlenecks"
   Output:
   - N+1 query in GetTemplates
   - Unnecessary parsing on each load
   ✅ Analysis approved

2. Architect
   Dispatch: "Review optimization approach"
   Output:
   - Add caching layer
   - Batch queries
   Issues:
   - ❌ Proposed cache violates layer boundaries
   Fix: Move cache to Infrastructure
   Re-review: ✅ Approved

3. Implementation + Review
   [Standard loop]

Result: Optimization ready
```

## Command Sequences

```bash
# New feature
/test-plan "Define validation scenarios"
/architect "Review validation design"
[implement]
/review "Review implementation"

# Bug fix
/review "Analyze bug root cause"
[fix]
/test-plan "Add regression test"

# Research first
/research "Compare validation libraries"
[decide based on output]
/test-plan "Define scenarios with chosen approach"
```
