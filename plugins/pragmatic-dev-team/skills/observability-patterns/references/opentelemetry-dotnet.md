# OpenTelemetry .NET

Distributed tracing and metrics with OpenTelemetry.

## Architecture

.NET uses native APIs that map to OpenTelemetry:

| .NET API | OpenTelemetry Concept |
|----------|----------------------|
| `ActivitySource` | Tracer |
| `Activity` | Span |
| `Meter` | Meter |
| `Counter<T>` | Counter |
| `Histogram<T>` | Histogram |

---

## Basic Setup

### Install Packages

```bash
dotnet add package OpenTelemetry.Extensions.Hosting
dotnet add package OpenTelemetry.Exporter.OpenTelemetryProtocol
dotnet add package OpenTelemetry.Instrumentation.AspNetCore
dotnet add package OpenTelemetry.Instrumentation.Http
```

### Configure Services

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddSource("MyCompany.MyService")  // Your ActivitySource
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri("http://localhost:4317");
        }))
    .WithMetrics(metrics => metrics
        .AddMeter("MyCompany.MyService")
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter());
```

---

## Custom Tracing

### Create ActivitySource

```csharp
using System.Diagnostics;

public class OrderService
{
    // Create once, reuse throughout app
    private static readonly ActivitySource s_source = new(
        "MyCompany.MyService.Orders",
        "1.0.0");

    public async Task ProcessOrderAsync(string orderId)
    {
        // StartActivity returns null if no listener
        using Activity? activity = s_source.StartActivity("ProcessOrder");

        // Add structured data
        activity?.SetTag("order.id", orderId);
        activity?.SetTag("order.type", "standard");

        await ValidateOrderAsync(orderId);

        // Add event with timestamp
        activity?.AddEvent(new ActivityEvent("OrderValidated"));

        await ChargePaymentAsync(orderId);

        // Set status
        activity?.SetStatus(ActivityStatusCode.Ok);
    }
}
```

### Nested Activities

```csharp
public async Task ProcessOrderAsync(string orderId)
{
    using var activity = s_source.StartActivity("ProcessOrder");
    activity?.SetTag("order.id", orderId);

    await ValidateAsync(orderId);
    await ChargeAsync(orderId);
    await ShipAsync(orderId);
}

private async Task ValidateAsync(string orderId)
{
    // Child span automatically linked to parent
    using var activity = s_source.StartActivity("ValidateOrder");
    activity?.SetTag("order.id", orderId);
    // Validation logic...
}

private async Task ChargeAsync(string orderId)
{
    using var activity = s_source.StartActivity("ChargePayment");
    activity?.SetTag("order.id", orderId);
    // Payment logic...
}
```

### Error Recording

```csharp
try
{
    await ProcessAsync();
    activity?.SetStatus(ActivityStatusCode.Ok);
}
catch (Exception ex)
{
    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
    activity?.RecordException(ex);
    throw;
}
```

---

## Custom Metrics

### Define Meters

```csharp
using System.Diagnostics.Metrics;

public class OrderMetrics
{
    private static readonly Meter s_meter = new(
        "MyCompany.MyService.Orders",
        "1.0.0");

    private static readonly Counter<long> s_ordersPlaced =
        s_meter.CreateCounter<long>("orders.placed", "orders", "Number of orders placed");

    private static readonly Histogram<double> s_orderValue =
        s_meter.CreateHistogram<double>("orders.value", "USD", "Order value distribution");

    private static readonly UpDownCounter<int> s_activeOrders =
        s_meter.CreateUpDownCounter<int>("orders.active", "orders", "Currently active orders");

    public void RecordOrderPlaced(string orderType, decimal value)
    {
        s_ordersPlaced.Add(1, new KeyValuePair<string, object?>("order.type", orderType));
        s_orderValue.Record((double)value);
        s_activeOrders.Add(1);
    }

    public void RecordOrderCompleted()
    {
        s_activeOrders.Add(-1);
    }
}
```

### Register Meters

```csharp
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics => metrics
        .AddMeter("MyCompany.MyService.Orders")
        .AddOtlpExporter());
```

---

## Context Propagation

### Automatic (HTTP)

OpenTelemetry automatically propagates W3C Trace Context via HTTP headers:
- `traceparent`: Trace ID, Span ID, flags
- `tracestate`: Vendor-specific data

### Manual (Messaging)

```csharp
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

public class MessageProducer
{
    private static readonly TextMapPropagator s_propagator =
        Propagators.DefaultTextMapPropagator;

    public async Task SendMessageAsync(MyMessage message)
    {
        using var activity = s_source.StartActivity("SendMessage", ActivityKind.Producer);

        // Inject trace context into message headers
        var headers = new Dictionary<string, string>();
        s_propagator.Inject(
            new PropagationContext(Activity.Current.Context, Baggage.Current),
            headers,
            (h, key, value) => h[key] = value);

        message.Headers = headers;
        await _queue.PublishAsync(message);
    }
}

public class MessageConsumer
{
    public async Task ProcessMessageAsync(MyMessage message)
    {
        // Extract trace context from message
        var parentContext = s_propagator.Extract(
            default,
            message.Headers,
            (h, key) => h.TryGetValue(key, out var value)
                ? new[] { value }
                : Array.Empty<string>());

        // Start child activity linked to producer
        using var activity = s_source.StartActivity(
            "ProcessMessage",
            ActivityKind.Consumer,
            parentContext.ActivityContext);

        // Process message...
    }
}
```

---

## Baggage

Propagate user-defined properties across services.

```csharp
// Set baggage (producer side)
Baggage.SetBaggage("tenant.id", tenantId);
Baggage.SetBaggage("feature.flag", "new-checkout");

// Read baggage (consumer side)
var tenantId = Baggage.GetBaggage("tenant.id");
if (Baggage.GetBaggage("feature.flag") == "new-checkout")
{
    // Use new checkout flow
}
```

**Warning**: Baggage is transmitted in plaintext. Never include secrets or PII.

---

## Sampling

### Head Sampling (At Trace Start)

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        // Sample 10% of traces
        .SetSampler(new TraceIdRatioBasedSampler(0.1))
        .AddSource("MyCompany.MyService")
        .AddOtlpExporter());
```

### Parent-Based Sampling

```csharp
// Respect parent's sampling decision
.SetSampler(new ParentBasedSampler(
    new TraceIdRatioBasedSampler(0.1)))
```

### Sampling Options

| Sampler | Behavior |
|---------|----------|
| `AlwaysOnSampler` | Sample all traces |
| `AlwaysOffSampler` | Sample nothing |
| `TraceIdRatioBasedSampler(0.1)` | Sample 10% |
| `ParentBasedSampler(inner)` | Follow parent decision |

---

## Performance Tips

### Avoid Expensive Operations When Not Sampled

```csharp
// Only compute when telemetry is being collected
if (activity is { IsAllDataRequested: true })
{
    activity.SetTag("expensive.data", ComputeExpensiveValue());
}
```

### Use Proper Tag Names

Follow [OpenTelemetry Semantic Conventions](https://opentelemetry.io/docs/concepts/semantic-conventions/):

```csharp
// HTTP
activity?.SetTag("http.method", "POST");
activity?.SetTag("http.route", "/api/orders");
activity?.SetTag("http.status_code", 200);

// Database
activity?.SetTag("db.system", "postgresql");
activity?.SetTag("db.operation", "SELECT");
activity?.SetTag("db.name", "orders");

// Custom (use namespace prefix)
activity?.SetTag("myapp.order.id", orderId);
activity?.SetTag("myapp.customer.tier", "premium");
```

---

## Anti-Patterns

| Anti-Pattern | Problem | Fix |
|--------------|---------|-----|
| Creating ActivitySource per request | Memory/CPU overhead | Create once, reuse |
| Ignoring null Activity | Crash when not sampled | Use null-conditional `activity?.` |
| Expensive tag computation | Performance | Check `IsAllDataRequested` |
| No semantic conventions | Inconsistent data | Follow OTel conventions |
| Baggage with secrets | Security exposure | Never include sensitive data |

---

## Quick Reference

```csharp
// Tracing
using var activity = s_source.StartActivity("OperationName");
activity?.SetTag("key", "value");
activity?.AddEvent(new ActivityEvent("EventName"));
activity?.SetStatus(ActivityStatusCode.Ok);

// Metrics
s_counter.Add(1, new KeyValuePair<string, object?>("label", "value"));
s_histogram.Record(123.45);
s_upDownCounter.Add(1);  // or Add(-1)
```

## Sources

- [.NET Observability with OpenTelemetry](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/observability-with-otel)
- [OpenTelemetry .NET](https://github.com/open-telemetry/opentelemetry-dotnet)
- [Semantic Conventions](https://opentelemetry.io/docs/concepts/semantic-conventions/)
- [Distributed Tracing Instrumentation](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/distributed-tracing-instrumentation-walkthroughs)
