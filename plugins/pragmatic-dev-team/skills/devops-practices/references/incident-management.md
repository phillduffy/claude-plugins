# Incident Management

SLOs, error budgets, and blameless postmortems.

## SLO Fundamentals

### Definitions

| Term | Meaning | Example |
|------|---------|---------|
| **SLI** | Service Level Indicator | 99.92% uptime this month |
| **SLO** | Service Level Objective | Target: 99.9% uptime |
| **SLA** | Service Level Agreement | Contract: 99.5% guaranteed |
| **Error Budget** | Allowable failures | 1 - SLO = 0.1% downtime allowed |

### Setting SLOs

```
SLO should be tighter than SLA:

SLA: 99.5%  (contractual commitment)
SLO: 99.9%  (internal target - gives buffer)
```

### Error Budget Calculation

```
Monthly Error Budget for 99.9% SLO:

30 days × 24 hours × 60 min = 43,200 minutes
Error budget = 43,200 × 0.001 = 43.2 minutes

If you've used 30 minutes, you have 13.2 minutes left.
```

---

## Burn Rate Alerting

**Burn Rate** = How fast you're consuming error budget.

```
Burn Rate = (Observed Error Rate) / (Allowed Error Rate)

If burn rate > 1: Consuming budget too fast
If burn rate < 1: Within budget
```

### Alert Thresholds

| Burn Rate | Response | Action |
|-----------|----------|--------|
| 14x | Page immediately | Major incident |
| 6x | Page within 30 min | Urgent investigation |
| 2x | Ticket | Schedule fix |
| 1x | Normal | Monitor |

### C# Monitoring Example

```csharp
public class SloMonitor
{
    private readonly double _sloTarget = 0.999; // 99.9%
    private readonly IMetrics _metrics;

    public void CheckBurnRate(int successCount, int errorCount)
    {
        var totalRequests = successCount + errorCount;
        if (totalRequests == 0) return;

        var errorRate = (double)errorCount / totalRequests;
        var allowedErrorRate = 1 - _sloTarget;
        var burnRate = errorRate / allowedErrorRate;

        _metrics.RecordGauge("slo.burn_rate", burnRate);

        if (burnRate > 14)
        {
            // P1 Alert: Page on-call
            _alerting.PageOnCall("SLO burn rate critical: {0:F1}x", burnRate);
        }
        else if (burnRate > 6)
        {
            // P2 Alert: Urgent ticket
            _alerting.CreateUrgentTicket("SLO burn rate elevated: {0:F1}x", burnRate);
        }
    }
}
```

---

## Blameless Postmortems

Focus on systems, not people.

### Key Principles

1. **Assume good intent** - Everyone did their best with available info
2. **Use roles, not names** - "On-call engineer" not "John"
3. **Focus on learning** - What can we improve?
4. **No punishment** - Fear prevents honest reporting
5. **Systemic fixes** - Process/tooling, not "be more careful"

### Postmortem Template

```markdown
# Incident Postmortem: [Title]

## Summary
- **Duration:** 45 minutes (10:05 - 10:50 UTC)
- **Impact:** 15% of users saw 500 errors on checkout
- **Root Cause:** NuGet package cache corruption

## Timeline

| Time | Event |
|------|-------|
| 10:05 | Monitoring alerts on elevated 5xx |
| 10:08 | On-call engineer acknowledges |
| 10:12 | Escalation to platform team |
| 10:30 | Root cause identified: corrupted cache |
| 10:45 | Cache cleared, service recovering |
| 10:50 | All metrics back to normal |

## Root Cause

NuGet package cache became corrupted during parallel CI builds.
Concurrent writes without locking caused partial file corruption.

## Five Whys

1. Why 500 errors? → App couldn't load dependencies
2. Why couldn't it load? → NuGet cache corrupted
3. Why corrupted? → Concurrent writes from parallel builds
4. Why concurrent writes? → Matrix builds run simultaneously
5. Why no locking? → Cache action doesn't support file locking

## Contributing Factors

- No health check for cache validity
- Monitoring didn't alert until user impact
- Runbook didn't cover cache corruption

## Action Items

| Action | Owner | Due | Status |
|--------|-------|-----|--------|
| Add mutex to cache writes | Platform | 2025-01-25 | Open |
| Add cache health check | SRE | 2025-01-28 | Open |
| Update runbook | On-call | 2025-01-22 | Done |
| Add pre-deployment cache validation | Platform | 2025-02-01 | Open |

## Lessons Learned

### What Went Well
- Alerting caught issue within 3 minutes
- Escalation was smooth
- Good communication in incident channel

### What Could Improve
- Faster identification of cache as root cause
- Better monitoring for cache state
- Automated cache validation

## Prevention

This incident could have been prevented by:
- Cache locking mechanism
- Health check on deployment
- Pre-flight cache validation in CI
```

---

## Incident Response

### Severity Levels

| Level | Description | Response | Example |
|-------|-------------|----------|---------|
| **P1** | Critical - major outage | Page immediately | Checkout down |
| **P2** | High - significant impact | Page within 30min | Slow responses |
| **P3** | Medium - limited impact | Next business day | Feature degraded |
| **P4** | Low - minimal impact | When convenient | Minor UI bug |

### On-Call Rotation

```yaml
# PagerDuty-style config
schedule:
  primary:
    rotation: weekly
    handoff: monday-09:00
    members:
      - alice
      - bob
      - charlie

  secondary:
    rotation: weekly
    handoff: monday-09:00
    offset: 1  # One week behind primary

escalation:
  - level: 1
    delay: 0
    targets: [primary]
  - level: 2
    delay: 15m
    targets: [secondary]
  - level: 3
    delay: 30m
    targets: [engineering-manager]
```

---

## Alert Fatigue

### Causes

- Non-actionable alerts
- Too many alerts
- False positives
- Alerting on symptoms, not causes

### Prevention

| Principle | Implementation |
|-----------|----------------|
| **Actionable** | Every alert has a runbook |
| **Prioritized** | Use severity levels |
| **Silenced** | Suppress during maintenance |
| **Reviewed** | Quarterly alert review |
| **Contextual** | Include owner, service, runbook URL |

### Alert Context

```csharp
_logger.LogError(
    "High error rate detected. " +
    "Service: {Service}, " +
    "Owner: {Owner}, " +
    "Runbook: {RunbookUrl}, " +
    "ErrorRate: {ErrorRate:P2}",
    serviceName,
    ownerTeam,
    runbookUrl,
    errorRate);
```

---

## DORA 2024 Insights

### Key Findings

| Factor | Impact |
|--------|--------|
| Stable priorities | 40% lower burnout |
| Unstable priorities | Worse delivery metrics |
| Internal platforms | +10% performance |
| Forced platform use | -14% stability |

### Recommendations

1. **Psychological safety** - Blameless culture
2. **Stable priorities** - Reduce context switching
3. **Platform as option** - Not mandated
4. **Documentation** - Runbooks, not tribal knowledge

---

## Quick Reference

### Error Budget Status

```
Budget Remaining    Action
> 50%              Ship features
25-50%             Careful, prioritize reliability
< 25%              Focus on reliability
< 0%               Freeze deployments, fix reliability
```

### Postmortem Checklist

- [ ] Timeline documented
- [ ] Root cause identified (5 Whys)
- [ ] Contributing factors listed
- [ ] Action items assigned with owners
- [ ] Lessons learned captured
- [ ] No blame assigned
- [ ] Systemic fixes proposed

## Sources

- [Google SRE - Postmortem Culture](https://sre.google/workbook/postmortem-culture/)
- [Atlassian - Blameless Postmortems](https://www.atlassian.com/incident-management/postmortem/blameless)
- [DORA 2024 Report](https://dora.dev/research/2024/dora-report/)
- [Grafana - SLO Monitoring](https://grafana.com/blog/2024/09/06/incident-management-that-actually-makes-sense-slos-error-budgets-and-blameless-reviews/)
