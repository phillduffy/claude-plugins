# Word Object Model Patterns in OfficeAddins

This reference maps Word interop patterns to concrete implementations in the OfficeAddins codebase.

## Architecture Overview

```
WordAddIn/
├── Interop/                      # Word COM wrapper layer
│   ├── WordObjectModelTextReplacer.cs
│   ├── WordObjectModelDocumentContentControl.cs
│   ├── WordDocumentRange.cs      # IDisposable wrapper
│   └── WordContentControlMapper.cs
├── Features/
│   └── Scripting/
│       └── WordRangeResolver.cs  # Range resolution
└── ThisAddIn.cs                  # Entry point
```

## Key Interfaces

| Interface | Implementation | Purpose |
|-----------|---------------|---------|
| `IDocumentRange` | `WordDocumentRange` | Abstract Range without Word dependency |
| `ITextReplacer` | `WordObjectModelTextReplacer` | Find & Replace operations |
| `IDocumentContentControl` | `WordObjectModelDocumentContentControl` | Content control manipulation |
| `IWordDocumentContext` | `WordDocumentContext` | Document + Application access |

## COM Object Lifecycle

### Your Release Patterns

```csharp
// Pattern 1: try/finally for single objects
var range = document.Range(0, 100);
try
{
    // Use range
}
finally
{
    Marshal.ReleaseComObject(range);
}

// Pattern 2: using with WordDocumentRange wrapper
using var range = new WordDocumentRange(document.Range(0, 100));

// Pattern 3: out parameter for deferred release
Range[] ranges = GetParagraphRanges(out Paragraphs? paragraphs);
try { /* use ranges */ }
finally { if (paragraphs != null) Marshal.ReleaseComObject(paragraphs); }
```

### Two-Dot Rule Compliance

```csharp
// WRONG - can't release intermediate object
document.Range(0, 100).Select();

// RIGHT - store intermediate
var range = document.Range(0, 100);
try { range.Select(); }
finally { Marshal.ReleaseComObject(range); }
```

## Document Structure Access

| Structure | Access Pattern | Story Type |
|-----------|---------------|------------|
| Main text | `document.Content` | `wdMainTextStory` |
| Headers | `section.Headers[WdHeaderFooterIndex]` | Per-section |
| Footers | `section.Footers[WdHeaderFooterIndex]` | Per-section |
| Footnotes | `StoryRanges[wdFootnotesStory]` | Document-level |
| Endnotes | `StoryRanges[wdEndnotesStory]` | Document-level |
| Text boxes | `shape.TextFrame.TextRange` | Per-shape |

## Range Operations

| Operation | Method | Example |
|-----------|--------|---------|
| Duplicate | `range.Duplicate` | Safe copy before modification |
| Collapse | `range.Collapse(wdCollapseStart)` | Convert to insertion point |
| Move end | `range.MoveEnd(wdCharacter, -1)` | Shrink range |
| Check content | `range.Information[wdInContentControl]` | Query range state |
| Select | `range.Select()` | Move user selection |

## Find & Replace

### Your Find Configuration

```csharp
var find = range.Find;
find.ClearFormatting();           // Reset previous settings
find.Text = "pattern";
find.Replacement.Text = "new";
find.Forward = true;
find.Wrap = WdFindWrap.wdFindStop; // Don't wrap around
find.MatchCase = false;
find.MatchWildcards = true;        // For regex-like patterns
```

### Wildcard Patterns You Use

| Pattern | Purpose | Example |
|---------|---------|---------|
| `[ ]{2,}` | Extra spaces | Delete double spaces |
| `(^13){2,}` | Extra paragraphs | Delete blank lines |
| `[0-9]{1,}` | Numbers | Find digits |

## Content Control Handling

### Selection States

| State | CC Count | In CC | Parent CC |
|-------|----------|-------|-----------|
| Insertion point in CC | 0 | true | exists |
| Text selected in CC | 0 | true | exists |
| CC itself selected | 1 | true | null |
| Multiple CCs selected | >1 | true | varies |

### Your Navigation Pattern

```csharp
// Find next CC after current position
foreach (var cc in searchRange.ContentControls.Cast<ContentControl>())
    if (cc.Range.Start > currentPosition)
        return cc;
```

## Quick Reference: Your Interop Classes

| Class | Responsibility | Key Methods |
|-------|---------------|-------------|
| `WordObjectModelTextReplacer` | Find/Replace across all stories | `ReplaceAll()`, `ReplaceInRange()` |
| `WordObjectModelDocumentContentControl` | CC operations | `GetNextContentControl()`, `IsInContentControl()` |
| `WordRangeResolver` | Command → Range mapping | `ResolveRanges()` |
| `WordDocumentRange` | IDisposable Range wrapper | Implements `IDocumentRange` |

## Error Handling

```csharp
try
{
    // COM operation
}
catch (COMException ex) when (ex.HResult == -2147023174)
{
    // RPC_E_DISCONNECTED - Word closed
}
catch (InvalidComObjectException)
{
    // Object already released
}
```

## Performance Patterns

| Pattern | When | Why |
|---------|------|-----|
| `Application.ScreenUpdating = false` | Batch operations | Prevent UI flicker |
| `document.UndoRecord.StartCustomRecord()` | Multi-step changes | Single undo action |
| Save selection, restore after | Find/Replace | Don't disrupt user |
