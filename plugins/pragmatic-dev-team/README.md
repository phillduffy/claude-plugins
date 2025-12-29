# pragmatic-dev-team

![Version](https://img.shields.io/badge/version-2.1.0-blue)
![Claude Code](https://img.shields.io/badge/claude--code->=1.0.0-green)
![License](https://img.shields.io/badge/license-MIT-yellow)
![Agents](https://img.shields.io/badge/agents-6-purple)
![Skills](https://img.shields.io/badge/skills-13-orange)

**Process-enforcing development team with iron laws, gate functions, and anti-hallucination constraints.**

## Overview

A focused plugin providing **6 consolidated agents** and 13 skills for the complete software development lifecycle. Unlike advisory plugins, pragmatic-dev-team **enforces** development processes through mandatory verification gates that prevent hallucinations and ensure production-ready code.

**v2.0 Changes:** Consolidated from 16 agents to 6 orthogonal specialists following [Anthropic's best practices](https://www.anthropic.com/engineering/claude-code-best-practices) - each agent has a narrow, distinct purpose with no overlap.

**Core Philosophy:**
- **Iron Laws** - Every skill has an unbreakable rule (e.g., "NO IMPLEMENTATION WITHOUT SCENARIO FIRST")
- **Gate Functions** - Mandatory verification steps before proceeding (e.g., check dependency direction before writing code)
- **Verification Before Completion** - No success claims without fresh evidence
- **Anti-Hallucination Constraints** - Agents must read actual files, quote exact code, cite file:line numbers

**This plugin doesn't suggest - it gates. It doesn't advise - it enforces.**

## Installation

### Claude Code (Local Testing)

```bash
# From your project directory
claude --plugin-dir E:\Claude\plugins\pragmatic-dev-team
```

### Project Installation

```bash
# Copy to your project's plugin directory
cp -r E:\Claude\plugins\pragmatic-dev-team <your-project>/.claude/plugins/
```

### Verify Installation

```bash
# Check that commands appear
/help

# Should see:
# /review - Code review on specified scope
# /architect - Architecture review
# /test-plan - Create BDD test plan for feature
# /research - Technical research and evaluation
# /team - Comprehensive team review or session summary
```

## Core Concepts

### Iron Laws

Every skill enforces an **Iron Law** - an unbreakable rule that prevents common development mistakes:

| Skill | Iron Law | Consequence |
|-------|----------|-------------|
| BDD Patterns | `NO IMPLEMENTATION WITHOUT SCENARIO FIRST` | Code written before scenarios? Delete it. Start over. |
| C# Architecture | `NO CODE WITHOUT CHECKING DEPENDENCY DIRECTION FIRST` | Haven't verified layer dependencies? Cannot write code. |
| Verification Before Completion | `NO COMPLETION CLAIMS WITHOUT FRESH VERIFICATION EVIDENCE` | Claiming "tests pass" without running them? Not allowed. |
| VSTO COM Interop | `NO COM REFERENCE WITHOUT DISPOSAL` | Created COM object without explicit disposal? Violation. |

**Key Insight:** These aren't suggestions. They're process gates that halt progress until satisfied.

### Gate Functions

Before allowing certain actions, skills require passing through verification gates:

**Example: The Architecture Gate (from C# Pragmatic Architecture skill)**

```
Before writing ANY code, you MUST answer:

Step 1: Identify Your Layer
  - Where am I writing this code?
  - Domain? Application? Infrastructure? Presentation?

Step 2: Know Your Dependencies
  - What CAN this layer depend on?
  - Domain → NOTHING
  - Application → Domain only
  - Infrastructure → Domain, Application
  - Presentation → Application, Infrastructure

Step 3: Verify Direction
  - What layer is the type I'm importing?
  - Does my layer point toward it? (Yes → OK)
  - Does it point toward me? (Yes → VIOLATION)
  - Does it point sideways? (Yes → VIOLATION)

If you can't answer these questions: STOP. Cannot proceed.
```

### Verification Before Completion

The plugin enforces evidence-based development:

```
BEFORE claiming "tests pass", you must:
1. Run: dotnet test (in THIS message)
2. Read: Full output
3. Verify: 0 failed, X passed
4. THEN: Claim tests pass

No shortcuts. No "should pass". No trust without verify.
```

**Rationale:** Prevents AI hallucinations about code correctness. If you didn't run the command, you can't claim the result.

### Anti-Hallucination Constraints

Every agent has strict constraints preventing fabricated findings:

**From code-reviewer agent:**
```yaml
constraints:
  - MUST read the actual file using the Read tool
  - MUST quote the exact code from the file (not fabricated examples)
  - MUST cite file:line where you found the issue
  - NEVER generate example code - only quote code you actually read
  - NEVER cite line numbers you haven't verified with the Read tool
  - ALWAYS say "No issues found" if you can't find an issue after searching
```

**Result:** Agents cannot report issues they haven't verified in actual files. No invented problems.

## Skills Reference

### Architecture & Design

| Skill | Iron Law | When to Use | Gate Function |
|-------|----------|-------------|---------------|
| **csharp-pragmatic-architecture** | `NO CODE WITHOUT CHECKING DEPENDENCY DIRECTION` | Adding classes, dependencies, domain logic | The Architecture Gate (3-step layer verification) |
| **domain-driven-design** | `NO AGGREGATE WITHOUT BOUNDARY DEFINITION` | Designing aggregates, bounded contexts | Aggregate Boundary Check |
| **adr-writing** | `NO SIGNIFICANT DECISION WITHOUT ADR` | Architectural decisions, tech choices | Decision Impact Assessment |

### Quality & Testing

| Skill | Iron Law | When to Use | Gate Function |
|-------|----------|-------------|---------------|
| **bdd-patterns** | `NO IMPLEMENTATION WITHOUT SCENARIO FIRST` | Features, bug fixes, acceptance criteria | Three Amigos + Red-Green-Refactor |
| **debugging-techniques** | `NO FIX WITHOUT ROOT CAUSE` | Debugging, systematic problem-solving | 4-Phase Root Cause Process |
| **verification-before-completion** | `NO COMPLETION CLAIMS WITHOUT EVIDENCE` | Before any success claim, commit, or PR | Evidence Verification Gate |

### Operations & DevOps

| Skill | Iron Law | When to Use | Gate Function |
|-------|----------|-------------|---------------|
| **devops-practices** | `NO DEPLOYMENT WITHOUT PIPELINE` | CI/CD, DORA metrics, pipelines | Pipeline Quality Check |
| **observability-patterns** | `NO PRODUCTION CODE WITHOUT LOGS` | Structured logging, tracing, metrics | Observability Coverage Check |

### Documentation

| Skill | Iron Law | When to Use | Gate Function |
|-------|----------|-------------|---------------|
| **technical-writing** | `NO API WITHOUT DOCUMENTATION` | Developer docs, README files | Documentation Completeness Check |

### VSTO Specialist Skills

| Skill | Iron Law | When to Use | Gate Function |
|-------|----------|-------------|---------------|
| **vsto-com-interop** | `NO COM REFERENCE WITHOUT DISPOSAL` | COM cleanup, two-dot rule | COM Lifetime Verification |
| **vsto-word-object-model** | `NO RANGE MANIPULATION WITHOUT BOUNDS CHECK` | Word API patterns, content controls | Range Safety Check |
| **vsto-build-deploy** | `MUST USE MSBUILD, NOT DOTNET BUILD` | MSBuild, ClickOnce, deployment | Build Tool Verification |

### Meta Skills

| Skill | Iron Law | When to Use | Gate Function |
|-------|----------|-------------|---------------|
| **team-orchestration** | `NO FEATURE WITHOUT SPECIALIST REVIEW` | Complex tasks requiring multiple specialists | Specialist Selection + Review Loops |

## Agents Reference

**6 Consolidated Specialists** - Each has a narrow, orthogonal purpose with no overlap.

| Agent | Focus Areas | Triggers | Anti-Hallucination Constraint |
|-------|-------------|----------|-------------------------------|
| **team-coordinator** | Orchestrates specialists, release planning, session summaries | On-demand via `/team` | Delegates to specialists, never analyzes code directly |
| **bdd-strategist** | Requirements, BDD scenarios, test coverage, exploratory testing, user outcomes | On-demand via `/test-plan` | NO IMPLEMENTATION WITHOUT SCENARIO FIRST |
| **architecture-reviewer** | Clean Architecture, DDD, code smells, refactoring, legacy code seams | Proactive (critical violations), on-demand via `/architect` | Must use Grep to verify dependencies, quote exact code |
| **code-reviewer** | Quality, performance, observability (logging), accessibility (WCAG) | Proactive (pre-commit), on-demand via `/review` | Must quote exact code from files, cite file:line numbers |
| **security-reviewer** | OWASP, threat modeling, vulnerabilities | Proactive (security-critical code), on-demand | Must identify actual vulnerabilities in codebase |
| **issue-specialist** | GitHub issues, technical research, build-vs-buy evaluation | On-demand via `/research` | Issue Quality Gate, Comparison Matrix Required |

### Consolidated Capabilities

| Old Agents | Now In | Why |
|------------|--------|-----|
| test-coverage-analyst, exploratory-tester, product-advocate | **bdd-strategist** | All about requirements and testing |
| refactoring-advisor, legacy-code-navigator | **architecture-reviewer** | All about code structure decisions |
| performance-analyst, observability-advisor, accessibility-reviewer | **code-reviewer** | All about code quality concerns |
| technical-researcher | **issue-specialist** | Research feeds into actionable issues |
| release-advisor | **team-coordinator** | Release planning is coordination |

## Team Orchestration

When tasks require multiple specialists, the **team-coordinator** orchestrates reviews:

```
Task: New Feature → Document Template Validation

1. bdd-strategist (First)
   ✅ Define scenarios + acceptance criteria
   ✅ Identify edge cases + test coverage plan
   Gate: Scenarios approved? → YES, proceed

2. architecture-reviewer
   ✅ Verify layer boundaries
   ❌ ValidationService in wrong layer
   Fix: Move to Domain layer
   Re-review: ✅ Approved → Proceed

3. Implementation
   [Developer implements]

4. code-reviewer
   ✅ Check quality, performance, observability
   ❌ Magic strings found
   Fix: Extract constants
   Re-review: ✅ Approved → Proceed

Result: Feature ready for merge
```

**Key Principle:** Each specialist must ✅ approve before moving to next. No shortcuts.

## Commands

| Command | Usage | What It Does |
|---------|-------|--------------|
| `/review` | `/review` or `/review src/Handlers/` | Code review on specified scope (uncommitted changes or directory) |
| `/architect` | `/architect` or `/architect design` | Architecture review (recent changes or overall assessment) |
| `/test-plan` | `/test-plan password reset feature` | Create BDD test plan with Gherkin scenarios |
| `/research` | `/research validation library for .NET` | Technical research with comparison matrix |
| `/team` | `/team ready for PR` or `/team summary` | Comprehensive multi-specialist review or end-of-session summary |

**Example Workflow:**

```bash
# 1. Start feature with BDD scenarios
/test-plan "User uploads invalid file format"

# 2. Implement based on scenarios
[write code]

# 3. Architecture check
/architect

# 4. Code quality check
/review

# 5. Pre-PR comprehensive review
/team ready for PR

# 6. End of session
/team summary
```

## Hooks

**PostToolUse Hook** (C# file modifications):

```json
{
  "matcher": "Write|Edit",
  "hooks": [
    {
      "type": "prompt",
      "prompt": "Check if modified file is C# (.cs extension). If yes: 'C# modified. Run /review when ready.' Otherwise: output nothing.",
      "timeout": 10000
    }
  ]
}
```

**Result:** Non-intrusive reminder to run `/review` after modifying C# files. Respects user's flow.

## Best Practices

### 1. Let Gate Functions Guide You

**Don't fight the gates - use them as checklists:**

```bash
# ❌ BAD: Jump into coding
# "I'll add this handler real quick"

# ✅ GOOD: Use Architecture Gate
# Step 1: I'm in Core.Application
# Step 2: Application can depend on Domain only
# Step 3: Verify each import points inward
# NOW write code
```

### 2. Trust the Iron Laws

**They prevent common mistakes:**

```bash
# ❌ BAD: "I'll write scenarios after to document it"
# Result: Scenarios pass immediately = prove nothing

# ✅ GOOD: Write scenario → Watch fail → Implement → Pass
# Result: Scenario caught misunderstandings BEFORE coding
```

### 3. Use Verification Before Completion

**Prevent hallucination about code status:**

```bash
# ❌ BAD: "The tests should pass now"
# Result: Maybe they don't, hallucination

# ✅ GOOD:
dotnet test
# [Read output: 0 Failed, 42 Passed]
# "All 42 tests pass"
```

### 4. Leverage Team Orchestration

**For complex work, let specialists coordinate:**

```bash
# ❌ BAD: Single general review
/review   # Might miss architecture issues, missing scenarios

# ✅ GOOD: Orchestrated team
/team ready for PR
# Runs: BDD check, architecture review, code review, test coverage
# Each specialist must ✅ before next
```

### 5. Respect Anti-Hallucination Constraints

**Agents cannot invent issues:**

```bash
# If agent says:
# "No issues found after checking 15 files"

# You can trust:
# - Agent actually read those files
# - Agent didn't fabricate problems
# - Agent would report if issues existed

# Why: Constraints prevent false positives
```

## Comparison with Superpowers

| Aspect | Superpowers | pragmatic-dev-team |
|--------|-------------|-------------------|
| **Approach** | Workflow automation | Process enforcement |
| **Philosophy** | Systematic over ad-hoc | Iron laws + gates |
| **Focus** | TDD, planning, subagents | DDD, architecture, verification |
| **Enforcement** | Skills trigger workflows | Gates halt until satisfied |
| **Verification** | Evidence over claims | Mandatory fresh evidence |
| **Best Together** | Superpowers orchestrates workflow | pragmatic-dev-team enforces quality gates |

**Recommended Combo:**

```
Superpowers:
  - /brainstorm (refine idea)
  - /write-plan (create implementation plan)

pragmatic-dev-team:
  - /test-plan (BDD scenarios)
  - /architect (verify design)
  - /review (code quality)

Superpowers:
  - /execute-plan (implement with subagents)

pragmatic-dev-team:
  - /team ready for PR (comprehensive review)
```

**Superpowers** gets you moving fast. **pragmatic-dev-team** ensures you're moving in the right direction.

## Philosophy

This plugin embodies principles from:

| Source | Key Principles Applied |
|--------|----------------------|
| **Pragmatic Programmer** | DRY, Orthogonality, Good Enough Software, Broken Windows |
| **Clean Code** | SOLID, Meaningful Names, Small Functions, No Side Effects |
| **Clean Architecture** | Dependency Rule, Boundaries, Testability |
| **Domain-Driven Design** | Ubiquitous Language, Aggregates, Value Objects, Bounded Contexts |
| **Refactoring** | Code Smells, Small Steps, Behavior Preservation |
| **Working Effectively with Legacy Code** | Characterization Tests, Seams |
| **BDD in Action** | Three Amigos, Concrete Examples, Living Documentation |
| **Accelerate** | DORA Metrics, DevOps Practices, Continuous Delivery |
| **Debugging: 9 Rules** | Systematic Approach, Root Cause Analysis |

**Core Innovation:** Taking these principles and making them **enforceable** through iron laws and gate functions.

## Reference Codebase

Examples drawn from real C# VSTO project demonstrating:

**Patterns:**
- Result<T, Error> pattern for error handling
- Decorator chain for cross-cutting concerns (logging, transactions)
- Vertical slice organization (by feature, not layer)
- Value Objects with private constructors
- Aggregates with domain events
- CQRS without MediatR (licensing concerns)

**Location:** `E:\Code\OfficeAddins`

**Skills reference real code:**
- `skills/csharp-pragmatic-architecture/references/your-codebase.md`
- `skills/domain-driven-design/references/your-codebase.md`
- `skills/bdd-patterns/references/your-codebase.md`
- `skills/vsto-word-object-model/references/your-codebase.md`

## Library Preferences

**Recommended:**
- **CSharpFunctionalExtensions** - Result, Maybe, ValueObject base classes
- **Scrutor** - DI auto-registration, decorator pattern
- **Reqnroll** - BDD/Gherkin (SpecFlow successor)
- **Serilog** - Structured logging
- **FluentValidation** - Validation rules
- **Polly** - Resilience patterns

**Avoided:**
- **MediatR** - Licensing concerns (relicensed under Elastic License 2.0)
- **Heavy ORMs** - Prefer explicit queries over magic

**Why This Matters:** Skills recommend libraries aligned with these preferences.

## File Structure

```
pragmatic-dev-team/
├── .claude-plugin/
│   └── plugin.json              # Plugin metadata
├── agents/                      # 6 consolidated specialists
│   ├── team-coordinator.md      # Orchestration + release planning
│   ├── bdd-strategist.md        # Requirements, testing, coverage
│   ├── architecture-reviewer.md # Architecture, refactoring, legacy
│   ├── code-reviewer.md         # Quality, perf, observability, a11y
│   ├── security-reviewer.md     # OWASP, threats, vulnerabilities
│   └── issue-specialist.md      # Issues, research, build-vs-buy
├── skills/
│   ├── csharp-pragmatic-architecture/
│   │   ├── SKILL.md
│   │   └── references/
│   │       ├── result-pattern.md
│   │       ├── illegal-states.md
│   │       ├── vertical-slices.md
│   │       ├── pure-core.md
│   │       ├── anti-patterns.md
│   │       ├── meta-principles.md
│   │       └── your-codebase.md
│   ├── domain-driven-design/
│   │   ├── SKILL.md
│   │   └── references/
│   │       ├── aggregate-design.md
│   │       ├── bounded-context-integration.md
│   │       ├── csharp-patterns.md
│   │       └── your-codebase.md
│   ├── bdd-patterns/
│   │   ├── SKILL.md
│   │   └── references/
│   │       ├── scenario-anti-patterns.md
│   │       ├── reqnroll-advanced.md
│   │       ├── step-reuse.md
│   │       └── your-codebase.md
│   ├── debugging-techniques/
│   ├── devops-practices/
│   ├── observability-patterns/
│   ├── adr-writing/
│   ├── technical-writing/
│   ├── vsto-com-interop/
│   ├── vsto-word-object-model/
│   ├── vsto-build-deploy/
│   ├── verification-before-completion/
│   └── team-orchestration/
├── commands/
│   ├── review.md
│   ├── architect.md
│   ├── test-plan.md
│   ├── research.md
│   └── team.md
├── hooks/
│   └── hooks.json              # PostToolUse hook for C# files
└── README.md
```

## Troubleshooting

| Problem | Solution |
|---------|----------|
| Agent not triggering | Use explicit command: `/review`, `/architect`, `/test-plan` |
| Hooks not working | Restart Claude Code, run `claude --debug` |
| Skill not loading | Check SKILL.md frontmatter syntax |
| Build check failing | Run: `msbuild -v:m && dotnet test` |
| Stop hook blocking exit | Run build+tests to satisfy verification gate |
| Security hook blocking | Legitimate ops get auto-approved; file contains sensitive patterns |

### Debug Mode

```bash
# See hook execution, plugin loading, agent triggering
claude --debug
```

### Validate Plugin Structure

```bash
# Load plugin and verify
claude --plugin-dir E:\Claude\plugins\pragmatic-dev-team
/help  # Should show /review, /architect, /test-plan, /research, /team
```

## Contributing

This is a personal plugin tailored to specific development practices and reference codebase. **Not accepting external contributions**, but encouraged to:

1. **Fork** for your own team
2. **Customize** iron laws to your context
3. **Adapt** gate functions to your architecture
4. **Modify** agents for your tech stack

**Tips for customization:**
- Update `references/your-codebase.md` files to point to YOUR patterns
- Adjust iron laws severity based on team experience
- Modify agent proactivity thresholds
- Add/remove specialists based on tech stack

## License

Private plugin - see plugin author for licensing details.

## Support

This plugin is maintained as part of a personal development workflow. For questions about:

- **Concepts** - Reference source books listed in Philosophy section
- **Implementation** - Read skill SKILL.md files and references
- **Customization** - Fork and adapt to your needs

## Key Takeaways

**What makes this plugin different:**

1. **Process Enforcement** - Iron laws halt progress until satisfied
2. **Gate Functions** - Mandatory verification steps before proceeding
3. **Anti-Hallucination** - Agents must read actual files, quote real code
4. **Verification Before Completion** - No success claims without fresh evidence
5. **Research-Backed** - Built on principles from industry-leading sources

**This plugin is opinionated.** It enforces a specific development philosophy. If your team values different practices, fork and customize.

**This plugin is strict.** It won't let you skip steps "just this once". If you want advisory suggestions, this isn't the plugin for you.

**This plugin is honest.** It prevents AI hallucinations about code correctness through mandatory verification gates.

Use it to build production-ready code with confidence.
