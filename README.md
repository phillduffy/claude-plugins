# Pragmatic Dev Team

A collection of specialized AI agents, skills, and tools for Claude Code focused on pragmatic C# development with Clean Architecture, DDD, and BDD practices.

## Quick Start

```bash
claude plugins:add phillduffy-marketplace/pragmatic-dev-team
```

## What's Included

### Agents (16)

Specialized AI agents that can be invoked for specific tasks:

| Agent | Purpose |
|-------|---------|
| **team-coordinator** | Orchestrates multiple specialists for comprehensive reviews |
| **code-reviewer** | Code quality, maintainability, best practices |
| **security-reviewer** | OWASP vulnerabilities, secure coding |
| **architecture-reviewer** | Clean Architecture, DDD compliance |
| **performance-analyst** | Bottlenecks, optimization |
| **test-coverage-analyst** | Test strategy, coverage gaps |
| **bdd-strategist** | Gherkin scenarios, specification by example |
| **accessibility-reviewer** | WCAG compliance, a11y |
| **product-advocate** | User outcomes, acceptance criteria |
| **refactoring-advisor** | Code smells, safe transformations |
| **legacy-code-navigator** | Working with untested code |
| **exploratory-tester** | Charter-based testing, heuristics |
| **release-advisor** | Deployment strategies, rollback |
| **observability-advisor** | Logging, tracing, metrics |
| **technical-researcher** | Library evaluation, build vs buy |
| **issue-manager** | GitHub issues, backlog organization |

### Commands (5)

Slash commands for quick access:

| Command | Description |
|---------|-------------|
| `/team` | Comprehensive team review |
| `/review` | Code review on current changes |
| `/architect` | Architecture review |
| `/research` | Technical research and evaluation |
| `/test-plan` | Test strategy planning |

### Skills (12)

Domain knowledge packages loaded on-demand:

| Skill | Focus |
|-------|-------|
| **csharp-pragmatic-architecture** | Clean Architecture patterns, Result pattern, vertical slices |
| **domain-driven-design** | Aggregates, Value Objects, Bounded Contexts |
| **bdd-patterns** | Reqnroll/SpecFlow, Gherkin best practices |
| **adr-writing** | Architecture Decision Records |
| **debugging-techniques** | Systematic debugging, memory profiling |
| **devops-practices** | GitHub Actions, deployment strategies |
| **observability-patterns** | Structured logging, OpenTelemetry |
| **technical-writing** | Diataxis framework, API docs |
| **vsto-com-interop** | COM object lifecycle, RCW management |
| **vsto-word-object-model** | Word automation, ranges, content controls |
| **vsto-build-deploy** | VSTO CI/CD, deployment |

## Documentation

- [Plugin Reference](docs/plugin-reference.md) - Full plugin details
- [Agent Reference](docs/agent-reference.md) - How agents work together
- [Skills Guide](docs/skills-guide.md) - Using skills effectively

## Philosophy

This plugin embodies pragmatic software development:

- **KISS** - Keep it simple, avoid over-engineering
- **YAGNI** - Don't build features until needed
- **Clean Architecture** - Dependencies point inward
- **DDD** - Ubiquitous language, rich domain models
- **BDD** - Executable specifications, living documentation

## Architecture

```
plugins/
  pragmatic-dev-team/
    agents/          # 16 specialized AI agents
    commands/        # 5 slash commands
    skills/          # 12 domain knowledge packages
    hooks/           # Automation hooks
```

## Contributing

Contributions welcome! Please read the contribution guidelines first.

## License

MIT License - see [LICENSE](LICENSE)
