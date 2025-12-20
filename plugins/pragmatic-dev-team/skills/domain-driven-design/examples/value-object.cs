// Value Object Pattern - TextReplacementSpecification
// From: OfficeAddins/src/Core/Core.Domain/Documents/TextReplacementSpecification.cs
// Demonstrates: Immutability, equality by value, domain validation, static factories

using CSharpFunctionalExtensions;

namespace Core.Domain.Documents;

/// <summary>
/// Specification for text replacement - encapsulates find/replace behavior.
/// Immutable, equality by value, validates on construction.
/// </summary>
public sealed class TextReplacementSpecification : ValueObject
{
    // Private constructor forces use of static factories
    private TextReplacementSpecification(
        string findText,
        string? replacementText,
        string displayName,
        bool matchCase = false,
        bool matchWholeWord = false,
        bool useWildcards = false)
    {
        // VALIDATE ON CONSTRUCTION - illegal states unrepresentable
        if (string.IsNullOrEmpty(findText))
            throw new DomainException(DomainErrors.TextReplacement.EmptyFindText);

        DisplayName = displayName;
        FindText = findText;
        ReplacementText = replacementText ?? string.Empty;
        MatchCase = matchCase;
        MatchWholeWord = matchWholeWord;
        UseWildcards = useWildcards;
    }

    // All properties immutable (no setters)
    public string DisplayName { get; }
    public string FindText { get; }
    public string ReplacementText { get; }
    public bool MatchCase { get; }
    public bool MatchWholeWord { get; }
    public bool UseWildcards { get; }

    // STATIC FACTORIES for common domain operations
    public static TextReplacementSpecification DeleteExtraSpaces()
        => new("[ ]{2,}", " ", useWildcards: true, displayName: "Delete Extra Spaces");

    public static TextReplacementSpecification DeleteExtraParagraphs()
        => new("(^13){2,}", "\r", useWildcards: true, displayName: "Delete Extra Paragraphs");

    // General factory with explicit parameters
    public static TextReplacementSpecification FromText(
        string findText,
        string replacementText,
        string displayName = "Replace",
        bool matchCase = false,
        bool matchWholeWord = false,
        bool useWildcards = false)
        => new(findText, replacementText, displayName, matchCase, matchWholeWord, useWildcards);

    // VALUE EQUALITY - required by CSharpFunctionalExtensions.ValueObject
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return DisplayName;
        yield return FindText;
        yield return ReplacementText;
        yield return MatchCase;
        yield return MatchWholeWord;
        yield return UseWildcards;
    }
}

// SIMPLE VALUE OBJECTS using C# records
public sealed record CustomProperty(string Name, string Value);
public sealed record DocumentProperty(string Name, string? Value);
public sealed record Range(float Start, float End);

// KEY PATTERNS:
// 1. Private constructor + static factories = parse-don't-validate
// 2. Validation in constructor - invalid objects cannot exist
// 3. All properties readonly - immutability guaranteed
// 4. GetEqualityComponents() for value equality
// 5. Records for simple value objects (auto-implements equality)
// 6. Domain-specific factories (DeleteExtraSpaces) encode business rules
