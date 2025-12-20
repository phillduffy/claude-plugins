# BDD Patterns in OfficeAddins

This reference maps BDD patterns to concrete implementations in the OfficeAddins test suite.

## Project Structure

```
tests/
├── Core.Application.Tests/
│   ├── Features/           # Feature-specific tests
│   │   ├── Licensing/
│   │   │   ├── ActivateLicence.feature
│   │   │   └── ActivateLicenceStepDefinitions.cs
│   │   ├── Documents/
│   │   └── Templates/
│   └── Shared/
│       ├── TestDependencies.cs   # DI setup
│       └── SharedContext.cs      # Cross-step state
└── Shared/
    └── SharedStepDefinitions.cs  # Reusable steps
```

## Feature File Conventions

| Convention | Example | Why |
|------------|---------|-----|
| One feature per file | `ActivateLicence.feature` | Easy to find, focused tests |
| Background for shared setup | `Given the licensing service is available` | DRY, clear preconditions |
| Rule blocks for grouping | `Rule: Licence can be activated` | Organize related scenarios |
| Semantic Then steps | `fails because the licence is already active` | Hides error codes |

## Step Definition Patterns

### Your Handler Test Pattern

```csharp
[Binding]
public class SomeFeatureStepDefinitions(
    IDocumentRepository documentRepository,    // Injected mock
    SharedContext sharedContext)               // Cross-step state
{
    private SomeHandler _handler = null!;

    [BeforeScenario]
    public void Setup()
    {
        _handler = new SomeHandler(documentRepository);
    }

    [When("I do something")]
    public void WhenIDoSomething()
    {
        sharedContext.Result = _handler.Handle(new Command());
    }

    [Then("it succeeds")]
    public void ThenItSucceeds()
    {
        Assert.True(sharedContext.Result.IsSuccess);
    }
}
```

### Your Mock Setup Pattern

```csharp
// In Given steps, configure mock returns
documentRepository.GetActiveDocument()
    .Returns(Maybe<Document>.From(document));

// In Then steps, verify calls
domainEventDispatcher.Received(1).DispatchAndClear(Arg.Any<IAggregateRoot>());
```

## Shared Steps Inventory

| Step Pattern | Where Used | Purpose |
|--------------|------------|---------|
| `Given an active document` | Most features | Sets up Document + repository mock |
| `Given no active document` | Error cases | Clears Document + returns None |
| `Then the result should fail` | All failures | Asserts Result.IsFailure |
| `Then domain events were dispatched` | Side effect tests | Verifies dispatcher called |

## Test Dependency Registration

Your `TestDependencies.cs` registers:

| Registration | Purpose |
|--------------|---------|
| `IDomainEventDispatcher` | Verify domain events dispatched |
| `IDocumentRepository` | Mock document queries |
| `ILocalisationService` | Return error codes as strings |
| `ILogger<THandler>` | Per-handler logger mocks |
| `SharedContext` | Cross-step state sharing |

## Builder Patterns

```csharp
// DocumentBuilder for test data
var doc = new DocumentBuilder("Test.docx")
    .SetIsProtected()
    .WithCustomProperty("Key", "Value")
    .Build();
```

## Quick Reference: Your Conventions

| When Writing | Do This | Example |
|--------------|---------|---------|
| New feature | Create `.feature` + `StepDefinitions.cs` | `Features/Licensing/` |
| Shared Given | Add to `SharedStepDefinitions.cs` | `Given an active document` |
| Mock setup | In Given step with `.Returns()` | `repository.Get().Returns(doc)` |
| Verify call | In Then step with `.Received()` | `service.Received(1).Save()` |
| Cross-step state | Use `SharedContext` | `sharedContext.Document = doc` |
| Error assertion | Semantic Then step | `fails because no document is active` |

## Feature Files by Domain

| Domain | Features | Key Scenarios |
|--------|----------|---------------|
| **Licensing** | ActivateLicence | Success, already active |
| **Documents** | CreateDocument, SaveDocument, CloseDocument | CRUD + protection |
| **Templates** | AttachTemplate, ResetStyles | Template operations |
| **Properties** | SetCustomProperty, GetProperty | Metadata management |
