# pragmatic-dev-team

Ultimate C# development team with 16 specialized agents covering the complete product lifecycle.

## Overview

A comprehensive plugin providing research-backed AI agents for software development, from product planning to deployment. Based on principles from "The Pragmatic Programmer", "Clean Code", "Clean Architecture", "Domain-Driven Design", "BDD in Action", "Accelerate", and other industry-leading sources.

## Team Structure

### Coordinator
| Agent | Role |
|-------|------|
| **team-coordinator** | Orchestrates specialists, prevents notification fatigue, provides session summaries |

### Architecture Domain
| Agent | Focus |
|-------|-------|
| **architecture-reviewer** | Clean Architecture, DDD, dependency direction |
| **refactoring-advisor** | Code smells, safe transformations (Fowler) |
| **legacy-code-navigator** | Characterization tests, seams (Feathers) |

### Code Quality Domain
| Agent | Focus |
|-------|-------|
| **code-reviewer** | Maintainability, readability, patterns |
| **security-reviewer** | OWASP, threat modeling, vulnerabilities |
| **performance-analyst** | Bottlenecks, profiling, USE method |

### Quality Assurance Domain
| Agent | Focus |
|-------|-------|
| **bdd-strategist** | Gherkin scenarios, Three Amigos, living docs |
| **test-coverage-analyst** | Test pyramid, edge cases, automation |
| **exploratory-tester** | Charter-based testing, heuristics |

### Product & UX Domain
| Agent | Focus |
|-------|-------|
| **product-advocate** | User outcomes, acceptance criteria |
| **accessibility-reviewer** | WCAG, POUR principles, a11y |

### Operations Domain
| Agent | Focus |
|-------|-------|
| **release-advisor** | Progressive delivery, feature flags, rollback |
| **observability-advisor** | Logging, tracing, structured events |

### Research Domain
| Agent | Focus |
|-------|-------|
| **technical-researcher** | Build vs buy, library evaluation, TCO |
| **issue-manager** | GitHub issues, triage, actionable tickets |

## Skills (18)

### Architecture
- `csharp-pragmatic-architecture` - Core C# patterns (Result, Value Objects, Vertical Slices)
- `domain-driven-design` - DDD tactical and strategic patterns
- `adr-writing` - Architecture Decision Records

### Quality
- `bdd-patterns` - Gherkin, Given-When-Then, Reqnroll
- `debugging-techniques` - Agans' 9 Rules, systematic debugging

### Operations
- `devops-practices` - CI/CD, DORA metrics, pipelines
- `observability-patterns` - Structured logging, tracing, metrics

### Documentation
- `technical-writing` - Developer documentation best practices

### VSTO Specialist
- `vsto-com-interop` - COM cleanup, two-dot rule
- `vsto-word-object-model` - Word API patterns
- `vsto-build-deploy` - MSBuild, ClickOnce, deployment

## Commands

| Command | Purpose |
|---------|---------|
| `/review` | Code review on specified scope |
| `/architect` | Architecture review |
| `/test-plan` | Create BDD test plan for feature |
| `/research` | Technical research and evaluation |
| `/team` | Comprehensive team review or session summary |

## Usage Examples

**Get code reviewed:**
```
/review                    # Review uncommitted changes
/review src/Handlers/      # Review specific directory
```

**Architecture check:**
```
/architect                 # Check recent changes
/architect design          # Overall assessment
```

**Plan tests:**
```
/test-plan password reset feature
```

**Research decision:**
```
/research validation library for .NET
/research build vs buy for caching
```

**Team review:**
```
/team ready for PR         # Pre-PR comprehensive check
/team summary              # End-of-session summary
```

## Proactive Triggering

Based on research (90% user preference at 20s intervals):

| Level | Agents | When |
|-------|--------|------|
| **Proactive** | Security, Architecture | Critical issues only |
| **On-demand** | All others | User explicitly requests |
| **Batched** | Coordinator | End of session summary |

**Quality gates before proactive triggers:**
- Confidence > 85%
- Issue is Critical or Important
- 20s+ since last suggestion
- User not actively typing

## Installation

### Local Testing
```bash
claude --plugin-dir E:\Claude\plugins\pragmatic-dev-team
```

### Project Installation
Copy to your project's `.claude/plugins/` directory.

## Philosophy

This plugin embodies principles from:

| Source | Key Principles Applied |
|--------|----------------------|
| **Pragmatic Programmer** | DRY, Orthogonality, Good Enough Software |
| **Clean Code** | SOLID, Meaningful Names, Small Functions |
| **Clean Architecture** | Dependency Rule, Boundaries |
| **Domain-Driven Design** | Ubiquitous Language, Aggregates, Value Objects |
| **Refactoring** | Code Smells, Small Steps, Behavior Preservation |
| **Working Effectively with Legacy Code** | Characterization Tests, Seams |
| **BDD in Action** | Three Amigos, Concrete Examples, Living Docs |
| **Accelerate** | DORA Metrics, DevOps Practices |
| **Debugging: 9 Rules** | Systematic Debugging Approach |

## Reference Codebase

Examples drawn from `E:\Code\OfficeAddins` demonstrating:
- Result<T, Error> pattern
- Decorator chain for cross-cutting concerns
- Vertical slice organization
- Value Objects and aggregates
- CQRS without MediatR

## Library Preferences

**Recommended:**
- CSharpFunctionalExtensions (Result, Maybe, ValueObject)
- Scrutor (DI auto-registration, decorators)
- Reqnroll (BDD/Gherkin)
- Serilog (Structured logging)

**Avoided:**
- MediatR (licensing concerns)
- Heavy ORMs that obscure queries

## File Structure

```
pragmatic-dev-team/
├── .claude-plugin/
│   └── plugin.json
├── agents/
│   ├── team-coordinator.md
│   ├── architecture-reviewer.md
│   ├── code-reviewer.md
│   ├── security-reviewer.md
│   └── ... (16 agents)
├── skills/
│   ├── csharp-pragmatic-architecture/
│   ├── domain-driven-design/
│   ├── bdd-patterns/
│   └── ... (11+ skills)
├── commands/
│   ├── review.md
│   ├── architect.md
│   ├── test-plan.md
│   ├── research.md
│   └── team.md
├── hooks/
│   └── hooks.json
└── README.md
```

## Contributing

This is a personal plugin. Fork and customize for your own team's needs.
