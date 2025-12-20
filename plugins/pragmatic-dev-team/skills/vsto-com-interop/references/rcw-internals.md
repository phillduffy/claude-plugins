# RCW Internals: How Runtime Callable Wrappers Work

Understanding the mechanics of COM/.NET interop for proper object lifecycle management.

## What is an RCW?

The **Runtime Callable Wrapper (RCW)** is a .NET proxy that:
1. Holds a reference to a COM object
2. Marshals calls between .NET and COM
3. Translates COM HRESULTs to .NET exceptions
4. Aggregates multiple interface references to one COM object

## Reference Counting

### COM Side
COM objects use `AddRef()` and `Release()` for reference counting:
- Object created: refcount = 1
- `AddRef()`: refcount++
- `Release()`: refcount--
- refcount = 0: object destroys itself

### RCW Side
Each RCW maintains its own reference count:
- RCW created: calls COM `AddRef()`
- `Marshal.ReleaseComObject()`: decrements RCW count
- RCW count = 0: calls COM `Release()`
- RCW finalized by GC: calls COM `Release()` if not already

## One RCW Per COM Object

**Critical:** The CLR maintains a single RCW per COM object per AppDomain.

```csharp
var doc1 = app.ActiveDocument;  // Creates RCW
var doc2 = app.ActiveDocument;  // Returns SAME RCW

// doc1 and doc2 are the same .NET object
Object.ReferenceEquals(doc1, doc2) == true
```

**Implication:** `Marshal.ReleaseComObject(doc1)` invalidates `doc2` too!

## RCW Identity Cache

The CLR maintains a cache mapping COM IUnknown pointers to RCWs:
1. COM method returns interface pointer
2. CLR queries cache for existing RCW
3. If found: return existing RCW, increment .NET reference count
4. If not: create new RCW, add to cache

## Marshal.ReleaseComObject Behavior

```csharp
int count = Marshal.ReleaseComObject(comObject);
```

1. Decrements internal RCW reference count
2. Returns remaining count
3. If count reaches 0:
   - Calls COM `Release()`
   - Marks RCW as "separated"
   - Removes from identity cache

## FinalReleaseComObject vs ReleaseComObject

```csharp
// Decrements once
Marshal.ReleaseComObject(obj);

// Sets count to 0, fully releases
Marshal.FinalReleaseComObject(obj);
```

**FinalReleaseComObject** is rarely needed but useful when RCW ref count is uncertain.

## The "RCW separated" Error

```
System.Runtime.InteropServices.InvalidComObjectException:
COM object that has been separated from its underlying RCW cannot be used.
```

**Cause:** Accessing RCW after `ReleaseComObject` reduced count to 0.

**Common scenario:**
```csharp
var doc = app.ActiveDocument;
Marshal.ReleaseComObject(doc);

// Later, same or different code:
var doc2 = app.ActiveDocument;  // Same RCW!
doc2.Range(); // CRASH - RCW is separated
```

## Why GC is Sometimes Better

In VSTO add-ins, multiple services may hold the same RCW:

```csharp
// Service A
_currentDoc = app.ActiveDocument;

// Service B (same AppDomain)
var doc = app.ActiveDocument; // Same RCW as Service A!
Marshal.ReleaseComObject(doc); // Invalidates Service A's reference!
```

**Solution:** Let GC handle cleanup:
```csharp
// Service A cleans up
_currentDoc = null;

// Service B cleans up
doc = null;

// Eventually, GC finalizes the RCW and calls COM Release()
```

## STA Threading and RCWs

COM objects are typically STA (Single-Threaded Apartment):
- Created on one thread
- Must be accessed from same thread
- Cross-thread calls are marshaled

**Async/await hazard:**
```csharp
var range = doc.Range();        // Created on UI thread
await SomeTask();               // May resume on different thread
range.Text = "Hello";           // Cross-apartment call - may fail
```

## Memory Pressure and GC

RCWs don't report COM object memory to GC. Large COM objects may not trigger collection.

**Force cleanup for large operations:**
```csharp
// After processing many documents
GC.Collect();
GC.WaitForPendingFinalizers();
GC.Collect(); // Collect objects freed by finalizers
```

## Best Practices Summary

| Scenario | Approach |
|----------|----------|
| VSTO add-in services | Null assignment, let GC handle |
| Short-lived operations | ComObjectTracker pattern |
| Standalone automation | try-finally with ReleaseComObject |
| Uncertain reference count | FinalReleaseComObject |
| Cross-thread access | Stay on STA thread or use Invoke |

## Debugging RCW Issues

**Check if RCW is valid:**
```csharp
try
{
    Marshal.GetIUnknownForObject(obj);
    // RCW is valid
}
catch (InvalidComObjectException)
{
    // RCW is separated
}
```

**Get RCW reference count:**
```csharp
// No direct way, but can infer from ReleaseComObject return
int remaining = Marshal.ReleaseComObject(obj);
// Then re-add reference if needed (not recommended)
```
