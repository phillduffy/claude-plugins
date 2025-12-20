# Word Find and Replace Patterns

Comprehensive reference for Word's Find and Replace functionality.

## Find Object Properties

| Property | Type | Description |
|----------|------|-------------|
| `Text` | string | Search text or pattern |
| `Forward` | bool | Search direction |
| `Wrap` | WdFindWrap | What happens at document end |
| `MatchCase` | bool | Case-sensitive matching |
| `MatchWholeWord` | bool | Match whole words only |
| `MatchWildcards` | bool | Enable wildcard patterns |
| `MatchSoundsLike` | bool | Phonetic matching |
| `MatchAllWordForms` | bool | Match word variations |
| `Format` | bool | Include formatting in search |
| `Replacement.Text` | string | Replacement text |

## WdFindWrap Values

| Value | Description |
|-------|-------------|
| `wdFindStop` | Stop at range end (recommended) |
| `wdFindContinue` | Continue from beginning |
| `wdFindAsk` | Prompt user at end |

## Basic Patterns

### Simple Text Search

```csharp
var find = range.Find;
find.ClearFormatting();
find.Text = "search text";
find.Forward = true;
find.Wrap = WdFindWrap.wdFindStop;

while (find.Execute())
{
    // range now contains the match
    Console.WriteLine($"Found at {range.Start}-{range.End}");
}
```

### Find and Replace All

```csharp
var find = range.Find;
find.ClearFormatting();
find.Text = "old";
find.Replacement.ClearFormatting();
find.Replacement.Text = "new";
find.Wrap = WdFindWrap.wdFindStop;
find.Forward = true;

find.Execute(Replace: WdReplace.wdReplaceAll);
```

### Count Matches

```csharp
int count = 0;
var tempRange = range.Duplicate;
tempRange.Collapse(WdCollapseDirection.wdCollapseStart);
var find = tempRange.Find;
find.Text = "pattern";
find.Forward = true;
find.Wrap = WdFindWrap.wdFindStop;

while (find.Execute())
{
    if (tempRange.Start >= range.Start && tempRange.End <= range.End)
        count++;
    else
        break;
}
```

## Wildcard Patterns

Enable with `find.MatchWildcards = true`.

### Character Wildcards

| Pattern | Matches |
|---------|---------|
| `?` | Any single character |
| `*` | Any sequence of characters |
| `@` | One or more of preceding |
| `<` | Word start |
| `>` | Word end |

### Character Classes

| Pattern | Matches |
|---------|---------|
| `[abc]` | Any char in set |
| `[!abc]` | Any char NOT in set |
| `[a-z]` | Any char in range |
| `[!0-9]` | Not a digit |
| `[A-Za-z]` | Any letter |

### Repetition

| Pattern | Matches |
|---------|---------|
| `{n}` | Exactly n times |
| `{n,}` | n or more times |
| `{n,m}` | Between n and m times |

### Common Wildcard Examples

```csharp
// Match 3 digits
find.MatchWildcards = true;
find.Text = "[0-9]{3}";

// Match word starting with "pre"
find.Text = "<pre*>";

// Match date pattern (MM/DD/YYYY)
find.Text = "[0-9]{2}/[0-9]{2}/[0-9]{4}";

// Match phone number (XXX-XXX-XXXX)
find.Text = "[0-9]{3}-[0-9]{3}-[0-9]{4}";

// Match email-like pattern
find.Text = "<[A-Za-z0-9._%+-]@[A-Za-z0-9.-].[A-Za-z]{2,}>";

// Match any word in parentheses
find.Text = "\\(*\\)";
```

### Special Characters in Wildcards

| To Match | Use |
|----------|-----|
| `(` | `\\(` |
| `)` | `\\)` |
| `[` | `\\[` |
| `]` | `\\]` |
| `{` | `\\{` |
| `}` | `\\}` |
| `?` | `\\?` |
| `*` | `\\*` |
| `\` | `\\\\` |

## Special Characters (Non-Wildcard)

When `MatchWildcards = false`:

| Code | Meaning |
|------|---------|
| `^p` | Paragraph mark |
| `^t` | Tab character |
| `^l` | Line break (soft return) |
| `^n` | Column break |
| `^b` | Section break |
| `^m` | Page break |
| `^-` | Optional hyphen |
| `^~` | Non-breaking hyphen |
| `^s` | Non-breaking space |
| `^c` | Clipboard contents (replacement only) |
| `^d` | Field (find only) |
| `^g` | Graphic (find only) |
| `^f` | Footnote mark (find only) |
| `^e` | Endnote mark (find only) |
| `^w` | White space |
| `^?` | Any single character |
| `^#` | Any digit |
| `^$` | Any letter |

### Examples

```csharp
// Remove double paragraph marks
find.MatchWildcards = false;
find.Text = "^p^p";
find.Replacement.Text = "^p";
find.Execute(Replace: WdReplace.wdReplaceAll);

// Replace tabs with spaces
find.Text = "^t";
find.Replacement.Text = "    ";

// Find page breaks
find.Text = "^m";
```

## Formatting Search

```csharp
// Find bold text
find.ClearFormatting();
find.Font.Bold = 1;
find.Text = "";  // Match any bold text

// Find specific style
find.ClearFormatting();
find.Style = doc.Styles["Heading 1"];
find.Text = "";

// Replace with formatting
find.ClearFormatting();
find.Text = "important";
find.Replacement.ClearFormatting();
find.Replacement.Font.Bold = 1;
find.Replacement.Font.Color = WdColor.wdColorRed;
find.Execute(Replace: WdReplace.wdReplaceAll);
```

## Replacement Backreferences

With wildcards, use `\1`, `\2`, etc. for captured groups:

```csharp
find.MatchWildcards = true;

// Swap first and last name
find.Text = "(<[A-Za-z]@>) (<[A-Za-z]@>)";
find.Replacement.Text = "\\2 \\1";

// Add parentheses around numbers
find.Text = "([0-9]{1,})";
find.Replacement.Text = "(\\1)";

// Reformat date: 12/25/2024 -> 2024-12-25
find.Text = "([0-9]{2})/([0-9]{2})/([0-9]{4})";
find.Replacement.Text = "\\3-\\1-\\2";
```

## Replace Within Range Only

```csharp
private TextReplacementResult ReplaceInRange(Range range, string findText, string replaceText)
{
    var originalStart = range.Start;
    var originalEnd = range.End;

    // First pass: count matches in range
    int count = 0;
    var tempRange = range.Duplicate;
    tempRange.Collapse(WdCollapseDirection.wdCollapseStart);
    var tempFind = tempRange.Find;
    tempFind.Text = findText;
    tempFind.Forward = true;
    tempFind.Wrap = WdFindWrap.wdFindStop;

    while (tempFind.Execute())
    {
        if (tempRange.Start >= originalStart && tempRange.End <= originalEnd)
            count++;
        else
            break;
    }

    if (count == 0)
        return new TextReplacementResult(0);

    // Second pass: execute replacement
    var find = range.Find;
    find.Text = findText;
    find.Replacement.Text = replaceText;
    find.Execute(Replace: count == 1 ? WdReplace.wdReplaceOne : WdReplace.wdReplaceAll);

    return new TextReplacementResult(count);
}
```

## Common Issues

| Issue | Solution |
|-------|----------|
| Wildcards not working | Ensure `MatchWildcards = true` |
| Special chars not matching | Use correct escape sequences |
| Replace affects whole doc | Set `Wrap = wdFindStop` and use range |
| Format search persists | Call `ClearFormatting()` before each search |
| Replacement inserts field codes | Clear replacement formatting |
| Case not matching | Set `MatchCase` appropriately |
