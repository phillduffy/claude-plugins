# COM Interop Anti-Patterns Catalog

Complete reference of common anti-patterns in Word COM interop code.

## 1. Two-Dot Rule Violations

### Pattern: Chained Property Access

```csharp
// ANTI-PATTERN: 3 hidden COM objects
doc.Paragraphs[1].Range.Text = "Hello";
```

**Hidden objects:**
1. `Paragraphs` collection
2. `Paragraph` at index 1
3. `Range` object

**Risk:** All three objects leak, accumulating with each call.

### Pattern: Fluent-Style Chaining

```csharp
// ANTI-PATTERN: Fluent chains hide objects
selection.Range.Paragraphs[1].Range.Font.Bold = 1;
```

**Hidden objects:** Range, Paragraphs, Paragraph, Range, Font (5 objects!)

### Pattern: Method Return Values

```csharp
// ANTI-PATTERN: Hidden Range from InsertBefore return
selection.Range.InsertBefore("Prefix: ").Font.Italic = 1;
```

**Hidden objects:** Range, new Range from InsertBefore, Font

## 2. Collection Enumeration Issues

### Pattern: foreach on COM Collections

```csharp
// ANTI-PATTERN: IEnumerator COM object leaks
foreach (Word.Paragraph para in doc.Paragraphs)
{
    para.Range.Text = "Modified";
}
```

**Problem:** `foreach` uses `GetEnumerator()` which creates a COM IEnumerator that's never released.

**Correct:**
```csharp
var paragraphs = doc.Paragraphs;
try
{
    for (int i = 1; i <= paragraphs.Count; i++)
    {
        var para = paragraphs[i];
        try
        {
            var range = para.Range;
            try { range.Text = "Modified"; }
            finally { Marshal.ReleaseComObject(range); }
        }
        finally { Marshal.ReleaseComObject(para); }
    }
}
finally { Marshal.ReleaseComObject(paragraphs); }
```

### Pattern: LINQ on COM Collections

```csharp
// ANTI-PATTERN: LINQ creates multiple enumerators
var headings = doc.Paragraphs
    .Cast<Word.Paragraph>()
    .Where(p => p.Style.NameLocal == "Heading 1");
```

**Problem:** `Cast<>` and `Where` both enumerate, creating multiple hidden enumerators.

## 3. Missing Cleanup Patterns

### Pattern: No try-finally

```csharp
// ANTI-PATTERN: Exception causes leak
var range = doc.Range();
range.Text = "Hello";
// What if exception here?
Marshal.ReleaseComObject(range);
```

**Correct:**
```csharp
Word.Range range = null;
try
{
    range = doc.Range();
    range.Text = "Hello";
}
finally
{
    if (range != null) Marshal.ReleaseComObject(range);
}
```

### Pattern: Early Return Without Cleanup

```csharp
// ANTI-PATTERN: Early return skips cleanup
var range = doc.Range();
if (range.Text.Length == 0)
    return; // LEAK!
Marshal.ReleaseComObject(range);
```

## 4. RCW Sharing Issues

### Pattern: ReleaseComObject on Shared Objects

```csharp
// ANTI-PATTERN: Releasing shared RCW
public void ProcessDocument(Word.Document doc)
{
    // doc is passed in, other code may hold reference
    var range = doc.Range();
    // work
    Marshal.ReleaseComObject(doc); // WRONG! Caller still needs it
}
```

### Pattern: Singleton Services with ReleaseComObject

```csharp
// ANTI-PATTERN: Singleton releasing ActiveDocument
public class DocumentService
{
    public void Process()
    {
        var doc = Globals.ThisAddIn.Application.ActiveDocument;
        // work
        Marshal.ReleaseComObject(doc); // Other services may hold same RCW!
    }
}
```

**Correct:** Set to null and let GC handle:
```csharp
public void Process()
{
    var doc = Globals.ThisAddIn.Application.ActiveDocument;
    try { /* work */ }
    finally { doc = null; }
}
```

## 5. Application Lifecycle Issues

### Pattern: Missing Quit() Call

```csharp
// ANTI-PATTERN: Application never quits
var word = new Word.Application();
var doc = word.Documents.Add();
doc.SaveAs("file.docx");
doc.Close();
// word.Quit() never called - orphaned WINWORD.EXE!
```

### Pattern: Missing GC After Quit

```csharp
// ANTI-PATTERN: GC not triggered
word.Quit();
// Process may still be alive until GC runs
```

**Correct:**
```csharp
word.Quit();
Marshal.ReleaseComObject(word);
word = null;
GC.Collect();
GC.WaitForPendingFinalizers();
```

## 6. Event Handler Issues

### Pattern: Event Handlers Holding References

```csharp
// ANTI-PATTERN: Event handler holds COM reference
private Word.Document _trackedDoc;

private void OnDocumentOpen(Word.Document doc)
{
    _trackedDoc = doc; // Holds RCW reference indefinitely
}
```

**Correct:** Use weak references or clear on document close:
```csharp
private void OnDocumentBeforeClose(Word.Document doc, ref bool cancel)
{
    if (doc == _trackedDoc)
        _trackedDoc = null;
}
```

## 7. Async/Await Issues

### Pattern: COM Objects Across Await

```csharp
// ANTI-PATTERN: COM object used after await
var range = doc.Range();
await SomeAsyncOperation();
range.Text = "Hello"; // May fail - STA context changed
```

**Problem:** COM objects are STA-bound. After await, execution may resume on different thread.

**Correct:** Complete COM work before await or use ConfigureAwait(true).

## Detection Commands

```bash
# Two-dot violations
rg "\w+\.\w+\.\w+\(" --type cs

# foreach on known COM collections
rg "foreach.*\b(Paragraphs|Documents|Sections|Tables|Rows|Cells|Bookmarks|ContentControls|Styles)\b" --type cs

# Missing try-finally (COM access without finally)
rg "Marshal\.ReleaseComObject" --type cs -A5 | rg -v "finally"

# Shared Application.ActiveDocument releases
rg "ReleaseComObject.*ActiveDocument" --type cs

# LINQ on COM collections
rg "\.(Cast|OfType|Where|Select|First|Any).*Paragraphs" --type cs
```
