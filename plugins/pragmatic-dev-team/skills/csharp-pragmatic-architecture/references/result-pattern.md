# Result Pattern Deep Dive

## Why Result over Exceptions?

Exceptions are for **exceptional** circumstances - disk failures, network outages, out-of-memory. They should not control normal program flow.

**Problems with exceptions for flow control:**
- Caller must know implementation details (which exceptions to catch)
- Easy to forget to handle failures
- Stack traces are expensive (~7x slower than Result)
- Non-local control flow is hard to reason about

**Benefits of Result pattern:**
- Forces caller to handle both success and failure
- Explicit in method signature
- Composable with LINQ-like operations
- Better performance for expected failures

## CSharpFunctionalExtensions

Recommended library for Result pattern in C#. Battle-tested, well-maintained.

### Core Types

```csharp
// Success or failure with typed error
Result<T, Error>

// Optional value (instead of nullable)
Maybe<T>

// Value equality for domain objects
ValueObject
```

### Basic Usage

```csharp
// Returning success
return Result.Success<Unit, Error>(Unit.Instance);
return documentResult.Value; // implicit conversion

// Returning failure
return Result.Failure<Unit, Error>(DomainErrors.Document.NotFound);
return documentResult.Error; // when already have Error

// Converting Maybe to Result
var documentResult = documentRepository.GetActiveDocument()
    .ToResult(DomainErrors.Document.NotFound);
```

### Chaining Operations

```csharp
// Railway-oriented programming
var result = GetUser(userId)
    .Bind(user => ValidateUser(user))
    .Bind(user => SaveUser(user))
    .Map(user => user.Id);

// With error handling
result.Match(
    onSuccess: id => Console.WriteLine($"Saved user {id}"),
    onFailure: error => Console.WriteLine($"Failed: {error.Code}")
);
```

### Error Design

Create a domain-specific Error class as ValueObject:

```csharp
public sealed class Error : ValueObject
{
    public Error(string code, params object[] args)
    {
        Code = code;
        Args = args;
    }

    public string Code { get; }
    public object[] Args { get; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Code;
        yield return Args;
    }
}
```

**Static error definitions** for discoverability:

```csharp
public static class DomainErrors
{
    public static class Document
    {
        public static readonly Error NotFound = new("Document.NotFound");
        public static readonly Error InvalidSelection = new("Document.InvalidSelection");
        public static Error InvalidFormat(string format) => new("Document.InvalidFormat", format);
    }

    public static class Licensing
    {
        public static readonly Error NoEntitlement = new("Licensing.NoEntitlement");
        public static Error FeatureNotLicensed(string feature) => new("Licensing.FeatureNotLicensed", feature);
    }
}
```

## When to Use Each

| Scenario | Use |
|----------|-----|
| User input validation | `Result<T, Error>` |
| Entity not found | `Result<T, Error>` |
| Business rule violation | `Result<T, Error>` |
| Optional value | `Maybe<T>` |
| Truly exceptional (disk fail) | `throw Exception` |
| Programmer error (null arg) | `throw ArgumentException` |

## Handler Pattern

Standard handler pattern with Result:

```csharp
public class Handler(
    IDocumentRepository documentRepository,
    IDomainEventDispatcher eventDispatcher) : ICommandHandler<Command, Unit>
{
    public Result<Unit, Error> Handle(Command command)
    {
        // Early return on failure
        var documentResult = documentRepository.GetActiveDocument()
            .ToResult(DomainErrors.Document.NotFound);

        if (documentResult.IsFailure)
            return documentResult.Error;

        var document = documentResult.Value;

        // Business logic
        var result = document.DoSomething();
        if (result.IsFailure)
            return result.Error;

        // Side effects at end
        eventDispatcher.DispatchAndClear(document);
        return Unit.Instance;
    }
}
```

## Anti-Patterns

### Wrapping exceptions in Result

```csharp
// BAD: Defeats the purpose
try
{
    var user = await GetUserOrThrow(id);
    return Result.Success(user);
}
catch (Exception ex)
{
    return Result.Failure(ex.Message);
}
```

### Throwing inside Result-returning method

```csharp
// BAD: Method signature lies
public Result<User, Error> GetUser(int id)
{
    if (id < 0)
        throw new ArgumentException(); // Unexpected throw!
    ...
}
```

### Ignoring Result

```csharp
// BAD: Discarding result
SaveUser(user); // Result ignored!

// GOOD: Handle or propagate
var result = SaveUser(user);
if (result.IsFailure)
    return result.Error;
```

## Industry Resources

- [Milan Jovanovic - Functional Error Handling](https://www.milanjovanovic.tech/blog/functional-error-handling-in-dotnet-with-the-result-pattern)
- [ErrorOr library](https://github.com/amantinband/error-or) - Alternative with built-in error types
- [FluentResults](https://github.com/altmann/FluentResults) - Supports multiple errors per result
