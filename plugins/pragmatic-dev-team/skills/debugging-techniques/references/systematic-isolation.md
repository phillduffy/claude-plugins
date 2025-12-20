# Systematic Isolation

Binary search and minimal reproduction techniques for finding bugs.

## Core Principle

> **Eliminate half the problem space each iteration.**

Instead of guessing, systematically narrow down:
- Which commit introduced the bug
- Which code path fails
- Which input triggers the issue
- Which environment component differs

---

## Git Bisect

Find the commit that introduced a regression.

### Basic Workflow

```bash
# Start bisect
git bisect start

# Mark current as bad
git bisect bad

# Mark known good commit (e.g., last release)
git bisect good v2.3.0

# Git checks out middle commit
# Test the application, then:
git bisect good   # if this commit works
git bisect bad    # if this commit is broken

# Repeat until Git identifies the culprit
# Example output: "abc123 is the first bad commit"

# Clean up
git bisect reset
```

### Automated Bisect

```bash
# Run a test script automatically
git bisect run dotnet test --filter "Category=Regression"

# Script should exit 0 for good, non-0 for bad
git bisect run ./check-bug.sh
```

### Bisect with Skips

```bash
# If commit can't be tested (build broken, etc.)
git bisect skip
```

---

## Code Bisection

When you can't use git bisect, manually bisect code.

### Comment-Out Strategy

```csharp
public async Task ProcessOrderAsync(Order order)
{
    // Step 1: Comment out ~half
    ValidateOrder(order);
    // await CalculateShipping(order);
    // await ChargePayment(order);
    // await UpdateInventory(order);
    // await SendConfirmation(order);

    // If bug still occurs → ValidateOrder is involved
    // If bug disappears → Bug is in commented section

    // Step 2: Uncomment half of suspected section
    ValidateOrder(order);
    await CalculateShipping(order);
    await ChargePayment(order);
    // await UpdateInventory(order);
    // await SendConfirmation(order);

    // Continue until isolated
}
```

### Binary Search in Data

```csharp
// Bug occurs with large dataset
var allItems = await GetAllItemsAsync();

// Split in half
var firstHalf = allItems.Take(allItems.Count / 2).ToList();
var secondHalf = allItems.Skip(allItems.Count / 2).ToList();

// Test each half
await ProcessAsync(firstHalf);   // Works?
await ProcessAsync(secondHalf);  // Works?

// Continue splitting the failing half
```

---

## Minimal Reproduction

Reduce the problem to smallest failing case.

### Stripping Process

```
Full system → Bug occurs
    ↓
Remove feature A → Bug occurs
    ↓
Remove feature B → Bug disappears!
    ↓
Feature B is involved, add it back
    ↓
Simplify feature B → Bug still occurs
    ↓
Minimal repro found
```

### Minimal Repro Template

```csharp
// MinimalRepro.csproj
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
</Project>

// Program.cs
class Program
{
    static async Task Main()
    {
        // Minimal setup
        var service = new OrderService();

        // Minimal trigger
        var result = await service.ProcessAsync(new Order { Id = 1 });

        // Show unexpected behavior
        Console.WriteLine($"Expected: Success, Got: {result}");
    }
}
```

### Checklist for Minimal Repro

- [ ] Single project (if possible)
- [ ] No unnecessary dependencies
- [ ] Hardcoded values (no config files)
- [ ] No network calls (mock if needed)
- [ ] Clear expected vs actual output
- [ ] Can run in seconds

---

## Divide and Conquer Questions

Ask these questions to systematically narrow down:

### Layer Isolation

```
Input correct?
├── Yes → Check processing
├── No → Bug in input handling

Processing correct?
├── Yes → Check output
├── No → Bug in processing

Where does correct become incorrect?
```

### Environment Isolation

| Question | If Yes | If No |
|----------|--------|-------|
| Works locally? | Environment diff | Code bug |
| Works in staging? | Prod config diff | Infrastructure |
| Works with smaller data? | Scale issue | Logic bug |
| Works with specific user? | Data issue | General bug |

### Time Isolation

```
Worked yesterday?
├── Yes → Check recent changes
│   ├── Code changes (git log)
│   ├── Config changes
│   └── External dependencies
├── No → When did it last work?
```

---

## Logging for Isolation

Add strategic logging to narrow down:

```csharp
public async Task<Result> ProcessOrderAsync(Order order)
{
    _logger.LogDebug("CHECKPOINT 1: Starting ProcessOrder {OrderId}", order.Id);

    var validation = ValidateOrder(order);
    _logger.LogDebug("CHECKPOINT 2: Validation result {IsValid}", validation.IsSuccess);

    if (validation.IsFailure)
        return validation;

    var pricing = await CalculatePricingAsync(order);
    _logger.LogDebug("CHECKPOINT 3: Pricing result {Total}", pricing.Total);

    // Add checkpoints until you find where behavior changes
    _logger.LogDebug("CHECKPOINT 4: Before payment");
    var payment = await ChargeAsync(order, pricing);
    _logger.LogDebug("CHECKPOINT 5: After payment {Status}", payment.Status);

    return payment;
}
```

### Output Pattern

```
CHECKPOINT 1: Starting ProcessOrder 12345
CHECKPOINT 2: Validation result True
CHECKPOINT 3: Pricing result 99.99
CHECKPOINT 4: Before payment
[Exception thrown]  ← Bug is between checkpoint 4 and 5
```

---

## Conditional Breakpoints

### Visual Studio

Right-click breakpoint → Conditions:

```csharp
// Break only when:
orderId == 12345                        // Specific value
order.Status == OrderStatus.Failed      // Specific state
items.Count > 100                       // Threshold
_retryCount > 3                         // Iteration count
```

### Rider

```csharp
// Condition expression
orderId == "problem-order" && customer.Tier == "Premium"
```

---

## The Five Whys

Dig deeper to find root cause:

```
Bug: Order total is wrong

Why? → Tax calculation is incorrect
Why? → Tax rate is 0%
Why? → Tax rate lookup returned null
Why? → Customer address has no state
Why? → Address validation accepts empty state

Root cause: Missing validation rule
Fix: Add state requirement to address validation
```

---

## Anti-Patterns

| Anti-Pattern | Problem | Better Approach |
|--------------|---------|-----------------|
| **Shotgun debugging** | Change 5 things, don't know which fixed | One change at a time |
| **Assuming the cause** | "It's probably..." | Gather data first |
| **No documentation** | Repeat same investigation | Keep audit trail |
| **Fixing symptoms** | Bug returns | Find root cause |
| **Not testing fix** | May not actually work | Verify against repro |

---

## Investigation Template

```markdown
## Bug: [Description]

### Environment
- Version:
- OS:
- .NET:

### Reproduction Steps
1.
2.
3.

### Expected vs Actual
- Expected:
- Actual:

### Investigation

#### Hypothesis 1: [Theory]
- Tried:
- Result:
- Conclusion:

#### Hypothesis 2: [Theory]
- Tried:
- Result:
- Conclusion:

### Root Cause
[What was actually wrong]

### Fix
[What was changed]

### Verification
[How we confirmed fix works]
```

## Sources

- [Git Bisect Manual](https://git-scm.com/docs/git-bisect)
- [Binary Search Debugging](https://vladimirzdrazil.com/posts/binary-search-debugging/)
- [A Systematic Approach to Debugging](https://ntietz.com/blog/how-i-debug-2023/)
- [Debugging: The 9 Indispensable Rules](https://debuggingrules.com/)
