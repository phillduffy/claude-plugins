# Structured Logging

Serilog best practices for .NET applications.

## Setup

### Basic Configuration

```csharp
// Program.cs - Configure BEFORE builder to capture startup issues
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .WriteTo.Console()
    .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSerilog();
```

### Configuration via appsettings.json

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "File",
        "Args": {
          "path": "logs/app-.txt",
          "rollingInterval": "Day"
        }
      }
    ],
    "Enrich": ["FromLogContext", "WithMachineName", "WithThreadId"]
  }
}
```

```csharp
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();
```

---

## Message Templates

### Structured Logging (Do This)

```csharp
// ✅ Structured logging with placeholders
_logger.LogInformation(
    "Processing order {OrderId} for customer {CustomerId}",
    orderId, customerId);

// Creates properties: OrderId, CustomerId
// Searchable in log aggregators
```

### String Interpolation (Don't Do This)

```csharp
// ❌ String interpolation - loses structure
_logger.LogInformation($"Processing order {orderId} for customer {customerId}");

// Just a flat string, not searchable by OrderId
```

### Object Destructuring

```csharp
// @ operator = serialize entire object
_logger.LogInformation("Processing order {@Order}", order);
// Output: Processing order {"Id": "123", "Total": 99.99, "Status": "Pending"}

// $ operator = ToString()
_logger.LogInformation("Processing order {$Order}", order);
// Output: Processing order MyApp.Order
```

---

## Log Levels

| Level | When | Example |
|-------|------|---------|
| **Critical** | System failure | DB connection lost |
| **Error** | Operation failed | Payment declined |
| **Warning** | Degraded but working | Cache miss, retry succeeded |
| **Information** | Business events | Order placed, user logged in |
| **Debug** | Developer troubleshooting | Query returned 42 rows |
| **Trace** | Detailed flow | Entering/exiting methods |

### Usage

```csharp
_logger.LogCritical(ex, "Database connection lost");
_logger.LogError(ex, "Payment failed for order {OrderId}", orderId);
_logger.LogWarning("Cache miss for key {CacheKey}, falling back to DB", key);
_logger.LogInformation("Order {OrderId} shipped to {Address}", orderId, address);
_logger.LogDebug("Query returned {RowCount} rows in {Duration}ms", count, ms);
_logger.LogTrace("Entering ProcessOrder with {OrderId}", orderId);
```

---

## Enrichers

### Built-in Enrichers

```csharp
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()        // Scoped properties
    .Enrich.WithMachineName()       // MachineName property
    .Enrich.WithThreadId()          // ThreadId property
    .Enrich.WithEnvironmentName()   // ASPNETCORE_ENVIRONMENT
    .Enrich.WithProcessId()         // ProcessId property
    .CreateLogger();
```

### Custom Enricher

```csharp
public class UserIdEnricher : ILogEventEnricher
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserIdEnricher(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory factory)
    {
        var userId = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
        if (!string.IsNullOrEmpty(userId))
        {
            logEvent.AddPropertyIfAbsent(
                factory.CreateProperty("UserId", userId));
        }
    }
}
```

---

## Scoped Context

### LogContext.PushProperty

```csharp
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;

    public async Task Invoke(HttpContext context)
    {
        var correlationId = context.Request.Headers["X-Correlation-Id"]
            .FirstOrDefault() ?? Guid.NewGuid().ToString();

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            context.Response.Headers["X-Correlation-Id"] = correlationId;
            await _next(context);
        }
        // CorrelationId automatically added to all logs in this scope
    }
}
```

### BeginScope (ILogger)

```csharp
public async Task ProcessOrderAsync(Order order)
{
    using (_logger.BeginScope(new Dictionary<string, object>
    {
        ["OrderId"] = order.Id,
        ["CustomerId"] = order.CustomerId
    }))
    {
        _logger.LogInformation("Processing started");
        await ValidateAsync(order);
        _logger.LogInformation("Validation complete");
        await ChargeAsync(order);
        _logger.LogInformation("Payment complete");
    }
    // All logs above include OrderId and CustomerId
}
```

---

## Output Formatting

### Console Template

```csharp
.WriteTo.Console(outputTemplate:
    "[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId} {Message:lj}{NewLine}{Exception}")
```

### JSON Output

```csharp
.WriteTo.Console(new JsonFormatter())
// Or
.WriteTo.Console(new RenderedCompactJsonFormatter())
```

### File with Correlation ID

```csharp
.WriteTo.File(
    path: "logs/app-.txt",
    rollingInterval: RollingInterval.Day,
    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}")
```

---

## What to Log

### Do Log

```csharp
// Business events
_logger.LogInformation("Order {OrderId} placed by {CustomerId}", orderId, customerId);

// Errors with context
_logger.LogError(ex, "Failed to process payment for order {OrderId}", orderId);

// Performance data
_logger.LogDebug("Database query completed in {Duration}ms", stopwatch.ElapsedMilliseconds);

// External service calls
_logger.LogInformation("Called payment gateway {Gateway}, status: {Status}", gateway, status);

// Security events
_logger.LogWarning("Failed login attempt for user {Username} from {IpAddress}", username, ip);
```

### Don't Log

```csharp
// ❌ Sensitive data
_logger.LogInformation("User logged in with password {Password}", password);  // NEVER!
_logger.LogDebug("Credit card: {CardNumber}", cardNumber);  // NEVER!

// ❌ High-frequency without sampling
foreach (var item in millionItems)
    _logger.LogTrace("Processing {Item}", item);  // Too noisy

// ❌ Large objects
_logger.LogDebug("Full response: {@Response}", largeObject);  // Performance issue
```

---

## Anti-Patterns

| Anti-Pattern | Problem | Fix |
|--------------|---------|-----|
| String interpolation | Not structured | Use message templates |
| Log and throw | Duplicate logs | Log at handling point |
| Sensitive data | Security risk | Mask or exclude |
| No correlation | Can't trace | Add correlation ID |
| Blocking console | Performance | Async, reduce in prod |
| Too verbose | Noise, cost | Appropriate levels |

---

## Quick Reference

### Template Placeholders

| Placeholder | Description |
|-------------|-------------|
| `{PropertyName}` | Scalar value |
| `{@PropertyName}` | Destructure object |
| `{$PropertyName}` | ToString() |
| `{PropertyName:format}` | Format specifier |

### Common Packages

```
Serilog.AspNetCore
Serilog.Sinks.Console
Serilog.Sinks.File
Serilog.Sinks.Seq
Serilog.Enrichers.Environment
Serilog.Enrichers.Thread
```

## Sources

- [Serilog Best Practices](https://www.milanjovanovic.tech/blog/5-serilog-best-practices-for-better-structured-logging)
- [Writing Log Events](https://github.com/serilog/serilog/wiki/Writing-Log-Events)
- [Enrichment](https://github.com/serilog/serilog/wiki/Enrichment)
- [Serilog and .NET 8](https://nblumhardt.com/2024/04/serilog-net8-0-minimal/)
