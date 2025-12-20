// Example: Handler with Result Pattern
// From: Core.Application/Word/ResetStyles.cs
// Demonstrates: Result<T, Error>, vertical slice, attribute guards, explicit dependencies

using CSharpFunctionalExtensions;

namespace Examples;

/// <summary>
/// Complete vertical slice: Command + Handler in single file.
/// All code for "Reset Styles" feature lives here.
/// </summary>
public static class ResetStyles
{
    /// <summary>
    /// Command - immutable request data.
    /// DisplayName used by decorators for logging/UI.
    /// </summary>
    public class Command : ICommand
    {
        public string DisplayName => "Reset Styles";
    }

    /// <summary>
    /// Handler - pure business logic.
    /// Attributes declare cross-cutting concerns (handled by decorators).
    /// </summary>
    [RequireEntitlement(EntitlementType.WordStandardTools)]  // Licensing check
    [DocumentRequired]                                        // Document must be open
    public class Handler(
        IDocumentRepository documentRepository,
        IDomainEventDispatcher eventDispatcher,
        IDocumentEditor documentEditor) : ICommandHandler<Command, Unit>
    {
        public Result<Unit, Error> Handle(Command command)
        {
            // 1. Get aggregate, convert Maybe to Result
            var documentResult = documentRepository.GetActiveDocument()
                .ToResult(DomainErrors.Document.NotFound);

            // 2. Early return on failure - no exceptions
            if (documentResult.IsFailure)
                return documentResult.Error;

            var document = documentResult.Value;

            // 3. Execute domain logic
            var result = document.ResetStyles();
            if (result.IsFailure)
            {
                return DomainErrors.ResetStyles.Failed;
            }

            // 4. Execute infrastructure operation
            var resetStyleResult = documentEditor.ResetStyles();
            if (resetStyleResult.IsFailure)
            {
                return resetStyleResult;
            }

            // 5. Dispatch domain events (side effect at edge)
            eventDispatcher.DispatchAndClear(document);

            // 6. Return success
            return Unit.Instance;
        }
    }
}

// ============================================================================
// STATIC ERROR DEFINITIONS
// ============================================================================

/// <summary>
/// Centralized error definitions - discoverable, consistent, localizable.
/// From: Core.Domain/Common/Errors/DomainErrors.cs
/// </summary>
public static class DomainErrors
{
    public static class Document
    {
        public static readonly Error NotFound = new("Document.NotFound");
        public static readonly Error NoActiveDocument = new("Document.NoActiveDocument");
        public static readonly Error InvalidSelection = new("Document.InvalidSelection");
    }

    public static class ResetStyles
    {
        public static readonly Error Failed = new("ResetStyles.Failed");
    }

    public static class ApplyStyle
    {
        // Parameterized error for context
        public static Error StyleNotFound(string styleName) => new("ApplyStyle.NotFound", styleName);
        public static readonly Error Failed = new("ApplyStyle.Failed");
    }
}

// ============================================================================
// INTERFACES (No MediatR!)
// ============================================================================

public interface ICommand
{
    string DisplayName { get; }
}

public interface ICommandHandler<TCommand, TResult>
    where TCommand : ICommand
{
    Result<TResult, Error> Handle(TCommand command);
}

public interface IQuery
{
    string DisplayName { get; }
}

public interface IQueryHandler<TQuery, TResult>
    where TQuery : IQuery
{
    Result<TResult, Error> Handle(TQuery query);
}

// ============================================================================
// QUERY EXAMPLE
// ============================================================================

public static class GetAboutInformation
{
    public class Query : IQuery
    {
        public string DisplayName => "About Information";
    }

    public record Response(
        string Version,
        string Copyright,
        string LicenseInfo);

    public class Handler(
        IAboutInformationStrategy aboutInformationStrategy)
        : IQueryHandler<Query, Response>
    {
        public Result<Response, Error> Handle(Query query)
        {
            var info = aboutInformationStrategy.GetInfo();

            return new Response(
                info.Version,
                info.Copyright,
                info.LicenseInfo);
        }
    }
}

// ============================================================================
// ATTRIBUTES FOR CROSS-CUTTING CONCERNS
// ============================================================================

/// <summary>
/// Marks handler as requiring an open document.
/// DocumentRequiredDecorator checks this before calling handler.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class DocumentRequiredAttribute : Attribute { }

/// <summary>
/// Marks handler as requiring specific license entitlement.
/// LicensingDecorator checks this before calling handler.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class RequireEntitlementAttribute(EntitlementType entitlement) : Attribute
{
    public EntitlementType Entitlement { get; } = entitlement;
}

public enum EntitlementType
{
    WordStandardTools,
    WordAdvancedTools,
    DocumentManagement
}

// Stub interfaces for compilation
public interface IDocumentRepository
{
    Maybe<Document> GetActiveDocument();
}

public interface IDomainEventDispatcher
{
    void DispatchAndClear(object aggregate);
}

public interface IDocumentEditor
{
    Result<Unit, Error> ResetStyles();
}

public interface IAboutInformationStrategy
{
    AboutInfo GetInfo();
}

public record AboutInfo(string Version, string Copyright, string LicenseInfo);
public class Document
{
    public Result ResetStyles() => Result.Success();
}
public struct Unit
{
    public static Unit Instance => default;
}
