# Make Illegal States Unrepresentable

## Core Concept

Use the type system to prevent bugs at compile time. If invalid states can't be represented, they can't occur at runtime.

**Origin:** Yaron Minsky (OCaml community), popularized by F# and Rust.

## Techniques in C#

### 1. Private Constructors with Factory Methods

```csharp
public sealed class EmailAddress
{
    private EmailAddress(string value) => Value = value;

    public string Value { get; }

    public static Result<EmailAddress, Error> Create(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return DomainErrors.Email.Empty;

        if (!raw.Contains('@'))
            return DomainErrors.Email.InvalidFormat;

        return new EmailAddress(raw.Trim().ToLowerInvariant());
    }
}
```

### 2. Value Objects for Domain Primitives

Replace primitive types with domain-specific types:

| Instead of | Use |
|------------|-----|
| `int userId` | `UserId` struct |
| `string email` | `EmailAddress` class |
| `decimal amount` | `Money` record |
| `DateTime created` | `CreatedAt` record |

```csharp
public readonly record struct UserId(int Value)
{
    public static Result<UserId, Error> Create(int value)
    {
        if (value <= 0)
            return DomainErrors.User.InvalidId;
        return new UserId(value);
    }
}
```

### 3. Maybe<T> for Optional Values

Use `Maybe<T>` instead of null for domain concepts that are optional:

```csharp
public sealed class Selection : ValueObject
{
    public Maybe<Selection> AfterSelection
        => HasContentAfter
            ? Create(End, TotalLength, TotalLength)
            : Maybe<Selection>.None;

    public Maybe<Selection> BeforeSelection
        => HasContentBefore
            ? Create(0, Start, TotalLength)
            : Maybe<Selection>.None;
}
```

### 4. Sealed Classes by Default

Prevent unintended inheritance that could violate invariants:

```csharp
public sealed class Document : AggregateRoot<string>
{
    // Invariants protected
}
```

### 5. Required Properties (C# 11+)

Ensure all required data is provided at construction:

```csharp
public sealed class CreateUserCommand
{
    public required string Email { get; init; }
    public required string Name { get; init; }
    public string? PhoneNumber { get; init; } // Optional
}
```

### 6. Immutability

Prevent objects transitioning into invalid states after construction:

```csharp
// Immutable record
public sealed record CustomProperty(string Name, string Value);

// Immutable collection
public sealed class CustomProperties : IReadOnlyCollection<CustomProperty>
{
    private readonly Dictionary<string, CustomProperty> _values;

    // No Add/Remove methods - create new instance instead
}
```

## Aggregate Guards

Aggregates protect their invariants through guards:

```csharp
public sealed class Document : AggregateRoot<string>
{
    private bool IsOpen { get; set; } = true;

    public void RequestSave()
    {
        if (!IsOpen)
            throw new InvalidOperationException("Cannot save a closed document.");

        AddDomainEvent(new DocumentSavedEvent(this));
    }

    public Result SetCustomProperty(string name, string value)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure("Property name cannot be empty");

        // Invariant protected
        _customProperties[name] = new CustomProperty(name, value);
        return Result.Success();
    }
}
```

## State Machines via Types

Represent workflow states as distinct types:

```csharp
// Instead of: Order with Status enum
public abstract record Order;
public sealed record DraftOrder(IReadOnlyList<OrderItem> Items) : Order;
public sealed record SubmittedOrder(IReadOnlyList<OrderItem> Items, DateTime SubmittedAt) : Order;
public sealed record ShippedOrder(IReadOnlyList<OrderItem> Items, DateTime SubmittedAt, TrackingNumber Tracking) : Order;

// Now impossible to ship a draft order!
public ShippedOrder Ship(SubmittedOrder order, TrackingNumber tracking)
    => new ShippedOrder(order.Items, order.SubmittedAt, tracking);
```

## Discriminated Unions (Future C#)

C# lacks native discriminated unions, but they're under active discussion. Until then, use:

- OneOf library for sum types
- Sealed class hierarchies
- Switch expressions with pattern matching

```csharp
// Using sealed hierarchy
public abstract record PaymentMethod;
public sealed record CreditCard(string Last4, DateTime Expiry) : PaymentMethod;
public sealed record BankTransfer(string AccountNumber) : PaymentMethod;
public sealed record Cryptocurrency(string WalletAddress) : PaymentMethod;

// Exhaustive matching
string Describe(PaymentMethod method) => method switch
{
    CreditCard cc => $"Card ending {cc.Last4}",
    BankTransfer bt => $"Bank account {bt.AccountNumber}",
    Cryptocurrency crypto => $"Wallet {crypto.WalletAddress}",
    _ => throw new InvalidOperationException("Unknown payment method")
};
```

## Industry Resources

- [Enterprise Craftsmanship - C# and F# approaches](https://enterprisecraftsmanship.com/posts/c-and-f-approaches-to-illegal-state)
- [DevIQ - Make Invalid States Unrepresentable](https://deviq.com/principles/make-invalid-states-unrepresentable/)
- [F# for Fun and Profit - Designing with types](https://fsharpforfunandprofit.com/posts/designing-with-types-making-illegal-states-unrepresentable/)
