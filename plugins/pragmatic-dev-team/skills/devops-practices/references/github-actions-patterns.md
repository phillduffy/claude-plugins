# GitHub Actions Patterns

Modern patterns for .NET CI/CD with GitHub Actions.

## Matrix Builds

Run tests across multiple configurations in parallel.

### Basic Matrix

```yaml
jobs:
  build:
    strategy:
      matrix:
        os: [ubuntu-latest, windows-latest]
        dotnet-version: ['6.0.x', '8.0.x', '9.0.x']
    runs-on: ${{ matrix.os }}
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ matrix.dotnet-version }}
      - run: dotnet build
      - run: dotnet test
```

### Matrix with Exclusions

```yaml
strategy:
  matrix:
    os: [ubuntu-latest, windows-latest, macos-latest]
    dotnet-version: ['6.0.x', '8.0.x']
    exclude:
      # Skip .NET 6 on macOS (not needed)
      - os: macos-latest
        dotnet-version: '6.0.x'
  fail-fast: false  # Continue other jobs if one fails
```

### Dynamic Matrix

```yaml
jobs:
  generate:
    runs-on: ubuntu-latest
    outputs:
      matrix: ${{ steps.set-matrix.outputs.matrix }}
    steps:
      - id: set-matrix
        run: |
          echo 'matrix={"dotnet":["8.0.x","9.0.x"]}' >> $GITHUB_OUTPUT

  build:
    needs: generate
    strategy:
      matrix: ${{ fromJson(needs.generate.outputs.matrix) }}
    runs-on: ubuntu-latest
    steps:
      - run: dotnet --version
```

---

## Caching

### Built-in NuGet Caching (.NET 5+)

```yaml
steps:
  - uses: actions/checkout@v4
  - uses: actions/setup-dotnet@v4
    with:
      dotnet-version: 8.x
      cache: true  # Enable built-in NuGet caching
      cache-dependency-path: '**/packages.lock.json'
  - run: dotnet restore --locked-mode
  - run: dotnet build --no-restore
```

**Required project setting:**
```xml
<PropertyGroup>
  <RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>
</PropertyGroup>
```

### Manual Caching

```yaml
- uses: actions/cache@v4
  with:
    path: ~/.nuget/packages
    key: nuget-${{ runner.os }}-${{ hashFiles('**/*.csproj') }}
    restore-keys: |
      nuget-${{ runner.os }}-
```

### Fix NU1403 Error

```xml
<PropertyGroup>
  <DisableImplicitNuGetFallbackFolder>true</DisableImplicitNuGetFallbackFolder>
</PropertyGroup>
```

---

## Artifacts

### Upload Build Output

```yaml
- run: dotnet publish -c Release -o ./publish

- uses: actions/upload-artifact@v4
  with:
    name: dotnet-app
    path: ./publish
    retention-days: 7
```

### Download in Another Job

```yaml
deploy:
  needs: build
  runs-on: ubuntu-latest
  steps:
    - uses: actions/download-artifact@v4
      with:
        name: dotnet-app

    - run: ls -la  # Files are in current directory
```

### Cache vs Artifact

| Use Case | Tool | Scope |
|----------|------|-------|
| Dependencies (NuGet) | Cache | Across workflows |
| Build outputs | Artifact | Single workflow |
| Test results | Artifact | Single workflow |

---

## Reusable Workflows

### Define Reusable Workflow

```yaml
# .github/workflows/reusable-dotnet-build.yml
name: Reusable .NET Build

on:
  workflow_call:
    inputs:
      dotnet-version:
        required: true
        type: string
      configuration:
        required: false
        type: string
        default: Release
    secrets:
      NUGET_TOKEN:
        required: false

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ inputs.dotnet-version }}
          cache: true

      - run: dotnet restore
      - run: dotnet build -c ${{ inputs.configuration }} --no-restore
      - run: dotnet test -c ${{ inputs.configuration }} --no-build
```

### Call Reusable Workflow

```yaml
# .github/workflows/ci.yml
name: CI

on: [push, pull_request]

jobs:
  build:
    uses: ./.github/workflows/reusable-dotnet-build.yml
    with:
      dotnet-version: '8.0.x'
      configuration: Release
    secrets:
      NUGET_TOKEN: ${{ secrets.NUGET_TOKEN }}
```

---

## Composite Actions

Reusable steps within a job.

### Define Composite Action

```yaml
# .github/actions/setup-dotnet-project/action.yml
name: Setup .NET Project
description: Setup .NET, restore, and build

inputs:
  dotnet-version:
    required: true
    description: .NET version
  project-path:
    required: false
    default: '.'

runs:
  using: composite
  steps:
    - uses: actions/setup-dotnet@v4
      with:
        dotnet-version: ${{ inputs.dotnet-version }}
        cache: true

    - run: dotnet restore
      shell: bash
      working-directory: ${{ inputs.project-path }}

    - run: dotnet build --no-restore
      shell: bash
      working-directory: ${{ inputs.project-path }}
```

### Use Composite Action

```yaml
steps:
  - uses: actions/checkout@v4
  - uses: ./.github/actions/setup-dotnet-project
    with:
      dotnet-version: '8.0.x'
      project-path: './src'
```

---

## Complete .NET Workflow

```yaml
name: .NET CI/CD

on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

env:
  DOTNET_SKIP_FIRST_TIME_EXPERIENCE: true
  DOTNET_CLI_TELEMETRY_OPTOUT: true

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: 8.x
          cache: true

      - name: Restore
        run: dotnet restore

      - name: Build
        run: dotnet build -c Release --no-restore

      - name: Test
        run: dotnet test -c Release --no-build --logger trx --results-directory TestResults

      - name: Upload test results
        uses: actions/upload-artifact@v4
        if: always()
        with:
          name: test-results
          path: TestResults

      - name: Publish
        run: dotnet publish -c Release -o ./publish --no-build

      - name: Upload artifact
        uses: actions/upload-artifact@v4
        with:
          name: app
          path: ./publish

  deploy:
    needs: build
    if: github.ref == 'refs/heads/main'
    runs-on: ubuntu-latest
    environment: production
    steps:
      - uses: actions/download-artifact@v4
        with:
          name: app

      - name: Deploy
        run: |
          # Your deployment script
          echo "Deploying..."
```

---

## Anti-Patterns

| Anti-Pattern | Problem | Fix |
|--------------|---------|-----|
| `ubuntu-latest` | Breaks when GitHub updates | Pin: `ubuntu-22.04` |
| Unpinned actions | Security risk | Pin to SHA or tag |
| No caching | Slow builds, wasted resources | Cache dependencies |
| Secrets in logs | Security exposure | Use `add-mask` |
| Matrix explosion | 25 jobs = slow + expensive | Strategic excludes |
| No fail-fast | Wait for all when one fails | `fail-fast: true` |

### Security Best Practices

```yaml
# Pin to SHA for production
- uses: actions/checkout@8ade135a41bc03ea155e62e844d188df1ea18608

# Minimal permissions
permissions:
  contents: read
  packages: write

# Mask secrets
- run: echo "::add-mask::${{ secrets.API_KEY }}"
```

## Sources

- [GitHub Actions - Matrix](https://docs.github.com/en/actions/using-jobs/using-a-matrix-for-your-jobs)
- [actions/setup-dotnet](https://github.com/actions/setup-dotnet)
- [Reusable Workflows](https://docs.github.com/en/actions/using-workflows/reusing-workflows)
- [Security Hardening](https://docs.github.com/en/actions/security-guides/security-hardening-for-github-actions)
