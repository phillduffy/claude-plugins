# Bounded Context Integration

Patterns for integrating bounded contexts while preserving model integrity.

## Context Mapping Patterns

| Pattern | Relationship | Use When |
|---------|-------------|----------|
| **Shared Kernel** | Shared subset | Close collaboration, shared ownership |
| **Customer-Supplier** | Upstream/downstream | Clear dependency direction |
| **Conformist** | Adopt upstream model | No influence on upstream |
| **Anti-Corruption Layer** | Translation layer | Protect from external models |
| **Open Host Service** | Published API | Serving multiple consumers |
| **Published Language** | Shared interchange | Cross-system integration |

---

## Anti-Corruption Layer (ACL)

Protects your domain from external concepts.

### Concept

```
External System          ACL                Your Domain
┌──────────────┐    ┌──────────────┐    ┌──────────────┐
│ PaymentGateway│ → │   Adapter    │ → │   Payment    │
│   Response   │    │  Translator  │    │   Entity     │
└──────────────┘    └──────────────┘    └──────────────┘
```

### Implementation

```csharp
// External system model (not under our control)
public class PaymentGatewayResponse
{
    public string tx_id { get; set; }
    public int status_code { get; set; }
    public decimal amt { get; set; }
    public string curr { get; set; }
}

// Our domain model
public record PaymentId(Guid Value);
public enum PaymentStatus { Pending, Completed, Failed, Refunded }
public record Payment(PaymentId Id, Money Amount, PaymentStatus Status);

// ACL translates between them
public interface IPaymentGatewayAdapter
{
    Task<Result<Payment>> ProcessPaymentAsync(Money amount);
}

public class PaymentGatewayAdapter : IPaymentGatewayAdapter
{
    private readonly IExternalPaymentClient _client;

    public async Task<Result<Payment>> ProcessPaymentAsync(Money amount)
    {
        var response = await _client.ProcessAsync(new
        {
            amt = amount.Value,
            currency = amount.Currency.Code
        });

        // Translate external model to domain model
        return TranslateResponse(response, amount);
    }

    private Result<Payment> TranslateResponse(
        PaymentGatewayResponse response,
        Money amount)
    {
        var status = response.status_code switch
        {
            200 => PaymentStatus.Completed,
            202 => PaymentStatus.Pending,
            _ => PaymentStatus.Failed
        };

        if (status == PaymentStatus.Failed)
            return Result.Fail<Payment>("Payment declined");

        return Result.Ok(new Payment(
            new PaymentId(Guid.Parse(response.tx_id)),
            amount,
            status
        ));
    }
}
```

### Benefits

- Domain model stays pure
- External changes isolated to ACL
- Only update adapter when external API changes
- Testable with fake adapters

---

## Domain Events

Communicate across bounded contexts without coupling.

### Internal Domain Event

```csharp
// Raised within aggregate
public record OrderSubmitted(
    OrderId OrderId,
    CustomerId CustomerId,
    IReadOnlyList<OrderLine> Lines,
    DateTime SubmittedAt
) : IDomainEvent;

public class Order
{
    public Result Submit()
    {
        if (!_lines.Any())
            return Result.Fail("Order must have lines");

        Status = OrderStatus.Submitted;
        RaiseDomainEvent(new OrderSubmitted(Id, CustomerId, _lines, DateTime.UtcNow));
        return Result.Ok();
    }
}
```

### Integration Event

```csharp
// Crosses bounded context boundaries (published to message bus)
public record OrderSubmittedIntegrationEvent(
    Guid OrderId,
    Guid CustomerId,
    OrderLineDto[] Lines,
    DateTime SubmittedAt,
    DateTime PublishedAt
);

// Mapper from domain to integration event
public class OrderSubmittedHandler
{
    private readonly IMessageBus _bus;

    public async Task Handle(OrderSubmitted domainEvent)
    {
        var integrationEvent = new OrderSubmittedIntegrationEvent(
            domainEvent.OrderId.Value,
            domainEvent.CustomerId.Value,
            domainEvent.Lines.Select(MapToDto).ToArray(),
            domainEvent.SubmittedAt,
            DateTime.UtcNow
        );

        await _bus.PublishAsync("orders.submitted", integrationEvent);
    }

    private OrderLineDto MapToDto(OrderLine line) =>
        new(line.ProductId.Value, line.Quantity.Value, line.UnitPrice.Value);
}
```

### Event Naming Conventions

| Type | Tense | Example |
|------|-------|---------|
| Domain Event | Past | `OrderSubmitted` |
| Command | Imperative | `SubmitOrder` |
| Integration Event | Past | `OrderSubmittedIntegrationEvent` |

---

## Saga vs Choreography

### Choreography (Event-Driven)

Decentralized - services react to events.

```
OrderService              InventoryService          PaymentService
     │                          │                        │
     ├── OrderSubmitted ──────▶│                        │
     │                          ├── StockReserved ─────▶│
     │                          │                        ├── PaymentCompleted
     │◀──────────────────────── OrderConfirmed ─────────┤
```

```csharp
// Order context
public class Order
{
    public Result Submit()
    {
        Status = OrderStatus.Submitted;
        RaiseDomainEvent(new OrderSubmitted(Id, CustomerId, _lines));
        return Result.Ok();
    }
}

// Inventory context reacts
public class StockReservationHandler
{
    public async Task Handle(OrderSubmitted evt)
    {
        foreach (var line in evt.Lines)
        {
            var stock = await _stockRepo.GetAsync(line.ProductId);
            stock.Reserve(line.Quantity);
        }
        await _bus.PublishAsync(new StockReserved(evt.OrderId));
    }
}

// Payment context reacts
public class PaymentHandler
{
    public async Task Handle(StockReserved evt)
    {
        var order = await _orderRepo.GetAsync(evt.OrderId);
        await _paymentService.ChargeAsync(order.CustomerId, order.Total);
    }
}
```

**Pros**: No single point of failure, loosely coupled
**Cons**: Hard to see full flow, complex error handling

---

### Orchestration (Process Manager/Saga)

Centralized coordinator manages the workflow.

```csharp
public class OrderSaga
{
    private OrderSagaState _state;

    public async Task<Result> ExecuteAsync(SubmitOrderCommand cmd)
    {
        _state = new OrderSagaState(cmd.OrderId);

        // Step 1: Reserve inventory
        var inventoryResult = await _inventory.ReserveStockAsync(cmd.Lines);
        if (inventoryResult.IsFailure)
        {
            return Result.Fail("Insufficient stock");
        }
        _state.StockReserved = true;

        // Step 2: Charge payment
        var paymentResult = await _payment.ChargeAsync(cmd.CustomerId, cmd.Total);
        if (paymentResult.IsFailure)
        {
            // Compensate: Release reserved stock
            await _inventory.ReleaseStockAsync(cmd.Lines);
            return Result.Fail("Payment failed");
        }
        _state.PaymentCompleted = true;

        // Step 3: Confirm order
        var order = await _orderRepo.GetAsync(cmd.OrderId);
        order.Confirm();
        await _orderRepo.SaveAsync(order);

        return Result.Ok();
    }
}
```

**Pros**: Easy to understand flow, centralized error handling
**Cons**: Single point of failure, coordinator can become complex

---

### When to Use Each

| Scenario | Pattern |
|----------|---------|
| Simple workflow (2-3 steps) | Choreography |
| Complex workflow with state | Orchestration |
| High decoupling needed | Choreography |
| Critical consistency | Orchestration |
| Multiple failure modes | Orchestration |
| Independent team ownership | Choreography |

---

## Shared Kernel

Small, shared portion of domain model.

```csharp
// Shared kernel assembly: Company.Shared.Domain
namespace Company.Shared.Domain;

// Shared value object used by multiple contexts
public sealed record Money
{
    public decimal Value { get; }
    public Currency Currency { get; }

    public static Money operator +(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new InvalidOperationException("Currency mismatch");
        return new Money(a.Value + b.Value, a.Currency);
    }
}

// Shared domain event
public record PaymentReceived(
    Guid PaymentId,
    Guid CustomerId,
    decimal Amount,
    string Currency,
    DateTime ReceivedAt
);
```

**Warning**: Shared kernel creates coupling. Keep it minimal.

---

## Quick Reference

| Need | Pattern |
|------|---------|
| Protect from external model | Anti-Corruption Layer |
| Cross-context updates | Domain Events → Integration Events |
| Simple workflow | Choreography |
| Complex workflow | Saga/Orchestration |
| Shared types | Shared Kernel (minimal) |
| Serving consumers | Open Host Service + Published Language |

## Sources

- [Bounded Context](https://martinfowler.com/bliki/BoundedContext.html) - Martin Fowler
- [Anti-Corruption Layer](https://deviq.com/domain-driven-design/anti-corruption-layer/)
- [Saga Pattern](https://learn.microsoft.com/en-us/azure/architecture/patterns/saga)
- [Event-Driven Architecture](https://event-driven.io/)
