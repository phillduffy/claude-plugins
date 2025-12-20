# Agent Reference

## How Agents Work

Agents are specialized AI assistants that Claude Code can spawn for specific tasks. Each agent has:

- **Focused expertise** - Deep knowledge in one domain
- **Specific tools** - Only the tools needed for their job
- **Clear triggers** - When to invoke them

## Agent Hierarchy

```
team-coordinator (orchestrator)
├── code-reviewer
├── security-reviewer
├── architecture-reviewer
├── performance-analyst
├── test-coverage-analyst
├── bdd-strategist
├── accessibility-reviewer
├── product-advocate
├── refactoring-advisor
├── legacy-code-navigator
├── exploratory-tester
├── release-advisor
├── observability-advisor
├── technical-researcher
└── issue-manager
```

## The Team Coordinator

The `team-coordinator` is special - it **orchestrates** other agents but doesn't do reviews itself.

When you ask for a "team review" or "comprehensive review", the coordinator:
1. Assesses which domains are relevant
2. Spawns appropriate specialist agents in parallel
3. Collects and deduplicates findings
4. Synthesizes a unified report

### Example Flow

```
User: "Review this auth feature before PR"
     ↓
team-coordinator spawns:
├── security-reviewer (auth = high security risk)
├── code-reviewer (always for PR)
└── test-coverage-analyst (verify test coverage)
     ↓
Each specialist analyzes independently
     ↓
team-coordinator synthesizes:
## Team Review Summary
### Critical
- [security-reviewer] SQL injection in login query
### Important
- [code-reviewer] Long method in AuthService
### Suggestions
- [test-coverage-analyst] Missing edge case tests
```

## Specialist Agents

### Code Quality Domain

| Agent | When to Use | Key References |
|-------|-------------|----------------|
| **code-reviewer** | Pre-commit, PR creation | Google Engineering Practices, Clean Code |
| **security-reviewer** | Auth, data handling, APIs | OWASP Top 10 |
| **architecture-reviewer** | New modules, design decisions | Clean Architecture, DDD |
| **performance-analyst** | Slow code, optimization | USE Method (Brendan Gregg) |

### Testing Domain

| Agent | When to Use | Key References |
|-------|-------------|----------------|
| **test-coverage-analyst** | Test planning, coverage gaps | Test Pyramid |
| **bdd-strategist** | Feature planning, Gherkin | BDD in Action |
| **exploratory-tester** | Bug hunting, edge cases | Explore It! |

### Product Domain

| Agent | When to Use | Key References |
|-------|-------------|----------------|
| **product-advocate** | Feature scoping, requirements | Marty Cagan |
| **accessibility-reviewer** | UI changes, forms | WCAG, POUR |

### Operations Domain

| Agent | When to Use | Key References |
|-------|-------------|----------------|
| **release-advisor** | Deployments, rollbacks | Progressive Delivery |
| **observability-advisor** | Logging, monitoring | OpenTelemetry |

### Code Evolution

| Agent | When to Use | Key References |
|-------|-------------|----------------|
| **refactoring-advisor** | Code smells, improvements | Fowler's Refactoring |
| **legacy-code-navigator** | Untested code, seams | Feathers' Legacy Code |

### Research

| Agent | When to Use | Key References |
|-------|-------------|----------------|
| **technical-researcher** | Library evaluation | Build vs Buy analysis |
| **issue-manager** | Issue creation, backlog | GitHub best practices |

## Tools by Agent

| Agent | Tools |
|-------|-------|
| team-coordinator | Read, Grep, Glob, Bash, Task, TodoWrite |
| code-reviewer | Read, Grep, Glob, Bash, TodoWrite |
| security-reviewer | Read, Grep, Glob, Bash |
| architecture-reviewer | Read, Grep, Glob, Bash |
| performance-analyst | Read, Grep, Glob, Bash |
| test-coverage-analyst | Read, Grep, Glob, Bash, TodoWrite |
| technical-researcher | Read, Grep, Glob, Bash, WebSearch, WebFetch |
| (all others) | Read, Grep, Glob, Bash |

## Invoking Agents

Agents are typically invoked automatically based on context, but you can request specific specialists:

```
"Run a security review on this endpoint"
→ security-reviewer

"Check the architecture of this module"
→ architecture-reviewer

"Get the team's input on this design"
→ team-coordinator (spawns relevant specialists)
```
