---
name: Debugging Techniques
description: Use when debugging VSTO add-in issues, COM interop problems, Word automation failures, or systematic bug investigation. Covers add-in loading failures, manifest errors, RCW tracking, Office event logs, and VSTO runtime diagnostics.
version: 0.2.0
load: on-demand
---

# Debugging Techniques

Systematic debugging for VSTO add-ins with Office-specific diagnostic patterns.

## VSTO Add-in Loading Failures

### Check LoadBehavior Registry

```powershell
# Check if add-in is enabled
$path = "HKCU:\Software\Microsoft\Office\Word\Addins\YourAddIn"
Get-ItemProperty -Path $path | Select LoadBehavior

# LoadBehavior values:
# 0 = Disabled (crashed)
# 1 = Disabled by user
# 2 = Enabled (load on startup)
# 3 = Load on demand
# 16 = Connected (COM add-in loaded)

# Reset crashed add-in:
Set-ItemProperty -Path $path -Name "LoadBehavior" -Value 3
```

### VSTO Runtime Logging

```powershell
# Enable VSTO runtime logging
$regPath = "HKCU:\Software\Microsoft\VSTO"
New-Item -Path $regPath -Force
Set-ItemProperty -Path $regPath -Name "EnableLogging" -Value 1
Set-ItemProperty -Path $regPath -Name "LogFile" -Value "C:\temp\vsto.log"
# Restart Word, reproduce issue, check log
```

### Manifest Validation

```bash
# Check manifest for errors
mage -verify YourAddIn.vsto

# Common manifest errors:
# - Certificate expired/untrusted
# - Wrong deployment URL
# - Missing dependency
# - Version mismatch
```

## COM Interop Debugging

### RCW Reference Tracking

```csharp
// Track COM object lifecycle
public static class ComDebug
{
    public static void TrackRelease(object comObj, string name)
    {
        if (comObj == null) return;
        var remaining = Marshal.ReleaseComObject(comObj);
        Debug.WriteLine($"[COM] Released {name}, refs remaining: {remaining}");
    }
}

// Usage in debugging:
var range = doc.Range(0, 10);
// ... use range ...
ComDebug.TrackRelease(range, "doc.Range");  // Should show 0
```

### Two-Dot Rule Violations

```csharp
// PROBLEM: Intermediate COM objects leak
doc.Paragraphs[1].Range.Text = "Hello";  // 2 leaked objects!

// DEBUG: Break into components to find leak
var paragraphs = doc.Paragraphs;           // Track this
var para = paragraphs[1];                  // Track this
var range = para.Range;                    // Track this
range.Text = "Hello";

// Release in reverse order
Marshal.ReleaseComObject(range);
Marshal.ReleaseComObject(para);
Marshal.ReleaseComObject(paragraphs);
```

### Orphaned RCW Detection

```csharp
// Force GC to find leaks (debugging only!)
GC.Collect();
GC.WaitForPendingFinalizers();
GC.Collect();

// Check for COM exceptions after this - indicates leaked objects
```

## Word Process Debugging

### Attach to WINWORD.exe

```
Visual Studio → Debug → Attach to Process → WINWORD.exe
- Enable "Managed" and "Native" code debugging
- Set breakpoints in add-in code
- Reproduce issue in Word
```

### Office Event Viewer Logs

```powershell
# Check Office-specific logs
Get-WinEvent -LogName "OAlerts" -MaxEvents 20 |
    Where-Object { $_.Message -like "*add-in*" }

# Application log for crashes
Get-WinEvent -LogName "Application" -MaxEvents 50 |
    Where-Object { $_.ProviderName -eq "Microsoft Office" }
```

### ClickOnce Deployment Errors

```powershell
# Check ClickOnce cache
Get-ChildItem "$env:LOCALAPPDATA\Apps\2.0" -Recurse -Filter "*.vsto"

# Clear corrupted cache (last resort!)
# rd /s /q "%LOCALAPPDATA%\Apps\2.0"
```

## Systematic Debugging (Agans' Rules Applied)

### 1. Make It Fail Consistently

```
VSTO-specific reproduction:
├── Fresh Word instance? (File → Exit, reopen)
├── Same document type? (.docx vs .doc vs .dotx)
├── Same user profile? (Admin vs standard)
├── Same Office version? (32-bit vs 64-bit)
└── Ribbon callback timing? (add delay to test)
```

### 2. Divide and Conquer

```csharp
// Isolate which layer fails
public void DebugHandler()
{
    Debug.WriteLine("1. Handler entered");

    var doc = Application.ActiveDocument;
    Debug.WriteLine($"2. Got document: {doc != null}");

    var range = doc.Range(0, 10);
    Debug.WriteLine($"3. Got range: {range != null}");

    range.Text = "Test";
    Debug.WriteLine("4. Set text - SUCCESS");
}
// Check output to find where it breaks
```

### 3. Check the Plug (VSTO Edition)

| "Plug" | Check Command |
|--------|---------------|
| Add-in loaded? | `Application.COMAddIns["YourAddIn"].Connect` |
| Trust settings? | File → Options → Trust Center |
| Correct Office bitness? | `IntPtr.Size == 4 ? "32-bit" : "64-bit"` |
| Document protected? | `doc.ProtectionType != wdNoProtection` |
| Macro security? | Trust Center → Macro Settings |

## Common VSTO Bug Patterns

| Pattern | Symptoms | Debug Approach |
|---------|----------|----------------|
| **Add-in disabled** | Ribbon missing | Check LoadBehavior registry |
| **COM leak** | Word hangs on exit | RCW tracking, two-dot audit |
| **Ribbon callback crash** | Button does nothing | Wrap in try-catch, check Output |
| **Document timing** | Null reference | Check `Application.Documents.Count` |
| **Cross-thread** | "Wrong thread" | Ensure UI thread for Office calls |
| **Manifest trust** | Won't install | Check certificate, ClickOnce logs |

## Quick Diagnostic Checklist

```markdown
## VSTO Debugging Session

### Environment
- [ ] Office version: ___
- [ ] Office bitness: 32/64
- [ ] Add-in LoadBehavior: ___

### Reproduction
- [ ] Consistent? Y/N
- [ ] Document type: ___
- [ ] User profile: ___

### Checked
- [ ] Event Viewer logs
- [ ] VSTO runtime log enabled
- [ ] LoadBehavior registry
- [ ] Trust Center settings
- [ ] COM object disposal

### Findings
- Root cause: ___
- Fix: ___
```

## References

- **`references/vsto-diagnostics.md`** - Extended diagnostic commands
- **`references/com-debugging.md`** - Deep COM interop debugging
