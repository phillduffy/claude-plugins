# Functional Core, Imperative Shell

## Core Concept

Separate pure business logic (core) from side effects (shell). The core calculates and returns changes; the shell applies them.

**Origin:** Gary Bernhardt (2012), related to Hexagonal Architecture and Clean Architecture.

## The Pattern

```
┌─────────────────────────────────────────────┐
│            IMPURE SHELL                      │
│  (Controllers, DB, APIs, Logging, Time)      │
│  ┌───────────────────────────────────────┐  │
│  │           PURE CORE                    │  │
│  │  (Domain logic, calculations,          │  │
│  │   transformations, decisions)          │  │
│  │  Same input → Same output              │  │
│  │  No side effects                       │  │
│  └───────────────────────────────────────┘  │
└─────────────────────────────────────────────┘
```

## Rules

1. **Shell can call Core** - Shell orchestrates
2. **Core cannot call Shell** - Core is pure
3. **Push impurity outward** - When in doubt, make it core

## Implementation: Decorator Pattern

Decorators wrap pure handlers with impure concerns:

```csharp
// Execution order (outside → inside):
// Logging → Licensing → DocumentRequired → Performance → Undo → [Pure Handler]

services.Decorate(typeof(ICommandHandler<,>), typeof(UndoDecorator.Handler<,>));
services.Decorate(typeof(ICommandHandler<,>), typeof(PerformanceDecorator.Handler<,>));
services.Decorate(typeof(ICommandHandler<,>), typeof(DocumentRequiredDecorator.CommandHandler<,>));
services.Decorate(typeof(ICommandHandler<,>), typeof(LicensingDecorator.CommandHandler<,>));
services.Decorate(typeof(ICommandHandler<,>), typeof(LoggingDecorator.CommandHandler<,>));
```

### Logging Decorator (Impure Shell)

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

            var result = decorated.Handle(command);  // Call inner (eventually pure)

            if (result.IsSuccess)
                logger.LogDebug("Command - {RequestType} handled successfully", command.DisplayName);
            else
            {
                var localisedErrorMessage = localisationService.GetString(result.Error);
                userNotifier.Alert(localisedErrorMessage, command.DisplayName);
                logger.LogWarning("Command - {RequestType} failed: {Error}", command.DisplayName, result.Error);
            }

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling Command {RequestType}", command.DisplayName);
            return Result.Failure<TResult, Error>(new Error("UnexpectedError", ex.Message));
        }
    }
}
```

### Pure Handler (Functional Core)

```csharp
public class Handler(
    IDocumentRepository documentRepository,
    IDomainEventDispatcher eventDispatcher,
    IDocumentEditor documentEditor) : ICommandHandler<Command, Unit>
{
    public Result<Unit, Error> Handle(Command command)
    {
        // Pure: Get data, make decisions, return result
        var documentResult = documentRepository.GetActiveDocument()
            .ToResult(DomainErrors.Document.NotFound);

        if (documentResult.IsFailure)
            return documentResult.Error;

        var document = documentResult.Value;
        var result = document.ResetStyles();

        if (result.IsFailure)
            return DomainErrors.ResetStyles.Failed;

        // Side effect delegated to shell (via event dispatcher)
        eventDispatcher.DispatchAndClear(document);
        return Unit.Instance;
    }
}
```

## Impureim Sandwich (Mark Seemann)

A practical pattern for structuring code:

```
1. Impure: Collect all data needed
2. Pure:   Make decisions, transform data
3. Impure: Apply side effects
```

```csharp
public async Task<IActionResult> ProcessOrder(OrderRequest request)
{
    // 1. IMPURE: Gather data
    var user = await _userRepo.GetById(request.UserId);
    var products = await _productRepo.GetByIds(request.ProductIds);
    var inventory = await _inventoryService.GetLevels(request.ProductIds);

    // 2. PURE: Business logic (no I/O)
    var orderResult = OrderCalculator.CreateOrder(user, products, inventory, request);

    if (orderResult.IsFailure)
        return BadRequest(orderResult.Error);

    // 3. IMPURE: Apply changes
    await _orderRepo.Save(orderResult.Value);
    await _emailService.SendConfirmation(user.Email, orderResult.Value);

    return Ok(orderResult.Value.Id);
}
```

## Testing Benefits

Pure core is trivially testable:

```csharp
[Fact]
public void CreateOrder_InsufficientInventory_ReturnsError()
{
    // Arrange - just data
    var user = new User("test@example.com");
    var products = new[] { new Product("Widget", 10.00m) };
    var inventory = new Dictionary<string, int> { ["Widget"] = 0 };
    var request = new OrderRequest { Quantity = 5 };

    // Act - pure function
    var result = OrderCalculator.CreateOrder(user, products, inventory, request);

    // Assert
    Assert.True(result.IsFailure);
    Assert.Equal("InsufficientInventory", result.Error.Code);
}
```

No mocks needed for pure core logic!

## Domain Events as Deferred Side Effects

Aggregate collects events (pure), dispatcher executes them (impure):

```csharp
// PURE: Aggregate collects intent
public void RequestSave()
{
    AddDomainEvent(new DocumentSavedEvent(this));  // Just adds to list
}

// IMPURE: Shell dispatches
eventDispatcher.DispatchAndClear(document);  // Triggers side effects
```

## Anti-Patterns

### Injecting side-effects into core

```csharp
// BAD: Core has side effects
public class OrderCalculator(IEmailService emailService)  // Impure dep!
{
    public Order CreateOrder(...)
    {
        var order = new Order(...);
        emailService.Send(...);  // Side effect in core!
        return order;
    }
}
```

### Hidden impurity

```csharp
// BAD: Looks pure, but isn't
public decimal CalculatePrice(Product product)
{
    var discount = HttpContext.Current.Session["discount"];  // Hidden I/O!
    return product.Price * (1 - discount);
}
```

## Industry Resources

- [Mark Seemann - Impureim Sandwich](https://blog.ploeh.dk/2024/12/16/a-restaurant-sandwich/)
- [Kenneth Lange - Functional Core, Imperative Shell](https://kennethlange.com/functional-core-imperative-shell/)
- [Gary Bernhardt - Original Screencast](https://www.destroyallsoftware.com/screencasts/catalog/functional-core-imperative-shell)
