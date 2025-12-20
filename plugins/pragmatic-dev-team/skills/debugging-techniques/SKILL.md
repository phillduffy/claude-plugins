---
name: Debugging Techniques
description: This skill should be used when debugging issues, investigating bugs, troubleshooting errors, or applying systematic debugging approaches. Based on David Agans' "Debugging: The 9 Indispensable Rules" and practical debugging patterns.
version: 0.1.0
---

# Debugging Techniques

Systematic approaches to finding and fixing bugs, based on David Agans' "Debugging: The 9 Indispensable Rules for Finding Even the Most Elusive Software and Hardware Problems".

## The 9 Rules

### 1. Understand the System
Before debugging, know how it's supposed to work.

- Read the documentation
- Understand the architecture
- Know your tools (debugger, profiler, logs)
- Understand the data flow

### 2. Make It Fail
Consistent reproduction is essential.

```
Can you make it fail?
├── Yes, every time → Great, proceed
├── Sometimes → Find the pattern (timing, data, sequence)
└── Can't reproduce → Collect more data, different environment
```

**Techniques:**
- Start with exact failure conditions
- Simplify step by step
- Record what you change
- Note what makes it better/worse

### 3. Quit Thinking and Look
Gather data instead of guessing.

```csharp
// DON'T: Guess and change
"Maybe it's a threading issue..." *changes code*

// DO: Add observation
_logger.LogDebug("Value at checkpoint: {Value}", variable);
// or use debugger breakpoint
```

**Ways to look:**
- Debugger (breakpoints, watch)
- Logging (structured, with context)
- Metrics (timing, counts)
- Traces (request flow)

### 4. Divide and Conquer
Binary search through the problem space.

```
System works end-to-end?
├── Input correct? → Check processing
├── Processing correct? → Check output
└── Where does correct become incorrect? ← Found it
```

**Strategies:**
- Start from a known good state
- Find where behavior changes
- Bisect commits (`git bisect`)
- Comment out code blocks

### 5. Change One Thing at a Time
Rifle approach, not shotgun.

```
❌ Bad: Changed 5 things, now it works. Which one fixed it?
✅ Good: Changed one thing, tested, recorded, repeat
```

**Rules:**
- Make one change
- Test
- If not fixed, revert
- Record what you tried

### 6. Keep an Audit Trail
Document everything.

```markdown
## Debugging Session: [Issue]

### Hypothesis 1: Database connection timeout
- Tried: Increased timeout to 60s
- Result: Still fails
- Conclusion: Not timeout related

### Hypothesis 2: Race condition in initialization
- Tried: Added lock around init
- Result: Works!
- Root cause: Confirmed
```

### 7. Check the Plug
Look for simple, obvious problems first.

**Common "plugs":**
- Is it configured correctly?
- Is the service running?
- Is the database connected?
- Are environment variables set?
- Did deployment actually happen?
- Is it the right version?

### 8. Get a Fresh View
Explain the problem to someone else.

**Techniques:**
- Rubber duck debugging
- Ask a colleague
- Write it down (often reveals the issue)
- Take a break, come back fresh

### 9. If You Didn't Fix It, It Ain't Fixed
Verify the actual fix, not assumptions.

```
✅ Prove the fix:
1. Reproduce the bug
2. Apply fix
3. Confirm bug is gone
4. Verify fix addresses root cause
5. Check for regressions
```

## Debugging Strategies

### Binary Search (Git Bisect)
```bash
git bisect start
git bisect bad HEAD
git bisect good v1.0
# Git checks out middle commit
# Test, then:
git bisect good  # or git bisect bad
# Repeat until found
```

### Add Strategic Logging
```csharp
public Result<Order, Error> ProcessOrder(OrderRequest request)
{
    _logger.LogDebug("Processing order. Request={@Request}", request);

    var validation = Validate(request);
    _logger.LogDebug("Validation result. IsValid={IsValid}", validation.IsSuccess);

    if (validation.IsFailure)
    {
        _logger.LogWarning("Validation failed. Error={Error}", validation.Error);
        return validation.Error;
    }

    // Continue with logging at key decision points...
}
```

### Minimal Reproduction
```
Start with failing case
├── Remove components one by one
├── Does it still fail?
│   ├── Yes → Keep removing
│   └── No → That component is involved
└── Result: Smallest failing case
```

## Common Bug Patterns

| Pattern | Symptoms | Check |
|---------|----------|-------|
| **Null reference** | NullReferenceException | Check initialization |
| **Off-by-one** | Boundary failures | Check loop bounds |
| **Race condition** | Intermittent failures | Check threading |
| **State corruption** | Unexpected values | Check state mutations |
| **Resource leak** | Gradual degradation | Check disposal |
| **Configuration** | Works locally, fails elsewhere | Check environment |

## Debugging Questions

1. **What changed?** (Code, config, data, environment)
2. **When did it start?** (Commit, deployment, time)
3. **Who is affected?** (All users, some users, one user)
4. **What's the pattern?** (Always, sometimes, specific conditions)
5. **What does the data say?** (Logs, metrics, traces)

## Anti-Patterns

| Anti-Pattern | Problem | Alternative |
|--------------|---------|-------------|
| Shotgun debugging | Can't know what fixed it | One change at a time |
| Cargo cult fixes | Copying fixes without understanding | Understand the cause |
| Not documenting | Repeat investigations | Keep audit trail |
| Assuming | Missing obvious causes | Look at the data |
| Premature fixing | Fixing symptoms, not cause | Find root cause |
