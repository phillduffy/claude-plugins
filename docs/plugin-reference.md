# Plugin Reference

## pragmatic-dev-team

A comprehensive plugin providing AI-powered development assistance focused on pragmatic C# development practices.

### Overview

| Property | Value |
|----------|-------|
| **Version** | 0.0.1 |
| **Author** | Phill Duffy |
| **License** | MIT |

### Components

#### Agents (16)

Agents are specialized AI assistants that can be spawned for specific tasks.

##### Team Coordination
- **team-coordinator** - Orchestrates multiple specialists for comprehensive reviews. Delegates to other agents via Task tool.

##### Code Quality
- **code-reviewer** - Reviews code for maintainability, readability, and best practices
- **security-reviewer** - Identifies OWASP vulnerabilities and secure coding issues
- **architecture-reviewer** - Validates Clean Architecture and DDD compliance
- **performance-analyst** - Identifies bottlenecks and optimization opportunities

##### Testing
- **test-coverage-analyst** - Analyzes test coverage and recommends strategies
- **bdd-strategist** - Writes Gherkin scenarios and facilitates specification by example
- **exploratory-tester** - Charter-based testing using Elisabeth Hendrickson's methods

##### Product & UX
- **product-advocate** - Ensures features deliver user value
- **accessibility-reviewer** - WCAG compliance and inclusive design

##### Operations
- **release-advisor** - Deployment strategies, feature flags, rollback plans
- **observability-advisor** - Logging, tracing, and metrics guidance

##### Code Evolution
- **refactoring-advisor** - Martin Fowler's refactoring patterns
- **legacy-code-navigator** - Michael Feathers' legacy code techniques

##### Research & Planning
- **technical-researcher** - Library evaluation and build-vs-buy analysis
- **issue-manager** - GitHub issue management and backlog organization

#### Commands (5)

| Command | Description |
|---------|-------------|
| `/team` | Get comprehensive team review from multiple specialists |
| `/review` | Run code review on current changes |
| `/architect` | Run architecture review |
| `/research` | Evaluate libraries or technical decisions |
| `/test-plan` | Create test strategy for a feature |

#### Skills (12)

Skills are domain knowledge packages loaded on-demand.

##### C# Development
- **csharp-pragmatic-architecture** - Clean Architecture, Result pattern, vertical slices
- **domain-driven-design** - Aggregates, Value Objects, Bounded Contexts, domain events

##### Testing
- **bdd-patterns** - Reqnroll/SpecFlow patterns, step reuse, scenario design

##### Documentation
- **adr-writing** - Architecture Decision Records format and governance
- **technical-writing** - Diataxis framework, API documentation

##### DevOps
- **devops-practices** - GitHub Actions, deployment strategies, incident management
- **observability-patterns** - Structured logging, OpenTelemetry, alerting

##### Debugging
- **debugging-techniques** - Systematic isolation, memory profiling, production debugging

##### VSTO/Office Development
- **vsto-com-interop** - COM object lifecycle, RCW management, two-dot rule
- **vsto-word-object-model** - Word ranges, content controls, find/replace
- **vsto-build-deploy** - VSTO CI/CD with MSBuild and GitHub Actions

### Installation

```bash
claude plugins:add phillduffy-marketplace/pragmatic-dev-team
```

### Usage Examples

```bash
# Get team review before PR
/team

# Run architecture review
/architect

# Evaluate a library
/research "Should I use FluentValidation or DataAnnotations?"
```
