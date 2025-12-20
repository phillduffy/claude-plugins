---
name: VSTO COM Interop Patterns
description: This skill should be used when the user asks about "COM cleanup", "Marshal.ReleaseComObject", "two-dot rule", "COM memory leaks", "Office interop", "Word automation", "RCW", "Runtime Callable Wrapper", or mentions COM object lifecycle management in VSTO add-ins. Provides patterns for safe COM object handling.
version: 0.1.0
---

# VSTO COM Interop Patterns

Safe patterns for COM object lifecycle management in Office VSTO add-ins.

## Core Principle

Office COM objects are reference-counted. The .NET Runtime Callable Wrapper (RCW) holds a COM reference that must be explicitly released or allowed to be garbage collected. Improper handling causes:

- **Memory leaks** - Unreleased COM objects persist
- **Orphaned processes** - WINWORD.EXE remains after close
- **RCW separation errors** - Accessing released objects crashes

## When to Use Marshal.ReleaseComObject

| Context | Recommendation |
|---------|----------------|
| **VSTO Add-in (in-process)** | Avoid - let GC handle, use null assignment |
| **Standalone automation** | Required in finally block |
| **Singleton services** | Never - other code may hold RCW reference |
| **Short-lived operations** | Use ComObjectTracker pattern |

### In-Process Add-in Pattern (Preferred)

VSTO add-ins run in Word's process. Multiple services may reference the same RCW.

```csharp
public class WordDocumentContext : IWordDocumentContext
{
    private static readonly AsyncLocal<Word.Document?> CurrentDocument = new();

    public void Capture()
    {
        CurrentDocument.Value = Globals.ThisAddIn.Application.ActiveDocument;
    }

    public void Release()
    {
        // DON'T ReleaseComObject - other singleton services may hold
        // references to the same RCW. Let GC handle cleanup.
        CurrentDocument.Value = null;
    }
}
```

### Disposable Wrapper Pattern

Wrap COM objects in IDisposable for deterministic cleanup:

```csharp
public class WordDocumentRange : IDocumentRange, IDisposable
{
    public Range Range { get; }

    public WordDocumentRange(Range range)
    {
        Range = range ?? throw new ArgumentNullException(nameof(range));
    }

    public void Dispose()
    {
        Marshal.ReleaseComObject(Range);
    }
}
```

### ComObjectTracker Pattern (Testing/Short-lived)

Track multiple COM objects for LIFO release:

```csharp
public class ComObjectTracker : IDisposable
{
    private readonly Stack<object> _tracked = new();

    public T Track<T>(T comObject) where T : class
    {
        _tracked.Push(comObject);
        return comObject;
    }

    public void Dispose()
    {
        while (_tracked.Count > 0)
        {
            var obj = _tracked.Pop();
            try { Marshal.ReleaseComObject(obj); }
            catch (InvalidComObjectException) { /* Already released */ }
        }
    }
}
```

Usage:
```csharp
using var tracker = new ComObjectTracker();
var paragraphs = tracker.Track(doc.Paragraphs);
var para = tracker.Track(paragraphs[1]);
var range = tracker.Track(para.Range);
// All released in reverse order on dispose
```

## Two-Dot Rule

Every dot potentially creates a hidden COM object that leaks if not captured.

### Violation Examples

```csharp
// BAD: Hidden Paragraphs, Paragraph, and Range objects
doc.Paragraphs[1].Range.Text = "Hello";

// BAD: Hidden Documents object
word.Documents.Add();

// BAD: Hidden Styles object
doc.Styles["Heading 1"].Font.Bold = 1;
```

### Correct Pattern

```csharp
Word.Paragraphs paragraphs = null;
Word.Paragraph para = null;
Word.Range range = null;
try
{
    paragraphs = doc.Paragraphs;
    para = paragraphs[1];
    range = para.Range;
    range.Text = "Hello";
}
finally
{
    if (range != null) Marshal.ReleaseComObject(range);
    if (para != null) Marshal.ReleaseComObject(para);
    if (paragraphs != null) Marshal.ReleaseComObject(paragraphs);
}
```

## Collection Enumeration

`foreach` creates hidden enumerators that leak.

```csharp
// BAD: Hidden IEnumerator COM object
foreach (Paragraph para in doc.Paragraphs)
{
    // ...
}

// GOOD: Indexed access with explicit cleanup
var paragraphs = doc.Paragraphs;
try
{
    for (int i = 1; i <= paragraphs.Count; i++)
    {
        var para = paragraphs[i];
        try { /* work */ }
        finally { Marshal.ReleaseComObject(para); }
    }
}
finally
{
    Marshal.ReleaseComObject(paragraphs);
}
```

## Standalone Automation Cleanup

External apps must explicitly close/quit and trigger GC:

```csharp
Word.Application word = null;
Word.Document doc = null;
try
{
    word = new Word.Application { Visible = false };
    doc = word.Documents.Add();
    // work
}
finally
{
    if (doc != null)
    {
        doc.Close(SaveChanges: false);
        Marshal.ReleaseComObject(doc);
    }
    if (word != null)
    {
        word.Quit();
        Marshal.ReleaseComObject(word);
    }
    GC.Collect();
    GC.WaitForPendingFinalizers();
}
```

## Performance Optimization

Batch operations to reduce UI flicker:

```csharp
var app = Globals.ThisAddIn.Application;
app.ScreenUpdating = false;
try
{
    // Multiple Word operations
}
finally
{
    app.ScreenUpdating = true;
}
```

## Quick Detection Commands

```bash
# Two-dot violations (chained property access)
rg "\w+\.\w+\.\w+\(" --type cs

# foreach on COM collections
rg "foreach.*\b(Paragraphs|Documents|Sections|Tables|Rows|Cells)\b" --type cs

# Marshal.ReleaseComObject usage (audit)
rg "Marshal\.ReleaseComObject" --type cs
```

## Additional Resources

### Reference Files

- **`references/anti-patterns.md`** - Complete anti-pattern catalog with examples
- **`references/rcw-internals.md`** - How RCWs work and why cleanup matters

### Examples

- **`examples/ComObjectTracker.cs`** - Full tracker implementation
- **`examples/WordDocumentRange.cs`** - Disposable wrapper pattern
