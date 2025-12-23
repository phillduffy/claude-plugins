---
name: verification-before-completion
description: Use when about to claim work is complete, fixed, or passing. Evidence before assertions always.
---

# Verification Before Completion

## Overview

Claiming work is complete without verification is dishonesty, not efficiency.

**Core principle:** Evidence before claims, always.

**Violating the letter of this rule is violating the spirit of this rule.**

## The Iron Law

```
NO COMPLETION CLAIMS WITHOUT FRESH VERIFICATION EVIDENCE
```

If you haven't run the verification command in this message, you cannot claim it passes.

## The Gate Function

```
BEFORE claiming any status or expressing satisfaction:

1. IDENTIFY: What command proves this claim?
2. RUN: Execute the FULL command (fresh, complete)
3. READ: Full output, check exit code, count failures
4. VERIFY: Does output confirm the claim?
   - If NO: State actual status with evidence
   - If YES: State claim WITH evidence
5. ONLY THEN: Make the claim

Skip any step = lying, not verifying
```

## Common Failures

| Claim | Requires | Not Sufficient |
|-------|----------|----------------|
| Tests pass | `dotnet test` output: 0 failures | Previous run, "should pass" |
| Build succeeds | `msbuild` exit 0 | Linter passing, logs look good |
| Bug fixed | Test original symptom: passes | Code changed, assumed fixed |
| Regression test works | Red-green cycle verified | Test passes once |
| Feature complete | BDD scenarios pass | Code looks right |
| Requirements met | Line-by-line checklist | Tests passing |
| Agent completed | VCS diff shows changes | Agent reports "success" |
| DDD patterns correct | Architecture review passes | Looks like Clean Architecture |

## Red Flags - STOP

- Using "should", "probably", "seems to"
- Expressing satisfaction before verification ("Great!", "Perfect!", "Done!", etc.)
- About to commit/push/PR without verification
- Trusting agent success reports
- Relying on partial verification
- Thinking "just this once"
- Tired and wanting work over
- **ANY wording implying success without having run verification**

## Rationalization Prevention

| Excuse | Reality |
|--------|---------|
| "Should work now" | RUN the verification |
| "I'm confident" | Confidence ≠ evidence |
| "Just this once" | No exceptions |
| "Linter passed" | Linter ≠ MSBuild |
| "Agent said success" | Verify independently |
| "I'm tired" | Exhaustion ≠ excuse |
| "Partial check is enough" | Partial proves nothing |
| "Different words so rule doesn't apply" | Spirit over letter |

## Project-Specific Patterns

**Tests (Reqnroll/BDD):**
```
✅ [Run: dotnet test] [See: 0 failed, X passed] "All tests pass"
❌ "Should pass now" / "Scenarios look correct"
```

**Regression tests (TDD Red-Green):**
```
✅ Write → Run (pass) → Revert fix → Run (MUST FAIL) → Restore → Run (pass)
❌ "I've written a regression test" (without red-green verification)
```

**Build (VSTO/MSBuild):**
```
✅ [Run: msbuild.exe OfficeAddins.sln -verbosity:minimal -nologo] [See: Build succeeded]
❌ "Linter passed" (linter doesn't check compilation)
❌ "Code looks right" (not a build)
```

**Requirements:**
```
✅ Re-read plan → Create checklist → Verify each → Report gaps or completion
❌ "Tests pass, feature complete"
```

**DDD/Architecture verification:**
```
✅ Run quality-guardian agent → Read report → Verify boundaries respected
❌ "Follows Clean Architecture pattern" (visual inspection)
```

**Agent delegation:**
```
✅ Agent reports success → Check VCS diff → Verify changes → Run tests → Report actual state
❌ Trust agent report
```

## C# Specific Patterns

**Critical for this project:**

1. **MSBuild verification** (NOT `dotnet build`):
   ```bash
   msbuild.exe OfficeAddins.sln -verbosity:minimal -nologo
   ```
   Look for: "Build succeeded" / exit code 0

2. **Test verification**:
   ```bash
   dotnet test
   ```
   Look for: "0 Failed" count, total passed/skipped

3. **BDD scenario verification**:
   ```bash
   dotnet test --filter "Category=BDD"
   ```
   Look for: Specific feature scenarios passing

4. **COM disposal check** (VSTO-specific):
   - Can't automate - requires memory profiler
   - Manual verification in code review
   - Trust agent NOT sufficient

## When To Apply

**ALWAYS before:**
- ANY variation of success/completion claims
- ANY expression of satisfaction
- ANY positive statement about work state
- Committing, PR creation, task completion
- Moving to next task
- Delegating to agents

**Rule applies to:**
- Exact phrases
- Paraphrases and synonyms
- Implications of success
- ANY communication suggesting completion/correctness

## Why This Matters

From hard-learned lessons:
- "I don't believe you" - trust broken with user
- Undefined functions shipped - would crash in production
- Missing requirements shipped - incomplete features
- Time wasted on false completion → redirect → rework
- Violates core value: honesty over speed

## The Bottom Line

**No shortcuts for verification.**

Run the command. Read the output. THEN claim the result.

This is non-negotiable.
