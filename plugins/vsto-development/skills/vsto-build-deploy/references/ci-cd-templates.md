# CI/CD Pipeline Templates for VSTO Projects

Complete pipeline templates for GitHub Actions and Azure DevOps.

## GitHub Actions

### Basic Build and Test

```yaml
# .github/workflows/ci.yml
name: CI

on:
  push:
    branches: [main, master]
  pull_request:
    branches: [main, master]

env:
  SOLUTION_FILE: OfficeAddins.sln
  CONFIGURATION: Release

jobs:
  build:
    runs-on: windows-latest

    steps:
      - name: Checkout
        uses: actions/checkout@v4
        with:
          fetch-depth: 0  # Full history for versioning

      - name: Setup MSBuild
        uses: microsoft/setup-msbuild@v2

      - name: Setup NuGet
        uses: NuGet/setup-nuget@v2

      - name: Restore NuGet packages
        run: nuget restore ${{ env.SOLUTION_FILE }}

      - name: Build
        run: msbuild ${{ env.SOLUTION_FILE }} -p:Configuration=${{ env.CONFIGURATION }} -nologo -verbosity:minimal

      - name: Test
        run: dotnet test --configuration ${{ env.CONFIGURATION }} --no-build --verbosity normal
```

### With Artifact Publishing

```yaml
# .github/workflows/build-release.yml
name: Build Release

on:
  push:
    tags:
      - 'v*'

env:
  SOLUTION_FILE: OfficeAddins.sln
  PROJECT_FILE: src/WordAddIn/WordAddIn.csproj
  CONFIGURATION: Release

jobs:
  build:
    runs-on: windows-latest

    steps:
      - name: Checkout
        uses: actions/checkout@v4
        with:
          fetch-depth: 0

      - name: Setup MSBuild
        uses: microsoft/setup-msbuild@v2

      - name: Setup NuGet
        uses: NuGet/setup-nuget@v2

      - name: Extract version from tag
        id: version
        run: echo "VERSION=${GITHUB_REF#refs/tags/v}" >> $GITHUB_OUTPUT
        shell: bash

      - name: Restore NuGet packages
        run: nuget restore ${{ env.SOLUTION_FILE }}

      - name: Build
        run: msbuild ${{ env.SOLUTION_FILE }} -p:Configuration=${{ env.CONFIGURATION }} -nologo

      - name: Test
        run: dotnet test --configuration ${{ env.CONFIGURATION }} --no-build

      - name: Publish ClickOnce
        run: msbuild ${{ env.PROJECT_FILE }} -t:Publish -p:Configuration=${{ env.CONFIGURATION }} -p:PublishDir=./publish/

      - name: Upload artifacts
        uses: actions/upload-artifact@v4
        with:
          name: WordAddIn-${{ steps.version.outputs.VERSION }}
          path: |
            src/WordAddIn/publish/**
            !src/WordAddIn/publish/**/*.pdb

      - name: Create Release
        uses: softprops/action-gh-release@v1
        with:
          files: |
            src/WordAddIn/publish/*.vsto
            src/WordAddIn/publish/setup.exe
          body: |
            ## Changes
            - Release version ${{ steps.version.outputs.VERSION }}
```

### With Code Signing

```yaml
# .github/workflows/signed-build.yml
name: Signed Build

on:
  push:
    tags:
      - 'v*'

jobs:
  build:
    runs-on: windows-latest

    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Setup MSBuild
        uses: microsoft/setup-msbuild@v2

      - name: Setup NuGet
        uses: NuGet/setup-nuget@v2

      - name: Import Code Signing Certificate
        env:
          CERTIFICATE_BASE64: ${{ secrets.CODE_SIGNING_CERT }}
          CERTIFICATE_PASSWORD: ${{ secrets.CODE_SIGNING_PASSWORD }}
        run: |
          $certBytes = [Convert]::FromBase64String($env:CERTIFICATE_BASE64)
          $certPath = Join-Path $env:RUNNER_TEMP "cert.pfx"
          [IO.File]::WriteAllBytes($certPath, $certBytes)
          Import-PfxCertificate -FilePath $certPath -CertStoreLocation Cert:\CurrentUser\My -Password (ConvertTo-SecureString -String $env:CERTIFICATE_PASSWORD -AsPlainText -Force)
        shell: pwsh

      - name: Restore
        run: nuget restore OfficeAddins.sln

      - name: Build with signing
        run: |
          msbuild OfficeAddins.sln -p:Configuration=Release -p:SignManifests=true

      - name: Sign assemblies
        env:
          CERTIFICATE_PASSWORD: ${{ secrets.CODE_SIGNING_PASSWORD }}
        run: |
          $cert = Get-ChildItem Cert:\CurrentUser\My -CodeSigningCert | Select-Object -First 1
          Get-ChildItem -Path "src\WordAddIn\bin\Release\*.dll" | ForEach-Object {
            Set-AuthenticodeSignature -FilePath $_.FullName -Certificate $cert -TimestampServer "http://timestamp.digicert.com"
          }
        shell: pwsh
```

## Azure DevOps

### Basic Pipeline

```yaml
# azure-pipelines.yml
trigger:
  branches:
    include:
      - main
      - master

pool:
  vmImage: 'windows-latest'

variables:
  solution: 'OfficeAddins.sln'
  buildConfiguration: 'Release'

steps:
  - task: NuGetToolInstaller@1
    displayName: 'Install NuGet'

  - task: NuGetCommand@2
    displayName: 'Restore NuGet packages'
    inputs:
      restoreSolution: '$(solution)'

  - task: VSBuild@1
    displayName: 'Build solution'
    inputs:
      solution: '$(solution)'
      msbuildArgs: '/p:DeployOnBuild=true'
      platform: 'Any CPU'
      configuration: '$(buildConfiguration)'

  - task: VSTest@2
    displayName: 'Run tests'
    inputs:
      testSelector: 'testAssemblies'
      testAssemblyVer2: |
        **\*Tests.dll
        !**\*TestAdapter.dll
        !**\obj\**
      searchFolder: '$(System.DefaultWorkingDirectory)'
      configuration: '$(buildConfiguration)'
```

### With Stages

```yaml
# azure-pipelines-stages.yml
trigger:
  branches:
    include:
      - main
  tags:
    include:
      - v*

pool:
  vmImage: 'windows-latest'

variables:
  solution: 'OfficeAddins.sln'
  buildConfiguration: 'Release'

stages:
  - stage: Build
    displayName: 'Build and Test'
    jobs:
      - job: Build
        steps:
          - task: NuGetToolInstaller@1

          - task: NuGetCommand@2
            inputs:
              restoreSolution: '$(solution)'

          - task: VSBuild@1
            inputs:
              solution: '$(solution)'
              platform: 'Any CPU'
              configuration: '$(buildConfiguration)'

          - task: VSTest@2
            inputs:
              testSelector: 'testAssemblies'
              testAssemblyVer2: '**\*Tests.dll'
              configuration: '$(buildConfiguration)'

          - task: PublishBuildArtifacts@1
            inputs:
              PathtoPublish: 'src/WordAddIn/bin/$(buildConfiguration)'
              ArtifactName: 'WordAddIn'

  - stage: Deploy
    displayName: 'Deploy to Test'
    dependsOn: Build
    condition: and(succeeded(), startsWith(variables['Build.SourceBranch'], 'refs/tags/v'))
    jobs:
      - deployment: DeployTest
        environment: 'Test'
        strategy:
          runOnce:
            deploy:
              steps:
                - task: DownloadBuildArtifacts@1
                  inputs:
                    buildType: 'current'
                    downloadType: 'single'
                    artifactName: 'WordAddIn'
                    downloadPath: '$(System.ArtifactsDirectory)'

                - task: CopyFiles@2
                  inputs:
                    SourceFolder: '$(System.ArtifactsDirectory)/WordAddIn'
                    Contents: '**'
                    TargetFolder: '\\$(FileServer)\Deployments\WordAddIn\$(Build.BuildNumber)'
```

## Environment Variables

### GitHub Actions Secrets

| Secret | Description |
|--------|-------------|
| `CODE_SIGNING_CERT` | Base64-encoded .pfx certificate |
| `CODE_SIGNING_PASSWORD` | Certificate password |
| `NUGET_API_KEY` | For publishing packages |

### Azure DevOps Variables

| Variable | Description |
|----------|-------------|
| `CodeSigningCert` | Secure file reference |
| `CertPassword` | Secret variable |
| `FileServer` | Deployment share |

## MSBuild Arguments Reference

| Argument | Purpose |
|----------|---------|
| `-p:Configuration=Release` | Build configuration |
| `-p:Platform="Any CPU"` | Target platform |
| `-p:SignManifests=true` | Sign ClickOnce manifests |
| `-p:PublishDir=./publish/` | ClickOnce output folder |
| `-t:Publish` | Create ClickOnce deployment |
| `-t:Clean,Build` | Clean before build |
| `-nologo` | Suppress banner |
| `-verbosity:minimal` | Reduce output |

## Common Issues

| Issue | Solution |
|-------|----------|
| MSBuild not found | Use `microsoft/setup-msbuild@v2` action |
| NuGet restore fails | Use `NuGet/setup-nuget@v2` and `nuget restore` |
| Tests not found | Check test assembly pattern, verify test references |
| Signing fails | Verify certificate imported, password correct |
| ClickOnce publish fails | Ensure prerequisites installed on build agent |
