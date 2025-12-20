# Memory Profiling

Finding and fixing memory leaks in .NET applications.

## Common Causes of Memory Leaks

| Cause | Symptom | Detection |
|-------|---------|-----------|
| **Event handlers** | Objects not collected | GC root shows event delegate |
| **Static references** | Growing static collections | `dumpheap -stat` shows growth |
| **Caching** | Unbounded cache growth | Memory increases with requests |
| **Disposable not disposed** | Handle count grows | `!handle` command in dump |
| **Captive dependencies** | Scoped in singleton | Short-lived objects survive |

---

## Detection Workflow

### 1. Baseline Snapshot

```bash
# Take initial snapshot
dotnet gcdump collect -p <PID> -o baseline.gcdump
```

### 2. Exercise Leak Scenario

- Perform the suspected leaking operation
- Repeat multiple times
- Wait briefly for GC

### 3. Force GC and Second Snapshot

```bash
# Force GC (if possible via diagnostic endpoint)
curl http://localhost:5000/debug/gc

# Take second snapshot
dotnet gcdump collect -p <PID> -o after.gcdump
```

### 4. Compare Snapshots

Open both `.gcdump` files in Visual Studio:
- Look at "Survived objects" column
- Objects in both snapshots should be expected long-lived objects
- Unexpected survivors = potential leak

---

## dotnet-dump Analysis

### Finding Memory Hogs

```bash
dotnet dump analyze ./core_dump

# Memory usage by type
> dumpheap -stat

# Find specific type
> dumpheap -type MyNamespace.Order

# Inspect specific object
> dumpobj 0x00007f8a1234abcd

# Why isn't this object collected?
> gcroot 0x00007f8a1234abcd
```

### Async State Machine Leaks

```bash
# List async state machines (often overlooked leak source)
> dumpasync

# Look for:
# - Large numbers of pending tasks
# - Tasks that should have completed
# - Async methods that never finish
```

### Thread Analysis

```bash
# All threads
> threads

# Stack trace for specific thread
> setthread <thread-id>
> clrstack

# All stacks
> clrstack -a
```

---

## PerfView Analysis

### Heap Snapshot

```powershell
# Take heap snapshot
PerfView.exe /Heap /Process:MyApp.exe

# In PerfView UI:
# 1. Memory → Take Heap Snapshot
# 2. Dump GC Heap
# 3. Analyze by type
```

### GC Events

```powershell
# Collect GC events
PerfView.exe /GCCollectOnly /Process:MyApp.exe

# Analyze:
# - Gen 2 collections (should be rare)
# - GC pause times
# - Allocation rates
```

---

## JetBrains dotMemory

### Key Features

- Visual heap analysis
- Snapshot comparison
- Retention paths
- Dominators (what's holding memory)
- Frozen Object Heap (FOH) support

### Workflow

1. Attach to process
2. Collect baseline snapshot
3. Reproduce scenario
4. Collect comparison snapshot
5. Analyze "New Objects" between snapshots

---

## Common Leak Patterns

### Event Handler Leak

```csharp
// LEAK: Event handler never unsubscribed
public class Subscriber
{
    public Subscriber(Publisher publisher)
    {
        publisher.DataReceived += OnDataReceived;  // Leak!
    }

    private void OnDataReceived(object sender, EventArgs e) { }
}

// FIX: Implement IDisposable
public class Subscriber : IDisposable
{
    private readonly Publisher _publisher;

    public Subscriber(Publisher publisher)
    {
        _publisher = publisher;
        _publisher.DataReceived += OnDataReceived;
    }

    private void OnDataReceived(object sender, EventArgs e) { }

    public void Dispose()
    {
        _publisher.DataReceived -= OnDataReceived;
    }
}
```

### Captive Dependency

```csharp
// LEAK: Singleton holds reference to scoped service
public class MySingleton
{
    private readonly MyScopedService _scoped;  // Never released!

    public MySingleton(MyScopedService scoped)
    {
        _scoped = scoped;
    }
}

// FIX: Use IServiceScopeFactory
public class MySingleton
{
    private readonly IServiceScopeFactory _scopeFactory;

    public MySingleton(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task DoWork()
    {
        using var scope = _scopeFactory.CreateScope();
        var scoped = scope.ServiceProvider.GetRequiredService<MyScopedService>();
        await scoped.ProcessAsync();
    }
}
```

### Unbounded Cache

```csharp
// LEAK: Cache grows forever
private static readonly Dictionary<string, object> _cache = new();

public object GetOrAdd(string key, Func<object> factory)
{
    if (!_cache.TryGetValue(key, out var value))
    {
        value = factory();
        _cache[key] = value;  // Never evicted!
    }
    return value;
}

// FIX: Use MemoryCache with expiration
private readonly IMemoryCache _cache;

public object GetOrAdd(string key, Func<object> factory)
{
    return _cache.GetOrCreate(key, entry =>
    {
        entry.SlidingExpiration = TimeSpan.FromMinutes(10);
        entry.Size = 1;
        return factory();
    });
}
```

### HttpClient Creation

```csharp
// LEAK: Socket exhaustion
public async Task<string> GetDataAsync()
{
    using var client = new HttpClient();  // Don't do this!
    return await client.GetStringAsync(url);
}

// FIX: Use IHttpClientFactory
public class MyService
{
    private readonly HttpClient _client;

    public MyService(IHttpClientFactory factory)
    {
        _client = factory.CreateClient("MyApi");
    }

    public async Task<string> GetDataAsync()
    {
        return await _client.GetStringAsync(url);
    }
}
```

---

## GC Analysis Commands

```bash
# In dotnet-dump analyze session:

# GC heap statistics
> eeheap -gc

# LOH (Large Object Heap) analysis
> dumpheap -stat -min 85001

# Finalization queue
> finalizequeue

# Memory by generation
> gcheapstat
```

---

## Quick Reference

| Tool | Best For | Platform |
|------|----------|----------|
| **dotnet-gcdump** | Quick memory snapshot | Cross-platform |
| **dotnet-dump** | Deep analysis, threads | Cross-platform |
| **PerfView** | GC events, allocations | Windows |
| **dotMemory** | Visual analysis | Windows/Linux |
| **Visual Studio** | Integrated debugging | Windows |

## Analysis Priority

1. **Growing types** - What types are accumulating?
2. **GC roots** - Why aren't they collected?
3. **Retention paths** - What chain of references holds them?
4. **Event handlers** - Common hidden leak source
5. **Async state machines** - Tasks that never complete

## Sources

- [Debug a memory leak](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/debug-memory-leak)
- [dotnet-gcdump](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/dotnet-gcdump)
- [Finding that C# memory leak](https://timdeschryver.dev/blog/finding-that-csharp-memory-leak)
- [PerfView for .NET memory analysis](https://medium.com/@balakrishnanvinchu/perfview-for-net-application-memory-analysis-10b379f00bf1)
- [dotMemory](https://www.jetbrains.com/help/dotmemory/How_to_Find_a_Memory_Leak.html)
