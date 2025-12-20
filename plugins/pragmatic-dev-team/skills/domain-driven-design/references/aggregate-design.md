# Aggregate Design

Vaughn Vernon's rules and heuristics for designing aggregates.

## The Four Rules

### Rule 1: Model True Invariants in Consistency Boundaries

**Only modify one aggregate per transaction.**

An invariant is a business rule that must always be true. The aggregate boundary defines what must be consistent together.

```csharp
public class Order
{
    private readonly List<OrderLine> _lines = new();

    public Money Total { get; private set; }

    // Invariant: Total must always equal sum of lines
    public Result AddLine(Product product, Quantity quantity)
    {
        if (quantity.Value <= 0)
            return Result.Fail("Quantity must be positive");

        _lines.Add(new OrderLine(product.Id, quantity, product.Price));
        RecalculateTotal();  // Maintain invariant

        return Result.Ok();
    }

    private void RecalculateTotal()
    {
        Total = _lines.Aggregate(Money.Zero, (sum, line) => sum + line.Total);
    }
}
```

**Anti-pattern**: Modifying multiple aggregates in one transaction.

```csharp
// BAD: Two aggregates in one transaction
using var transaction = _context.BeginTransaction();
order.Submit();
inventory.Reserve(order.Lines);  // Different aggregate!
transaction.Commit();

// GOOD: Use domain events for eventual consistency
order.Submit();  // Raises OrderSubmitted event
// Event handler updates inventory asynchronously
```

---

### Rule 2: Design Small Aggregates

**Large aggregates cause contention and complexity.**

```csharp
// BAD: Large cluster aggregate
public class Order
{
    public Customer Customer { get; set; }        // Embedded!
    public List<Product> Products { get; set; }   // Embedded!
    public Shipment Shipment { get; set; }        // Embedded!
    public List<Payment> Payments { get; set; }   // Embedded!
}

// GOOD: Small aggregate with references by ID
public class Order
{
    public OrderId Id { get; }
    public CustomerId CustomerId { get; }
    private readonly List<OrderLine> _lines = new();

    public IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly();
}

public record OrderLine(ProductId ProductId, Quantity Qty, Money UnitPrice);
```

**Sizing heuristic**: If you can't describe the invariant, it's too big.

---

### Rule 3: Reference Other Aggregates by Identity

**Use strongly-typed IDs, not object references.**

```csharp
// Define strongly-typed IDs
public sealed record CustomerId(Guid Value);
public sealed record OrderId(Guid Value);
public sealed record ProductId(Guid Value);

public class Order
{
    public OrderId Id { get; }
    public CustomerId CustomerId { get; }  // Reference by ID

    // NOT: public Customer Customer { get; }  // Object reference
}
```

**Benefits**:
- Clear aggregate boundaries
- No accidental modifications across aggregates
- Easier to scale (different aggregates can be in different services)

---

### Rule 4: Update Other Aggregates via Domain Events

**Cross-aggregate updates happen asynchronously via events.**

```csharp
public class Order
{
    public Result Ship()
    {
        if (Status != OrderStatus.Paid)
            return Result.Fail("Only paid orders can ship");

        Status = OrderStatus.Shipped;

        // Don't update inventory directly - raise event
        RaiseDomainEvent(new OrderShipped(Id, CustomerId, _lines));
        return Result.Ok();
    }
}

// Event handler (different transaction)
public class OrderShippedHandler
{
    public async Task Handle(OrderShipped evt)
    {
        foreach (var line in evt.Lines)
        {
            var inventory = await _inventoryRepo.GetAsync(line.ProductId);
            inventory.Reduce(line.Quantity);
            await _inventoryRepo.SaveAsync(inventory);
        }
    }
}
```

---

## Sizing Heuristics

### Questions to Identify Boundaries

1. **What must be consistent immediately?**
   - Items in the same order → Same aggregate
   - Order and inventory → Different aggregates (eventual consistency)

2. **What's the contention level?**
   - High-traffic entities → Smaller aggregates
   - Multiple users editing same data → Split it

3. **What's the lifecycle?**
   - Created together → Consider same aggregate
   - Independent lifecycles → Different aggregates

### Size Guidelines

| Aggregate Size | Good For | Watch Out |
|---------------|----------|-----------|
| 1 entity | High contention | Too granular? |
| 2-5 entities | Typical domain | Sweet spot |
| >10 entities | Complex invariants | Likely too big |

---

## Common Mistakes

### Mistake 1: Large Aggregates

```csharp
// BAD: Everything in one aggregate
public class Customer
{
    public List<Order> Orders { get; }     // All orders ever!
    public List<Address> Addresses { get; }
    public List<PaymentMethod> PaymentMethods { get; }
}

// GOOD: Separate aggregates
public class Customer { /* Core customer data */ }
public class Order { public CustomerId CustomerId { get; } }
public class PaymentMethod { public CustomerId CustomerId { get; } }
```

### Mistake 2: Cross-Aggregate Transactions

```csharp
// BAD: Distributed transaction
public async Task TransferMoney(AccountId from, AccountId to, Money amount)
{
    using var tx = _context.BeginTransaction();

    var fromAccount = await _repo.GetAsync(from);
    var toAccount = await _repo.GetAsync(to);

    fromAccount.Withdraw(amount);  // Aggregate 1
    toAccount.Deposit(amount);     // Aggregate 2

    tx.Commit();  // Distributed transaction!
}

// GOOD: Saga pattern
public async Task TransferMoney(AccountId from, AccountId to, Money amount)
{
    var fromAccount = await _repo.GetAsync(from);
    var result = fromAccount.Withdraw(amount);

    if (result.IsSuccess)
    {
        // Raises TransferInitiated event
        // Handler will deposit to target account
    }
}
```

### Mistake 3: Exposing Internal Collections

```csharp
// BAD: Mutable collection
public List<OrderLine> Lines { get; set; }

// GOOD: Encapsulated
private readonly List<OrderLine> _lines = new();
public IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly();
```

---

## Design Process

1. **Start with events** - What happened in the domain?
2. **Group by invariants** - What must be consistent?
3. **Identify root** - What's the entry point?
4. **Size check** - Is it too big/small?
5. **Event check** - What crosses boundaries?

```
Events: OrderPlaced, OrderLineAdded, OrderShipped

Invariants:
- Order total = sum of lines ← Same aggregate
- Inventory must be available ← Different aggregate (eventual)

Aggregate: Order (root) + OrderLine
Cross-aggregate: Inventory (via OrderPlaced event)
```

---

## Testing Aggregates

```csharp
public class OrderTests
{
    [Fact]
    public void AddLine_UpdatesTotal()
    {
        var order = CreateOrder();
        var product = new Product(new ProductId(Guid.NewGuid()), Money.From(100));

        order.AddLine(product, Quantity.From(2));

        order.Total.Should().Be(Money.From(200));
    }

    [Fact]
    public void Submit_RaisesOrderSubmittedEvent()
    {
        var order = CreateOrderWithLines();

        order.Submit();

        order.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<OrderSubmitted>();
    }
}
```

## Sources

- [Effective Aggregate Design](https://www.dddcommunity.org/library/vernon_2011/) - Vaughn Vernon
- [Implementing DDD](https://www.oreilly.com/library/view/implementing-domain-driven-design/9780133039900/)
- [Domain-Driven Design Distilled](https://www.oreilly.com/library/view/domain-driven-design-distilled/9780134434964/)
