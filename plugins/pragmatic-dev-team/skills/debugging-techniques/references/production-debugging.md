# Production Debugging

Tools and techniques for debugging .NET applications in production.

## Dump Analysis

### Collecting Dumps

```bash
# Install dotnet-dump globally
dotnet tool install --global dotnet-dump

# Collect dump from running process
dotnet dump collect -p <PROCESS_ID>

# Collect dump with specific type
dotnet dump collect -p <PID> --type Full  # Full memory dump
dotnet dump collect -p <PID> --type Mini  # Smaller, faster
```

### Analyzing Dumps

```bash
# Start analysis session
dotnet dump analyze ./core_20250120_123456

# Common commands in analysis session
> clrstack              # Managed call stack (current thread)
> clrstack -a           # All threads with arguments
> threads               # List all managed threads
> dumpheap -stat        # Memory by type (summary)
> dumpheap -type Order  # Find objects of specific type
> dumpobj <address>     # Inspect object at address
> gcroot <address>      # Why isn't this collected?
> dumpasync             # Async state machines (crucial for async debugging)
```

### GC Dump (Lighter Weight)

```bash
# Install
dotnet tool install --global dotnet-gcdump

# Collect (much faster than full dump)
dotnet gcdump collect -p <PID>

# Open .gcdump file in Visual Studio for visual analysis
```

---

## Distributed Tracing with Activity

.NET's native tracing API, compatible with OpenTelemetry.

### Basic Setup

```csharp
using System.Diagnostics;

// Create once, reuse throughout app
static readonly ActivitySource s_source = new(
    "MyCompany.MyService",
    "1.0.0");

public async Task ProcessOrderAsync(string orderId)
{
    // StartActivity returns null if no listener
    using Activity? activity = s_source.StartActivity("ProcessOrder");

    // Add structured data
    activity?.SetTag("order.id", orderId);
    activity?.SetTag("order.type", "standard");

    await ValidateOrderAsync(orderId);

    // Mark events in timeline
    activity?.AddEvent(new ActivityEvent("OrderValidated"));

    await ChargePaymentAsync(orderId);

    // Record errors
    if (paymentFailed)
    {
        activity?.SetStatus(ActivityStatusCode.Error, "Payment declined");
    }
}
```

### Performance-Conscious Tagging

```csharp
// Only compute expensive tags when telemetry is being collected
if (activity is { IsAllDataRequested: true })
{
    activity.SetTag("expensive.data", ComputeExpensiveValue());
}
```

### Correlation with ILogger

```csharp
public class OrderService
{
    private readonly ILogger<OrderService> _logger;

    public async Task ProcessAsync(string orderId)
    {
        using var activity = s_source.StartActivity("ProcessOrder");
        activity?.SetTag("order.id", orderId);

        // Logs automatically include TraceId and SpanId
        _logger.LogInformation("Processing order {OrderId}", orderId);
    }
}
```

---

## Remote Debugging

### Visual Studio Remote Debugger

1. Install `msvsmon.exe` on remote machine
2. Start Remote Debugger
3. In VS: Debug → Attach to Process → Enter remote machine name
4. Select process

**Cautions:**
- Significant performance impact
- Security risk (exposes app state)
- Not recommended for production without controls
- High-latency connections may fail

### Docker Remote Debugging

```dockerfile
# Install debugger in container
RUN apt-get update && apt-get install -y unzip procps \
    && curl -sSL https://aka.ms/getvsdbgsh | bash /dev/stdin -v latest -l /vsdbg
```

```bash
# SSH tunnel for debugging
ssh -L 4024:localhost:4024 user@remote-host
```

---

## Structured Logging for Diagnostics

### Effective Logging Patterns

```csharp
public class OrderProcessor
{
    private readonly ILogger<OrderProcessor> _logger;

    public async Task<Result> ProcessAsync(Order order)
    {
        // ✅ Structured logging with placeholders
        _logger.LogInformation(
            "Processing order {OrderId} for customer {CustomerId}",
            order.Id, order.CustomerId);

        try
        {
            var result = await _repository.SaveAsync(order);

            _logger.LogDebug(
                "Order {OrderId} saved in {ElapsedMs}ms",
                order.Id, stopwatch.ElapsedMilliseconds);

            return result;
        }
        catch (DbException ex)
        {
            // ✅ Include exception, structured data, and context
            _logger.LogError(ex,
                "Failed to save order {OrderId}. Retry count: {RetryCount}",
                order.Id, retryCount);
            throw;
        }
    }
}
```

### Scoped Context

```csharp
public async Task HandleRequestAsync(HttpContext context)
{
    var correlationId = context.TraceIdentifier;

    using (_logger.BeginScope(new Dictionary<string, object>
    {
        ["CorrelationId"] = correlationId,
        ["UserId"] = context.User.Identity?.Name
    }))
    {
        // All logs in this scope include CorrelationId and UserId
        _logger.LogInformation("Request started");
        await ProcessRequestAsync();
        _logger.LogInformation("Request completed");
    }
}
```

---

## Performance Counters

### Real-Time Monitoring

```bash
# Install
dotnet tool install --global dotnet-counters

# Monitor process
dotnet counters monitor -p <PID>

# Monitor specific providers
dotnet counters monitor -p <PID> \
    --providers System.Runtime,Microsoft.AspNetCore.Hosting

# Collect to file
dotnet counters collect -p <PID> -o counters.json --format json
```

### Key Metrics to Watch

| Counter | Healthy Range | Problem Indicator |
|---------|--------------|-------------------|
| `gc-heap-size` | Stable | Continuously growing |
| `exception-count` | Low | High/increasing |
| `threadpool-queue-length` | <10 | >100 |
| `gen-2-gc-count` | Low | Frequent |
| `working-set` | Stable | Continuously growing |

---

## Trace Collection

```bash
# Install
dotnet tool install --global dotnet-trace

# Collect trace
dotnet trace collect -p <PID> --providers Microsoft-Windows-DotNETRuntime

# Convert for analysis
dotnet trace convert trace.nettrace --format Speedscope

# Open in https://www.speedscope.app/ for visualization
```

---

## Anti-Patterns

| Anti-Pattern | Problem | Fix |
|--------------|---------|-----|
| **String interpolation in logs** | Not structured, can't query | Use message templates |
| **Logging sensitive data** | PII/security exposure | Mask or exclude |
| **Catching generic Exception** | Hides root cause | Catch specific types |
| **No correlation ID** | Can't trace requests | Add CorrelationId middleware |
| **Debug builds in prod** | Performance, security | Release builds only |
| **Excessive logging** | Performance, noise | Strategic log points |

---

## Production Debugging Checklist

**Before debugging:**
- [ ] Check logs with correlation ID
- [ ] Review metrics/counters
- [ ] Check recent deployments
- [ ] Verify configuration

**When collecting dump:**
- [ ] Note timestamp and symptoms
- [ ] Collect during problem occurrence
- [ ] Keep dump file secure (may contain secrets)

**Analysis priority:**
1. Thread stacks (`clrstack -a`)
2. Exception objects (`dumpheap -type Exception`)
3. Memory pressure (`gcroot`, `dumpheap -stat`)
4. Async state (`dumpasync`)

## Sources

- [dotnet-dump](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/dotnet-dump)
- [dotnet-trace](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/dotnet-trace)
- [dotnet-counters](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/dotnet-counters)
- [Distributed tracing instrumentation](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/distributed-tracing-instrumentation-walkthroughs)
