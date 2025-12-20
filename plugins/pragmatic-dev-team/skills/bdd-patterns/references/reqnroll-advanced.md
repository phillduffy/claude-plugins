# Reqnroll Advanced Patterns

Modern Reqnroll features: async, parallel execution, DI, and step reuse.

## Async Testing

Reqnroll fully supports async/await in steps and hooks.

### Async Step Definitions

```csharp
[Binding]
public class OrderSteps
{
    private readonly HttpClient _client;
    private HttpResponseMessage _response;

    public OrderSteps(HttpClient client) => _client = client;

    [When(@"I request order details for ""(.*)""")]
    public async Task WhenIRequestOrderDetails(string orderId)
    {
        _response = await _client.GetAsync($"/api/orders/{orderId}");
    }

    [Then(@"the response status is (.*)")]
    public async Task ThenResponseStatusIs(int expectedStatus)
    {
        var content = await _response.Content.ReadAsStringAsync();
        Assert.Equal(expectedStatus, (int)_response.StatusCode);
    }
}
```

### Async Hooks

```csharp
[Binding]
public class Hooks
{
    [BeforeScenario]
    public async Task BeforeScenarioAsync(ScenarioContext context)
    {
        await InitializeDatabaseAsync();
        await SeedTestDataAsync();
    }

    [AfterScenario]
    public async Task AfterScenarioAsync()
    {
        await CleanupTestDataAsync();
    }

    // Static async hooks also supported
    [BeforeTestRun]
    public static async Task BeforeTestRun()
    {
        await GlobalSetupAsync();
    }
}
```

---

## Parallel Execution

Reqnroll supports scenario-level parallelization.

### Critical Rules

**NEVER use static context properties:**

```csharp
// BAD - Race conditions in parallel
var context = ScenarioContext.Current;  // ❌
var feature = FeatureContext.Current;   // ❌

// GOOD - Constructor injection
public class Steps
{
    private readonly ScenarioContext _context;

    public Steps(ScenarioContext context)
    {
        _context = context;  // ✅ Thread-isolated
    }
}
```

**Avoid Feature-Level Hooks with Parallel:**

```csharp
// CAUTION - May execute multiple times with parallel scenarios
[BeforeFeature]
public static void BeforeFeature()
{
    // No guarantee of single execution per feature
    // Multiple threads may run this simultaneously
}

// PREFER - Scenario-level hooks (parallel-safe)
[BeforeScenario]
public void BeforeScenario()
{
    // Each scenario gets its own isolated context
}
```

### Configuration

```json
{
  "reqnroll": {
    "runtime": {
      "parallelization": {
        "enabled": true
      }
    }
  }
}
```

**Test Framework Specifics:**
- **NUnit/MSTest:** Scenario-level parallelization
- **xUnit:** Feature-level parallelization (parallel fixtures)

---

## Dependency Injection

### Microsoft.Extensions.DependencyInjection Setup

```bash
Install-Package Reqnroll.Microsoft.Extensions.DependencyInjection
```

```csharp
public class TestDependencies
{
    [ScenarioDependencies]
    public static IServiceCollection CreateServices()
    {
        var services = new ServiceCollection();

        // Application services
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IOrderRepository, InMemoryOrderRepository>();

        // Test infrastructure
        services.AddHttpClient("TestApi", client =>
        {
            client.BaseAddress = new Uri("http://localhost:5000");
        });

        // Binding classes are auto-registered
        return services;
    }
}
```

### Accessing ScenarioContext with DI

```csharp
[Binding]
public class OrderSteps
{
    private readonly ScenarioContext _scenarioContext;
    private readonly IOrderService _orderService;

    public OrderSteps(ScenarioContext scenarioContext, IOrderService orderService)
    {
        _scenarioContext = scenarioContext;
        _orderService = orderService;
    }

    [Given(@"an order exists with id ""(.*)""")]
    public async Task GivenOrderExists(string orderId)
    {
        var order = await _orderService.GetByIdAsync(orderId);
        _scenarioContext.Set(order, "currentOrder");
    }

    [Then(@"the order status is ""(.*)""")]
    public void ThenOrderStatusIs(string expectedStatus)
    {
        var order = _scenarioContext.Get<Order>("currentOrder");
        Assert.Equal(expectedStatus, order.Status);
    }
}
```

### Container Hierarchy

```
Global Container (test run lifetime)
    └── Feature Container (feature lifetime)
            └── Scenario Container (scenario lifetime)
```

Each level inherits from parent. Scoped services are isolated per scenario.

---

## Step Argument Transformations

Convert step arguments to rich types automatically.

### Basic Transformations

```csharp
[Binding]
public class Transforms
{
    [StepArgumentTransformation]
    public Money MoneyTransform(string amount)
    {
        // "£100.50" → Money object
        var value = decimal.Parse(amount.TrimStart('£', '$', '€'));
        return new Money(value, "GBP");
    }

    [StepArgumentTransformation(@"in (\d+) days?")]
    public DateTime FutureDateTransform(int days)
    {
        return DateTime.Today.AddDays(days);
    }

    [StepArgumentTransformation]
    public CustomerId CustomerIdTransform(string id)
    {
        return new CustomerId(Guid.Parse(id));
    }
}
```

**Usage in scenarios:**

```gherkin
Given the order total is £100.50
And the delivery is scheduled for in 3 days
And customer "550e8400-e29b-41d4-a716-446655440000" exists
```

### DataTable Transformations

```csharp
[StepArgumentTransformation]
public IEnumerable<OrderLine> OrderLinesTransform(DataTable table)
{
    return table.CreateSet<OrderLine>();
}
```

```gherkin
Given the following order lines:
  | Product | Quantity | Price |
  | Laptop  | 1        | 999   |
  | Mouse   | 2        | 25    |
```

### Ordering Transformations

```csharp
[StepArgumentTransformation(@"(\d+) items?", Order = 1)]
public Quantity SpecificQuantityTransform(int count)
{
    return new Quantity(count);
}

[StepArgumentTransformation(Order = 100)]  // Lower priority fallback
public Quantity FallbackQuantityTransform(string input)
{
    return Quantity.Parse(input);
}
```

---

## Driver Pattern

Keep step definitions thin. Move automation logic to drivers.

### Step Definition (Thin Layer)

```csharp
[Binding]
public class CheckoutSteps
{
    private readonly CheckoutDriver _driver;

    public CheckoutSteps(CheckoutDriver driver)
    {
        _driver = driver;
    }

    [Given(@"product ""(.*)"" in cart")]
    public async Task GivenProductInCart(string productName)
    {
        await _driver.AddToCartAsync(productName);
    }

    [When(@"I complete checkout")]
    public async Task WhenICompleteCheckout()
    {
        await _driver.CheckoutAsync();
    }

    [Then(@"order confirmation is displayed")]
    public void ThenOrderConfirmationDisplayed()
    {
        _driver.AssertOrderConfirmed();
    }
}
```

### Driver (Automation Logic)

```csharp
public class CheckoutDriver
{
    private readonly IOrderService _orderService;
    private readonly ScenarioContext _context;
    private Order _currentOrder;

    public CheckoutDriver(IOrderService orderService, ScenarioContext context)
    {
        _orderService = orderService;
        _context = context;
    }

    public async Task AddToCartAsync(string productName)
    {
        var product = await _orderService.FindProductAsync(productName);
        var cart = _context.Get<Cart>("cart");
        cart.Add(product);
    }

    public async Task CheckoutAsync()
    {
        var cart = _context.Get<Cart>("cart");
        _currentOrder = await _orderService.PlaceOrderAsync(cart);
    }

    public void AssertOrderConfirmed()
    {
        Assert.NotNull(_currentOrder);
        Assert.Equal(OrderStatus.Confirmed, _currentOrder.Status);
    }
}
```

### External Driver Registration

For drivers in referenced projects, configure assembly binding:

```json
{
  "reqnroll": {
    "bindingAssemblies": [
      { "assembly": "MyProject.TestDrivers" }
    ]
  }
}
```

---

## Context Injection Patterns

### Type-Safe Context Extensions

```csharp
public static class ScenarioContextExtensions
{
    public static Order GetCurrentOrder(this ScenarioContext context)
        => context.Get<Order>("currentOrder");

    public static void SetCurrentOrder(this ScenarioContext context, Order order)
        => context.Set(order, "currentOrder");

    public static bool TryGetCurrentOrder(this ScenarioContext context, out Order order)
        => context.TryGetValue("currentOrder", out order);
}

// Usage
[Then(@"order is confirmed")]
public void ThenOrderConfirmed()
{
    var order = _context.GetCurrentOrder();
    Assert.Equal(OrderStatus.Confirmed, order.Status);
}
```

### Shared Context POCO

```csharp
public class OrderTestContext
{
    public Customer Customer { get; set; }
    public Order CurrentOrder { get; set; }
    public List<Product> AvailableProducts { get; } = new();
    public string LastErrorMessage { get; set; }
}

[Binding]
public class OrderSteps
{
    private readonly OrderTestContext _context;

    public OrderSteps(OrderTestContext context)
    {
        _context = context;
    }

    [Given(@"customer ""(.*)"" exists")]
    public void GivenCustomerExists(string name)
    {
        _context.Customer = new Customer(name);
    }
}
```

---

## Best Practices Summary

| Area | Do | Don't |
|------|-----|-------|
| **Async** | Use async/await throughout | Block on .Result or .Wait() |
| **Parallel** | Constructor inject ScenarioContext | Use ScenarioContext.Current |
| **DI** | Use Microsoft.Extensions.DependencyInjection | Service locator pattern |
| **Steps** | Keep under 10 lines | Put automation logic in steps |
| **Transforms** | Create rich types from strings | Parse manually in steps |
| **Drivers** | Separate automation from step logic | Monolithic step classes |

## Sources

- [Reqnroll Parallel Execution](https://docs.reqnroll.net/latest/execution/parallel-execution.html)
- [Reqnroll Hooks](https://docs.reqnroll.net/latest/automation/hooks.html)
- [Reqnroll DI Integration](https://docs.reqnroll.net/latest/integrations/dependency-injection.html)
- [Reqnroll Driver Pattern](https://docs.reqnroll.net/latest/guides/driver-pattern.html)
- [Step Argument Conversions](https://docs.reqnroll.net/latest/automation/step-argument-conversions.html)
