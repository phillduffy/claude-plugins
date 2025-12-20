// Step Definitions Pattern
// From: OfficeAddins/tests/Core.Application.Tests/Features/Licensing/ActivateLicenceStepDefinitions.cs
// Demonstrates: Binding, DI injection, BeforeScenario, Result assertions

using CSharpFunctionalExtensions;
using Reqnroll;

namespace Core.Application.Tests.Features.Licensing;

[Binding]
public class ActivateLicenceStepDefinitions(
    ILicenceRepository licenceRepository)  // Constructor injection from Reqnroll DI
{
    // Handler under test - instantiated fresh per scenario
    private ActivateLicence.Handler _handler = null!;
    private Result<Licence, Error> _result;

    // SETUP - runs before each scenario in this binding class
    [BeforeScenario]
    public void Setup()
    {
        _handler = new ActivateLicence.Handler(licenceRepository);
    }

    // WHEN STEPS - actions that trigger behavior
    [When("I activate the licence")]
    public void WhenIActivateTheLicence()
    {
        _result = _handler.Handle(new ActivateLicence.Command());
    }

    // THEN STEPS - assertions on outcomes
    [Then("the licence is activated")]
    public void ThenTheLicenceIsActivated()
    {
        Assert.True(_result.IsSuccess);
    }

    [Then("the activation fails because the licence is already active")]
    public void ThenTheActivationFailsBecauseAlreadyActive()
    {
        Assert.True(_result.IsFailure);
        Assert.Equal(DomainErrors.Licensing.AlreadyActive, _result.Error);
    }
}

// KEY PATTERNS:
// 1. Primary constructor for DI injection (mocks from TestDependencies)
// 2. BeforeScenario for per-scenario setup
// 3. Handler instantiated in setup, not constructor
// 4. Result stored as field, asserted in Then steps
// 5. Error assertions use domain error constants
// 6. Step methods are thin - just wire to handler + assert
// 7. One binding class per feature (or shared for cross-cutting)
