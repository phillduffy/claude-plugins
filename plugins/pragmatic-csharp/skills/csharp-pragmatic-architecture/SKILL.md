---
name: C# Pragmatic Architecture
description: This skill should be used when designing C# classes, reviewing architecture decisions, implementing domain logic, or when the user asks about "Result pattern", "value objects", "vertical slices", "composition over inheritance", "dependency injection", "functional core", "parse don't validate", "illegal states", "YAGNI", "IOptions pattern", "DRY", "broken windows", or "technical debt". Also triggers when writing handlers, aggregates, decorators, or domain primitives.
version: 0.2.0
---

# C# Pragmatic Architecture

Rust-inspired pragmatic principles for C# architecture. These principles prioritize compile-time safety, explicit dependencies, and composition over inheritance. They reduce runtime bugs, simplify testing, and prevent over-engineering.

## The 11 Core Principles

| # | Principle | Core Idea |
|---|-----------|-----------|
| 1 | **Make Illegal States Unrepresentable** | Types prevent bugs at compile time |
| 2 | **Composition via Interfaces** | Small interfaces, no inheritance for code sharing |
| 3 | **Result over Exceptions** | Return `Result<T, Error>`, don't throw for expected failures |
| 4 | **Data/Behavior Separation** | Immutable records + stateless services |
| 5 | **Pure Core, Impure Shell** | Side effects at edges only (decorators) |
| 6 | **Vertical Slices** | Organize by feature, not layer |
| 7 | **Explicit Dependencies** | Constructor injection, no hidden deps |
| 8 | **YAGNI** | Solve current problem simply |
| 9 | **Parse, Don't Validate** | Transform to trusted types at boundary |
| 10 | **Options Pattern** | Strongly typed config with `IOptions<T>` |
| 11 | **Maybe over null** | Return `Maybe<string>` never `string?` |

## 3 Meta-Principles (from Pragmatic Programmer & Code Complete)

| # | Principle | Core Idea |
|---|-----------|-----------|
| 11 | **DRY (Don't Repeat Yourself)** | Single source of truth for every piece of knowledge |
| 12 | **Broken Windows** | Fix technical debt immediately or it spreads |
| 13 | **Avoid Dogmatism** | Context over rules; no principle is universal |

## Principle Details

### 1. Make Illegal States Unrepresentable

Leverage C#'s type system to prevent bugs at compile time. Define types that can only exist in valid states.

**Techniques:**
- Private constructors with factory methods
- `sealed` classes by default
- `readonly record struct` for value types
- `Maybe<T>` instead of nullable for optional domain values
- `required` properties for mandatory fields

```csharp
// GOOD: Selection can only exist in valid state
public sealed class Selection : ValueObject
{
    private Selection(IDocumentRange range, float totalLength) { ... }

    public static Selection Get(IDocumentRange range, float totalLength)
        => new Selection(range, totalLength);

    public bool IsInsertionPoint => Length <= 0;
    public Maybe<Selection> AfterSelection => HasContentAfter
        ? Create(End, TotalLength, TotalLength)
        : Maybe<Selection>.None;
}
```

### 2. Composition via Interfaces

Use inheritance only for true "is-a" relationships (rare). Share behavior through small, focused interfaces.

**Rule:** If using `protected` keyword, pause and rethink.

```csharp
// GOOD: Handler composed of injected dependencies
public class Handler(
    IDocumentRepository documentRepository,
    IDomainEventDispatcher eventDispatcher,
    IDocumentEditor documentEditor) : ICommandHandler<Command, Unit>
```

**Anti-pattern:** `BaseService` classes with `protected` helper methods.

### 3. Result over Exceptions

Exceptions are for exceptional circumstances (disk full, network down). Return a result object for expected failures.

**Use:** CSharpFunctionalExtensions `Result<T, Error>`

```csharp
public Result<Unit, Error> Handle(Command command)
{
    var documentResult = documentRepository.GetActiveDocument()
        .ToResult(DomainErrors.Document.NotFound);

    if (documentResult.IsFailure)
        return documentResult.Error;

    var document = documentResult.Value;
    var result = document.ResetStyles();

    if (result.IsFailure)
        return DomainErrors.ResetStyles.Failed;

    eventDispatcher.DispatchAndClear(document);
    return Unit.Instance;
}
```

**Static error definitions** keep errors discoverable:
```csharp
public static class DomainErrors
{
    public static class Document
    {
        public static readonly Error NotFound = new("Document.NotFound");
        public static readonly Error InvalidSelection = new("Document.InvalidSelection");
    }
}
```

### 4. Data/Behavior Separation

Keep data structures (records) dumb. Put logic in stateless services/handlers.

```csharp
// DATA: Pure record, no behavior
public sealed record CustomProperty(string Name, string Value);

// BEHAVIOR: Stateless handler
public class Handler(deps) : ICommandHandler<Command, Unit>
{
    public Result<Unit, Error> Handle(Command command) { ... }
}
```

### 5. Pure Core, Impure Shell

Push side effects (database, APIs, DateTime.Now) to edges. Core logic is pure (same input = same output).

**Pattern:** Decorator chain wraps pure handlers with impure concerns.

```csharp
// PURE: Handler just calculates and returns
public Result<Unit, Error> Handle(Command command) { ... }

// IMPURE: Decorator handles logging, transactions, etc.
services.Decorate(typeof(ICommandHandler<,>), typeof(LoggingDecorator.CommandHandler<,>));
services.Decorate(typeof(ICommandHandler<,>), typeof(UndoDecorator.Handler<,>));
```

### 6. Vertical Slices

Group by feature, not technical layer. Everything for "ResetStyles" in one place.

```
Application/
├── Word/
│   ├── ResetStyles.cs      # Command + Handler together
│   ├── ApplyStyle.cs
│   └── InsertField.cs
├── AboutInformation/
│   ├── GetAboutInformation.cs
│   └── ShowAboutInformation.cs
```

**Avoid:** Generic repositories. Write specific queries for specific features.

### 7. Explicit Dependencies

Never hide dependencies. Constructor injection only.

```csharp
// GOOD: All dependencies visible in constructor
public class Handler(
    IDocumentRepository documentRepository,
    IDomainEventDispatcher eventDispatcher) { }

// BAD: Hidden static access
public void Handle() {
    var doc = ServiceLocator.Get<IDocumentRepository>().GetActive();
}
```

### 8. YAGNI (You Ain't Gonna Need It)

Write code for current problem only. No premature abstractions.

**Pattern Check:** If implementing Builder pattern for 3 properties, stop. Use object initializer.

**Database abstraction?** Don't create it "in case we switch databases." Wait until you actually switch.

### 9. Parse, Don't Validate

Validate once at boundary, transform to trusted type. Never pass raw strings around.

```csharp
// At boundary: Parse raw input to trusted type
public static Result<EmailAddress, Error> Create(string raw)
{
    if (!IsValidEmail(raw))
        return Error.Validation("Invalid email");
    return new EmailAddress(raw);
}

// Inside system: Trust the type
public void SendWelcome(EmailAddress email) { ... } // No re-validation needed
```

### 10. Options Pattern

Bind configuration to strongly typed classes. No `IConfiguration["key"]` throughout code.

```csharp
public sealed class DocumentManagementSettings
{
    public required string BaseUrl { get; init; }
    public required int TimeoutSeconds { get; init; }
}

// Inject typed settings
public class Handler(IOptions<DocumentManagementSettings> settings) { }
```

## Meta-Principles

### 11. DRY (Don't Repeat Yourself)

Every piece of knowledge must have a single, unambiguous, authoritative representation.

**Applies to:**
- Code (extract methods/classes)
- Data schemas (single source of truth)
- Configuration (one place to change)
- Documentation (generate from code when possible)

```csharp
// BAD: Duplicated validation logic
public void CreateUser(string email) {
    if (!email.Contains("@")) throw new Exception();
}
public void UpdateEmail(string email) {
    if (!email.Contains("@")) throw new Exception();  // Duplicated!
}

// GOOD: Single source of truth
public static Result<EmailAddress, Error> EmailAddress.Create(string raw) { ... }
```

**Note:** DRY is about knowledge duplication, not code duplication. Two similar-looking code blocks serving different purposes are NOT violations.

### 12. Broken Windows (Fix Technical Debt Early)

Don't let bad code linger. One broken window (bad design, wrong decision, poor code) leads to more.

**Symptoms of broken windows:**
- `// TODO: fix this later` comments older than a week
- Disabled tests
- Caught-and-ignored exceptions
- Copy-pasted code blocks
- Commented-out code

**Action:** When you see a broken window:
1. Fix it immediately if <15 min
2. Create a tracked issue if larger
3. Never add more debt on top of existing debt

### 13. Avoid Dogmatism (Context Over Rules)

No principle, pattern, or framework is universally correct. Adapt to the problem.

**Examples:**
- Result pattern is great, but overkill for a 10-line utility script
- Vertical slices work well, but a tiny CRUD app might not need them
- Value objects add safety, but `string` is fine for truly generic text

**Questions to ask:**
- What problem does this pattern solve?
- Do I actually have that problem?
- What's the cost of applying it here?

**The meta-rule:** If applying a principle makes code harder to understand or maintain, reconsider.

## CQRS Without MediatR

Implement Command/Query separation with simple interfaces and Scrutor for auto-registration:

```csharp
// Interfaces
public interface ICommand { string DisplayName { get; } }
public interface ICommandHandler<TCommand, TResult> where TCommand : ICommand
{
    Result<TResult, Error> Handle(TCommand command);
}

// Auto-registration with Scrutor
services.Scan(s => s.FromAssembliesOf(typeof(ICommand))
    .AddClasses(c => c.AssignableTo(typeof(ICommandHandler<,>)))
    .AsImplementedInterfaces()
    .WithTransientLifetime());

// Decorator chain
services.Decorate(typeof(ICommandHandler<,>), typeof(LoggingDecorator.CommandHandler<,>));
```

## Library Philosophy

**Prefer:**
- CSharpFunctionalExtensions - Result<T>, Maybe<T>, ValueObject
- Scrutor - DI auto-registration and decorators
- Standard Microsoft.Extensions.* packages

**Avoid:**
- Libraries that obscure principles (magic > understanding)
- Libraries with licensing risks (MediatR went paid)
- Heavy abstractions that hide what's happening

**Evaluate NuGet packages by:**
1. Can I understand what it does?
2. Is it actively maintained?
3. What's the license? Can it change?
4. Could I implement this myself if needed?

## Quick Reference

| When you see... | Consider... |
|-----------------|-------------|
| `throw new` for validation | Return `Result<T, Error>` |
| `: BaseService` | Composition via interfaces |
| `protected` methods | Extract to injectable interface |
| `string email` parameter | Create `EmailAddress` value object |
| Folder per layer | Folder per feature (vertical slice) |
| `ServiceLocator.Get<T>()` | Constructor injection |
| Builder pattern for 3 props | Object initializer |
| `IConfiguration["key"]` | `IOptions<TSettings>` |
| Nullable everywhere | `Maybe<T>` for optional domain values |
| Copy-pasted logic | Extract to single source (DRY) |
| `// TODO: fix later` | Fix now or create tracked issue |
| "We might need this" | Ask: Do I have this problem now? (YAGNI + Dogmatism) |

## Additional Resources

### Reference Files

For detailed patterns and techniques, consult:
- **`references/result-pattern.md`** - Deep dive on Result types, error handling strategies
- **`references/illegal-states.md`** - Value Objects, private constructors, Maybe<T>
- **`references/vertical-slices.md`** - Feature folders, CQRS without MediatR
- **`references/pure-core.md`** - Decorator pattern, functional core architecture
- **`references/anti-patterns.md`** - What NOT to do with examples
- **`references/meta-principles.md`** - DRY, Broken Windows, Avoid Dogmatism (from classic books)
- **`references/your-codebase.md`** - Patterns from your OfficeAddins solution

### Example Files

Working examples in `examples/`:
- **`value-object.cs`** - Selection and CustomProperty from your codebase
- **`result-handler.cs`** - ResetStyles handler with Result pattern
- **`decorator.cs`** - LoggingDecorator implementation
- **`aggregate-root.cs`** - Document aggregate with domain events
