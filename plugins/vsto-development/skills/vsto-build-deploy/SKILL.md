---
name: VSTO Build and Deployment
description: This skill should be used when the user asks about "MSBuild VSTO", "dotnet build VSTO", "ClickOnce", "VSTO manifest", "Office add-in deployment", "VSTO signing", "add-in loading", "VSTO troubleshooting", "Office trust", "registry add-in", or any build, deployment, or configuration topics for VSTO projects.
version: 0.1.0
---

# VSTO Build and Deployment

Build commands, deployment strategies, and troubleshooting for VSTO add-in projects.

## Critical: MSBuild Required

**VSTO projects require MSBuild.** The `dotnet build` command does NOT work.

```bash
# Correct - use MSBuild
msbuild.exe Solution.sln -verbosity:minimal -nologo

# WRONG - will fail
dotnet build Solution.sln
```

**Why?** VSTO projects target .NET Framework (not .NET Core/5+) and use Office-specific build targets that MSBuild provides.

## Build Commands

### Solution Build

```bash
# Full solution build
msbuild.exe OfficeAddins.sln -verbosity:minimal -nologo

# Specific configuration
msbuild.exe OfficeAddins.sln -p:Configuration=Release -nologo

# Clean build
msbuild.exe OfficeAddins.sln -t:Clean,Build -nologo
```

### Single Project Build

```bash
# Build specific project
msbuild.exe "src\WordAddIn\WordAddIn.csproj" -verbosity:minimal -nologo
```

### Flag Syntax (CRITICAL)

Use **dash-style flags** (`-flag`), NOT slash-style (`/flag`):

```bash
# CORRECT - dash flags
msbuild.exe Solution.sln -verbosity:minimal

# WRONG - Nushell/MINGW converts /flag to paths
msbuild.exe Solution.sln /verbosity:minimal  # Becomes C:\verbosity:minimal
```

## Testing

Tests CAN use `dotnet test`:

```bash
# Run all tests
dotnet test

# Specific test project
dotnet test tests\WordAddIn.Tests\WordAddIn.Tests.csproj

# With filter
dotnet test --filter "Category=Unit"
```

## Project Structure

```
Solution.sln
├── src/
│   ├── Core.Domain/             # .NET Standard (domain logic)
│   ├── Core.Application/        # .NET Standard (use cases)
│   ├── Infrastructure.*/        # .NET Framework (implementations)
│   └── WordAddIn/               # VSTO add-in project
│       ├── WordAddIn.csproj     # VSTO project file
│       ├── ThisAddIn.cs         # Entry point
│       ├── WordAddIn.vsto       # Deployment manifest (generated)
│       └── WordAddIn.dll.manifest  # Application manifest (generated)
└── tests/
    └── WordAddIn.Tests/         # Test project
```

## Manifest Files

VSTO generates two manifest files during build:

### Application Manifest (*.dll.manifest)

Describes the add-in assembly, permissions, and dependencies.

```xml
<assembly manifestVersion="1.0">
  <assemblyIdentity name="WordAddIn" version="1.0.0.0" />
  <application />
  <entryPoint>
    <assemblyIdentity name="WordAddIn" version="1.0.0.0" />
    <commandLine file="WordAddIn.dll" parameters="" />
  </entryPoint>
</assembly>
```

### Deployment Manifest (*.vsto)

Describes deployment location and update behavior.

```xml
<assembly manifestVersion="1.0">
  <assemblyIdentity name="WordAddIn.vsto" version="1.0.0.0" />
  <description publisher="Company" product="Word Add-in" />
  <deployment install="true">
    <subscription>
      <update beforeApplicationStartup="true" />
    </subscription>
  </deployment>
  <dependency>
    <dependentAssembly>
      <assemblyIdentity name="WordAddIn.dll" version="1.0.0.0" />
    </dependentAssembly>
  </dependency>
</assembly>
```

## Signing

### Code Signing Certificate

VSTO add-ins should be signed for trusted deployment:

```xml
<!-- In .csproj -->
<PropertyGroup>
  <SignManifests>true</SignManifests>
  <ManifestKeyFile>YourCert.pfx</ManifestKeyFile>
  <ManifestCertificateThumbprint>ABC123...</ManifestCertificateThumbprint>
</PropertyGroup>
```

### Strong Naming

```xml
<PropertyGroup>
  <SignAssembly>true</SignAssembly>
  <AssemblyOriginatorKeyFile>YourKey.snk</AssemblyOriginatorKeyFile>
</PropertyGroup>
```

## Deployment Options

### 1. ClickOnce (Recommended)

Publish via Visual Studio or MSBuild:

```bash
msbuild.exe WordAddIn.csproj -t:Publish -p:PublishDir=\\server\share\
```

Users install by running the `.vsto` file.

### 2. Registry Deployment

For enterprise deployment, register via registry:

```
HKEY_CURRENT_USER\Software\Microsoft\Office\Word\Addins\YourAddIn
  Description = "Your Add-in"
  FriendlyName = "Your Add-in Name"
  LoadBehavior = 3
  Manifest = file:///C:/path/to/WordAddIn.vsto|vstolocal
```

**LoadBehavior values:**
| Value | Meaning |
|-------|---------|
| 0 | Do not load |
| 1 | Load on demand |
| 2 | Load at startup, currently unloaded |
| 3 | Load at startup (default) |
| 8 | Load on demand, first time |
| 9 | Load on demand, currently unloaded |
| 16 | Load first time, then on demand |

### 3. Windows Installer (MSI)

Create MSI for IT deployment:
- Use WiX Toolset or InstallShield
- Include prerequisites detection
- Handle registry entries
- Support silent installation

## Trust and Security

### Trust Locations

Add-ins must be trusted. Options:

1. **Code signing certificate** in Trusted Publishers
2. **Inclusion list** (user prompt on first run)
3. **Group Policy** for enterprise

### Trusted Locations

Word trusts specific paths:

```
HKEY_CURRENT_USER\Software\Microsoft\Office\16.0\Word\Security\Trusted Locations\Location1
  Path = C:\Trusted\AddIns\
  AllowSubfolders = 1
```

### Enabling VSTO in Word

```
File → Options → Trust Center → Trust Center Settings
→ Add-ins → Uncheck "Require signed add-ins"
```

## Troubleshooting

### Add-in Not Loading

1. **Check Windows Event Viewer**
   - Application Log → VSTO errors
   - Look for ClickOnce deployment errors

2. **Check Office Trust**
   - File → Options → Trust Center
   - View disabled add-ins

3. **Check Registry**
   ```bash
   reg query "HKCU\Software\Microsoft\Office\Word\Addins" /s
   ```

4. **Enable VSTO Logging**
   ```
   HKEY_CURRENT_USER\Software\Microsoft\VSTO\Addins\YourAddIn
     LogVerbosity = "Verbose"
   ```

### Common Errors

| Error | Cause | Fix |
|-------|-------|-----|
| "This add-in is disabled" | Crashed previously | Enable in Trust Center |
| "Manifest parsing error" | Bad .vsto file | Rebuild, check signing |
| "Required trust not granted" | No certificate | Sign or add to Trusted Publishers |
| "Could not load file" | Missing dependency | Check bin folder, GAC |
| "Add-in not in list" | Registry issue | Re-register via setup |

### Debug vs Release

Development typically uses Debug with "vstolocal":

```xml
<!-- Manifest path for development -->
Manifest = file:///path/to/bin/Debug/WordAddIn.vsto|vstolocal
```

Remove `|vstolocal` for production deployment.

## CI/CD Integration

### GitHub Actions Example

```yaml
env:
  MSBUILD_PATH: C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe

jobs:
  build:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup MSBuild
        uses: microsoft/setup-msbuild@v1

      - name: Restore NuGet
        run: nuget restore Solution.sln

      - name: Build
        run: msbuild.exe Solution.sln -p:Configuration=Release -nologo

      - name: Test
        run: dotnet test --configuration Release
```

### Azure DevOps Pipeline

```yaml
pool:
  vmImage: 'windows-latest'

steps:
- task: NuGetToolInstaller@1

- task: NuGetCommand@2
  inputs:
    restoreSolution: '**/*.sln'

- task: VSBuild@1
  inputs:
    solution: '**/*.sln'
    msbuildArgs: '/p:Configuration=Release'
    platform: 'Any CPU'

- task: VSTest@2
  inputs:
    testSelector: 'testAssemblies'
    testAssemblyVer2: '**\*Tests.dll'
```

## Quick Commands Reference

| Task | Command |
|------|---------|
| Build solution | `msbuild.exe Solution.sln -nologo` |
| Release build | `msbuild.exe Solution.sln -p:Configuration=Release` |
| Clean + Build | `msbuild.exe Solution.sln -t:Clean,Build` |
| Run tests | `dotnet test` |
| Find .vsto | `fd vsto$` |
| Find csproj | `fd csproj$` |
| Check add-in registry | `reg query "HKCU\...\Addins"` |

## Additional Resources

### Reference Files

- **`references/deployment-checklist.md`** - Pre-deployment verification steps
- **`references/ci-cd-templates.md`** - Complete CI/CD pipeline templates
