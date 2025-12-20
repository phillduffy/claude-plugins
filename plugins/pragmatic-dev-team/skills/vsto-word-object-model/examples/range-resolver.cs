// Range Resolution Pattern
// From: OfficeAddins/src/VSTO/WordAddIn/Features/Scripting/WordRangeResolver.cs
// Demonstrates: StoryRanges, Paragraphs collection, COM lifecycle management

using System.Runtime.InteropServices;
using Microsoft.Office.Interop.Word;
using Range = Microsoft.Office.Interop.Word.Range;

namespace WordAddIn.Features.Scripting;

/// <summary>
/// Resolves ranges from command specifications.
/// Centralizes COM object lifecycle management.
/// </summary>
public class WordRangeResolver(IWordDocumentContext documentContext)
{
    public IEnumerable<WordDocumentRange> ResolveRanges(CommandRanges rangeType)
    {
        Range[] initialRanges;
        Paragraphs? paragraphs = null;

        try
        {
            initialRanges = rangeType switch
            {
                // DOCUMENT RANGE - main text story
                CommandRanges.Document =>
                    [documentContext.Document.StoryRanges[WdStoryType.wdMainTextStory]],

                // SELECTION RANGE - current user selection
                CommandRanges.Selection =>
                    [documentContext.Application.Selection.Range],

                // PARAGRAPH RANGES - iterate and collect
                CommandRanges.Paragraphs =>
                    GetAllParagraphRanges(out paragraphs),

                CommandRanges.FirstParagraph =>
                    GetFirstParagraphRange(out paragraphs),

                CommandRanges.LastParagraph =>
                    GetLastParagraphRange(out paragraphs),

                _ => []
            };
        }
        finally
        {
            // RELEASE PARAGRAPHS COLLECTION - even if exception
            if (paragraphs is not null)
                Marshal.ReleaseComObject(paragraphs);
        }

        return initialRanges.Select(r => new WordDocumentRange(r)).ToList();
    }

    private Range[] GetFirstParagraphRange(out Paragraphs? paragraphs)
    {
        paragraphs = documentContext.Application.Selection.Paragraphs;
        return [paragraphs.First.Range];
    }

    private Range[] GetLastParagraphRange(out Paragraphs? paragraphs)
    {
        paragraphs = documentContext.Application.Selection.Paragraphs;
        return [paragraphs.Last.Range];
    }

    private Range[] GetAllParagraphRanges(out Paragraphs? paragraphs)
    {
        paragraphs = documentContext.Application.Selection.Paragraphs;
        var ranges = new List<Range>();

        // ITERATE AND RELEASE - release each Paragraph after getting Range
        foreach (var para in paragraphs.Cast<Paragraph>())
        {
            ranges.Add(para.Range);
            Marshal.ReleaseComObject(para);
        }

        return ranges.ToArray();
    }

    // HANDLE TRAILING PARAGRAPH MARKS
    private static void HandleWordRangeEnd(Range[] ranges)
    {
        foreach (var r in ranges)
        {
            Words? words = null;
            Range? lastWord = null;

            try
            {
                words = r.Words;
                lastWord = words.Last;

                // Skip trailing paragraph mark
                if (string.Equals(lastWord.Text, "\r", StringComparison.Ordinal))
                    r.MoveEnd(WdUnits.wdCharacter, -1);

                r.Collapse(WdCollapseDirection.wdCollapseEnd);
            }
            finally
            {
                if (lastWord is not null) Marshal.ReleaseComObject(lastWord);
                if (words is not null) Marshal.ReleaseComObject(words);
            }
        }
    }
}

// KEY PATTERNS:
// 1. StoryRanges[WdStoryType.X] for document structure access
// 2. Selection.Paragraphs for paragraph iteration
// 3. out parameter for returning Paragraphs to release later
// 4. Release each Paragraph in loop, collection in finally
// 5. Words.Last to check for paragraph mark (\r)
// 6. MoveEnd with negative value to shrink range
// 7. Collapse to convert range to insertion point
