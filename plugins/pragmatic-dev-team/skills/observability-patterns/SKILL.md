---
name: Observability Patterns
description: This skill should be used when implementing logging, tracing, metrics, structured events, or monitoring. Also triggers for questions about OpenTelemetry, Serilog, Application Insights, or production debugging.
version: 0.1.0
---

# Observability Patterns

Patterns for building observable systems that can be debugged in production.

## Three Pillars (Traditional)

| Pillar | Purpose | Example |
|--------|---------|---------|
| **Logs** | Discrete events | "User 123 logged in" |
| **Metrics** | Aggregated numbers | "99th percentile latency: 200ms" |
| **Traces** | Request flow | "Request touched services A→B→C" |

## Modern View: High-Cardinality Events

Rich structured events that contain all context needed to debug:

```csharp
_logger.LogInformation(
    "Order processed. OrderId={OrderId} CustomerId={CustomerId} " +
    "ItemCount={ItemCount} Total={Total} Duration={Duration}ms",
    order.Id, customer.Id, order.Items.Count, order.Total, stopwatch.ElapsedMilliseconds);
```

## Structured Logging (Serilog)

### Setup
```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "MyApp")
    .WriteTo.Console(new JsonFormatter())
    .WriteTo.Seq("http://localhost:5341")
    .CreateLogger();
```

### Usage Patterns

**Property binding (preferred):**
```csharp
_logger.LogInformation("User {UserId} created order {OrderId}", userId, orderId);
```

**Object destructuring:**
```csharp
_logger.LogInformation("Processing order {@Order}", order); // Full object
_logger.LogInformation("Processing order {$Order}", order); // ToString()
```

**Scoped context:**
```csharp
using (_logger.BeginScope(new Dictionary<string, object>
{
    ["CorrelationId"] = correlationId,
    ["UserId"] = userId
}))
{
    // All logs in this scope include CorrelationId and UserId
    _logger.LogInformation("Processing started");
}
```

## Log Levels

| Level | When to Use | Example |
|-------|-------------|---------|
| **Critical** | System failure | "Database connection lost" |
| **Error** | Failed operation | "Payment failed for order 123" |
| **Warning** | Degraded but working | "Cache miss, falling back to DB" |
| **Information** | Business events | "Order 123 shipped" |
| **Debug** | Developer troubleshooting | "Query returned 42 rows" |
| **Trace** | Detailed flow | "Entering ProcessOrder method" |

## What to Log

### Do Log
- Business events (created, updated, deleted)
- Errors with context
- Performance data (duration, counts)
- External service calls
- Security events (login, access denied)

### Don't Log
- Sensitive data (passwords, tokens, PII)
- Every method entry/exit
- Large objects
- High-frequency events without sampling

## Correlation

### Request Correlation
```csharp
public class CorrelationMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
            ?? Guid.NewGuid().ToString();

        using (_logger.BeginScope(new { CorrelationId = correlationId }))
        {
            context.Response.Headers["X-Correlation-ID"] = correlationId;
            await next(context);
        }
    }
}
```

### Distributed Tracing
```csharp
// Pass correlation ID to downstream services
httpClient.DefaultRequestHeaders.Add("X-Correlation-ID", correlationId);
```

## Metrics Patterns

### RED Method (Request-driven)
```csharp
// Rate
_requestCounter.Inc();

// Errors
_errorCounter.Inc();

// Duration
using (_requestDuration.NewTimer())
{
    await ProcessRequest();
}
```

### USE Method (Resource-driven)
```csharp
// Utilization
_cpuUtilization.Set(GetCpuPercent());

// Saturation
_queueDepth.Set(queue.Count);

// Errors
_connectionErrors.Inc();
```

## Common Patterns

### Error Logging
```csharp
try
{
    await ProcessAsync();
}
catch (BusinessException ex)
{
    _logger.LogWarning(ex, "Business rule violation. Rule={Rule}", ex.Rule);
    throw;
}
catch (Exception ex)
{
    _logger.LogError(ex, "Unexpected error processing request");
    throw;
}
```

### Performance Logging
```csharp
var sw = Stopwatch.StartNew();
try
{
    var result = await _repository.GetAsync(id);
    _logger.LogDebug("Repository query completed. Id={Id} Duration={Duration}ms",
        id, sw.ElapsedMilliseconds);
    return result;
}
finally
{
    _queryDuration.Record(sw.ElapsedMilliseconds);
}
```

### Health Checks
```csharp
services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>()
    .AddUrlGroup(new Uri("https://api.external.com/health"), "external-api");
```

## Anti-Patterns

| Anti-Pattern | Problem | Solution |
|--------------|---------|----------|
| Log and throw | Duplicate logs | Log at handling point |
| String interpolation | Can't query | Use structured logging |
| Sensitive data | Security risk | Mask or exclude |
| No correlation | Can't trace requests | Add correlation ID |
| Log everything | Noise, cost | Log meaningful events |

## Quick Reference

| Need | Pattern |
|------|---------|
| Track request flow | Correlation ID |
| Debug production | Structured logging |
| Measure latency | Histogram metrics |
| Count events | Counter metrics |
| Alert on issues | Metrics + thresholds |
| Trace across services | Distributed tracing |
