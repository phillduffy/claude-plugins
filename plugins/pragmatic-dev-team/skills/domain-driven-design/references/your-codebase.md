# DDD Patterns in OfficeAddins

This reference maps DDD tactical patterns to concrete implementations in the OfficeAddins codebase.

## Aggregate Roots

| Aggregate | Location | Identity | Key Invariants |
|-----------|----------|----------|----------------|
| **Document** | `Core.Domain/Documents/Document.cs` | `string` (filename) | Selection bounds, protection status |
| **Licence** | `Core.Domain/Licensing/Licence.cs` | `Guid` | Status validity, entitlement checks |

### Document Aggregate Boundaries

```
Document (Aggregate Root)
├── CustomProperties (Value Object Collection)
├── DocumentProperties (Value Object Collection)
├── DocumentVariables (Value Object Collection)
├── TemplateReference (Value Object)
└── Selection (Value Object - transient)
```

The Document aggregate owns all document metadata. Template and licence are separate aggregates referenced by ID.

## Value Objects

| Value Object | Type | Validation | Location |
|--------------|------|------------|----------|
| `TextReplacementSpecification` | `ValueObject` base | Non-empty find text | `Documents/` |
| `CustomProperty` | `record` | None (simple data) | `Documents/` |
| `Selection` | `record` | Start/End range | `Documents/Selections/` |
| `Entitlement` | `enum` | N/A | `Licensing/` |
| `TemplateReference` | `record` | Non-empty name | `Word/Templates/` |

### When to Use Each Style

- **`ValueObject` base class**: Complex validation, multiple equality components
- **`record`**: Simple data containers, auto-equality
- **`enum`**: Fixed set of domain concepts

## Domain Events

Events follow past-tense naming and carry aggregate references:

| Event | Raised By | Handler Location |
|-------|-----------|------------------|
| `DocumentSavedEvent` | `Document.RequestSave()` | Infrastructure layer |
| `DocumentClosedEvent` | `Document.RequestClose()` | Infrastructure layer |
| `ResetStylesEvent` | `Document.ResetStyles()` | Word interop |
| `SetCustomPropertyEvent` | `Document.SetCustomProperty()` | Word interop |
| `LicenseUpdatedEvent` | `Licence.Create()` | Application layer |

### Event Flow

```
Aggregate.Method()
  → AddDomainEvent(event)
  → Handler calls IDomainEventDispatcher.DispatchAndClear()
  → Event handlers execute side effects
```

## Bounded Contexts

| Context | Purpose | Key Aggregates |
|---------|---------|----------------|
| **Documents** | Document state and metadata | Document |
| **Licensing** | Licence validation and entitlements | Licence |
| **Templates** | Template management | (external reference) |
| **Scripting** | Automation commands | (uses Document) |

### Context Integration

Documents and Licensing are separate contexts. Document doesn't know about Licence internals - it just checks `ILicenceRepository.HasEntitlement()`.

## Repository Pattern

```csharp
// Repository returns Maybe<T> for queries
public interface IDocumentRepository
{
    bool HasActiveDocument();
    Maybe<Document> GetActiveDocument();
}

// Application handler pattern
public Result Handle(Command cmd)
{
    var maybeDoc = _documentRepository.GetActiveDocument();
    if (maybeDoc.HasNoValue)
        return Result.Failure(DomainErrors.Document.NotFound);

    var document = maybeDoc.Value;
    // ... domain logic
}
```

## Quick Reference: Your Patterns

| When You See | It's This Pattern | Example |
|--------------|-------------------|---------|
| `AggregateRoot<TId>` | Aggregate root | `Document`, `Licence` |
| `ValueObject` + factories | Rich value object | `TextReplacementSpecification` |
| `sealed record` | Simple value object | `CustomProperty` |
| `AddDomainEvent()` | Event sourcing ready | All aggregates |
| `Maybe<T>` return | Query that may fail | Repository methods |
| `Result<T, Error>` | Command that may fail | Handler methods |
