---
name: team-orchestration
description: Use when a task requires multiple specialists (BDD, architecture, code review) working together. Coordinates the pragmatic-dev-team agents in the right order with review loops until approved.
version: 0.2.0
load: on-demand
---

# Team Orchestration

Coordinate specialists for complex tasks. Right specialist → Right order → Review loops = Production-ready.

## Quick Decision

| Task Type | Use Orchestration? | Specialists |
|-----------|-------------------|-------------|
| New feature | Yes | bdd → architect → implement → review |
| Complex bug | Yes | review → architect (if needed) → test-plan |
| Simple bug | No | review only |
| Refactoring | Yes | architect → implement → review |
| Documentation | No | Single pass |

## Core Process

```
1. ANALYZE → What specialists needed?
2. ORDER → BDD first (if feature), then architecture, then code
3. DISPATCH → One specialist at a time
4. LOOP → Issues found? Fix → Re-review same specialist
5. NEXT → Only proceed when current specialist ✅
6. COMPLETE → All specialists approved
```

## Specialist Selection

| Specialist | Purpose | Outputs |
|------------|---------|---------|
| **bdd-strategist** | Define behavior first | Gherkin scenarios |
| **architect** | Validate design | Layer boundaries, patterns |
| **code-reviewer** | Quality + patterns | Issues, recommendations |
| **test-plan** | Coverage strategy | Test plan, gaps |
| **research** | Compare options | Matrix, recommendation |

## Task Type → Specialist Order

| Task Type | Order |
|-----------|-------|
| **New Feature** | bdd → architect → implement → review → test-plan |
| **Bug Fix (Complex)** | review → architect (if needed) → test-plan |
| **Bug Fix (Simple)** | review only |
| **Refactoring** | architect → implement → review |
| **Performance** | review → architect → implement |
| **Research/Spike** | research → bdd (with findings) |

## Iron Law: Review Loops

**NEVER skip re-review when issues found.**

```
Specialist finds issues
    ↓
FIX the issues
    ↓
RE-DISPATCH same specialist
    ↓
✅ Approved? → Next specialist
❌ More issues? → Loop again
```

## Red Flags - STOP

- Moving to next specialist with open issues
- Skipping BDD for "simple" features
- Code review before architecture approval
- Self-review replacing specialist review
- "Good enough" when issues remain

## When to Skip Specialists

Document WHY when skipping:
- "Skip architect - cosmetic change only"
- "Skip bdd - hotfix, will add scenarios post-merge"

Add skipped items to tech debt.

## Success Criteria

- [ ] All selected specialists dispatched
- [ ] All specialists ✅ (no open issues)
- [ ] Review loops completed for issues
- [ ] Skipped specialists documented with reason

## References

- **[references/workflow-examples.md](references/workflow-examples.md)** - Full feature and bug fix walk-throughs
