# Word Story Types Reference

Word documents contain multiple "stories" - separate text containers for different document areas.

## WdStoryType Enumeration

| Story Type | Value | Description |
|------------|-------|-------------|
| `wdMainTextStory` | 1 | Main document body |
| `wdFootnotesStory` | 2 | Footnote text |
| `wdEndnotesStory` | 3 | Endnote text |
| `wdCommentsStory` | 4 | Comment text |
| `wdTextFrameStory` | 5 | Text box/shape text |
| `wdEvenPagesHeaderStory` | 6 | Even pages header |
| `wdPrimaryHeaderStory` | 7 | Primary (odd pages) header |
| `wdEvenPagesFooterStory` | 8 | Even pages footer |
| `wdPrimaryFooterStory` | 9 | Primary (odd pages) footer |
| `wdFirstPageHeaderStory` | 10 | First page header |
| `wdFirstPageFooterStory` | 11 | First page footer |
| `wdFootnoteSeparatorStory` | 12 | Footnote separator |
| `wdFootnoteContinuationSeparatorStory` | 13 | Footnote continuation separator |
| `wdFootnoteContinuationNoticeStory` | 14 | Footnote continuation notice |
| `wdEndnoteSeparatorStory` | 15 | Endnote separator |
| `wdEndnoteContinuationSeparatorStory` | 16 | Endnote continuation separator |
| `wdEndnoteContinuationNoticeStory` | 17 | Endnote continuation notice |

## Accessing Stories

### Via StoryRanges Collection

```csharp
// Check if story exists before accessing
if (doc.Footnotes.Count > 0)
{
    Range footnoteStory = doc.StoryRanges[WdStoryType.wdFootnotesStory];
    // Process all footnotes as one range
}

if (doc.Endnotes.Count > 0)
{
    Range endnoteStory = doc.StoryRanges[WdStoryType.wdEndnotesStory];
}

if (doc.Comments.Count > 0)
{
    Range commentStory = doc.StoryRanges[WdStoryType.wdCommentsStory];
}
```

### Iterating All Stories

```csharp
Range storyRange = doc.StoryRanges[WdStoryType.wdMainTextStory];
while (storyRange != null)
{
    // Process this story
    ProcessStory(storyRange);

    // Move to next linked story (for headers/footers across sections)
    try
    {
        storyRange = storyRange.NextStoryRange;
    }
    catch
    {
        storyRange = null;
    }
}
```

## Headers and Footers

Headers and footers are accessed differently - through Sections:

```csharp
foreach (Section section in doc.Sections)
{
    // Primary header (odd pages in different odd/even setup)
    HeaderFooter primaryHeader = section.Headers[WdHeaderFooterIndex.wdHeaderFooterPrimary];
    if (primaryHeader.Exists)
    {
        Range headerRange = primaryHeader.Range;
        // Process header
    }

    // First page header (if different first page enabled)
    HeaderFooter firstHeader = section.Headers[WdHeaderFooterIndex.wdHeaderFooterFirstPage];
    if (firstHeader.Exists)
    {
        Range firstHeaderRange = firstHeader.Range;
    }

    // Even pages header (if different odd/even enabled)
    HeaderFooter evenHeader = section.Headers[WdHeaderFooterIndex.wdHeaderFooterEvenPages];
    if (evenHeader.Exists)
    {
        Range evenHeaderRange = evenHeader.Range;
    }

    // Same pattern for Footers collection
}
```

## Text Boxes and Shapes

```csharp
foreach (Shape shape in doc.Shapes)
{
    // Check if shape has text
    if (shape.TextFrame.HasText != 0)
    {
        Range textRange = shape.TextFrame.TextRange;
        // Process text box content
    }
}

// Also check inline shapes (images with text wrapping)
foreach (InlineShape inline in doc.InlineShapes)
{
    if (inline.Type == WdInlineShapeType.wdInlineShapeLinkedPicture)
    {
        // Handle linked image
    }
}
```

## Complete Document Search Pattern

Process all text areas in a document:

```csharp
public void SearchAllStories(Document doc, string searchText)
{
    int totalMatches = 0;

    // 1. Main document body
    totalMatches += SearchInRange(doc.Content, searchText);

    // 2. Headers and Footers (via Sections)
    foreach (Section section in doc.Sections.Cast<Section>())
    {
        foreach (HeaderFooter header in section.Headers.Cast<HeaderFooter>())
        {
            if (header.Exists)
                totalMatches += SearchInRange(header.Range, searchText);
        }

        foreach (HeaderFooter footer in section.Footers.Cast<HeaderFooter>())
        {
            if (footer.Exists)
                totalMatches += SearchInRange(footer.Range, searchText);
        }
    }

    // 3. Footnotes
    if (doc.Footnotes.Count > 0)
        totalMatches += SearchInRange(
            doc.StoryRanges[WdStoryType.wdFootnotesStory], searchText);

    // 4. Endnotes
    if (doc.Endnotes.Count > 0)
        totalMatches += SearchInRange(
            doc.StoryRanges[WdStoryType.wdEndnotesStory], searchText);

    // 5. Comments
    if (doc.Comments.Count > 0)
        totalMatches += SearchInRange(
            doc.StoryRanges[WdStoryType.wdCommentsStory], searchText);

    // 6. Text boxes and shapes
    foreach (Shape shape in doc.Shapes.Cast<Shape>())
    {
        if (shape.TextFrame.HasText != 0)
            totalMatches += SearchInRange(shape.TextFrame.TextRange, searchText);
    }
}
```

## Story Linking

Some stories are "linked" across sections (headers/footers linked to previous section):

```csharp
// Check if header is linked to previous section
if (section.Headers[WdHeaderFooterIndex.wdHeaderFooterPrimary].LinkToPrevious)
{
    // This header uses previous section's content
    // Modifying it affects both sections
}

// Unlink to make independent
section.Headers[WdHeaderFooterIndex.wdHeaderFooterPrimary].LinkToPrevious = false;
```

## Common Issues

| Issue | Solution |
|-------|----------|
| StoryRanges throws if story empty | Check collection count first |
| Headers not found | Check `header.Exists` property |
| Can't access footnote story | Verify `doc.Footnotes.Count > 0` |
| Text frame has no text | Check `HasText != 0` |
| Linked header changes affect multiple sections | Unlink before modifying |
