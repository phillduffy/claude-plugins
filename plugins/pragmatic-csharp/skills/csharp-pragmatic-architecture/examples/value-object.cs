// Example: Value Objects from OfficeAddins codebase
// These demonstrate immutability, private constructors, and Maybe<T> for optional values

using CSharpFunctionalExtensions;

namespace Examples;

/// <summary>
/// Selection as a Value Object with factory method and Maybe<T> for optional navigation.
/// From: Core.Domain/Documents/Selections/Selection.cs
/// </summary>
public sealed class Selection : ValueObject
{
    private IDocumentRange Range { get; }
    public float TotalLength { get; }

    // Private constructor - only factory methods can create
    private Selection(IDocumentRange range, float totalLength)
    {
        Range = range;
        TotalLength = totalLength;
    }

    // Factory method for controlled creation
    public static Selection Get(IDocumentRange range, float totalLength)
        => new Selection(range, totalLength);

    public static Selection InsertionPoint(float position, float totalLength)
    {
        var range = new Range(position, position);
        return new Selection(range, totalLength);
    }

    // Computed properties - derived from immutable state
    public float Start => Range.Start;
    public float End => Range.End;
    private float Length => End - Start;

    public bool IsInsertionPoint => Length <= 0;
    public bool IsStartPoint => IsInsertionPoint && Start.Equals(0);
    public bool HasContentAfter => End < TotalLength - 1;
    public bool HasContentBefore => Start > 0;

    // Maybe<T> for optional values - no nulls!
    public Maybe<Selection> AfterSelection
        => HasContentAfter
            ? Create(End, TotalLength, TotalLength)
            : Maybe<Selection>.None;

    public Maybe<Selection> BeforeSelection
        => HasContentBefore
            ? Create(0, Start, TotalLength)
            : Maybe<Selection>.None;

    // Value equality - two Selections with same Start/End are equal
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Start;
        yield return End;
    }

    private static Selection Create(float start, float end, float totalLength)
    {
        var range = new Range(start, end);
        return new Selection(range, totalLength);
    }
}

/// <summary>
/// Simple record Value Object - immutable by default.
/// From: Core.Domain/Documents/CustomProperty.cs
/// </summary>
public sealed record CustomProperty(string Name, string Value);

/// <summary>
/// Error as a Value Object - used with Result pattern.
/// From: Core.Domain/Common/Primitives/Error.cs
/// </summary>
public sealed class Error : ValueObject
{
    public Error(string code, params object[] args)
    {
        Code = code;
        Args = args;
    }

    public string Code { get; }
    public object[] Args { get; }

    internal static Error None => new(string.Empty, string.Empty);

    // Implicit conversion for convenience
    public static implicit operator string(Error error)
        => error?.Code ?? string.Empty;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Code;
        yield return Args;
    }
}

// ============================================================================
// INDUSTRY PATTERN: Domain Primitive with Parse
// ============================================================================

/// <summary>
/// EmailAddress - Parse, Don't Validate pattern.
/// Once created, guaranteed valid. No re-validation needed downstream.
/// </summary>
public sealed class EmailAddress : ValueObject
{
    private EmailAddress(string value) => Value = value;

    public string Value { get; }

    public static Result<EmailAddress, Error> Create(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return new Error("Email.Empty");

        raw = raw.Trim().ToLowerInvariant();

        if (!raw.Contains('@') || !raw.Contains('.'))
            return new Error("Email.InvalidFormat");

        if (raw.Length > 254)
            return new Error("Email.TooLong");

        return new EmailAddress(raw);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}

/// <summary>
/// Money - prevents mixing currencies and invalid amounts.
/// </summary>
public readonly record struct Money(decimal Amount, string Currency)
{
    public static Result<Money, Error> Create(decimal amount, string currency)
    {
        if (amount < 0)
            return new Error("Money.NegativeAmount");

        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            return new Error("Money.InvalidCurrency");

        return new Money(amount, currency.ToUpperInvariant());
    }

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot add {Currency} to {other.Currency}");

        return this with { Amount = Amount + other.Amount };
    }
}
