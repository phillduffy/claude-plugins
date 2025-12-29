# Test Cases

Manual test cases for validating pragmatic-dev-team plugin components.

## How to Test

1. Load plugin: `claude --plugin-dir E:\Claude\plugins\pragmatic-dev-team --debug`
2. Run test scenarios from each file
3. Verify expected behavior matches actual

## Test Files

| File | What It Tests |
|------|---------------|
| `agents/test-triggers.md` | Agent triggering conditions |
| `skills/test-activation.md` | Skill activation scenarios |
| `hooks/test-hooks.ps1` | Hook execution validation |

## Quick Validation

```bash
# Verify commands available
/help

# Test a command
/review

# Test agent trigger
"Does this follow Clean Architecture?"

# Test hook (modify .cs file)
# Should see: "C# modified. Run /review when ready."
```
