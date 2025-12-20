# pragmatic-csharp

Rust-inspired pragmatic C# architecture principles for Claude Code.

## Overview

This plugin helps enforce and teach 10 core architecture principles inspired by Rust's type safety, functional programming, and pragmatic software design.

### 10 Core Principles (Rust-inspired)

| # | Principle | Core Idea |
|---|-----------|-----------|
| 1 | Make Illegal States Unrepresentable | Types prevent bugs at compile time |
| 2 | Composition via Interfaces | Small interfaces, no inheritance for code sharing |
| 3 | Result over Exceptions | Return `Result<T, Error>`, don't throw |
| 4 | Data/Behavior Separation | Immutable records + stateless services |
| 5 | Pure Core, Impure Shell | Side effects at edges only |
| 6 | Vertical Slices | Organize by feature, not layer |
| 7 | Explicit Dependencies | Constructor injection only |
| 8 | YAGNI | Solve current problem simply |
| 9 | Parse, Don't Validate | Transform to trusted types at boundary |
| 10 | Options Pattern | Strongly typed config with `IOptions<T>` |

### 3 Meta-Principles (from Pragmatic Programmer & Code Complete)

| # | Principle | Core Idea |
|---|-----------|-----------|
| 11 | DRY (Don't Repeat Yourself) | Single source of truth for knowledge |
| 12 | Broken Windows | Fix technical debt immediately |
| 13 | Avoid Dogmatism | Context over rules; no principle is universal |

## Components

### Skill: csharp-pragmatic-architecture

Automatically loads when discussing C# architecture, Result patterns, value objects, vertical slices, or related topics.

**Includes:**
- All 10 principles with code examples
- Reference files for deep dives
- Real examples from OfficeAddins codebase
- Anti-patterns to avoid

### Agent: architecture-reviewer

Reviews C# code against the 10 principles. Triggers:
- **Proactively** after writing C# code
- **On request** when you ask for architecture review

**Output:**
- Critical, Important, and Suggestion-level findings
- Before/after code examples
- Educational explanations (optional depth)

### Command: /review-architecture

On-demand architecture review.

```
/review-architecture              # Review git diff
/review-architecture file.cs      # Review specific file
/review-architecture all          # Full codebase (slow)
```

### Hook: PostToolUse reminder

After modifying `.cs` files, provides a gentle reminder:
> "C# code modified. Run /review-architecture when ready."

## Installation

### Local Testing

```bash
claude --plugin-dir E:\Claude\plugins\pragmatic-csharp
```

### Project Installation

Copy to your project's `.claude/plugins/` directory.

## Library Preferences

**Recommended:**
- CSharpFunctionalExtensions (Result, Maybe, ValueObject)
- Scrutor (DI auto-registration, decorators)
- Microsoft.Extensions.* packages

**Avoided:**
- MediatR (went paid, prefer simple ICommandHandler interfaces)
- Libraries that obscure principles with "magic"

## Reference Codebase

Examples are drawn from `E:\Code\OfficeAddins` which demonstrates:
- Result<T, Error> pattern with static error definitions
- Decorator chain for cross-cutting concerns
- Vertical slice organization
- Value Objects and aggregates
- CQRS without MediatR

## File Structure

```
pragmatic-csharp/
├── .claude-plugin/
│   └── plugin.json
├── skills/
│   └── csharp-pragmatic-architecture/
│       ├── SKILL.md              # Core principles
│       ├── references/           # Deep-dive docs
│       │   ├── result-pattern.md
│       │   ├── illegal-states.md
│       │   ├── vertical-slices.md
│       │   ├── pure-core.md
│       │   ├── anti-patterns.md
│       │   └── your-codebase.md
│       └── examples/             # Working code
│           ├── value-object.cs
│           ├── result-handler.cs
│           ├── decorator.cs
│           └── aggregate-root.cs
├── agents/
│   └── architecture-reviewer.md
├── commands/
│   └── review-architecture.md
├── hooks/
│   └── hooks.json
└── README.md
```

## Usage Examples

**Ask about principles:**
> "How should I handle errors in this handler?"
> "What's wrong with using a BaseRepository?"
> "Show me the Result pattern"

**Get code reviewed:**
> "Review the authentication module"
> "Check my new CreateUser handler"
> `/review-architecture src/Handlers/`

**Learn from violations:**
> "Explain why throwing exceptions is bad here"
> "Show me how to refactor this to use composition"

## Industry Sources

This plugin synthesizes best practices from:
- [Milan Jovanovic](https://www.milanjovanovic.tech/blog) - Result pattern, vertical slices
- [Enterprise Craftsmanship](https://enterprisecraftsmanship.com/) - Value objects, DDD
- [Mark Seemann (ploeh)](https://blog.ploeh.dk/) - Functional core, imperative shell
- [Jimmy Bogard](https://www.jimmybogard.com/) - Vertical slice architecture
- [Andrew Lock](https://andrewlock.net/) - Strongly-typed IDs, .NET best practices

## Contributing

This is a personal plugin. Fork and customize for your own architecture preferences.
