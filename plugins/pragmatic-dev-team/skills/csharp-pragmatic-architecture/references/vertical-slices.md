# Vertical Slice Architecture

## Core Concept

Organize code by **feature**, not by technical layer. Everything for a feature lives together.

**Origin:** Jimmy Bogard, as alternative to traditional layered/onion/clean architecture.

## Layered vs Vertical

### Layered (Traditional)
```
Controllers/
├── UserController.cs
├── OrderController.cs
├── ProductController.cs
Services/
├── UserService.cs
├── OrderService.cs
├── ProductService.cs
Repositories/
├── UserRepository.cs
├── OrderRepository.cs
├── ProductRepository.cs
```

**Problem:** Adding a feature touches many folders. Related code is scattered.

### Vertical Slices
```
Features/
├── Users/
│   ├── CreateUser.cs      # Command + Handler
│   ├── GetUser.cs         # Query + Handler
│   └── DeleteUser.cs
├── Orders/
│   ├── PlaceOrder.cs
│   ├── GetOrder.cs
│   └── CancelOrder.cs
```

**Benefit:** Adding a feature = adding one file/folder. All related code together.

## CQRS Without MediatR

Implement Command/Query Responsibility Segregation with simple interfaces:

### Interfaces

```csharp
// Commands change state
public interface ICommand
{
    string DisplayName { get; }
}

public interface ICommandHandler<TCommand, TResult> where TCommand : ICommand
{
    Result<TResult, Error> Handle(TCommand command);
}

// Queries read state
public interface IQuery
{
    string DisplayName { get; }
}

public interface IQueryHandler<TQuery, TResult> where TQuery : IQuery
{
    Result<TResult, Error> Handle(TQuery query);
}
```

### Auto-Registration with Scrutor

```csharp
// Scan and register all handlers
services.Scan(s => s.FromAssembliesOf(typeof(ICommand))
    .AddClasses(c => c.AssignableTo(typeof(ICommandHandler<,>)))
    .AsImplementedInterfaces()
    .WithTransientLifetime());

services.Scan(s => s.FromAssembliesOf(typeof(IQuery))
    .AddClasses(c => c.AssignableTo(typeof(IQueryHandler<,>)))
    .AsImplementedInterfaces()
    .WithTransientLifetime());
```

### Decorator Chain (Cross-Cutting Concerns)

```csharp
// Order matters! First registered = outermost wrapper
services.Decorate(typeof(ICommandHandler<,>), typeof(UndoDecorator.Handler<,>));
services.Decorate(typeof(ICommandHandler<,>), typeof(PerformanceDecorator.Handler<,>));
services.Decorate(typeof(ICommandHandler<,>), typeof(DocumentRequiredDecorator.CommandHandler<,>));
services.Decorate(typeof(ICommandHandler<,>), typeof(LicensingDecorator.CommandHandler<,>));
services.Decorate(typeof(ICommandHandler<,>), typeof(LoggingDecorator.CommandHandler<,>));
```

## Feature File Structure

Single file containing Command + Handler:

```csharp
// ResetStyles.cs
namespace MyApp.Application.Word;

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

            var resetStyleResult = documentEditor.ResetStyles();
            if (resetStyleResult.IsFailure)
                return resetStyleResult;

            eventDispatcher.DispatchAndClear(document);
            return Unit.Instance;
        }
    }
}
```

## No Generic Repositories

Vertical slices don't need generic `IRepository<T>` abstractions.

```csharp
// BAD: Generic abstraction
public interface IRepository<T>
{
    T GetById(int id);
    IEnumerable<T> GetAll();
    void Add(T entity);
    void Update(T entity);
    void Delete(T entity);
}

// GOOD: Specific to what you need
public interface IDocumentRepository
{
    Maybe<Document> GetActiveDocument();
    void Save(Document document);
}

public interface IUserRepository
{
    Result<User, Error> GetByEmail(EmailAddress email);
    void Add(User user);
}
```

## When to Extract Shared Code

Only extract when you have **actual duplication**, not anticipated duplication:

1. Wait for 3+ usages before extracting
2. Extract to a `Shared/` folder within the feature area first
3. Only move to cross-cutting if truly used everywhere

```
Features/
├── Users/
│   ├── CreateUser.cs
│   ├── GetUser.cs
│   └── Shared/
│       └── UserValidation.cs  # Shared within Users feature
├── Common/                     # Truly cross-cutting
│   └── EmailService.cs
```

## Benefits

- **Feature-focused**: Changes isolated to specific features
- **Scalability**: Teams work on different features independently
- **Maintainability**: Easy to navigate, understand single feature
- **Reduced coupling**: Minimal dependencies between slices
- **Flexibility**: Each slice can use different approaches

## Challenges & Solutions

| Challenge | Solution |
|-----------|----------|
| Code duplication | Extract only after 3+ usages |
| Cross-cutting concerns | Decorator pattern |
| Consistency | Architecture tests (ArchUnitNET) |
| Large number of files | Good folder organization |

## Industry Resources

- [Jimmy Bogard - Vertical Slice Architecture](https://www.jimmybogard.com/vertical-slice-architecture/)
- [Milan Jovanovic - Vertical Slice Architecture](https://www.milanjovanovic.tech/blog/vertical-slice-architecture)
- [Code Maze - VSA in ASP.NET Core](https://code-maze.com/vertical-slice-architecture-aspnet-core/)
