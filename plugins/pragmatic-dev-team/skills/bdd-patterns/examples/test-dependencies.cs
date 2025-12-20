// Test Dependencies Pattern - Reqnroll DI Setup
// From: OfficeAddins/tests/Core.Application.Tests/Shared/TestDependencies.cs
// Demonstrates: BeforeScenario hooks, IObjectContainer, mock registration

using Microsoft.Extensions.Logging;
using NSubstitute;
using Reqnroll;
using Reqnroll.BoDi;

namespace Core.Application.Tests.Shared;

/// <summary>
/// Factory for test loggers - avoids registering ILogger per handler.
/// </summary>
public class TestLoggerFactory
{
    public ILogger<T> Create<T>() => Substitute.For<ILogger<T>>();
}

[Binding]
public class TestDependencies(IObjectContainer container)
{
    // ORDER = 0 ensures this runs FIRST before other BeforeScenario hooks
    [BeforeScenario(Order = 0)]
    public void RegisterDependencies()
    {
        // CORE SERVICES - shared across all features
        container.RegisterInstanceAs(Substitute.For<IDomainEventDispatcher>());
        container.RegisterInstanceAs(Substitute.For<IDocumentRepository>());
        container.RegisterInstanceAs(Substitute.For<IUserNotifier>());

        // LOCALISATION SETUP - configure mock behavior
        var localisation = Substitute.For<ILocalisationService>();
        localisation.GetString(Arg.Any<Message>()).Returns(a => a.ArgAt<Message>(0).Code);
        localisation.GetString(Arg.Any<Error>()).Returns(a => a.ArgAt<Error>(0).Code);
        container.RegisterInstanceAs(localisation);

        // DOMAIN SERVICES
        container.RegisterInstanceAs(Substitute.For<ITemplateRepository>());
        container.RegisterInstanceAs(Substitute.For<ILicenceRepository>());
        container.RegisterInstanceAs(Substitute.For<IDocumentEditor>());

        // INFRASTRUCTURE
        container.RegisterInstanceAs(Substitute.For<IEnvironmentService>());
        container.RegisterInstanceAs(Substitute.For<IWordApplication>());

        // LOGGERS - registered per handler type
        container.RegisterInstanceAs(Substitute.For<ILogger<CreateDocument.Handler>>());
        container.RegisterInstanceAs(Substitute.For<ILogger<CloseDocument.Handler>>());
        container.RegisterInstanceAs(Substitute.For<ILogger<SaveDocument.Handler>>());
        container.RegisterInstanceAs(Substitute.For<ILogger<ResetStyles.Handler>>());

        // SHARED CONTEXT - for passing state between step classes
        container.RegisterTypeAs<SharedContext, SharedContext>();
    }
}

/// <summary>
/// Shared context for passing state between step definition classes.
/// Registered as instance per scenario.
/// </summary>
public class SharedContext
{
    public Document? Document { get; set; }
    public IResult Result { get; set; } = null!;
    public string? LastError { get; set; }
}

// KEY PATTERNS:
// 1. [BeforeScenario(Order = 0)] ensures DI setup runs first
// 2. IObjectContainer is Reqnroll's DI container
// 3. RegisterInstanceAs() for mock instances
// 4. RegisterTypeAs<T, T>() for context classes (new instance per scenario)
// 5. Loggers registered per handler type (ILogger<T> is not covariant)
// 6. Localisation mock configured with .Returns() for predictable behavior
// 7. SharedContext is the bridge between step definition classes
