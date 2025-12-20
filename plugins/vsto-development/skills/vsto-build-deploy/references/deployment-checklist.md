# VSTO Deployment Checklist

Pre-deployment verification steps for VSTO add-in releases.

## Pre-Build Verification

### Code Quality

- [ ] All tests pass (`dotnet test`)
- [ ] No compiler warnings in Release mode
- [ ] Code reviewed and approved
- [ ] Version number updated in AssemblyInfo.cs

### Dependencies

- [ ] All NuGet packages restored
- [ ] No preview/beta packages in production
- [ ] Third-party licenses reviewed
- [ ] Dependency conflicts resolved

### Security

- [ ] Code signing certificate valid (not expired)
- [ ] Strong name key available
- [ ] No hardcoded secrets in code
- [ ] Sensitive data encrypted

## Build Process

### Release Build

```bash
# Clean and rebuild in Release mode
msbuild.exe Solution.sln -t:Clean,Build -p:Configuration=Release -nologo

# Verify output
ls bin/Release/*.dll
ls bin/Release/*.vsto
ls bin/Release/*.manifest
```

### Output Verification

- [ ] Main assembly (WordAddIn.dll) present
- [ ] Application manifest (*.dll.manifest) present
- [ ] Deployment manifest (*.vsto) present
- [ ] All dependency DLLs included
- [ ] PDB files generated (for crash reporting)

### Manifest Verification

- [ ] .vsto file opens in text editor without errors
- [ ] Version numbers match across manifests
- [ ] Publisher information correct
- [ ] Update location URL correct (if applicable)

## Signing

### Code Signing

```bash
# Verify signature
signtool verify /pa bin/Release/WordAddIn.dll

# Check manifest signature
mage -verify bin/Release/WordAddIn.vsto
```

- [ ] DLL is signed with code signing certificate
- [ ] Manifests are signed
- [ ] Certificate not expired
- [ ] Certificate trusted on target machines

### Strong Naming

```bash
# Verify strong name
sn -vf bin/Release/WordAddIn.dll
```

- [ ] Assembly has strong name
- [ ] Strong name matches expected key

## ClickOnce Deployment

### Publish

```bash
# Publish to folder
msbuild.exe WordAddIn.csproj -t:Publish -p:PublishDir=\\server\share\WordAddIn\ -p:Configuration=Release
```

### Verify Publish Output

- [ ] setup.exe present (if bootstrapper enabled)
- [ ] Application files folder present
- [ ] .vsto file in root
- [ ] Version folder with correct files

### Web Server Deployment

- [ ] MIME types configured:
  - `.vsto` → `application/x-ms-vsto`
  - `.manifest` → `application/x-ms-manifest`
  - `.deploy` → `application/octet-stream`
- [ ] HTTPS certificate valid
- [ ] Files accessible from client machines

## Registry Deployment

### Verify Registry Entries

```bash
# Check current user registration
reg query "HKCU\Software\Microsoft\Office\Word\Addins\YourAddIn"
```

Expected values:
```
Description     REG_SZ    Your Add-in Description
FriendlyName    REG_SZ    Your Add-in
LoadBehavior    REG_DWORD 0x3
Manifest        REG_SZ    file:///C:/path/to/WordAddIn.vsto|vstolocal
```

- [ ] Registry key created correctly
- [ ] LoadBehavior = 3 (load at startup)
- [ ] Manifest path correct
- [ ] `|vstolocal` removed for network deployment

## MSI Deployment

### Installer Verification

- [ ] MSI installs without errors
- [ ] Prerequisites detected and installed
- [ ] Add-in loads after installation
- [ ] Uninstall removes all files and registry entries
- [ ] Upgrade from previous version works
- [ ] Silent install tested (`msiexec /i AddIn.msi /quiet`)

### Prerequisites

- [ ] .NET Framework version detected
- [ ] VSTO Runtime installed
- [ ] Office version compatible
- [ ] Previous versions handled (upgrade/conflict)

## Post-Deployment Testing

### Fresh Installation

1. [ ] Clean machine (no previous add-in)
2. [ ] Install add-in
3. [ ] Launch Word
4. [ ] Verify add-in appears in Add-ins list
5. [ ] Test core functionality
6. [ ] Verify ribbon/UI elements appear

### Upgrade Installation

1. [ ] Machine with previous version
2. [ ] Install new version
3. [ ] Verify version number updated
4. [ ] Settings/data preserved
5. [ ] No duplicate registry entries

### Error Handling

- [ ] Test behavior when Office not installed
- [ ] Test behavior with incompatible Office version
- [ ] Test network share unavailable (ClickOnce)
- [ ] Test user without admin rights

## Rollback Plan

### Preparation

- [ ] Previous version installer available
- [ ] Rollback procedure documented
- [ ] Registry cleanup script ready
- [ ] User communication plan ready

### Rollback Steps

1. Uninstall current version
2. Clean registry:
   ```bash
   reg delete "HKCU\Software\Microsoft\Office\Word\Addins\YourAddIn" /f
   ```
3. Install previous version
4. Verify functionality

## Monitoring

### Error Reporting

- [ ] Application Insights configured (if applicable)
- [ ] Error logging enabled
- [ ] Crash dump collection configured

### Health Checks

- [ ] Add-in load time acceptable
- [ ] Memory usage within limits
- [ ] No COM object leaks detected
- [ ] Event log clean (no VSTO errors)

## Documentation

### User-Facing

- [ ] Release notes prepared
- [ ] Installation guide updated
- [ ] Known issues documented
- [ ] FAQ updated

### Internal

- [ ] Deployment procedure documented
- [ ] Troubleshooting guide updated
- [ ] Support team briefed
- [ ] Rollback procedure tested

## Final Signoff

| Role | Name | Date | Signature |
|------|------|------|-----------|
| Developer | | | |
| QA | | | |
| Security | | | |
| Release Manager | | | |
