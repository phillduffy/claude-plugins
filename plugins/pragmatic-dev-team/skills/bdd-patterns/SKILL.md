---
name: BDD Patterns
description: This skill should be used when writing Gherkin scenarios, Given-When-Then syntax, feature files, step definitions, scenario outlines, or Reqnroll/SpecFlow/Cucumber patterns. Also triggers for questions about BDD, behavior-driven development, or specification by example.
version: 0.1.0
load: on-demand
---

# BDD Patterns

Best practices for Behavior-Driven Development using Gherkin syntax, based on "BDD in Action" and Reqnroll/SpecFlow patterns.

## Gherkin Syntax

### Basic Structure
```gherkin
Feature: [Business-readable feature name]
  As a [role]
  I want [goal]
  So that [benefit]

  Background:
    Given [shared setup for all scenarios]

  Scenario: [Specific behavior example]
    Given [initial context]
    When [action taken]
    Then [expected outcome]
```

### Keywords
| Keyword | Purpose | Example |
|---------|---------|---------|
| **Given** | Setup/preconditions | Given a logged-in user |
| **When** | Action/event | When they submit the form |
| **Then** | Expected outcome | Then order is created |
| **And/But** | Additional steps | And email is sent |
| **Background** | Shared Given steps | Common setup |
| **Scenario Outline** | Data-driven | Multiple examples |

## Writing Good Scenarios

### Declarative vs Imperative

```gherkin
# BAD: Imperative (UI-focused, brittle)
Scenario: Login
  Given I am on the login page
  When I enter "user@test.com" in the email field
  And I enter "password123" in the password field
  And I click the login button
  Then I should see the dashboard

# GOOD: Declarative (behavior-focused)
Scenario: Successful login
  Given a registered user
  When they log in with valid credentials
  Then they should see their dashboard
```

### Single Behavior Per Scenario
```gherkin
# BAD: Multiple behaviors
Scenario: User management
  Given a new user
  When they register
  Then account is created
  When they log in
  Then they see dashboard
  When they update profile
  Then changes are saved

# GOOD: One behavior
Scenario: Successful registration
  Given a new user
  When they complete registration
  Then their account is created
```

### Concrete Examples
```gherkin
# BAD: Vague
Scenario: Valid order
  Given a valid product
  When user places order
  Then order succeeds

# GOOD: Concrete
Scenario: Order with sufficient stock
  Given product "Widget" with 10 in stock
  When user orders 3 "Widget"
  Then order is confirmed
  And stock is reduced to 7
```

## Scenario Patterns

### Happy Path
```gherkin
Scenario: Successfully create order
  Given a customer with valid payment method
  And product "Laptop" priced at $999
  When customer orders 1 "Laptop"
  Then order total is $999
  And order status is "Pending"
```

### Error Handling
```gherkin
Scenario: Cannot order out-of-stock product
  Given product "Laptop" with 0 in stock
  When customer attempts to order "Laptop"
  Then order is rejected
  And error message is "Product out of stock"
```

### Edge Cases
```gherkin
Scenario: Order at stock boundary
  Given product "Widget" with exactly 1 in stock
  When customer orders 1 "Widget"
  Then order is confirmed
  And stock is reduced to 0
```

### Scenario Outline (Data-Driven)
```gherkin
Scenario Outline: Password validation
  Given a registration form
  When user enters password "<password>"
  Then validation result is "<result>"

  Examples:
    | password     | result                    |
    | abc          | Too short (min 8 chars)   |
    | abcdefgh     | Missing number            |
    | Abcdefgh1    | Valid                     |
    | abcdefgh1!   | Valid                     |
```

## Step Definitions (Reqnroll/C#)

### Basic Binding
```csharp
[Binding]
public class OrderSteps
{
    private readonly ScenarioContext _context;

    public OrderSteps(ScenarioContext context) => _context = context;

    [Given(@"product ""(.*)"" priced at \$(.*)")]
    public void GivenProductPricedAt(string name, decimal price)
    {
        var product = new Product(name, price);
        _context.Set(product, "product");
    }

    [When(@"customer orders (\d+) ""(.*)""")]
    public void WhenCustomerOrders(int quantity, string productName)
    {
        var product = _context.Get<Product>("product");
        var order = new Order();
        order.AddLine(product, quantity);
        _context.Set(order, "order");
    }

    [Then(@"order total is \$(.*)")]
    public void ThenOrderTotalIs(decimal expectedTotal)
    {
        var order = _context.Get<Order>("order");
        order.Total.Should().Be(expectedTotal);
    }
}
```

### Dependency Injection
```csharp
[Binding]
public class OrderSteps
{
    private readonly IOrderService _orderService;
    private readonly OrderContext _orderContext;

    public OrderSteps(IOrderService orderService, OrderContext orderContext)
    {
        _orderService = orderService;
        _orderContext = orderContext;
    }
}
```

### Hooks
```csharp
[Binding]
public class Hooks
{
    [BeforeScenario]
    public void BeforeScenario() { /* Setup */ }

    [AfterScenario]
    public void AfterScenario() { /* Cleanup */ }

    [BeforeScenario("@database")]
    public void BeforeDatabaseScenario() { /* Tag-specific */ }
}
```

## Anti-Patterns

| Anti-Pattern | Problem | Solution |
|--------------|---------|----------|
| UI in scenarios | Brittle, slow | Declarative language |
| Too many steps | Hard to read | Keep under 6 steps |
| Technical language | Business can't read | Ubiquitous language |
| Writing alone | Missing perspectives | Three Amigos |
| After the code | Not true BDD | Scenarios first |
| Testing implementation | Brittle | Test behavior |

## Three Amigos

Before writing code, gather:
- **Developer** - What's feasible? Questions?
- **Tester** - What could go wrong? Edge cases?
- **Business** - What value? What rules?

Output: Clear scenarios everyone understands.

## Tags

```gherkin
@smoke @critical
Feature: Checkout

  @wip
  Scenario: Guest checkout
    ...

  @slow @integration
  Scenario: Full order flow
    ...
```

Use tags for:
- Test categorization (@smoke, @regression)
- Work status (@wip, @ignore)
- Test characteristics (@slow, @flaky)
- Environment (@integration, @e2e)
