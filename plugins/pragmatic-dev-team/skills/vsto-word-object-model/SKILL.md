---
name: Word Object Model Reference
description: This skill should be used when the user asks about "Word.Document", "Word.Range", "Word.Selection", "Content Controls", "Word automation", "Word API", "Paragraphs collection", "Bookmarks", "Styles", "Find and Replace", "Headers and Footers", "Tables", "Sections", or any Word object model operations in VSTO add-ins.
version: 0.1.0
load: on-demand
---

# Word Object Model Reference

Comprehensive reference for the Microsoft Word object model in VSTO add-ins.

## Object Hierarchy Overview

```
Application
├── Documents (collection)
│   └── Document
│       ├── Content (Range - whole document)
│       ├── Range() (method - create Range)
│       ├── Paragraphs (collection)
│       ├── Sentences (collection)
│       ├── Words (collection)
│       ├── Bookmarks (collection)
│       ├── ContentControls (collection)
│       ├── Sections (collection)
│       │   └── Headers/Footers
│       ├── Tables (collection)
│       ├── Styles (collection)
│       ├── Fields (collection)
│       ├── StoryRanges (access to footnotes, endnotes, etc.)
│       └── Shapes (text boxes, images)
└── Selection (current user selection)
```

## Core Objects

### Range

The fundamental unit for working with document content. Every text operation uses Range.

```csharp
// Get entire document
Range content = doc.Content;

// Get specific positions
Range range = doc.Range(start, end);

// Get from selection
Range selRange = app.Selection.Range;

// Duplicate to preserve original
Range copy = range.Duplicate;
```

**Key properties:**
- `Start`, `End` - character positions
- `Text` - content as string
- `Font`, `ParagraphFormat` - formatting
- `Find` - Find object for search/replace

**Key methods:**
- `InsertBefore()`, `InsertAfter()` - add text
- `Collapse(WdCollapseDirection)` - collapse to point
- `MoveStart()`, `MoveEnd()` - adjust boundaries
- `Select()` - make this the Selection
- `Delete()` - remove content

### Selection

Represents what the user has selected. Most operations work through Range.

```csharp
var selection = documentContext.Application.Selection;

// Check if anything is selected
if (selection.Type == WdSelectionType.wdSelectionNormal)
{
    var range = selection.Range;
    // work with range
}
```

**Selection types:**
- `wdSelectionNormal` - text selected
- `wdSelectionIP` - insertion point (no selection)
- `wdSelectionInlineShape` - image selected

### Document

```csharp
var doc = documentContext.Document;

// Check if documents exist
if (app.Documents.Count > 0)
{
    var activeDoc = app.ActiveDocument;
}

// Document lifecycle
doc.Save();
doc.SaveAs(path);
doc.Close(WdSaveOptions.wdDoNotSaveChanges);
```

## Collections

### Paragraphs

```csharp
Paragraphs paragraphs = null;
try
{
    paragraphs = doc.Paragraphs;

    // 1-indexed access
    var firstPara = paragraphs[1];
    var range = firstPara.Range;

    // Iterate with indexed loop (not foreach)
    for (int i = 1; i <= paragraphs.Count; i++)
    {
        var para = paragraphs[i];
        try { /* work */ }
        finally { Marshal.ReleaseComObject(para); }
    }
}
finally
{
    if (paragraphs != null) Marshal.ReleaseComObject(paragraphs);
}
```

### Bookmarks

```csharp
if (doc.Bookmarks.Exists("BookmarkName"))
{
    var bookmark = doc.Bookmarks["BookmarkName"];
    var range = bookmark.Range;
    range.Text = "New content";
}
```

### Content Controls

Content Controls are structured document elements for data binding.

```csharp
var controls = doc.ContentControls;
foreach (ContentControl cc in controls)
{
    if (cc.Type == WdContentControlType.wdContentControlText)
    {
        cc.Range.Text = "Updated value";
    }
}
```

**Check if in locked content control:**
```csharp
ContentControl? cc = null;
try
{
    cc = range.ParentContentControl;
    if (cc?.LockContents == true)
        return; // Cannot modify
}
finally
{
    if (cc != null) Marshal.ReleaseComObject(cc);
}
```

## Find and Replace

### Basic Find

```csharp
var find = range.Find;
find.ClearFormatting();
find.Text = "search text";
find.Forward = true;
find.Wrap = WdFindWrap.wdFindStop;
find.MatchCase = true;
find.MatchWholeWord = false;
find.MatchWildcards = false;

while (find.Execute())
{
    // range now contains match
    var matchText = range.Text;
}
```

### Find and Replace

```csharp
var find = range.Find;
find.ClearFormatting();
find.Text = "old text";
find.Replacement.ClearFormatting();
find.Replacement.Text = "new text";
find.MatchCase = false;
find.MatchWholeWord = true;
find.Wrap = WdFindWrap.wdFindStop;
find.Forward = true;

find.Execute(Replace: WdReplace.wdReplaceAll);
```

### Wildcard Search

```csharp
find.MatchWildcards = true;
find.Text = "[0-9]{3}";  // Match 3 digits
```

**Common wildcards:**
- `?` - any single character
- `*` - any string
- `[abc]` - any char in set
- `[!abc]` - any char not in set
- `[a-z]` - range
- `{n}` - exactly n occurrences
- `{n,}` - n or more
- `{n,m}` - between n and m

## Story Ranges

Documents have multiple "stories" for different areas:

```csharp
// Main document
Range main = doc.Content;

// Headers/Footers - via Sections
foreach (Section section in doc.Sections)
{
    foreach (HeaderFooter header in section.Headers)
    {
        if (header.Exists)
        {
            var headerRange = header.Range;
            // work with header
        }
    }
}

// Footnotes (if any exist)
if (doc.Footnotes.Count > 0)
{
    var footnoteStory = doc.StoryRanges[WdStoryType.wdFootnotesStory];
}

// Endnotes
if (doc.Endnotes.Count > 0)
{
    var endnoteStory = doc.StoryRanges[WdStoryType.wdEndnotesStory];
}

// Text boxes/shapes
foreach (Shape shape in doc.Shapes)
{
    if (shape.TextFrame.HasText != 0)
    {
        var textRange = shape.TextFrame.TextRange;
    }
}
```

## Tables

```csharp
Tables tables = doc.Tables;
if (tables.Count > 0)
{
    Table table = tables[1];

    // Access cells
    Cell cell = table.Cell(1, 1); // row 1, col 1
    cell.Range.Text = "Header";

    // Iterate rows
    for (int r = 1; r <= table.Rows.Count; r++)
    {
        for (int c = 1; c <= table.Columns.Count; c++)
        {
            var cellRange = table.Cell(r, c).Range;
            // work with cell
        }
    }
}
```

## Styles

```csharp
// Apply style by name
range.Style = doc.Styles["Heading 1"];

// Check current style
var styleName = ((Style)range.Style).NameLocal;

// Get style object
Style style = doc.Styles["Normal"];
style.Font.Name = "Arial";
style.Font.Size = 11;
```

## Formatting

### Font

```csharp
range.Font.Bold = 1;      // -1 = true, 0 = false, 9999999 = mixed
range.Font.Italic = 1;
range.Font.Underline = WdUnderline.wdUnderlineSingle;
range.Font.Name = "Arial";
range.Font.Size = 12;
range.Font.Color = WdColor.wdColorRed;
```

### Paragraph

```csharp
range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
range.ParagraphFormat.SpaceBefore = 12;  // points
range.ParagraphFormat.SpaceAfter = 6;
range.ParagraphFormat.LineSpacing = 18;
range.ParagraphFormat.LeftIndent = 36;   // points
```

## Fields

```csharp
// Insert field at range
doc.Fields.Add(range, Type.Missing, "DATE \\@ \"MMMM d, yyyy\"");

// Update all fields
doc.Fields.Update();

// Iterate fields
foreach (Field field in doc.Fields)
{
    if (field.Type == WdFieldType.wdFieldDate)
    {
        field.Update();
    }
}
```

## Performance Optimization

```csharp
var app = documentContext.Application;
app.ScreenUpdating = false;
try
{
    // Batch operations
}
finally
{
    app.ScreenUpdating = true;
}
```

**Also consider:**
```csharp
app.DisplayAlerts = WdAlertLevel.wdAlertsNone;
doc.UndoRecord.StartCustomRecord("Operation Name");
// operations
doc.UndoRecord.EndCustomRecord();
```

## Common Gotchas

| Issue | Solution |
|-------|----------|
| 0-indexed vs 1-indexed | Collections are 1-indexed |
| Text property includes paragraph marks | Check for `\r` at end |
| Range.Text assignment replaces | Use InsertBefore/After to add |
| Selection vs Range | Get Range from Selection for manipulation |
| Empty documents | Check `Documents.Count > 0` first |
| Mixed formatting returns | Check for `9999999` (wdUndefined) |

## Additional Resources

### Reference Files

- **`references/story-types.md`** - Complete story type reference
- **`references/find-replace-patterns.md`** - Wildcard and regex patterns
- **`references/content-controls.md`** - Working with content controls
