// Content Control Pattern
// From: OfficeAddins/src/VSTO/WordAddIn/Interop/WordObjectModelDocumentContentControl.cs
// Demonstrates: ContentControl navigation, Range.Information, parent/child relationships

using Microsoft.Office.Interop.Word;
using Range = Microsoft.Office.Interop.Word.Range;
using WordContentControl = ContentControl;

namespace WordAddIn.Interop;

public class WordObjectModelDocumentContentControl(IWordDocumentContext documentContext)
{
    // CHECK IF RANGE IS IN CONTENT CONTROL
    public bool IsInContentControl(IDocumentRange range)
    {
        if (range is not WordDocumentRange wordRange)
            return false;

        try
        {
            // WdInformation.wdInContentControl returns bool
            return (bool)wordRange.Range.Information[WdInformation.wdInContentControl];
        }
        catch (Exception)
        {
            return false;  // COM exceptions = not in CC
        }
    }

    // NAVIGATE TO NEXT CONTENT CONTROL
    public IDocumentRange? GetNextContentControl(IDocumentRange? documentRange)
    {
        try
        {
            var document = documentContext.Document;
            if (document?.ContentControls.Count == 0)
                return null;

            // Search from current position to end
            var searchRange = document!.Range(documentRange?.Start ?? 0, document.Content.End);

            foreach (var cc in searchRange.ContentControls.Cast<WordContentControl>())
                if (cc.Range.Start > (documentRange?.Start ?? 0))
                    return new WordDocumentRange(cc.Range);
        }
        catch (Exception)
        {
            // Handle COM exceptions gracefully
        }

        return null;
    }

    // GET CONTENT CONTROL FROM RANGE
    public Result<WordContentControl, Error> GetContentControlFromRange(Range range)
    {
        // Not in a content control
        if (!(bool)range.Information[WdInformation.wdInContentControl])
            return DomainErrors.ContentControls.NotInContentControl;

        // Multiple CCs selected = ambiguous
        if (range.ContentControls.Count > 1)
            return DomainErrors.ContentControls.TooManySelected;

        // CC selected directly (not text inside)
        if (range.ContentControls.Count == 1 &&
            range.ParentContentControl is null &&
            range.ContentControls[1].Range.InRange(range))
            return Result.Success<WordContentControl, Error>(range.ContentControls[1]);

        // Text inside CC
        if (range.ParentContentControl is not null)
            return Result.Success<WordContentControl, Error>(range.ParentContentControl);

        return DomainErrors.ContentControls.RangeNotInContentControl;
    }

    // REMOVE SQUARE BRACKETS FROM CC TEXT
    public void RemoveSquareBrackets(DomainContentControl contentControl)
    {
        try
        {
            var wordCC = GetWordContentControl(contentControl);
            if (wordCC is null) return;

            if (!contentControl.ShowingPlaceholderText &&
                wordCC.Range.Text?.StartsWith("[") == true &&
                wordCC.Range.Text?.EndsWith("]") == true)
            {
                // Delete last char first (doesn't shift Start position)
                wordCC.Range.Characters.Last.Delete();
                wordCC.Range.Characters.First.Delete();
                wordCC.Range.Select();
            }
        }
        catch (Exception)
        {
            // Log and continue
        }
    }
}

// KEY PATTERNS:
// 1. Range.Information[WdInformation.X] for range properties
// 2. ParentContentControl for containing CC
// 3. ContentControls collection iteration with Cast<>
// 4. Range.InRange() checks if one range contains another
// 5. Characters.Last/First for single-char operations
// 6. Delete last before first to avoid position shifts
// 7. Null checks on Range.Text (empty ranges return null)
