# Changelog

All notable changes to pragmatic-dev-team will be documented here.

## [2.2.0] - 2025-01-05

### Changed
- **debugging-techniques** - Now VSTO-specific (LoadBehavior, VSTO logs, COM debugging)
- **observability-patterns** - Added OpenTelemetry, VSTO logging patterns, Event Log
- **team-orchestration** - Reduced 345→94 lines, moved examples to references

### Added
- team-orchestration/references/workflow-examples.md

## [2.1.1] - 2025-01-05

### Added
- **Smoke tests** - Automated validation (`tests/smoke-test.ps1`)
- **Parallel execution** - team-coordinator spawns specialists simultaneously
- **Escape hatch** - "force exit" bypasses Stop hook verification

### Changed
- **Skill lazy-loading** - All skills have `load: on-demand`
- **Optimized hooks** - Reduced timeouts from 5-30s to 2-5s

## [2.1.0] - 2024-12-29

### Added
- **SessionStart hook** - Detects C# solutions on session start
- **UserPromptSubmit hook** - Reminds verification before commit/PR/done
- **Security PreToolUse hooks** - Blocks dangerous operations (rm -rf, del /s, DROP TABLE, force push)
- **Sensitive file protection** - Prevents modification of .env, credentials, secrets
- **Stop hook** - Enforces verification-before-completion gate
- **Full plugin.json metadata** - repository, license, homepage, bugs, engines fields
- **Test structure** - Manual test cases for agents, skills, hooks
- **README badges** - Version, license, agents, skills counts

### Changed
- Bumped version to 2.1.0
- Enhanced skill trigger keyword descriptions
- Standardized command frontmatter with allowed-tools
- Added capabilities and skills arrays to agents
- Consolidated hooks for better security coverage

## [2.0.0] - Previous

### Changed
- **Major consolidation**: 16 agents → 6 orthogonal specialists
- Followed [Anthropic best practices](https://www.anthropic.com/engineering/claude-code-best-practices)
- Each agent now has narrow, distinct purpose with no overlap

### Added
- **Iron Laws** - Unbreakable rules per skill (e.g., NO IMPLEMENTATION WITHOUT SCENARIO FIRST)
- **Gate Functions** - Mandatory verification steps before proceeding
- **Anti-hallucination constraints** - Agents must read actual files, quote exact code
- **Verification Before Completion** - No success claims without fresh evidence

### Agents
| New Agent | Consolidated From |
|-----------|-------------------|
| team-coordinator | release-advisor |
| bdd-strategist | test-coverage-analyst, exploratory-tester, product-advocate |
| architecture-reviewer | refactoring-advisor, legacy-code-navigator |
| code-reviewer | performance-analyst, observability-advisor, accessibility-reviewer |
| security-reviewer | (standalone) |
| issue-specialist | technical-researcher |

## [1.0.0] - Initial

- 16 specialized agents
- 13+ domain skills
- 5 slash commands
- Basic PostToolUse hooks
