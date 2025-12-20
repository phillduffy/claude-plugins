// Find & Replace Pattern
// From: OfficeAddins/src/VSTO/WordAddIn/Interop/WordObjectModelTextReplacer.cs
// Demonstrates: Find.Execute, Range handling, document structure traversal

using System.Runtime.InteropServices;
using Microsoft.Office.Interop.Word;
using Range = Microsoft.Office.Interop.Word.Range;

namespace WordAddIn.Interop;

public class WordObjectModelTextReplacer(IWordDocumentContext documentContext)
{
    public TextReplacementResult ReplaceAll(TextReplacementSpecification parameters)
    {
        var activeDocument = documentContext.Document;
        var totalReplacements = 0;

        // SAVE SELECTION - restore user context after operation
        var originalSelection = documentContext.Application.Selection.Range;

        try
        {
            // MAIN DOCUMENT - StoryRanges for main text
            totalReplacements += ReplaceInRange(activeDocument.Content, parameters);

            // HEADERS & FOOTERS - iterate sections
            foreach (var section in activeDocument.Sections.Cast<Section>())
            {
                foreach (var header in section.Headers.Cast<HeaderFooter>())
                    if (header.Exists)
                        totalReplacements += ReplaceInRange(header.Range, parameters);

                foreach (var footer in section.Footers.Cast<HeaderFooter>())
                    if (footer.Exists)
                        totalReplacements += ReplaceInRange(footer.Range, parameters);
            }

            // FOOTNOTES & ENDNOTES - via StoryRanges
            if (activeDocument.Footnotes.Count > 0)
                totalReplacements += ReplaceInRange(
                    activeDocument.StoryRanges[WdStoryType.wdFootnotesStory], parameters);

            if (activeDocument.Endnotes.Count > 0)
                totalReplacements += ReplaceInRange(
                    activeDocument.StoryRanges[WdStoryType.wdEndnotesStory], parameters);

            // TEXT BOXES & SHAPES
            foreach (var shape in activeDocument.Shapes.Cast<Shape>())
                if (shape.TextFrame.HasText != 0)
                    totalReplacements += ReplaceInRange(shape.TextFrame.TextRange, parameters);

            return new TextReplacementResult(totalReplacements);
        }
        finally
        {
            // RESTORE SELECTION - always restore user context
            try { originalSelection.Select(); }
            catch { activeDocument.Range(0, 0).Select(); }
        }
    }

    private int ReplaceInRange(Range range, TextReplacementSpecification parameters)
    {
        // DUPLICATE RANGE - don't modify original
        var tempRange = range.Duplicate;
        tempRange.Collapse(WdCollapseDirection.wdCollapseStart);

        // CONFIGURE FIND - clear previous settings
        var findObject = tempRange.Find;
        findObject.ClearFormatting();
        findObject.Text = parameters.FindText;
        findObject.Forward = true;
        findObject.Wrap = WdFindWrap.wdFindStop;  // Don't wrap around
        findObject.MatchCase = parameters.MatchCase;
        findObject.MatchWholeWord = parameters.MatchWholeWord;
        findObject.MatchWildcards = parameters.UseWildcards;

        // CONFIGURE REPLACEMENT
        findObject.Replacement.ClearFormatting();
        findObject.Replacement.Text = parameters.ReplacementText;

        // EXECUTE REPLACEMENT
        object replaceAll = WdReplace.wdReplaceAll;
        findObject.Execute(Replace: ref replaceAll);

        // RELEASE COM OBJECTS
        Marshal.ReleaseComObject(findObject);
        Marshal.ReleaseComObject(tempRange);

        return 1; // Simplified - actual counts matches
    }
}

// KEY PATTERNS:
// 1. Save/restore Selection - don't disrupt user context
// 2. Range.Duplicate before modification
// 3. Find.ClearFormatting() to reset previous settings
// 4. WdFindWrap.wdFindStop prevents infinite loops
// 5. StoryRanges for footnotes/endnotes access
// 6. TextFrame.HasText check before accessing TextRange
// 7. Marshal.ReleaseComObject in finally blocks
