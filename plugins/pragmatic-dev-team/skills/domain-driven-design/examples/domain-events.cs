// Domain Events Pattern
// From: OfficeAddins/src/Core/Core.Domain/Common/Events/
// Demonstrates: Event base class, concrete events, aggregate integration

namespace Core.Domain.Common.Events;

/// <summary>
/// Marker interface for domain events - enables dispatching.
/// </summary>
public interface IDomainEvent { }

/// <summary>
/// Base class for all domain events with timestamp.
/// </summary>
public abstract class DomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

// CONCRETE DOMAIN EVENTS - named in past tense (something happened)

namespace Core.Domain.Documents.DomainEvents;

public class DocumentSavedEvent(Document document) : DomainEvent
{
    public Document Document { get; } = document;
}

public class DocumentClosedEvent(Document document) : DomainEvent
{
    public Document Document { get; } = document;
}

public class ResetStylesEvent(string documentId) : DomainEvent
{
    public string DocumentId { get; } = documentId;
}

public class SetCustomPropertyEvent(CustomProperty property, string? oldValue) : DomainEvent
{
    public CustomProperty Property { get; } = property;
    public string? OldValue { get; } = oldValue;
}

public class ReplaceTextEvent(TextReplacementSpecification spec, int total) : DomainEvent
{
    public TextReplacementSpecification Specification { get; } = spec;
    public int TotalReplacements { get; } = total;
}

// AGGREGATE ROOT BASE - manages event collection

namespace Core.Domain.Common.Primitives;

public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = [];

    protected AggregateRoot(TId id) : base(id) { }

    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent @event) => _domainEvents.Add(@event);

    public void ClearDomainEvents() => _domainEvents.Clear();
}

// KEY PATTERNS:
// 1. Events named in PAST TENSE (DocumentSaved, not SaveDocument)
// 2. Events are immutable - capture what happened
// 3. Events carry relevant data (aggregate reference, changed values)
// 4. OccurredOn timestamp for audit/ordering
// 5. AggregateRoot collects events, dispatcher clears after handling
// 6. Primary constructors for concise event definitions
