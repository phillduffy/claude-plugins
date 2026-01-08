---
name: Domain-Driven Design
description: This skill should be used when discussing bounded contexts, aggregates, entities, value objects, domain events, ubiquitous language, repositories, domain services, or strategic DDD patterns. Also triggers for questions about DDD, domain modeling, or Eric Evans' patterns.
version: 0.1.0
load: on-demand
---

# Domain-Driven Design

Strategic and tactical patterns for modeling complex business domains, based on Eric Evans' "Domain-Driven Design" and subsequent community practices.

## Strategic Patterns

### Bounded Context
A boundary within which a particular domain model applies. Models are valid within their context but may differ across contexts.

```
┌─────────────────────┐     ┌─────────────────────┐
│   Sales Context     │     │   Shipping Context  │
│                     │     │                     │
│  Customer = Buyer   │     │  Customer = Address │
│  Product = Offering │     │  Product = Package  │
│                     │     │                     │
└─────────────────────┘     └─────────────────────┘
         ↓ Anti-Corruption Layer ↓
```

### Ubiquitous Language
Shared vocabulary between developers and domain experts. Code should read like the business speaks.

| Instead Of | Use |
|------------|-----|
| `user.active = false` | `user.Deactivate()` |
| `order.status = 2` | `order.Ship()` |
| `if (balance < 0)` | `if (account.IsOverdrawn)` |

### Context Mapping
Relationships between bounded contexts:

| Pattern | Relationship |
|---------|-------------|
| **Shared Kernel** | Shared subset, tight coupling |
| **Customer-Supplier** | Upstream/downstream, negotiated |
| **Conformist** | Downstream adopts upstream model |
| **Anti-Corruption Layer** | Translation layer, isolation |
| **Open Host Service** | Published API for consumers |
| **Published Language** | Shared interchange format |

## Tactical Patterns

### Entity
Has identity that persists over time. Equality by identity, not attributes.

```csharp
public class Customer : Entity<CustomerId>
{
    public CustomerName Name { get; private set; }
    public Email Email { get; private set; }

    public void ChangeName(CustomerName newName)
    {
        Name = newName;
        AddDomainEvent(new CustomerNameChanged(Id, newName));
    }
}
```

### Value Object
Defined by attributes, not identity. Immutable. Equality by value.

```csharp
public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public Currency Currency { get; }

    private Money(decimal amount, Currency currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Result<Money, Error> Create(decimal amount, Currency currency)
    {
        if (amount < 0)
            return DomainErrors.Money.NegativeAmount;
        return new Money(amount, currency);
    }

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot add different currencies");
        return new Money(Amount + other.Amount, Currency);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}
```

### Aggregate
Cluster of entities and value objects with a root entity. External access only through root.

```csharp
public class Order : AggregateRoot<OrderId>
{
    private readonly List<OrderLine> _lines = new();
    public IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly();

    public CustomerId CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }

    public Result<Unit, Error> AddLine(Product product, Quantity quantity)
    {
        if (Status != OrderStatus.Draft)
            return DomainErrors.Order.NotDraft;

        var line = OrderLine.Create(product, quantity);
        _lines.Add(line);
        return Unit.Instance;
    }

    public Result<Unit, Error> Submit()
    {
        if (!_lines.Any())
            return DomainErrors.Order.NoLines;

        Status = OrderStatus.Submitted;
        AddDomainEvent(new OrderSubmitted(Id));
        return Unit.Instance;
    }
}
```

### Aggregate Design Rules
1. **Reference by ID** - Don't hold object references across aggregates
2. **One transaction per aggregate** - Don't modify multiple aggregates
3. **Keep small** - Large aggregates cause contention
4. **Eventual consistency** - Between aggregates via domain events

### Domain Event
Something that happened in the domain. Past tense. Immutable.

```csharp
public sealed record OrderSubmitted(OrderId OrderId, DateTime OccurredAt) : IDomainEvent;

public sealed record CustomerNameChanged(CustomerId CustomerId, CustomerName NewName) : IDomainEvent;
```

### Repository
Collection-like abstraction for aggregate persistence. One per aggregate root.

```csharp
public interface IOrderRepository
{
    Task<Maybe<Order>> GetByIdAsync(OrderId id);
    Task AddAsync(Order order);
    Task<IReadOnlyList<Order>> GetByCustomerAsync(CustomerId customerId);
}
```

**Anti-pattern:** Generic repositories. Write specific methods for specific needs.

### Domain Service
Operations that don't naturally belong to an entity or value object.

```csharp
public class TransferService
{
    public Result<Unit, Error> Transfer(Account from, Account to, Money amount)
    {
        var withdrawResult = from.Withdraw(amount);
        if (withdrawResult.IsFailure)
            return withdrawResult.Error;

        to.Deposit(amount);
        return Unit.Instance;
    }
}
```

## Quick Reference

| Concept | Key Characteristic |
|---------|-------------------|
| Entity | Has ID, mutable, lifecycle |
| Value Object | No ID, immutable, replaceable |
| Aggregate | Consistency boundary, root entity |
| Domain Event | Past tense, immutable, happened |
| Repository | Aggregate persistence |
| Domain Service | Stateless operation |
| Bounded Context | Model boundary |
| Ubiquitous Language | Shared vocabulary |

## Anti-Patterns

| Anti-Pattern | Problem | Solution |
|--------------|---------|----------|
| Anemic domain model | Logic in services, not entities | Rich domain objects |
| Generic repository | Premature abstraction | Specific repositories |
| Cross-aggregate references | Coupling, inconsistency | Reference by ID |
| Large aggregates | Contention, complexity | Keep small, eventual consistency |
| Technical language in domain | Disconnect from business | Ubiquitous language |
