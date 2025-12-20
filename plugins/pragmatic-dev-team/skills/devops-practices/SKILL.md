---
name: DevOps Practices
description: This skill should be used when discussing CI/CD pipelines, DORA metrics, deployment automation, GitHub Actions, infrastructure as code, or DevOps culture. Based on "Accelerate" research and DevOps best practices.
version: 0.1.0
---

# DevOps Practices

Key practices for high-performing software delivery, based on "Accelerate" by Forsgren, Humble, and Kim, and DORA research.

## DORA Metrics

The four key metrics that predict software delivery performance:

| Metric | Elite | High | Medium | Low |
|--------|-------|------|--------|-----|
| **Deployment Frequency** | Multiple/day | Weekly-Monthly | Monthly-Biannual | >6 months |
| **Lead Time for Changes** | <1 hour | 1 day-1 week | 1 week-1 month | >1 month |
| **Change Failure Rate** | 0-15% | 16-30% | 16-30% | 46-60% |
| **Time to Restore** | <1 hour | <1 day | 1 day-1 week | >1 week |

**Key insight:** Speed and stability are NOT trade-offs. Elite teams excel at both.

## Three Ways (Phoenix Project)

### 1. Flow
Optimize left-to-right work flow.

- Small batch sizes
- Reduce work in progress
- Eliminate bottlenecks
- Automate everything possible

### 2. Feedback
Amplify feedback loops.

- Fast test feedback
- Monitoring and alerting
- Blameless postmortems
- Customer feedback loops

### 3. Continuous Learning
Experiment and learn.

- Psychological safety
- Allocate time for improvement
- Share learnings broadly
- Embrace failure as learning

## CI/CD Pipeline

### Continuous Integration
```yaml
# GitHub Actions example
name: CI
on: [push, pull_request]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      - name: Restore
        run: dotnet restore
      - name: Build
        run: dotnet build --no-restore
      - name: Test
        run: dotnet test --no-build
```

### Continuous Deployment
```yaml
  deploy:
    needs: build
    if: github.ref == 'refs/heads/main'
    runs-on: ubuntu-latest
    steps:
      - name: Deploy to staging
        run: ./deploy.sh staging
      - name: Smoke tests
        run: ./smoke-tests.sh staging
      - name: Deploy to production
        run: ./deploy.sh production
```

## Pipeline Best Practices

### Fast Feedback
| Stage | Target Time | Purpose |
|-------|-------------|---------|
| Lint/Format | <30 seconds | Immediate style feedback |
| Unit Tests | <2 minutes | Logic verification |
| Integration Tests | <10 minutes | Component interaction |
| E2E Tests | <30 minutes | Critical path verification |

### Fail Fast
```yaml
steps:
  - name: Lint
    run: dotnet format --verify-no-changes
  - name: Build
    run: dotnet build --warnaserror
  - name: Test
    run: dotnet test --fail-on-error
```

### Cache Dependencies
```yaml
- uses: actions/cache@v4
  with:
    path: ~/.nuget/packages
    key: nuget-${{ hashFiles('**/*.csproj') }}
```

## Deployment Strategies

| Strategy | Risk | Use When |
|----------|------|----------|
| **Blue/Green** | Low | Can run two versions |
| **Canary** | Lower | Have monitoring |
| **Rolling** | Low | Stateless apps |
| **Feature Flag** | Lowest | Need fine control |

## Infrastructure as Code

### Principles
- Version controlled
- Reviewed like code
- Tested before deploy
- Idempotent
- Self-documenting

### Example (Terraform concept)
```hcl
resource "azure_app_service" "main" {
  name                = "myapp-${var.environment}"
  resource_group_name = azurerm_resource_group.main.name
  app_service_plan_id = azurerm_app_service_plan.main.id

  app_settings = {
    "ASPNETCORE_ENVIRONMENT" = var.environment
  }
}
```

## Branch Strategy

### Trunk-Based Development
```
main ─────●─────●─────●─────●─────→
          ↑     ↑     ↑     ↑
         PR    PR    PR    PR

Feature branches live <1 day
Merge to main frequently
Deploy from main
```

### Feature Flags
```csharp
if (await _featureFlags.IsEnabled("new-checkout", user))
{
    return NewCheckoutFlow();
}
return LegacyCheckoutFlow();
```

## Key Practices

| Practice | Why It Matters |
|----------|---------------|
| Automate everything | Reduce toil, increase consistency |
| Small batches | Faster feedback, easier debugging |
| Trunk-based development | Reduce merge conflicts |
| Feature flags | Decouple deploy from release |
| Monitoring | Know when things break |
| Blameless postmortems | Learn from failures |

## Anti-Patterns

| Anti-Pattern | Problem | Solution |
|--------------|---------|----------|
| Long-lived branches | Merge hell | Trunk-based, short branches |
| Manual deployments | Error-prone, slow | Automate |
| No tests in pipeline | Late feedback | Test early |
| Deploy on Friday | Weekend incidents | Early week deploys |
| Hero culture | Unsustainable | Sustainable pace, automation |
