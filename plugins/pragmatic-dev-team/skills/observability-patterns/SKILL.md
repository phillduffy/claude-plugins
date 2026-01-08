---
name: Observability Patterns
description: Use when implementing logging, tracing, metrics, or monitoring in VSTO add-ins. Covers Serilog structured logging, OpenTelemetry .NET, Application Insights, and Office-specific diagnostic patterns (Event Viewer, VSTO runtime logs).
version: 0.2.0
load: on-demand
---

# Observability Patterns

Production debugging for VSTO add-ins. Structured logging, tracing, and metrics.

## VSTO-Specific Logging

### Write to Windows Event Log

```csharp
// Create source during installation (requires admin)
if (!EventLog.SourceExists("MyWordAddin"))
    EventLog.CreateEventSource("MyWordAddin", "Application");

// Log during runtime
EventLog.WriteEntry("MyWordAddin", 
    $"Template applied. TemplateId={templateId}", 
    EventLogEntryType.Information);
```

### Log File in User Profile

```csharp
// Safe location that doesn't require admin
var logPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "MyWordAddin", "logs", "addin.log");

Log.Logger = new LoggerConfiguration()
    .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
    .CreateLogger();
```

### Output Window (Debug)

```csharp
// Visible in VS Output window when attached
Debug.WriteLine($"[MyAddin] Document opened: {doc.Name}");
System.Diagnostics.Trace.WriteLine($"Range modified: {range.Start}-{range.End}");
```

## Structured Logging (Serilog)

### Setup for VSTO

```csharp
// In ThisAddIn_Startup
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .Enrich.WithProperty("AddIn", "MyWordAddin")
    .Enrich.WithProperty("OfficeVersion", Application.Version)
    .WriteTo.File(GetLogPath(), rollingInterval: RollingInterval.Day)
    .WriteTo.Debug()  // VS Output window
    .CreateLogger();
```

### Property Binding (Queryable)

```csharp
// GOOD: Properties are queryable
Log.Information("Template applied. TemplateId={TemplateId} DocName={DocName}", 
    templateId, doc.Name);

// BAD: String interpolation - not queryable
Log.Information($"Template applied. TemplateId={templateId} DocName={doc.Name}");
```

### Scoped Context

```csharp
// All logs in scope include DocumentId
using (LogContext.PushProperty("DocumentId", doc.Name))
{
    Log.Information("Starting template application");
    ApplyTemplate();
    Log.Information("Template application complete");
}
```

## OpenTelemetry .NET

### Setup

```csharp
// NuGet: OpenTelemetry, OpenTelemetry.Exporter.Console
var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .SetResourceBuilder(ResourceBuilder.CreateDefault()
        .AddService("MyWordAddin"))
    .AddSource("MyWordAddin")
    .AddConsoleExporter()
    .Build();

private static readonly ActivitySource ActivitySource = new("MyWordAddin");
```

### Create Spans

```csharp
public void ApplyTemplate(Document doc, string templateId)
{
    using var activity = ActivitySource.StartActivity("ApplyTemplate");
    activity?.SetTag("templateId", templateId);
    activity?.SetTag("docName", doc.Name);
    
    try
    {
        // Template application logic
        activity?.SetStatus(ActivityStatusCode.Ok);
    }
    catch (Exception ex)
    {
        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
        throw;
    }
}
```

## Log Levels

| Level | When | VSTO Example |
|-------|------|--------------|
| **Critical** | Add-in can't function | "Failed to connect to Word" |
| **Error** | Operation failed | "Template application failed" |
| **Warning** | Degraded but working | "Cache miss, regenerating" |
| **Information** | Business events | "Document saved with template" |
| **Debug** | Developer troubleshooting | "Range: {Start}-{End}" |

## What to Log in VSTO

### Do Log
- Document events (open, save, close)
- Template applications
- Ribbon command executions
- COM exceptions
- External service calls
- Performance (operation duration)

### Don't Log
- Document content (privacy)
- Every Range operation (noise)
- User credentials
- High-frequency events without sampling

## Error Logging Pattern

```csharp
public Result<Unit, Error> ExecuteRibbonCommand(IRibbonControl control)
{
    var sw = Stopwatch.StartNew();
    
    try
    {
        Log.Debug("Command started. CommandId={CommandId}", control.Id);
        
        var result = DoWork();
        
        Log.Information("Command completed. CommandId={CommandId} Duration={Duration}ms",
            control.Id, sw.ElapsedMilliseconds);
        
        return result;
    }
    catch (COMException ex)
    {
        Log.Error(ex, "COM error in command. CommandId={CommandId} HResult={HResult:X}",
            control.Id, ex.HResult);
        return DomainErrors.Command.ComFailed;
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Unexpected error. CommandId={CommandId}", control.Id);
        throw;
    }
}
```

## Metrics (RED Method)

```csharp
// Rate - commands per minute
private static readonly Counter<long> CommandCounter = 
    Meter.CreateCounter<long>("commands.executed");

// Errors - failures
private static readonly Counter<long> ErrorCounter = 
    Meter.CreateCounter<long>("commands.failed");

// Duration - latency histogram
private static readonly Histogram<double> Duration = 
    Meter.CreateHistogram<double>("commands.duration");

public void ExecuteCommand(string id)
{
    var sw = Stopwatch.StartNew();
    try
    {
        DoWork();
        CommandCounter.Add(1, new("command", id));
    }
    catch
    {
        ErrorCounter.Add(1, new("command", id));
        throw;
    }
    finally
    {
        Duration.Record(sw.ElapsedMilliseconds, new("command", id));
    }
}
```

## Anti-Patterns

| Anti-Pattern | Problem | Fix |
|--------------|---------|-----|
| Log and throw | Duplicate logs | Log at handling point only |
| String interpolation | Can't query | Use property binding |
| Document content logged | Privacy violation | Log metadata only |
| No correlation | Can't trace user session | Add SessionId to scope |
| COM exceptions ignored | Silent failures | Always log HResult |

## Quick Reference

| Need | Pattern |
|------|---------|
| Debug in production | Structured file logging |
| Track user session | Push SessionId to LogContext |
| Measure performance | Stopwatch + duration logging |
| Find COM issues | Log ex.HResult in hex |
| View during development | Debug.WriteLine + VS Output |
