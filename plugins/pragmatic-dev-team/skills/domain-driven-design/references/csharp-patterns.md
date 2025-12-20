# DDD C# Implementation Patterns

Modern C# idioms for implementing DDD tactical patterns.

## Value Objects with Records

### Simple Value Object

```csharp
public sealed record Money(decimal Value, string Currency)
{
    public static Money Zero => new(0m, "USD");

    public static Money operator +(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new InvalidOperationException("Currency mismatch");
        return new Money(a.Value + b.Value, a.Currency);
    }

    public static Money operator *(Money a, decimal multiplier)
    {
        return new Money(a.Value * multiplier, a.Currency);
    }
}
```

### Value Object with Validation

```csharp
public sealed record Email
{
    public string Value { get; }

    private Email(string value) => Value = value;

    public static Result<Email> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Fail<Email>("Email cannot be empty");

        if (!value.Contains('@'))
            return Result.Fail<Email>("Invalid email format");

        if (value.Length > 254)
            return Result.Fail<Email>("Email too long");

        return Result.Ok(new Email(value.Trim().ToLowerInvariant()));
    }

    public override string ToString() => Value;
}
```

### Strongly-Typed IDs

```csharp
public sealed record OrderId(Guid Value)
{
    public static OrderId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}

public sealed record CustomerId(Guid Value)
{
    public static CustomerId New() => new(Guid.NewGuid());
}

// Usage
public class Order
{
    public OrderId Id { get; }
    public CustomerId CustomerId { get; }
}
```

---

## Result Pattern (No Exceptions for Expected Failures)

### Simple Result

```csharp
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; }

    protected Result(bool isSuccess, string error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Ok() => new(true, string.Empty);
    public static Result Fail(string error) => new(false, error);
}

public class Result<T> : Result
{
    public T Value { get; }

    private Result(T value) : base(true, string.Empty)
        => Value = value;

    private Result(string error) : base(false, error)
        => Value = default!;

    public static Result<T> Ok(T value) => new(value);
    public new static Result<T> Fail(string error) => new(error);
}
```

### Using Result in Domain

```csharp
public class Order
{
    public Result AddLine(Product product, Quantity quantity)
    {
        if (Status != OrderStatus.Draft)
            return Result.Fail("Cannot modify non-draft order");

        if (quantity.Value <= 0)
            return Result.Fail("Quantity must be positive");

        _lines.Add(new OrderLine(product.Id, quantity, product.Price));
        RecalculateTotal();

        return Result.Ok();
    }

    public Result<Money> CalculateDiscount(DiscountCode code)
    {
        if (code.IsExpired)
            return Result<Money>.Fail("Discount code expired");

        if (Total.Value < code.MinimumAmount)
            return Result<Money>.Fail($"Order must be at least {code.MinimumAmount}");

        var discount = Total * code.Percentage;
        return Result<Money>.Ok(discount);
    }
}
```

### Pattern Matching with Result

```csharp
var result = order.AddLine(product, quantity);

return result switch
{
    { IsSuccess: true } => Ok(),
    { Error: var error } => BadRequest(error)
};

// Or with explicit check
if (result.IsFailure)
{
    _logger.LogWarning("Failed to add line: {Error}", result.Error);
    return BadRequest(result.Error);
}
```

---

## Command Handler Pattern (No MediatR)

### Define Handler Interface

```csharp
public interface ICommandHandler<TCommand, TResult>
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken ct = default);
}

public interface IQueryHandler<TQuery, TResult>
{
    Task<TResult> HandleAsync(TQuery query, CancellationToken ct = default);
}
```

### Implement Handler

```csharp
public record PlaceOrderCommand(
    CustomerId CustomerId,
    IReadOnlyList<OrderLineRequest> Lines
);

public class PlaceOrderHandler : ICommandHandler<PlaceOrderCommand, Result<OrderId>>
{
    private readonly IOrderRepository _orders;
    private readonly IProductRepository _products;

    public PlaceOrderHandler(
        IOrderRepository orders,
        IProductRepository products)
    {
        _orders = orders;
        _products = products;
    }

    public async Task<Result<OrderId>> HandleAsync(
        PlaceOrderCommand cmd,
        CancellationToken ct = default)
    {
        var orderResult = Order.Create(cmd.CustomerId);
        if (orderResult.IsFailure)
            return Result<OrderId>.Fail(orderResult.Error);

        var order = orderResult.Value;

        foreach (var line in cmd.Lines)
        {
            var product = await _products.GetByIdAsync(line.ProductId, ct);
            if (product is null)
                return Result<OrderId>.Fail($"Product {line.ProductId} not found");

            var addResult = order.AddLine(product, line.Quantity);
            if (addResult.IsFailure)
                return Result<OrderId>.Fail(addResult.Error);
        }

        await _orders.SaveAsync(order, ct);
        return Result<OrderId>.Ok(order.Id);
    }
}
```

### Register in DI

```csharp
// Program.cs
services.AddScoped<ICommandHandler<PlaceOrderCommand, Result<OrderId>>, PlaceOrderHandler>();

// Or use assembly scanning
services.Scan(scan => scan
    .FromAssemblyOf<PlaceOrderHandler>()
    .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<,>)))
    .AsImplementedInterfaces()
    .WithScopedLifetime());
```

### Use in Controller

```csharp
[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly ICommandHandler<PlaceOrderCommand, Result<OrderId>> _handler;

    [HttpPost]
    public async Task<IActionResult> PlaceOrder(PlaceOrderRequest request)
    {
        var command = new PlaceOrderCommand(
            new CustomerId(request.CustomerId),
            request.Lines.Select(l => new OrderLineRequest(
                new ProductId(l.ProductId),
                new Quantity(l.Quantity)
            )).ToList()
        );

        var result = await _handler.HandleAsync(command);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetOrder), new { id = result.Value.Value }, result.Value)
            : BadRequest(result.Error);
    }
}
```

---

## Always-Valid Domain Model

### Factory Method with Validation

```csharp
public class Order
{
    public OrderId Id { get; }
    public CustomerId CustomerId { get; }
    public OrderStatus Status { get; private set; }
    public Money Total { get; private set; }

    private readonly List<OrderLine> _lines = new();
    public IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly();

    // Private constructor - must use factory
    private Order(CustomerId customerId)
    {
        Id = OrderId.New();
        CustomerId = customerId;
        Status = OrderStatus.Draft;
        Total = Money.Zero;
    }

    // Factory method enforces invariants
    public static Result<Order> Create(CustomerId customerId)
    {
        if (customerId is null)
            return Result<Order>.Fail("Customer ID required");

        return Result<Order>.Ok(new Order(customerId));
    }

    // Methods maintain invariants
    public Result AddLine(Product product, Quantity quantity)
    {
        // Guard clauses
        if (Status != OrderStatus.Draft)
            return Result.Fail("Cannot modify non-draft order");

        if (quantity.Value <= 0)
            return Result.Fail("Quantity must be positive");

        if (product.IsDiscontinued)
            return Result.Fail("Product discontinued");

        _lines.Add(new OrderLine(product.Id, quantity, product.Price));
        RecalculateTotal();

        return Result.Ok();
    }

    public Result Submit()
    {
        if (Status != OrderStatus.Draft)
            return Result.Fail("Only draft orders can be submitted");

        if (!_lines.Any())
            return Result.Fail("Order must have at least one line");

        Status = OrderStatus.Submitted;
        RaiseDomainEvent(new OrderSubmitted(Id, CustomerId, Total, DateTime.UtcNow));

        return Result.Ok();
    }

    // Invariant always maintained
    private void RecalculateTotal()
    {
        Total = _lines.Aggregate(Money.Zero, (sum, line) => sum + line.Total);
    }
}
```

---

## Domain Events

### Base Infrastructure

```csharp
public interface IDomainEvent
{
    DateTime OccurredAt { get; }
}

public abstract class Entity
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent evt) => _domainEvents.Add(evt);
    public void ClearDomainEvents() => _domainEvents.Clear();
}
```

### Define Event

```csharp
public record OrderSubmitted(
    OrderId OrderId,
    CustomerId CustomerId,
    Money Total,
    DateTime OccurredAt
) : IDomainEvent;
```

### Dispatch After Save

```csharp
public class UnitOfWork
{
    private readonly DbContext _context;
    private readonly IDomainEventDispatcher _dispatcher;

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        var entities = _context.ChangeTracker
            .Entries<Entity>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Any())
            .ToList();

        var events = entities.SelectMany(e => e.DomainEvents).ToList();

        await _context.SaveChangesAsync(ct);

        foreach (var evt in events)
            await _dispatcher.DispatchAsync(evt, ct);

        entities.ForEach(e => e.ClearDomainEvents());
    }
}
```

---

## Quick Reference

| Pattern | When | C# Idiom |
|---------|------|----------|
| Value Object | Immutable with validation | `record` + private ctor + factory |
| Strongly-Typed ID | Entity identity | `record TypeId(Guid Value)` |
| Result | Expected failures | `Result<T>` instead of exceptions |
| Factory Method | Complex creation | `static Result<T> Create()` |
| Domain Event | Notify other parts | `record Event : IDomainEvent` |
| Handler | Commands/Queries | `ICommandHandler<TCmd, TResult>` |

## Libraries

| Library | Purpose |
|---------|---------|
| **FluentResults** | Result pattern |
| **ErrorOr** | Discriminated union errors |
| **Vogen** | Value object source generator |
| **Ardalis.GuardClauses** | Guard clauses |

## Sources

- [C# Records as Value Objects](https://enterprisecraftsmanship.com/posts/csharp-records-value-objects/)
- [Functional Error Handling](https://www.milanjovanovic.tech/blog/functional-error-handling-in-dotnet-with-the-result-pattern)
- [CQRS Without MediatR](https://event-driven.io/en/cqrs_is_simpler_than_you_think_with_net6/)
- [Always-Valid Domain Model](https://enterprisecraftsmanship.com/posts/always-valid-domain-model/)
