# Step Reuse Strategies

Patterns for reusable, maintainable step definitions.

## Page Object Model

Abstract UI interactions into reusable page classes.

### Page Object

```csharp
public class LoginPage
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;

    public LoginPage(IWebDriver driver)
    {
        _driver = driver;
        _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
    }

    // Element locators
    private IWebElement EmailField => _driver.FindElement(By.Id("email"));
    private IWebElement PasswordField => _driver.FindElement(By.Id("password"));
    private IWebElement LoginButton => _driver.FindElement(By.Id("login-btn"));
    private IWebElement ErrorMessage => _driver.FindElement(By.CssSelector(".error"));

    // Actions
    public void Login(string email, string password)
    {
        EmailField.Clear();
        EmailField.SendKeys(email);
        PasswordField.Clear();
        PasswordField.SendKeys(password);
        LoginButton.Click();
    }

    public void WaitForLogin()
    {
        _wait.Until(d => d.Url.Contains("/dashboard"));
    }

    // Assertions
    public bool IsErrorDisplayed => ErrorMessage.Displayed;
    public string ErrorText => ErrorMessage.Text;
}
```

### Step Definition Using Page Object

```csharp
[Binding]
public class LoginSteps
{
    private readonly LoginPage _loginPage;
    private readonly DashboardPage _dashboardPage;

    public LoginSteps(IWebDriver driver)
    {
        _loginPage = new LoginPage(driver);
        _dashboardPage = new DashboardPage(driver);
    }

    [When(@"I log in with email ""(.*)"" and password ""(.*)""")]
    public void WhenILogIn(string email, string password)
    {
        _loginPage.Login(email, password);
    }

    [Then(@"I should see the dashboard")]
    public void ThenIShouldSeeDashboard()
    {
        _loginPage.WaitForLogin();
        Assert.True(_dashboardPage.IsDisplayed);
    }

    [Then(@"I should see error ""(.*)""")]
    public void ThenIShouldSeeError(string expectedError)
    {
        Assert.True(_loginPage.IsErrorDisplayed);
        Assert.Equal(expectedError, _loginPage.ErrorText);
    }
}
```

### Benefits

| Without Page Objects | With Page Objects |
|---------------------|-------------------|
| Locators scattered in steps | Locators centralized |
| Duplicate element lookups | Single source of truth |
| Brittle to UI changes | Change in one place |
| Hard to read steps | Semantic actions |

---

## Context Injection

Share data between step definition classes.

### Shared Context POCO

```csharp
// Shared state container
public class ShoppingContext
{
    public Customer Customer { get; set; }
    public Cart Cart { get; set; } = new();
    public Order PlacedOrder { get; set; }
    public string LastError { get; set; }
}
```

### Multiple Step Classes Share Context

```csharp
[Binding]
public class CustomerSteps
{
    private readonly ShoppingContext _context;

    public CustomerSteps(ShoppingContext context)
    {
        _context = context;
    }

    [Given(@"customer ""(.*)"" is logged in")]
    public void GivenCustomerLoggedIn(string name)
    {
        _context.Customer = new Customer(name);
    }
}

[Binding]
public class CartSteps
{
    private readonly ShoppingContext _context;

    public CartSteps(ShoppingContext context)
    {
        _context = context;
    }

    [When(@"I add ""(.*)"" to cart")]
    public void WhenIAddToCart(string product)
    {
        _context.Cart.Add(new Product(product));
    }
}

[Binding]
public class CheckoutSteps
{
    private readonly ShoppingContext _context;
    private readonly IOrderService _orderService;

    public CheckoutSteps(ShoppingContext context, IOrderService orderService)
    {
        _context = context;
        _orderService = orderService;
    }

    [When(@"I checkout")]
    public async Task WhenICheckout()
    {
        var result = await _orderService.PlaceOrderAsync(
            _context.Customer,
            _context.Cart);

        if (result.IsSuccess)
            _context.PlacedOrder = result.Value;
        else
            _context.LastError = result.Error;
    }
}
```

---

## Step Argument Transformations

Convert string arguments to domain types automatically.

### Common Transformations

```csharp
[Binding]
public class CommonTransforms
{
    // Money: "$100.50" → Money object
    [StepArgumentTransformation]
    public Money MoneyTransform(string value)
    {
        var cleaned = value.Replace("$", "").Replace(",", "");
        return new Money(decimal.Parse(cleaned), "USD");
    }

    // Date: "in 3 days" → DateTime
    [StepArgumentTransformation(@"in (\d+) days?")]
    public DateTime FutureDateTransform(int days)
    {
        return DateTime.Today.AddDays(days);
    }

    // Date: "yesterday" → DateTime
    [StepArgumentTransformation(@"yesterday")]
    public DateTime YesterdayTransform()
    {
        return DateTime.Today.AddDays(-1);
    }

    // Quantity: "5 items" → Quantity
    [StepArgumentTransformation(@"(\d+) items?")]
    public Quantity QuantityTransform(int count)
    {
        return new Quantity(count);
    }

    // Boolean: "enabled"/"disabled" → bool
    [StepArgumentTransformation(@"(enabled|disabled)")]
    public bool EnabledDisabledTransform(string value)
    {
        return value == "enabled";
    }
}
```

### DataTable to Collection

```csharp
[StepArgumentTransformation]
public IEnumerable<Product> ProductsTransform(DataTable table)
{
    return table.CreateSet<Product>();
}
```

**Scenario:**
```gherkin
Given the following products:
  | Name   | Price  | Stock |
  | Laptop | 999.00 | 10    |
  | Mouse  | 25.00  | 100   |
```

### Transformation Priority

```csharp
// Higher priority (lower Order number) checked first
[StepArgumentTransformation(@"special product ""(.*)""", Order = 1)]
public Product SpecialProductTransform(string name)
{
    return new Product(name) { IsSpecial = true };
}

// Fallback (higher Order number)
[StepArgumentTransformation(Order = 100)]
public Product DefaultProductTransform(string name)
{
    return new Product(name);
}
```

---

## Reusable Step Libraries

### Common Setup Steps

```csharp
[Binding]
public class CommonSetupSteps
{
    private readonly TestContext _context;

    [Given(@"the system date is ""(.*)""")]
    public void GivenSystemDate(DateTime date)
    {
        _context.SystemClock = new FakeClock(date);
    }

    [Given(@"database is empty")]
    public async Task GivenDatabaseEmpty()
    {
        await _context.Database.ResetAsync();
    }

    [Given(@"I am authenticated as ""(.*)""")]
    public void GivenAuthenticated(string username)
    {
        _context.CurrentUser = new TestUser(username);
    }
}
```

### Common Assertion Steps

```csharp
[Binding]
public class CommonAssertionSteps
{
    private readonly TestContext _context;

    [Then(@"the operation succeeds")]
    public void ThenOperationSucceeds()
    {
        Assert.Null(_context.LastError);
    }

    [Then(@"the operation fails with ""(.*)""")]
    public void ThenOperationFails(string expectedError)
    {
        Assert.Contains(expectedError, _context.LastError);
    }

    [Then(@"an email is sent to ""(.*)""")]
    public void ThenEmailSent(string recipient)
    {
        Assert.Contains(
            _context.SentEmails,
            e => e.To == recipient);
    }
}
```

---

## Generic Step Patterns

### CRUD Steps

```csharp
[Binding]
public class CrudSteps<TEntity> where TEntity : class
{
    private readonly IRepository<TEntity> _repository;
    private readonly ScenarioContext _context;

    [Given(@"a (.*) exists with id ""(.*)""")]
    public async Task GivenEntityExists(string entityType, string id)
    {
        var entity = await _repository.GetByIdAsync(id);
        _context.Set(entity, $"current{entityType}");
    }

    [When(@"I delete the (.*)")]
    public async Task WhenIDeleteEntity(string entityType)
    {
        var entity = _context.Get<TEntity>($"current{entityType}");
        await _repository.DeleteAsync(entity);
    }
}
```

### API Steps

```csharp
[Binding]
public class ApiSteps
{
    private readonly HttpClient _client;
    private HttpResponseMessage _response;
    private object _responseBody;

    [When(@"I GET ""(.*)""")]
    public async Task WhenIGet(string url)
    {
        _response = await _client.GetAsync(url);
    }

    [When(@"I POST to ""(.*)"" with:")]
    public async Task WhenIPost(string url, string body)
    {
        var content = new StringContent(body, Encoding.UTF8, "application/json");
        _response = await _client.PostAsync(url, content);
    }

    [Then(@"response status is (.*)")]
    public void ThenResponseStatus(int expectedStatus)
    {
        Assert.Equal(expectedStatus, (int)_response.StatusCode);
    }

    [Then(@"response contains ""(.*)""")]
    public async Task ThenResponseContains(string expected)
    {
        var body = await _response.Content.ReadAsStringAsync();
        Assert.Contains(expected, body);
    }
}
```

---

## Organization Patterns

### By Feature (Recommended for Small Projects)

```
Features/
├── Orders/
│   ├── PlaceOrder.feature
│   └── Steps/
│       ├── OrderSteps.cs
│       └── OrderDriver.cs
├── Customers/
│   ├── Registration.feature
│   └── Steps/
│       └── CustomerSteps.cs
└── Shared/
    ├── CommonSteps.cs
    └── Transforms.cs
```

### By Layer (Recommended for Large Projects)

```
Features/
├── Orders.feature
├── Customers.feature
└── Checkout.feature

Steps/
├── OrderSteps.cs
├── CustomerSteps.cs
└── CheckoutSteps.cs

Drivers/
├── OrderDriver.cs
├── CustomerDriver.cs
└── CheckoutDriver.cs

Pages/
├── LoginPage.cs
├── DashboardPage.cs
└── CheckoutPage.cs

Support/
├── TestContext.cs
├── Transforms.cs
└── Hooks.cs
```

---

## Quick Reference

| Pattern | Use When | Benefit |
|---------|----------|---------|
| Page Objects | UI automation | Centralized locators |
| Context Injection | Sharing state | Decoupled step classes |
| Transformations | Domain types in steps | Cleaner Gherkin |
| Driver Pattern | Complex automation | Thin step definitions |
| Generic Steps | CRUD operations | Reduce duplication |

## Sources

- [Reqnroll Page Object Model](https://docs.reqnroll.net/latest/guides/page-object-model.html)
- [Reqnroll Context Injection](https://docs.reqnroll.net/latest/automation/context-injection.html)
- [Reqnroll Driver Pattern](https://docs.reqnroll.net/latest/guides/driver-pattern.html)
- [Step Argument Conversions](https://docs.reqnroll.net/latest/automation/step-argument-conversions.html)
