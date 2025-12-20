# Meta-Principles from Classic Software Engineering Books

These principles come from **The Pragmatic Programmer** (Hunt & Thomas) and **Code Complete** (McConnell). They complement the 10 core Rust-inspired principles by providing broader software engineering wisdom.

## 11. DRY (Don't Repeat Yourself)

**Source:** The Pragmatic Programmer

> "Every piece of knowledge must have a single, unambiguous, authoritative representation within a system."

### What DRY Really Means

DRY is about **knowledge duplication**, not code duplication. Two pieces of code that look similar but represent different knowledge are NOT violations.

**Is a violation:**
```csharp
// Same validation rule in two places
public class CreateUserHandler {
    public Result Handle(CreateUserCommand cmd) {
        if (cmd.Email.Length > 254) return Error("Email too long");  // Rule: max 254
        // ...
    }
}

public class UpdateEmailHandler {
    public Result Handle(UpdateEmailCommand cmd) {
        if (cmd.Email.Length > 254) return Error("Email too long");  // Same rule duplicated!
        // ...
    }
}
```

**Is NOT a violation:**
```csharp
// Similar code, different knowledge
public decimal CalculateShippingCost(Order order) {
    return order.Weight * 2.5m;  // Shipping: $2.50 per kg
}

public decimal CalculateInsuranceCost(Order order) {
    return order.Weight * 2.5m;  // Insurance: $2.50 per kg (coincidentally same!)
}
```

The second example looks duplicated but represents different business rules that might change independently.

### DRY Applies Beyond Code

| Domain | Single Source of Truth |
|--------|------------------------|
| **Database schema** | One place defines the schema (migrations, not scattered scripts) |
| **API contracts** | OpenAPI spec generates clients, not manual duplication |
| **Configuration** | One config file per environment, not scattered settings |
| **Business rules** | Domain layer, not duplicated in UI and API |
| **Error messages** | Resource files or error classes, not inline strings |

### The Fix: Extract to Authority

```csharp
// GOOD: Single source of truth for email rules
public sealed class EmailAddress : ValueObject
{
    public const int MaxLength = 254;

    private EmailAddress(string value) => Value = value;
    public string Value { get; }

    public static Result<EmailAddress, Error> Create(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return DomainErrors.Email.Empty;
        if (raw.Length > MaxLength)
            return DomainErrors.Email.TooLong;
        if (!raw.Contains('@'))
            return DomainErrors.Email.InvalidFormat;

        return new EmailAddress(raw.Trim().ToLowerInvariant());
    }
}

// Now handlers just use the trusted type
public class CreateUserHandler {
    public Result Handle(CreateUserCommand cmd) {
        var email = EmailAddress.Create(cmd.Email);  // Validation in one place
        // ...
    }
}
```

### When Duplication is Acceptable

1. **Across bounded contexts** - Different domains may have different rules for "email"
2. **Test fixtures** - Some duplication improves test readability
3. **Early prototyping** - Extract after patterns emerge, not before

---

## 12. Broken Windows (Fix Technical Debt Early)

**Source:** The Pragmatic Programmer

> "Don't leave 'broken windows' (bad designs, wrong decisions, poor code) unrepaired. Fix each one as soon as it is discovered."

### The Psychology

Research on urban decay found that one broken window in a building leads to more vandalism. The same applies to code:

- One `// TODO: hack, fix later` leads to more
- One ignored exception leads to more
- One disabled test leads to more
- One "temporary" workaround becomes permanent

### Symptoms of Broken Windows

```csharp
// Broken Window #1: Caught and ignored exception
try {
    ProcessPayment();
} catch { }  // What could go wrong?

// Broken Window #2: Commented-out code
// public void OldMethod() { ... }  // Keeping "just in case"

// Broken Window #3: TODO older than a sprint
// TODO: Refactor this (added 2019-03-15)

// Broken Window #4: Dead code
public void NeverCalledMethod() { ... }

// Broken Window #5: Magic numbers/strings
if (status == 3) { ... }  // What is 3?

// Broken Window #6: Copy-paste with minor changes
public void ProcessOrderA() { /* 200 lines */ }
public void ProcessOrderB() { /* same 200 lines with 2 changes */ }
```

### The Fix: Immediate Action Protocol

**< 15 minutes:** Fix it now
```csharp
// See a magic number? Fix immediately
if (order.Status == OrderStatus.Shipped) { ... }
```

**15-60 minutes:** Fix it today or create tracked issue
```csharp
// Add to backlog with context
// Issue #1234: Refactor duplicate order processing logic
// Impact: Medium (maintainability)
// Effort: 45 min
```

**> 60 minutes:** Never add more debt on top
```csharp
// If you can't fix the broken window, at least don't make it worse
// DON'T add another copy-pasted method
// DON'T add another ignored exception
// DON'T add another TODO
```

### Related: The Boy Scout Rule

> "Leave the code cleaner than you found it."

Even if you can't fix the whole broken window, improve something:
- Rename a confusing variable
- Extract a method for clarity
- Add a missing null check
- Delete dead code

---

## 13. Avoid Dogmatism (Context Over Rules)

**Source:** Code Complete

> "Treat the dogma (including mine) as a set of hypotheses rather than as gospel."

### No Principle is Universal

Every principle in this skill, including itself, has contexts where it doesn't apply:

| Principle | When to Reconsider |
|-----------|-------------------|
| Result over Exceptions | Tiny scripts, throwaway code |
| Vertical Slices | 3-entity CRUD app |
| Value Objects | Genuinely generic string handling |
| Pure Core/Impure Shell | Simple stateless utilities |
| CQRS | Single-user, read-heavy apps |

### The Anti-Pattern: Cargo Cult Programming

```csharp
// Cargo cult: Applying patterns without understanding why
public interface IUserRepositoryFactory { }
public class UserRepositoryFactory : IUserRepositoryFactory
{
    public IUserRepository Create() => new UserRepository();
}

// There's only ONE implementation. Why the factory? Why the interface?
// "Because that's what the patterns book said"

// Pragmatic: Just use the concrete class until you need the abstraction
public class UserRepository { ... }
```

### Questions Before Applying a Pattern

1. **What problem does this solve?**
   - Repository pattern: Abstracts data access for testability
   - Do I need to test this in isolation? Maybe not for a simple query.

2. **Do I have that problem?**
   - Result pattern: Handles expected failures explicitly
   - Is this a 10-line utility script? Maybe exceptions are fine.

3. **What's the cost?**
   - CQRS: Separates reads and writes for scalability
   - Am I building a high-traffic system? If not, simpler might be better.

### The Meta-Rule

> If applying a principle makes code **harder to understand** or **harder to change**, reconsider.

```csharp
// Over-engineered: 5 files for simple operation
// - CreateUserCommand.cs
// - CreateUserCommandHandler.cs
// - CreateUserCommandValidator.cs
// - ICreateUserCommandHandler.cs
// - CreateUserCommandTests.cs

// Pragmatic for small feature:
// - CreateUser.cs (command + handler together)
// - CreateUserTests.cs

// Scale up WHEN complexity demands it, not before
```

### Balance: Principles vs Pragmatism

```
                    DOGMATIC
                        |
  "Every method needs   |   "Patterns are
   Result<T, Error>"    |    always good"
                        |
    -------- PRAGMATIC ZONE --------
                        |
  "Patterns when they   |   "Simple code
   solve real problems" |    that works"
                        |
                    CHAOS
                        |
  "No principles"       |   "Whatever works"
```

Stay in the pragmatic zone: Apply principles **when they solve problems you actually have**.

---

## Summary: When to Apply Each

| Meta-Principle | Apply When | Skip When |
|----------------|------------|-----------|
| **DRY** | Same knowledge in multiple places | Similar code, different reasons |
| **Broken Windows** | You see debt and can fix it | Debt is tracked and prioritized |
| **Avoid Dogmatism** | Pattern seems overkill | Pattern clearly fits problem |

## Industry Sources

- [The Pragmatic Programmer, 20th Anniversary Edition](https://pragprog.com/titles/tpp20/the-pragmatic-programmer-20th-anniversary-edition/)
- [Code Complete, 2nd Edition](https://www.microsoftpressstore.com/store/code-complete-9780735619678)
- [65 Key Takeaways from Pragmatic Programmer](https://hackernoon.com/65-key-takeaways-from-the-pragmatic-programmer-from-journeyman-to-master-1b4n32cy)
- [Code Complete Summary](https://newsletter.pragmaticengineer.com/p/code-complete-with-steve-mcconnell)
