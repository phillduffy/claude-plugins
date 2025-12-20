# Alerting Strategies

SLOs, error budgets, and actionable alerts.

## SLO Fundamentals

### Definitions

| Term | Meaning | Example |
|------|---------|---------|
| **SLI** | Measured indicator | 99.92% requests successful |
| **SLO** | Internal target | 99.9% success rate |
| **SLA** | Customer contract | 99.5% uptime guaranteed |
| **Error Budget** | Allowable failures | 0.1% = 43 min/month |

### Setting SLOs

```
SLA:  99.5%   (contractual - penalties if breached)
SLO:  99.9%   (internal target - gives buffer before SLA breach)
Alert: 99.85% (early warning)
```

---

## Error Budget

**Error Budget = 100% - SLO**

### Monthly Budget Calculation

For 99.9% SLO:
```
Error budget = 0.1%
Monthly minutes = 30 days × 24 hours × 60 min = 43,200 min
Allowed downtime = 43,200 × 0.001 = 43.2 minutes
```

### Budget Status Actions

| Budget Remaining | Action |
|------------------|--------|
| >50% | Ship features, take risks |
| 25-50% | Balance features and reliability |
| <25% | Prioritize reliability work |
| 0% (exhausted) | Freeze deployments, fix reliability |

---

## Burn Rate Alerting

**Burn Rate = (Observed Error Rate) / (Allowed Error Rate)**

```
SLO: 99.9% (allowed error rate = 0.1%)
Current: 99.5% (current error rate = 0.5%)
Burn rate = 0.5% / 0.1% = 5x

At 5x burn rate, monthly budget consumed in ~6 days
```

### Alert Thresholds

| Burn Rate | Meaning | Response |
|-----------|---------|----------|
| 14x | Critical | Page immediately |
| 6x | Urgent | Page within 30 min |
| 2x | Elevated | Create ticket |
| 1x | Normal | Monitor |
| <1x | Healthy | Under budget |

### Implementation

```csharp
public class SloMonitor
{
    private readonly double _sloTarget = 0.999;
    private readonly IMetrics _metrics;
    private readonly IAlerting _alerting;

    public async Task CheckBurnRateAsync(
        int successCount,
        int errorCount,
        CancellationToken ct = default)
    {
        var totalRequests = successCount + errorCount;
        if (totalRequests == 0) return;

        var errorRate = (double)errorCount / totalRequests;
        var allowedErrorRate = 1 - _sloTarget;
        var burnRate = errorRate / allowedErrorRate;

        _metrics.RecordGauge("slo.burn_rate", burnRate);

        if (burnRate > 14)
        {
            await _alerting.PageAsync(
                severity: "P1",
                message: $"SLO burn rate critical: {burnRate:F1}x",
                ct);
        }
        else if (burnRate > 6)
        {
            await _alerting.PageAsync(
                severity: "P2",
                message: $"SLO burn rate elevated: {burnRate:F1}x",
                ct);
        }
        else if (burnRate > 2)
        {
            await _alerting.CreateTicketAsync(
                message: $"SLO burn rate warning: {burnRate:F1}x",
                ct);
        }
    }
}
```

---

## Actionable Alerts

### Principles

1. **Every alert requires human action** - If no action needed, remove it
2. **Include context** - Owner, runbook, affected service
3. **Prioritize by impact** - Customer-facing first
4. **Work backwards from objectives** - Start with customer experience

### Alert Context Template

```csharp
public record AlertContext
{
    public string ServiceName { get; init; }
    public string OwnerTeam { get; init; }
    public string Severity { get; init; }
    public string RunbookUrl { get; init; }
    public Dictionary<string, object> Metrics { get; init; }
    public string[] RecentChanges { get; init; }
}

// Usage in logging/alerting
_logger.LogError(
    "High error rate detected. " +
    "Service: {ServiceName}, " +
    "Owner: {OwnerTeam}, " +
    "Runbook: {RunbookUrl}, " +
    "ErrorRate: {ErrorRate:P2}",
    ctx.ServiceName,
    ctx.OwnerTeam,
    ctx.RunbookUrl,
    errorRate);
```

---

## Alert Fatigue

### Causes

- Non-actionable alerts
- False positives
- Too many alerts
- Duplicate alerts for same issue

### Statistics

- 27% of alerts ignored in mid-size companies
- Alert fatigue causes missed critical incidents
- Leads to burnout and turnover

### Prevention

| Strategy | Implementation |
|----------|---------------|
| **Actionable** | Every alert has a runbook |
| **Deduplicated** | Group related alerts |
| **Prioritized** | P1/P2/P3/P4 severity levels |
| **Silenced** | Suppress during maintenance |
| **Reviewed** | Quarterly alert audit |

---

## Alert Hygiene

### Quarterly Review Checklist

- [ ] How often did this alert fire?
- [ ] Did it require action every time?
- [ ] What was the false positive rate?
- [ ] Is the threshold still appropriate?
- [ ] Should it be combined with other alerts?
- [ ] Is the runbook up to date?

### Maintenance Windows

```csharp
public class AlertSuppressionMiddleware
{
    public bool ShouldSuppress(Alert alert)
    {
        // Check maintenance windows
        if (_maintenanceService.IsInMaintenanceWindow(alert.ServiceName))
            return true;

        // Check inhibition rules
        if (IsRelatedToActiveP1(alert))
            return true;

        return false;
    }
}
```

---

## RED Method (Request-Driven)

For services, monitor:

| Metric | What | Alert When |
|--------|------|------------|
| **Rate** | Requests/sec | Unusual drop or spike |
| **Errors** | Error rate | Above SLO threshold |
| **Duration** | Latency (p50, p95, p99) | Above acceptable threshold |

```csharp
// Rate
s_requestCounter.Add(1, new KeyValuePair<string, object?>("endpoint", endpoint));

// Errors
if (response.IsError)
    s_errorCounter.Add(1);

// Duration
s_latencyHistogram.Record(stopwatch.ElapsedMilliseconds);
```

---

## USE Method (Resource-Driven)

For resources (CPU, memory, disk):

| Metric | What | Alert When |
|--------|------|------------|
| **Utilization** | % capacity used | Sustained high |
| **Saturation** | Queue depth | Growing |
| **Errors** | Error count | Non-zero |

---

## Anti-Patterns

| Anti-Pattern | Problem | Fix |
|--------------|---------|-----|
| Alert on everything | Fatigue, missed critical | Focus on customer impact |
| No runbook | Unclear response | Every alert needs runbook |
| Static thresholds | False positives | Adaptive/dynamic thresholds |
| No deduplication | Alert storms | Group related alerts |
| No review | Stale, irrelevant alerts | Quarterly audit |
| Ignore during oncall | Normalization | Page = action required |

---

## Quick Reference

### Severity Levels

| Level | Response Time | Example |
|-------|--------------|---------|
| P1 | Immediate | Outage, data loss |
| P2 | 30 minutes | Significant degradation |
| P3 | Next business day | Limited impact |
| P4 | When convenient | Minor issue |

### SLO Quick Math

```
SLO 99.9% = 43.2 min/month downtime allowed
SLO 99.5% = 216 min/month downtime allowed
SLO 99%   = 432 min/month downtime allowed

Burn rate = (current error rate) / (allowed error rate)
```

## Sources

- [Alerting on SLOs](https://cloud.google.com/stackdriver/docs/solutions/slo-monitoring/alerting-on-budget-burn-rate)
- [SLO Monitoring](https://grafana.com/blog/2024/09/06/incident-management-that-actually-makes-sense-slos-error-budgets-and-blameless-reviews/)
- [Alert Fatigue](https://middleware.io/blog/what-is-alert-fatigue/)
- [Actionable Alerts](https://drdroid.io/engineering-tools/the-art-of-actionable-alerts-a-guide-to-effective-monitoring)
- [Google SRE Book](https://sre.google/workbook/alerting-on-slos/)
