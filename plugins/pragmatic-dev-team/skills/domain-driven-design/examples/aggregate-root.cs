// Aggregate Root Pattern - Document Entity
// From: OfficeAddins/src/Core/Core.Domain/Documents/Document.cs
// Demonstrates: AggregateRoot inheritance, domain events, invariant enforcement

using CSharpFunctionalExtensions;

namespace Core.Domain.Documents;

/// <summary>
/// Document aggregate root - encapsulates document state and enforces invariants.
/// All mutations go through domain methods that raise events.
/// </summary>
public sealed class Document : AggregateRoot<string>
{
    private bool IsOpen { get; set; } = true;

    internal Document(string identifier) : base(identifier) { }

    public required string Name { get; init; }
    public bool IsProtected { get; init; }
    public int Length { get; init; }
    public required TemplateReference AttachedTemplate { get; init; }
    public required CustomProperties CustomProperties { get; init; }

    // Derived property from value object collection
    public string? Comment => Properties.TryGet("Comment").GetValueOrDefault();

    // INVARIANT: Selection must be within document bounds
    public bool IsValidSelection(Selection selection)
        => selection.Start >= 0 && selection.End <= Length;

    // INVARIANT: Protected documents cannot be edited
    public bool CanEdit() => !IsProtected;

    // Domain method with state transition + event
    public void RequestClose()
    {
        if (!IsOpen) return;  // Idempotent

        IsOpen = false;
        AddDomainEvent(new DocumentClosedEvent(this));
    }

    // Domain method with precondition check
    public void RequestSave()
    {
        if (!IsOpen)
            throw new InvalidOperationException("Cannot save a closed document.");

        AddDomainEvent(new DocumentSavedEvent(this));
    }

    // Domain method returning Result for expected failures
    public Result ResetStyles()
    {
        if (IsTemplateFileFormat())
            return Result.Failure(DomainErrors.Document.InvalidFormat);

        AddDomainEvent(new ResetStylesEvent(Id));
        return Result.Success();
    }

    // Domain method with change detection
    public Result<bool> SetCustomProperty(string name, string value)
    {
        var maybeOldProperty = CustomProperties.TryGet(name);
        var oldValue = maybeOldProperty.HasValue ? maybeOldProperty.Value : null;

        // No change = no event
        if (oldValue?.Equals(value, StringComparison.Ordinal) == true)
            return false;

        var updated = new CustomProperty(name, value);
        AddDomainEvent(new SetCustomPropertyEvent(updated, oldValue));
        return true;
    }

    private bool IsTemplateFileFormat()
        => Name.EndsWith(".dotx", StringComparison.OrdinalIgnoreCase) ||
           Name.EndsWith(".dotm", StringComparison.OrdinalIgnoreCase);
}

// KEY PATTERNS:
// 1. AggregateRoot<TId> base class provides Id + domain event collection
// 2. Internal constructor - only factories/repositories create aggregates
// 3. Domain methods express business operations, not CRUD
// 4. Invariants checked before mutations
// 5. Events raised for state changes (event sourcing ready)
// 6. Result<T> for expected failures, exceptions for bugs
