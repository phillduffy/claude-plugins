# Working with Content Controls

Content Controls are structured document elements for data binding and form-like behavior.

## Content Control Types

| Type | Enum Value | Use Case |
|------|------------|----------|
| Rich Text | `wdContentControlRichText` | Formatted text |
| Plain Text | `wdContentControlText` | Single-line text |
| Picture | `wdContentControlPicture` | Image placeholder |
| Combo Box | `wdContentControlComboBox` | Dropdown with custom input |
| Drop-Down List | `wdContentControlDropdownList` | Dropdown, fixed options |
| Date Picker | `wdContentControlDate` | Date selection |
| Building Block Gallery | `wdContentControlBuildingBlockGallery` | Quick Parts |
| Checkbox | `wdContentControlCheckBox` | Boolean toggle |
| Repeating Section | `wdContentControlRepeatingSection` | Repeatable blocks |
| Group | `wdContentControlGroup` | Container for other controls |

## Accessing Content Controls

### By Collection

```csharp
// All controls in document
ContentControls controls = doc.ContentControls;

// Controls in specific range
ContentControls rangeControls = range.ContentControls;

// By index (1-based)
ContentControl first = controls[1];

// By ID (unique within document)
ContentControl byId = controls.Cast<ContentControl>()
    .FirstOrDefault(c => c.ID == "123456789");

// By tag
ContentControl byTag = controls.Cast<ContentControl>()
    .FirstOrDefault(c => c.Tag == "CompanyName");

// By title
ContentControl byTitle = controls.Cast<ContentControl>()
    .FirstOrDefault(c => c.Title == "Enter Name");
```

### Parent Content Control

Check if range is inside a content control:

```csharp
ContentControl? parentCC = null;
try
{
    parentCC = range.ParentContentControl;
    if (parentCC != null)
    {
        // Range is inside a content control
        var title = parentCC.Title;
        var isLocked = parentCC.LockContents;
    }
}
finally
{
    if (parentCC != null) Marshal.ReleaseComObject(parentCC);
}
```

## Content Control Properties

| Property | Description |
|----------|-------------|
| `ID` | Unique identifier (string) |
| `Tag` | Custom metadata string |
| `Title` | Display title (shown in control) |
| `Range` | The Range containing control content |
| `Type` | WdContentControlType |
| `LockContents` | Prevent editing |
| `LockContentControl` | Prevent deletion |
| `Temporary` | Removed when contents edited |
| `PlaceholderText` | Shown when empty |
| `XMLMapping` | Data binding to custom XML |
| `DateDisplayFormat` | For date controls |
| `DateDisplayLocale` | Locale for date display |
| `DateStorageFormat` | How date is stored |

## Reading Content Control Values

```csharp
public string GetContentControlValue(ContentControl cc)
{
    return cc.Type switch
    {
        WdContentControlType.wdContentControlCheckBox =>
            cc.Checked ? "true" : "false",

        WdContentControlType.wdContentControlDate =>
            cc.Range.Text,  // Formatted date

        WdContentControlType.wdContentControlDropdownList or
        WdContentControlType.wdContentControlComboBox =>
            cc.Range.Text,  // Selected option text

        _ => cc.Range.Text
    };
}
```

## Setting Content Control Values

```csharp
public void SetContentControlValue(ContentControl cc, string value)
{
    switch (cc.Type)
    {
        case WdContentControlType.wdContentControlCheckBox:
            cc.Checked = bool.TryParse(value, out var b) && b;
            break;

        case WdContentControlType.wdContentControlDate:
            // Set via range for date picker
            cc.Range.Text = value;
            break;

        case WdContentControlType.wdContentControlDropdownList:
        case WdContentControlType.wdContentControlComboBox:
            // Select matching option
            SelectDropdownOption(cc, value);
            break;

        default:
            cc.Range.Text = value;
            break;
    }
}

private void SelectDropdownOption(ContentControl cc, string value)
{
    var entries = cc.DropdownListEntries;
    for (int i = 1; i <= entries.Count; i++)
    {
        var entry = entries[i];
        if (entry.Text == value || entry.Value == value)
        {
            entry.Select();
            break;
        }
    }
}
```

## Creating Content Controls

```csharp
// Add at specific range
ContentControl cc = doc.ContentControls.Add(
    WdContentControlType.wdContentControlText,
    range);

// Configure
cc.Title = "Customer Name";
cc.Tag = "customer_name";
cc.PlaceholderText = "Enter customer name...";
cc.LockContentControl = true;  // Can't delete
cc.LockContents = false;       // Can edit contents
```

### Dropdown List

```csharp
var dropdown = doc.ContentControls.Add(
    WdContentControlType.wdContentControlDropdownList,
    range);

dropdown.Title = "Select Country";
dropdown.Tag = "country";

// Add options
dropdown.DropdownListEntries.Add("United States", "US");
dropdown.DropdownListEntries.Add("United Kingdom", "UK");
dropdown.DropdownListEntries.Add("Canada", "CA");

// Set default
dropdown.DropdownListEntries["US"].Select();
```

### Date Picker

```csharp
var datePicker = doc.ContentControls.Add(
    WdContentControlType.wdContentControlDate,
    range);

datePicker.Title = "Due Date";
datePicker.DateDisplayFormat = "MMMM d, yyyy";
datePicker.DateStorageFormat = WdContentControlDateStorageFormat.wdContentControlDateStorageDate;
datePicker.DateDisplayLocale = WdLanguageID.wdEnglishUS;
```

## Locking Behavior

| LockContentControl | LockContents | Behavior |
|-------------------|--------------|----------|
| false | false | Fully editable, deletable |
| true | false | Editable, cannot delete |
| false | true | Read-only, can delete |
| true | true | Fully locked |

## XML Mapping (Data Binding)

Bind to Custom XML Part:

```csharp
// Create custom XML part
var xmlPart = doc.CustomXMLParts.Add("<data><name></name></data>");

// Map content control to XML node
cc.XMLMapping.SetMapping("/data/name", "", xmlPart);

// Now cc.Range.Text syncs with XML
```

Read mapped value:

```csharp
if (cc.XMLMapping.IsMapped)
{
    var xmlNode = cc.XMLMapping.CustomXMLNode;
    var value = xmlNode.Text;
}
```

## Repeating Sections

```csharp
// Add repeating section container
var repeating = doc.ContentControls.Add(
    WdContentControlType.wdContentControlRepeatingSection,
    range);

// Each item in the section
repeating.RepeatingSectionItems.Count;

// Add new item
repeating.RepeatingSectionItemColl.Add();
```

## Iterating Safely

```csharp
var controls = doc.ContentControls;
try
{
    for (int i = 1; i <= controls.Count; i++)
    {
        var cc = controls[i];
        try
        {
            if (cc.Type == WdContentControlType.wdContentControlText)
            {
                ProcessTextControl(cc);
            }
        }
        finally
        {
            Marshal.ReleaseComObject(cc);
        }
    }
}
finally
{
    Marshal.ReleaseComObject(controls);
}
```

## Checking for Locked Content

Before modifying text, check if it's in a locked control:

```csharp
public bool CanModifyRange(Range range)
{
    ContentControl? cc = null;
    try
    {
        cc = range.ParentContentControl;
        if (cc != null && cc.LockContents)
        {
            return false;  // Cannot modify
        }
        return true;
    }
    finally
    {
        if (cc != null) Marshal.ReleaseComObject(cc);
    }
}
```

## Common Issues

| Issue | Solution |
|-------|----------|
| Control ID not unique | Word assigns, don't set manually |
| Can't delete control | Set `LockContentControl = false` |
| Range.Text empty | Check `PlaceholderText` or `ShowingPlaceholderText` |
| Dropdown has no selection | First entry is auto-selected |
| XML mapping fails | Verify XPath and namespace |
| Repeating section items | Iterate via `RepeatingSectionItems` collection |
