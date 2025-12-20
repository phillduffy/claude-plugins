# Anti-Patterns to Avoid

## 1. Exception-Driven Flow Control

### The Anti-Pattern
```csharp
// BAD: Using exceptions for expected outcomes
public User GetUser(int id)
{
    var user = _db.Users.Find(id);
    if (user == null)
        throw new UserNotFoundException(id);  // Expected case!
    return user;
}

// Caller must know to catch
try
{
    var user = GetUser(id);
    Process(user);
}
catch (UserNotFoundException)
{
    ShowNotFoundMessage();
}
```

### The Fix
```csharp
// GOOD: Result for expected failures
public Result<User, Error> GetUser(int id)
{
    var user = _db.Users.Find(id);
    if (user == null)
        return DomainErrors.User.NotFound(id);
    return user;
}

// Caller forced to handle
var result = GetUser(id);
if (result.IsFailure)
{
    ShowNotFoundMessage();
    return;
}
Process(result.Value);
```

---

## 2. Inheritance for Code Sharing

### The Anti-Pattern
```csharp
// BAD: Base class to share code
public abstract class BaseService
{
    protected readonly ILogger _logger;
    protected readonly IConfiguration _config;

    protected void LogOperation(string op) => _logger.LogDebug(op);
    protected string GetSetting(string key) => _config[key];
}

public class UserService : BaseService
{
    public void CreateUser(User user)
    {
        LogOperation("Creating user");  // Inherited
        var setting = GetSetting("MaxUsers");  // Inherited
        // ...
    }
}
```

### The Fix
```csharp
// GOOD: Composition via injection
public class UserService(
    ILogger<UserService> logger,
    IOptions<UserSettings> settings)
{
    public void CreateUser(User user)
    {
        logger.LogDebug("Creating user");
        var maxUsers = settings.Value.MaxUsers;
        // ...
    }
}
```

---

## 3. Service Locator / Static Access

### The Anti-Pattern
```csharp
// BAD: Hidden dependencies
public class OrderProcessor
{
    public void Process(Order order)
    {
        var emailService = ServiceLocator.Get<IEmailService>();  // Hidden!
        var config = AppConfig.Instance.Get("smtp");  // Static!
        var now = DateTime.Now;  // Impure!

        emailService.Send(...);
    }
}
```

### The Fix
```csharp
// GOOD: Explicit dependencies
public class OrderProcessor(
    IEmailService emailService,
    IOptions<SmtpSettings> smtpSettings,
    IDateTimeProvider dateTimeProvider)
{
    public void Process(Order order)
    {
        var now = dateTimeProvider.UtcNow;
        emailService.Send(...);
    }
}
```

---

## 4. Primitive Obsession

### The Anti-Pattern
```csharp
// BAD: Primitives for domain concepts
public class User
{
    public string Email { get; set; }      // Any string valid?
    public string PhoneNumber { get; set; } // Format?
    public int Age { get; set; }            // Negative age?
    public decimal Balance { get; set; }    // Currency?
}

public void SendEmail(string email, string subject)
{
    // Must validate email every time it's used!
    if (!IsValidEmail(email))
        throw new ArgumentException("Invalid email");
    // ...
}
```

### The Fix
```csharp
// GOOD: Domain primitives
public class User
{
    public EmailAddress Email { get; }      // Validated at creation
    public PhoneNumber Phone { get; }       // Formatted correctly
    public Age Age { get; }                 // Must be positive
    public Money Balance { get; }           // Includes currency
}

public void SendEmail(EmailAddress email, string subject)
{
    // No validation needed - type guarantees validity
    _smtp.Send(email.Value, subject);
}
```

---

## 5. Validation Scattered Everywhere

### The Anti-Pattern
```csharp
// BAD: Repeated validation
public class UserController
{
    public IActionResult Create(string email)
    {
        if (!IsValidEmail(email)) return BadRequest();
        _service.Create(email);
        return Ok();
    }
}

public class UserService
{
    public void Create(string email)
    {
        if (!IsValidEmail(email)) throw new Exception();  // Again!
        _repo.Save(email);
    }
}

public class UserRepository
{
    public void Save(string email)
    {
        if (!IsValidEmail(email)) throw new Exception();  // Again!!
        _db.Insert(email);
    }
}
```

### The Fix
```csharp
// GOOD: Parse once at boundary
public class UserController
{
    public IActionResult Create(string rawEmail)
    {
        var emailResult = EmailAddress.Create(rawEmail);  // Parse here
        if (emailResult.IsFailure)
            return BadRequest(emailResult.Error);

        _service.Create(emailResult.Value);  // Trusted type flows through
        return Ok();
    }
}

public class UserService
{
    public void Create(EmailAddress email)  // Already valid
    {
        _repo.Save(email);
    }
}
```

---

## 6. Generic Repository Abstraction

### The Anti-Pattern
```csharp
// BAD: One-size-fits-all abstraction
public interface IRepository<T>
{
    T GetById(int id);
    IEnumerable<T> GetAll();
    IEnumerable<T> Find(Expression<Func<T, bool>> predicate);
    void Add(T entity);
    void Update(T entity);
    void Delete(T entity);
}

// Forces every entity to support every operation
// Leaks query details to consumers
// Hard to optimize specific queries
```

### The Fix
```csharp
// GOOD: Specific interfaces for specific needs
public interface IUserRepository
{
    Maybe<User> GetByEmail(EmailAddress email);
    void Add(User user);
}

public interface IOrderRepository
{
    Result<Order, Error> GetById(OrderId id);
    IReadOnlyList<Order> GetPendingForUser(UserId userId);
    void Save(Order order);
}
```

---

## 7. Premature Abstraction (YAGNI Violation)

### The Anti-Pattern
```csharp
// BAD: Abstraction for hypothetical future
public interface IDatabaseProvider { }
public interface ISqlDatabaseProvider : IDatabaseProvider { }
public interface INoSqlDatabaseProvider : IDatabaseProvider { }
public class SqlServerProvider : ISqlDatabaseProvider { }
public class PostgresProvider : ISqlDatabaseProvider { }  // Never used
public class MongoProvider : INoSqlDatabaseProvider { }   // Never used

public class DatabaseProviderFactory
{
    public IDatabaseProvider Create(string type) => type switch
    {
        "sqlserver" => new SqlServerProvider(),
        "postgres" => new PostgresProvider(),
        "mongo" => new MongoProvider(),
        _ => throw new NotSupportedException()
    };
}
```

### The Fix
```csharp
// GOOD: Just use what you need
public class SqlServerDatabase(string connectionString)
{
    public async Task<T> QueryAsync<T>(string sql) { ... }
}

// Add abstraction WHEN you actually need to support multiple databases
```

---

## 8. Configuration Magic Strings

### The Anti-Pattern
```csharp
// BAD: Strings scattered throughout
public class EmailService
{
    public void Send(string to)
    {
        var host = _config["Smtp:Host"];        // Magic string
        var port = int.Parse(_config["Smtp:Port"]);  // No type safety
        var user = _config["Smtp:Username"];    // Typo = runtime error
    }
}
```

### The Fix
```csharp
// GOOD: Strongly typed options
public class SmtpSettings
{
    public required string Host { get; init; }
    public required int Port { get; init; }
    public required string Username { get; init; }
}

public class EmailService(IOptions<SmtpSettings> options)
{
    public void Send(string to)
    {
        var settings = options.Value;
        // settings.Host, settings.Port - type safe, discoverable
    }
}
```

---

## 9. Mixed Purity (Side Effects in Core)

### The Anti-Pattern
```csharp
// BAD: Business logic with side effects
public decimal CalculateDiscount(Order order)
{
    _logger.LogDebug("Calculating discount");  // Side effect!

    var discount = order.Total > 100 ? 0.1m : 0;

    _db.SaveDiscountHistory(order.Id, discount);  // Side effect!

    return discount;
}
```

### The Fix
```csharp
// GOOD: Pure calculation, side effects in shell
public decimal CalculateDiscount(Order order)
{
    return order.Total > 100 ? 0.1m : 0;  // Pure!
}

// Shell handles side effects
public void ProcessOrder(Order order)
{
    _logger.LogDebug("Calculating discount");

    var discount = CalculateDiscount(order);  // Pure call

    _db.SaveDiscountHistory(order.Id, discount);  // Side effect in shell
}
```

---

## 10. Anemic Domain Model (Wrong Way)

### The Anti-Pattern
```csharp
// BAD: All logic in services, entities are just bags of data
public class Order
{
    public List<OrderItem> Items { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; }
}

public class OrderService
{
    public void AddItem(Order order, Product product, int qty)
    {
        order.Items.Add(new OrderItem(product, qty));
        order.Total = order.Items.Sum(i => i.Price * i.Qty);
    }

    public void Submit(Order order)
    {
        if (order.Total == 0) throw new Exception();
        order.Status = "Submitted";
    }
}
```

### The Better Way
```csharp
// GOOD: Entities protect their invariants
public class Order
{
    private readonly List<OrderItem> _items = new();
    public IReadOnlyList<OrderItem> Items => _items;
    public decimal Total => _items.Sum(i => i.Subtotal);

    public Result AddItem(Product product, int qty)
    {
        if (qty <= 0)
            return Result.Failure("Quantity must be positive");

        _items.Add(new OrderItem(product, qty));
        return Result.Success();
    }

    public Result Submit()
    {
        if (!_items.Any())
            return Result.Failure("Cannot submit empty order");

        AddDomainEvent(new OrderSubmittedEvent(this));
        return Result.Success();
    }
}
```

---

## Quick Reference: Smell → Fix

| Code Smell | Fix |
|------------|-----|
| `throw` for validation | Return `Result<T, Error>` |
| `: BaseClass` for sharing | Inject interface |
| `ServiceLocator.Get<T>()` | Constructor parameter |
| `string email` parameter | `EmailAddress` value object |
| Validation in multiple layers | Parse at boundary |
| `IRepository<T>` | Specific repository interface |
| Abstraction with one impl | Delete abstraction |
| `IConfiguration["key"]` | `IOptions<TSettings>` |
| Logger in pure method | Move to decorator/shell |
| Public setters on entity | Private set, methods for mutations |
