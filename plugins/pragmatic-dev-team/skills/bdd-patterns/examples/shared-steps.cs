// Shared Step Definitions Pattern
// From: OfficeAddins/tests/Shared/SharedStepDefinitions.cs
// Demonstrates: Reusable steps, alternative bindings, mock setup, context sharing

using CSharpFunctionalExtensions;
using NSubstitute;
using Reqnroll;

namespace Shared;

[Binding]
public class SharedStepDefinitions(
    SharedContext sharedContext,
    IDocumentRepository documentRepository,
    IDomainEventDispatcher domainEventDispatcher)
{
    // DOCUMENT STATE - reusable Given steps

    // ALTERNATIVE BINDINGS - multiple ways to say the same thing
    [Given("there is an active document")]
    [Given("an active document")]
    public void GivenThereIsAnActiveDocument()
    {
        sharedContext.Document = new DocumentBuilder("TestDoc.docx").Build();
        documentRepository.HasActiveDocument().Returns(true);
        documentRepository.GetActiveDocument()
            .Returns(Maybe<Document>.From(sharedContext.Document));
    }

    [Given("there is no active document")]
    [Given("no active document")]
    public void GivenThereIsNoActiveDocument()
    {
        sharedContext.Document = null;
        documentRepository.HasActiveDocument().Returns(false);
        documentRepository.GetActiveDocument().Returns(Maybe<Document>.None);
    }

    // BUILDER PATTERN for complex setup
    [Given("the document is protected and cannot perform automated replacements")]
    public void GivenTheDocumentIsProtectedAndCannotPerformAutomatedReplacements()
    {
        var builder = new DocumentBuilder("TestDoc.docx").SetIsProtected();
        sharedContext.Document = builder.Build();
        documentRepository.GetActiveDocument()
            .Returns(Maybe<Document>.From(sharedContext.Document));
    }

    // DOMAIN EVENT ASSERTIONS - verify side effects
    [Then("all domain events for the document should be dispatched")]
    public void ThenAllDomainEventsForTheDocumentShouldBeDispatched()
    {
        domainEventDispatcher.Received(1).DispatchAndClear(Arg.Any<IAggregateRoot>());
    }

    [Then("no domain events should be dispatched")]
    public void ThenNoDomainEventsShouldBeDispatched()
    {
        domainEventDispatcher.DidNotReceive().DispatchAndClear(Arg.Any<IAggregateRoot>());
    }

    // RESULT ASSERTIONS - shared outcome checks
    [Then("the result should fail")]
    [Then("the operation fails")]
    public void ThenTheResultShouldFail()
    {
        Assert.True(sharedContext.Result.IsFailure);
    }

    [Then("the result should be successful")]
    [Then("the query succeeds")]
    public void ThenTheResultShouldBeSuccessful()
    {
        Assert.NotNull(sharedContext.Result);
        if (sharedContext.Result.IsFailure)
            Assert.Fail($"Expected success but got failure: {sharedContext.Result.Error}");
        Assert.True(sharedContext.Result.IsSuccess);
    }

    // SEMANTIC ERROR ASSERTIONS - domain-focused, no error codes in features
    [Then("the operation fails because no document is active")]
    public void ThenOperationFailsBecauseNoDocumentActive()
    {
        Assert.NotNull(sharedContext.Result);
        Assert.True(sharedContext.Result.IsFailure);
        Assert.Equal("Document.NotFound", sharedContext.Result.Error);
    }
}

// KEY PATTERNS:
// 1. SharedContext for passing state between step classes
// 2. Alternative bindings [Given("X")] [Given("Y")] for readability
// 3. Builder pattern (DocumentBuilder) for complex test data
// 4. NSubstitute.Returns() for mock setup
// 5. .Received() / .DidNotReceive() for verifying calls
// 6. Semantic Then steps hide error codes from feature files
// 7. Shared steps in separate namespace, available to all features
