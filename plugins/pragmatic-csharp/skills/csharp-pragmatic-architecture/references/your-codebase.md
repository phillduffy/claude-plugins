# Patterns from Your Codebase (OfficeAddins)

Your OfficeAddins solution demonstrates excellent implementation of pragmatic C# architecture. Use these patterns as templates for new code.

## Project Structure

```text
src/
├── Core/
│   ├── Core.Domain/              # Pure domain, no external deps
│   │   ├── Common/Primitives/    # Error, Unit, Enumeration<T>, ValueObject
│   │   ├── Common/Errors/        # DomainErrors (static error definitions)
│   │   ├── Documents/            # Document aggregate
│   │   └── Licensing/            # Licensing domain
│   ├── Core.Application/         # Use cases, handlers
│   │   ├── Abstractions/         # Messaging, Behaviours (decorators)
│   │   ├── Word/                 # Vertical slice: Word features
│   │   └── AboutInformation/     # Vertical slice: About features
│   └── Infrastructure.*/         # External dependencies
└── VSTO/
    └── WordAddIn/                # Entry point
```

## Key Patterns

### 1. Result Pattern with CSharpFunctionalExtensions

**Error as ValueObject:**
```csharp
// Core.Domain/Common/Primitives/Error.cs
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

**Static Error Definitions:**
```csharp
// Core.Domain/Common/Errors/DomainErrors.cs
public static class DomainErrors
{
    public static class Document
    {
        public static readonly Error NotFound = new("Document.NotFound");
        public static readonly Error InvalidSelection = new("Document.InvalidSelection");
    }

    public static class ApplyStyle
    {
        public static Error StyleNotFound(string styleName) => new("ApplyStyle.NotFound", styleName);
        public static Error Failed => new("ApplyStyle.Failed");
    }
}
```

### 2. Command/Query Pattern (No MediatR)

**Interfaces:**
```csharp
public interface ICommand { string DisplayName { get; } }
public interface ICommandHandler<TCommand, TResult> where TCommand : ICommand
{
    Result<TResult, Error> Handle(TCommand command);
}

public interface IQuery { string DisplayName { get; } }
public interface IQueryHandler<TQuery, TResult> where TQuery : IQuery
{
    Result<TResult, Error> Handle(TQuery query);
}
```

**Feature File (Vertical Slice):**
```csharp
// Core.Application/Word/ResetStyles.cs
public static class ResetStyles
{
    public class Command : ICommand
    {
        public string DisplayName => "Reset Styles";
    }

    [RequireEntitlement(EntitlementType.WordStandardTools)]
    [DocumentRequired]
    public class Handler(
        IDocumentRepository documentRepository,
        IDomainEventDispatcher eventDispatcher,
        IDocumentEditor documentEditor) : ICommandHandler<Command, Unit>
    {
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
    }
}
```

### 3. Decorator Chain (Pure Core, Impure Shell)

**DI Registration Order:**
```csharp
// Infrastructure.Common/DependencyInjection.cs
// Order: First registered = outermost wrapper
services.Decorate(typeof(ICommandHandler<,>), typeof(UndoDecorator.Handler<,>));
services.Decorate(typeof(ICommandHandler<,>), typeof(PerformanceDecorator.Handler<,>));
services.Decorate(typeof(ICommandHandler<,>), typeof(DocumentRequiredDecorator.CommandHandler<,>));
services.Decorate(typeof(ICommandHandler<,>), typeof(LicensingDecorator.CommandHandler<,>));
services.Decorate(typeof(ICommandHandler<,>), typeof(DocumentContextDecorator.CommandHandler<,>));
services.Decorate(typeof(ICommandHandler<,>), typeof(LoggingDecorator.CommandHandler<,>));
```

**Logging Decorator:**
```csharp
public sealed class CommandHandler<TCommand, TResult>(
    ICommandHandler<TCommand, TResult> decorated,
    ILogger<CommandHandler<TCommand, TResult>> logger,
    ILocalisationService localisationService,
    IUserNotifier userNotifier)
    : ICommandHandler<TCommand, TResult> where TCommand : class, ICommand
{
    public Result<TResult, Error> Handle(TCommand command)
    {
        try
        {
            logger.LogDebug("Handling Command {RequestType}", command.DisplayName);
            var result = decorated.Handle(command);

            if (result.IsSuccess)
                logger.LogDebug("Command handled successfully");
            else
            {
                var msg = localisationService.GetString(result.Error);
                userNotifier.Alert(msg, command.DisplayName);
            }
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling Command");
            return Result.Failure<TResult, Error>(new Error("UnexpectedError", ex.Message));
        }
    }
}
```

### 4. Value Objects

**Selection (with Maybe<T>):**
```csharp
public sealed class Selection : ValueObject
{
    private Selection(IDocumentRange range, float totalLength)
    {
        Range = range;
        TotalLength = totalLength;
    }

    public float Start => Range.Start;
    public float End => Range.End;
    public bool IsInsertionPoint => Length <= 0;

    public Maybe<Selection> AfterSelection
        => HasContentAfter ? Create(End, TotalLength, TotalLength) : Maybe<Selection>.None;

    public static Selection Get(IDocumentRange range, float totalLength)
        => new Selection(range, totalLength);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Start;
        yield return End;
    }
}
```

**Simple Record:**
```csharp
public sealed record CustomProperty(string Name, string Value);
```

### 5. Attribute-Based Guards

**Custom Attributes:**
```csharp
[RequireEntitlement(EntitlementType.WordStandardTools)]
[DocumentRequired]
public class Handler(...) : ICommandHandler<Command, Unit>
```

**Decorator Reads Attributes:**
```csharp
public class DocumentRequiredDecorator
{
    public Result<TResult, Error> Handle(TCommand command)
    {
        if (!DecoratorHelpers.HasAttribute<DocumentRequiredAttribute, TCommand>())
            return decorated.Handle(command);

        // Check document exists...
    }
}
```

### 6. Auto-Registration with Scrutor

```csharp
services.Scan(s => s.FromAssembliesOf(typeof(ICommand))
    .AddClasses(c => c.AssignableTo(typeof(ICommandHandler<,>)))
    .AsImplementedInterfaces()
    .WithTransientLifetime());

services.Scan(s => s.FromAssembliesOf(typeof(IQuery))
    .AddClasses(c => c.AssignableTo(typeof(IQueryHandler<,>)))
    .AsImplementedInterfaces()
    .WithTransientLifetime());
```

### 7. Domain Events

**Aggregate Collects Events:**
```csharp
public sealed class Document : AggregateRoot<string>
{
    private readonly List<IDomainEvent> _domainEvents = new();

    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void RequestSave()
    {
        if (!IsOpen)
            throw new InvalidOperationException("Cannot save closed document");

        AddDomainEvent(new DocumentSavedEvent(this));
    }
}
```

**Handler Dispatches:**
```csharp
eventDispatcher.DispatchAndClear(document);
```

## File Reference

| Pattern | Key Files |
|---------|-----------|
| Error/Result | `Core.Domain/Common/Primitives/Error.cs` |
| Domain Errors | `Core.Domain/Common/Errors/DomainErrors.cs` |
| Value Object | `Core.Domain/Documents/Selections/Selection.cs` |
| Aggregate | `Core.Domain/Documents/Document.cs` |
| Handler | `Core.Application/Word/ResetStyles.cs` |
| Decorator | `Core.Application/Abstractions/Behaviours/LoggingDecorator.cs` |
| DI Setup | `Infrastructure.Common/DependencyInjection.cs` |
| Arch Tests | `tests/Architecture.Tests/ArchitectureTests.cs` |

## Template for New Features

When adding a new feature, follow this pattern:

```csharp
// Core.Application/[FeatureArea]/[FeatureName].cs
public static class DoSomething
{
    public class Command : ICommand
    {
        public required string SomeInput { get; init; }
        public string DisplayName => "Do Something";
    }

    [DocumentRequired]  // If needs document
    [RequireEntitlement(EntitlementType.SomeFeature)]  // If licensed
    public class Handler(
        IDocumentRepository documentRepository,
        IDomainEventDispatcher eventDispatcher
        /* other deps */) : ICommandHandler<Command, Unit>
    {
        public Result<Unit, Error> Handle(Command command)
        {
            // 1. Get aggregate
            var docResult = documentRepository.GetActiveDocument()
                .ToResult(DomainErrors.Document.NotFound);

            if (docResult.IsFailure)
                return docResult.Error;

            var document = docResult.Value;

            // 2. Execute domain logic
            var result = document.DoSomething(command.SomeInput);
            if (result.IsFailure)
                return result.Error;

            // 3. Dispatch events
            eventDispatcher.DispatchAndClear(document);
            return Unit.Instance;
        }
    }
}
```
