// Example: Aggregate Root with Domain Events
// Based on: Core.Domain/Documents/Document.cs
// Demonstrates: Aggregate guards, domain events, encapsulation

using CSharpFunctionalExtensions;

namespace Examples;

// ============================================================================
// AGGREGATE ROOT BASE CLASS
// ============================================================================

public abstract class AggregateRoot<TId>
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public TId Id { get; protected set; } = default!;

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}

public interface IDomainEvent
{
    DateTime OccurredAt { get; }
}

// ============================================================================
// DOCUMENT AGGREGATE - Protects Invariants
// ============================================================================

/// <summary>
/// Document aggregate root.
/// All mutations go through methods that protect invariants.
/// Domain events capture state changes for later dispatch.
/// </summary>
public sealed class Document : AggregateRoot<string>
{
    // Private state - not exposed for direct modification
    private readonly Dictionary<string, CustomProperty> _customProperties = new();
    private bool _isOpen = true;
    private bool _isProtected;

    // Private constructor - use factory method
    private Document(string id, string name)
    {
        Id = id;
        Name = name;
    }

    // Public read-only properties
    public string Name { get; private set; }
    public bool IsOpen => _isOpen;
    public bool CanEdit => !_isProtected;

    // Expose collection as read-only
    public IReadOnlyDictionary<string, CustomProperty> CustomProperties
        => _customProperties;

    // ========================================================================
    // FACTORY METHOD
    // ========================================================================

    public static Result<Document, Error> Create(string id, string name)
    {
        if (string.IsNullOrWhiteSpace(id))
            return new Error("Document.InvalidId");

        if (string.IsNullOrWhiteSpace(name))
            return new Error("Document.InvalidName");

        return new Document(id, name);
    }

    // ========================================================================
    // DOMAIN OPERATIONS - Protect Invariants
    // ========================================================================

    /// <summary>
    /// Request to save the document.
    /// Guards: Document must be open.
    /// </summary>
    public void RequestSave()
    {
        // Guard: Illegal state check
        if (!_isOpen)
            throw new InvalidOperationException("Cannot save a closed document.");

        // Raise domain event - actual save happens in infrastructure
        AddDomainEvent(new DocumentSavedEvent(this));
    }

    /// <summary>
    /// Request to close the document.
    /// </summary>
    public void RequestClose()
    {
        if (!_isOpen)
            return; // Already closed, idempotent

        _isOpen = false;
        AddDomainEvent(new DocumentClosedEvent(this));
    }

    /// <summary>
    /// Set a custom property.
    /// Guards: Document must be editable.
    /// </summary>
    public Result SetCustomProperty(string name, string value)
    {
        if (!CanEdit)
            return Result.Failure("Document is protected");

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure("Property name cannot be empty");

        var property = new CustomProperty(name, value);

        var isNew = !_customProperties.ContainsKey(name);
        _customProperties[name] = property;

        AddDomainEvent(new CustomPropertyChangedEvent(this, property, isNew));

        return Result.Success();
    }

    /// <summary>
    /// Reset document styles.
    /// Guards: Document must be editable.
    /// </summary>
    public Result ResetStyles()
    {
        if (!CanEdit)
            return Result.Failure("Document is protected");

        // Domain logic here...

        AddDomainEvent(new StylesResetEvent(this));
        return Result.Success();
    }

    /// <summary>
    /// Check if a selection is valid for this document.
    /// Pure query - no side effects.
    /// </summary>
    public bool IsValidSelection(Selection selection)
    {
        return selection.End <= TotalLength;
    }

    public float TotalLength { get; private set; }
}

// ============================================================================
// DOMAIN EVENTS
// ============================================================================

public abstract record DomainEventBase : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public sealed record DocumentSavedEvent(Document Document) : DomainEventBase;

public sealed record DocumentClosedEvent(Document Document) : DomainEventBase;

public sealed record CustomPropertyChangedEvent(
    Document Document,
    CustomProperty Property,
    bool IsNew) : DomainEventBase;

public sealed record StylesResetEvent(Document Document) : DomainEventBase;

// ============================================================================
// EVENT DISPATCHER
// ============================================================================

public interface IDomainEventDispatcher
{
    void DispatchAndClear<T>(T aggregate) where T : AggregateRoot<string>;
}

/// <summary>
/// Dispatches domain events to registered handlers.
/// This is IMPURE SHELL - executes side effects.
/// </summary>
public class DomainEventDispatcher(IServiceProvider serviceProvider) : IDomainEventDispatcher
{
    public void DispatchAndClear<T>(T aggregate) where T : AggregateRoot<string>
    {
        foreach (var domainEvent in aggregate.DomainEvents)
        {
            // Find and invoke handler for this event type
            var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
            var handlers = serviceProvider.GetServices(handlerType);

            foreach (var handler in handlers)
            {
                // Invoke Handle method dynamically
                handlerType.GetMethod("Handle")?.Invoke(handler, new[] { domainEvent });
            }
        }

        aggregate.ClearDomainEvents();
    }
}

public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
{
    void Handle(TEvent @event);
}

// ============================================================================
// EXAMPLE EVENT HANDLER
// ============================================================================

public class DocumentSavedEventHandler(
    ILogger logger,
    IAuditService auditService) : IDomainEventHandler<DocumentSavedEvent>
{
    public void Handle(DocumentSavedEvent @event)
    {
        logger.LogInformation("Document {Id} saved", @event.Document.Id);
        auditService.RecordSave(@event.Document.Id, @event.OccurredAt);
    }
}

// Stub interfaces
public interface ILogger
{
    void LogInformation(string message, params object[] args);
}

public interface IAuditService
{
    void RecordSave(string documentId, DateTime occurredAt);
}
