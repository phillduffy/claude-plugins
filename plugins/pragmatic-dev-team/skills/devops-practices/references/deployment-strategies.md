# Deployment Strategies

Modern approaches to safe, zero-downtime deployments.

## Strategy Comparison

| Strategy | Risk | Cost | Speed | Best For |
|----------|------|------|-------|----------|
| **Blue-Green** | Low | High (2x infra) | Instant | Monoliths, instant rollback |
| **Canary** | Lower | Medium | Gradual | Microservices, monitoring |
| **Rolling** | Medium | Low | Moderate | Stateless, k8s native |
| **Feature Flags** | Lowest | Low | Decoupled | Trunk-based, gradual rollout |

---

## Blue-Green Deployment

Two identical production environments. Switch traffic instantly.

### Concept

```
                    Load Balancer
                         │
           ┌─────────────┴─────────────┐
           ▼                           ▼
      ┌─────────┐                 ┌─────────┐
      │  Blue   │  ←── LIVE      │  Green  │  ←── STAGING
      │  v1.0   │                │  v1.1   │
      └─────────┘                └─────────┘

After validation:

                    Load Balancer
                         │
           ┌─────────────┴─────────────┐
           ▼                           ▼
      ┌─────────┐                 ┌─────────┐
      │  Blue   │  ←── STAGING   │  Green  │  ←── LIVE
      │  v1.0   │                │  v1.1   │
      └─────────┘                └─────────┘
```

### GitHub Actions Example

```yaml
deploy:
  runs-on: ubuntu-latest
  steps:
    - name: Deploy to staging slot
      run: |
        az webapp deployment slot create --slot staging
        az webapp deploy --slot staging --src-path ./app.zip

    - name: Run smoke tests
      run: |
        curl -f https://myapp-staging.azurewebsites.net/health

    - name: Swap slots
      run: |
        az webapp deployment slot swap --slot staging --target-slot production
```

### Pros/Cons

| Pros | Cons |
|------|------|
| Instant rollback | Double infrastructure cost |
| Full environment testing | Database migrations complex |
| Zero downtime | Session handling challenges |

---

## Canary Deployment

Gradual rollout to subset of users.

### Concept

```
Step 1:  [█░░░░░░░░░]  5% on v2, 95% on v1
Step 2:  [███░░░░░░░]  25% on v2
Step 3:  [█████░░░░░]  50% on v2
Step 4:  [██████████]  100% on v2
```

### Kubernetes Example

```yaml
# v1 deployment (initial)
apiVersion: apps/v1
kind: Deployment
metadata:
  name: myapp-v1
spec:
  replicas: 9
  selector:
    matchLabels:
      app: myapp
      version: v1

---
# v2 canary deployment
apiVersion: apps/v1
kind: Deployment
metadata:
  name: myapp-v2
spec:
  replicas: 1  # 10% of traffic
  selector:
    matchLabels:
      app: myapp
      version: v2
```

### Canary with Monitoring

```yaml
canary:
  runs-on: ubuntu-latest
  steps:
    - name: Deploy canary (5%)
      run: kubectl scale deployment/myapp-v2 --replicas=1

    - name: Monitor error rate
      run: |
        # Check error rate for 10 minutes
        ERROR_RATE=$(curl -s "$PROMETHEUS_URL/api/v1/query?query=error_rate" | jq '.value')
        if [ "$ERROR_RATE" -gt "0.01" ]; then
          echo "Error rate too high: $ERROR_RATE"
          exit 1
        fi

    - name: Expand to 50%
      if: success()
      run: kubectl scale deployment/myapp-v2 --replicas=5

    - name: Full rollout
      if: success()
      run: kubectl scale deployment/myapp-v2 --replicas=10
```

### Rollback

```bash
# If canary fails, scale back to 0
kubectl scale deployment/myapp-v2 --replicas=0
```

---

## Rolling Deployment

Update instances one at a time (Kubernetes default).

```yaml
apiVersion: apps/v1
kind: Deployment
spec:
  replicas: 10
  strategy:
    type: RollingUpdate
    rollingUpdate:
      maxUnavailable: 1   # At most 1 pod down
      maxSurge: 1         # At most 1 extra pod
```

### Pros/Cons

| Pros | Cons |
|------|------|
| No extra infrastructure | Rollback slower |
| Kubernetes native | Both versions run simultaneously |
| Gradual resource usage | Stateful apps complex |

---

## Feature Flags

Decouple deployment from release.

### Concept

```csharp
// Deploy code to production (disabled)
if (await _featureFlags.IsEnabledAsync("new-checkout", user))
{
    return await NewCheckoutFlowAsync();
}
return await LegacyCheckoutFlowAsync();
```

### Progressive Rollout

```yaml
# Feature flag service configuration
new-checkout:
  enabled: true
  rollout:
    - percentage: 5
      start: 2025-01-20
    - percentage: 25
      start: 2025-01-22
    - percentage: 100
      start: 2025-01-25
```

### Targeting

```csharp
// Target specific users/groups
var context = new FeatureContext
{
    UserId = user.Id,
    UserTier = user.Tier,
    Region = user.Region
};

if (await _flags.IsEnabledAsync("beta-feature", context))
{
    // Beta users or specific regions
}
```

### Benefits

| Benefit | Description |
|---------|-------------|
| **Decoupled** | Deploy != Release |
| **Targeted** | Beta users, A/B testing |
| **Kill switch** | Instant rollback via flag |
| **Trunk-based** | No long-lived branches |

---

## Progressive Delivery

Combines canary + feature flags + monitoring.

### Workflow

```
1. Deploy new version (disabled via flag)
2. Enable for 1% of users
3. Monitor metrics (errors, latency, business KPIs)
4. If healthy, increase to 10%
5. Continue to 50%, then 100%
6. If issues at any step, disable flag (instant rollback)
```

### GitHub Actions Example

```yaml
deploy:
  steps:
    - name: Deploy (feature disabled)
      run: |
        az webapp deploy --src-path ./app.zip
        # Feature flag ensures new code not active

    - name: Enable for 5% via flag service
      run: |
        curl -X POST "$FLAG_SERVICE/api/flags/new-feature" \
          -d '{"rollout_percentage": 5}'

    - name: Monitor for 30 minutes
      run: |
        sleep 1800
        ERROR_RATE=$(curl -s "$METRICS/error-rate")
        if [ "$ERROR_RATE" -gt "0.01" ]; then
          curl -X POST "$FLAG_SERVICE/api/flags/new-feature" \
            -d '{"enabled": false}'
          exit 1
        fi

    - name: Increase to 100%
      run: |
        curl -X POST "$FLAG_SERVICE/api/flags/new-feature" \
          -d '{"rollout_percentage": 100}'
```

---

## Decision Matrix

| Scenario | Recommended Strategy |
|----------|---------------------|
| Monolith, need instant rollback | Blue-Green |
| Microservices, good monitoring | Canary |
| Kubernetes, stateless | Rolling |
| Trunk-based, gradual rollout | Feature Flags |
| High-risk, business-critical | Progressive Delivery |
| Simple app, low risk | Rolling |

---

## Database Migration Strategies

### Expand-Contract Pattern

1. **Expand**: Add new column/table (backward compatible)
2. **Deploy**: New app version uses new structure
3. **Contract**: Remove old column/table

```sql
-- Step 1: Expand (add new column)
ALTER TABLE Users ADD COLUMN email_verified BOOLEAN DEFAULT FALSE;

-- Step 2: Deploy new version that uses email_verified

-- Step 3: Contract (after old version gone)
-- ALTER TABLE Users DROP COLUMN old_email_status;
```

### Feature Flag for Migrations

```csharp
// Dual-write during migration
await SaveToOldTable(data);

if (await _flags.IsEnabledAsync("new-database"))
{
    await SaveToNewTable(data);
}
```

---

## Anti-Patterns

| Anti-Pattern | Problem | Fix |
|--------------|---------|-----|
| Deploy on Friday | Weekend incidents | Early week deploys |
| Big bang releases | High risk | Small batches |
| No rollback plan | Stuck with bugs | Always have rollback |
| Skip staging | Find bugs in prod | Test in staging first |
| Manual deployments | Inconsistent, slow | Automate everything |

## Sources

- [Harness - Blue-Green and Canary](https://www.harness.io/blog/blue-green-canary-deployment-strategies)
- [CircleCI - Canary vs Blue-Green](https://circleci.com/blog/canary-vs-blue-green-downtime/)
- [Unleash - Feature Flags](https://www.getunleash.io/blog/canary-release-vs-progressive-delivery)
- [Martin Fowler - Feature Toggles](https://martinfowler.com/articles/feature-toggles.html)
