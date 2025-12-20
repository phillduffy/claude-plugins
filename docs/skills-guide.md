# Skills Guide

## What Are Skills?

Skills are domain knowledge packages that Claude loads on-demand. They provide:

- **Procedural knowledge** - How to do specific tasks
- **Reference material** - Patterns, examples, anti-patterns
- **Context-specific guidance** - Tailored to your codebase

## Available Skills

### C# Development

#### csharp-pragmatic-architecture

Pragmatic Clean Architecture patterns for C#.

**Includes:**
- Result pattern implementation
- Vertical slice organization
- Pure core / impure shell separation
- Common anti-patterns to avoid

**Use when:** Building new features, reviewing architecture, refactoring

#### domain-driven-design

Domain-Driven Design tactical patterns.

**Includes:**
- Aggregate root design
- Value Object patterns
- Domain events
- Bounded context integration

**Use when:** Modeling domain logic, designing aggregates

### Testing

#### bdd-patterns

Behavior-Driven Development with Reqnroll/SpecFlow.

**Includes:**
- Gherkin scenario patterns
- Step definition reuse
- Test dependency injection
- Scenario anti-patterns

**Use when:** Writing feature files, designing step definitions

### Documentation

#### adr-writing

Architecture Decision Records.

**Includes:**
- ADR format comparison (MADR, Y-statements, etc.)
- Governance patterns
- Common anti-patterns

**Use when:** Documenting architectural decisions

#### technical-writing

Documentation best practices.

**Includes:**
- Diataxis framework (tutorials, how-tos, reference, explanation)
- API documentation patterns
- Diagrams as code

**Use when:** Writing documentation, API specs

### DevOps

#### devops-practices

CI/CD and deployment patterns.

**Includes:**
- GitHub Actions patterns
- Deployment strategies
- Incident management

**Use when:** Setting up pipelines, planning deployments

#### observability-patterns

Logging, tracing, and monitoring.

**Includes:**
- Structured logging patterns
- OpenTelemetry for .NET
- Alerting strategies

**Use when:** Adding observability to services

### Debugging

#### debugging-techniques

Systematic debugging approaches.

**Includes:**
- Systematic isolation method
- Memory profiling
- Production debugging

**Use when:** Investigating bugs, performance issues

### VSTO/Office Development

#### vsto-com-interop

COM object lifecycle management.

**Includes:**
- RCW internals
- Two-dot rule
- ComObjectTracker pattern
- Common anti-patterns

**Use when:** Working with Office COM objects

#### vsto-word-object-model

Word automation patterns.

**Includes:**
- Range operations
- Content controls
- Find/replace patterns
- Story types

**Use when:** Automating Word documents

#### vsto-build-deploy

VSTO CI/CD patterns.

**Includes:**
- MSBuild configuration
- GitHub Actions for VSTO
- Deployment checklist

**Use when:** Building/deploying VSTO add-ins

## How Skills Are Loaded

Skills load automatically based on context:

1. **Trigger detection** - Claude recognizes relevant context
2. **Skill activation** - SKILL.md is loaded
3. **Reference loading** - Additional references loaded as needed

## Skill Structure

```
skill-name/
├── SKILL.md           # Main instructions (always loaded)
├── examples/          # Code examples
│   └── example.cs
└── references/        # Detailed reference material
    ├── patterns.md
    └── anti-patterns.md
```

## Tips for Effective Skill Use

1. **Be specific** - Mention the domain to trigger relevant skills
2. **Ask for patterns** - "Show me the Result pattern" loads relevant examples
3. **Reference anti-patterns** - "What should I avoid?" surfaces common mistakes
