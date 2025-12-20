// Example: Decorator Pattern for Cross-Cutting Concerns
// From: Core.Application/Abstractions/Behaviours/LoggingDecorator.cs
// Demonstrates: Pure Core / Impure Shell, decorator chain, Scrutor registration

using CSharpFunctionalExtensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Examples;

// ============================================================================
// LOGGING DECORATOR - Wraps all handlers with logging
// ============================================================================

public static class LoggingDecorator
{
    /// <summary>
    /// Wraps query handlers with logging and error notification.
    /// This is the IMPURE SHELL - handles side effects.
    /// </summary>
    public sealed class QueryHandler<TQuery, TResult>(
        IQueryHandler<TQuery, TResult> decorated,        // Inner handler
        ILogger<QueryHandler<TQuery, TResult>> logger,
        ILocalisationService localisationService,
        IUserNotifier userNotifier)
        : IQueryHandler<TQuery, TResult>
        where TQuery : class, IQuery
    {
        public Result<TResult, Error> Handle(TQuery query)
        {
            try
            {
                // IMPURE: Logging side effect
                logger.LogDebug("Handling Query {RequestType}", query.DisplayName);

                // Delegate to inner (eventually pure) handler
                var result = decorated.Handle(query);

                if (result.IsSuccess)
                {
                    logger.LogDebug("Query - {RequestType} handled successfully", query.DisplayName);
                }
                else
                {
                    // IMPURE: User notification side effect
                    var localisedErrorMessage = localisationService.GetString(result.Error);
                    userNotifier.Alert(localisedErrorMessage, query.DisplayName);
                    logger.LogWarning("Query - {RequestType} failed: {Error}",
                        query.DisplayName, localisedErrorMessage);
                }

                return result;
            }
            catch (Exception ex)
            {
                // Convert unexpected exceptions to Result
                logger.LogError(ex, "Error handling Query {RequestType}", query.DisplayName);
                return Result.Failure<TResult, Error>(new Error("UnexpectedError", ex.Message));
            }
        }
    }

    /// <summary>
    /// Wraps command handlers with logging and error notification.
    /// </summary>
    public sealed class CommandHandler<TCommand, TResult>(
        ICommandHandler<TCommand, TResult> decorated,
        ILogger<CommandHandler<TCommand, TResult>> logger,
        ILocalisationService localisationService,
        IUserNotifier userNotifier)
        : ICommandHandler<TCommand, TResult>
        where TCommand : class, ICommand
    {
        public Result<TResult, Error> Handle(TCommand command)
        {
            try
            {
                logger.LogDebug("Handling Command {RequestType}", command.DisplayName);

                var result = decorated.Handle(command);

                if (result.IsSuccess)
                {
                    logger.LogDebug("Command - {RequestType} handled successfully",
                        command.DisplayName);
                }
                else
                {
                    var localisedErrorMessage = localisationService.GetString(result.Error);
                    userNotifier.Alert(localisedErrorMessage, command.DisplayName);
                    logger.LogWarning("Command - {RequestType} failed: {Error}",
                        command.DisplayName, result.Error);
                }

                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error handling Command {RequestType}", command.DisplayName);
                return Result.Failure<TResult, Error>(new Error("UnexpectedError", ex.Message));
            }
        }
    }
}

// ============================================================================
// DOCUMENT REQUIRED DECORATOR - Checks document is open
// ============================================================================

public static class DocumentRequiredDecorator
{
    public sealed class CommandHandler<TCommand, TResult>(
        ICommandHandler<TCommand, TResult> decorated,
        IDocumentRepository documentRepository)
        : ICommandHandler<TCommand, TResult>
        where TCommand : class, ICommand
    {
        public Result<TResult, Error> Handle(TCommand command)
        {
            // Check if handler requires document (via attribute)
            if (!DecoratorHelpers.HasAttribute<DocumentRequiredAttribute, TCommand>())
                return decorated.Handle(command);

            // Verify document exists
            var document = documentRepository.GetActiveDocument();
            if (document.HasNoValue)
                return Result.Failure<TResult, Error>(DomainErrors.Document.NoActiveDocument);

            return decorated.Handle(command);
        }
    }
}

// ============================================================================
// DECORATOR HELPERS - Reflection for attribute checking
// ============================================================================

public static class DecoratorHelpers
{
    public static bool HasAttribute<TAttribute, TCommand>()
        where TAttribute : Attribute
    {
        // Get the handler type from the command
        var handlerType = typeof(TCommand).DeclaringType?
            .GetNestedTypes()
            .FirstOrDefault(t => t.Name == "Handler");

        return handlerType?.GetCustomAttributes(typeof(TAttribute), true).Any() ?? false;
    }
}

// ============================================================================
// DI REGISTRATION WITH SCRUTOR
// ============================================================================

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Auto-register all handlers
        services.Scan(s => s.FromAssembliesOf(typeof(ICommand))
            .AddClasses(c => c.AssignableTo(typeof(ICommandHandler<,>)))
            .AsImplementedInterfaces()
            .WithTransientLifetime());

        services.Scan(s => s.FromAssembliesOf(typeof(IQuery))
            .AddClasses(c => c.AssignableTo(typeof(IQueryHandler<,>)))
            .AsImplementedInterfaces()
            .WithTransientLifetime());

        // Decorator chain - ORDER MATTERS!
        // First registered = outermost wrapper
        // Execution: Logging -> DocumentRequired -> [Handler]
        services.Decorate(typeof(ICommandHandler<,>),
            typeof(DocumentRequiredDecorator.CommandHandler<,>));
        services.Decorate(typeof(ICommandHandler<,>),
            typeof(LoggingDecorator.CommandHandler<,>));

        services.Decorate(typeof(IQueryHandler<,>),
            typeof(LoggingDecorator.QueryHandler<,>));

        return services;
    }
}

// ============================================================================
// FULL DECORATOR CHAIN EXAMPLE (from your codebase)
// ============================================================================

/*
Actual decorator order in OfficeAddins:

services.Decorate(typeof(ICommandHandler<,>), typeof(UndoDecorator.Handler<,>));
services.Decorate(typeof(ICommandHandler<,>), typeof(PerformanceDecorator.Handler<,>));
services.Decorate(typeof(ICommandHandler<,>), typeof(DocumentRequiredDecorator.CommandHandler<,>));
services.Decorate(typeof(ICommandHandler<,>), typeof(LicensingDecorator.CommandHandler<,>));
services.Decorate(typeof(ICommandHandler<,>), typeof(DocumentContextDecorator.CommandHandler<,>));
services.Decorate(typeof(ICommandHandler<,>), typeof(LoggingDecorator.CommandHandler<,>));

Execution order (outside -> inside):
1. LoggingDecorator - Log start, catch exceptions
2. DocumentContextDecorator - Set up document context
3. LicensingDecorator - Check entitlements
4. DocumentRequiredDecorator - Verify document open
5. PerformanceDecorator - Measure execution time
6. UndoDecorator - Wrap in undo action
7. [Pure Handler] - Business logic
*/

// Stub interfaces for compilation
public interface ILocalisationService
{
    string GetString(Error error);
}

public interface IUserNotifier
{
    void Alert(string message, string title);
}
