# VSTO Development Plugin

Specialized knowledge for Visual Studio Tools for Office (VSTO) add-in development.

## Skills

| Skill | Purpose |
|-------|---------|
| `vsto-com-interop` | COM cleanup patterns, two-dot rule, memory management |
| `vsto-word-object-model` | Word API: Document, Range, Selection, Tables, Styles |
| `vsto-build-deploy` | MSBuild, ClickOnce, manifests, troubleshooting |

## Installation

Copy to `~/.claude/plugins/` or use `--plugin-dir`:

```bash
cc --plugin-dir /path/to/vsto-development
```

## Triggers

Skills activate when discussing:
- COM interop, Office automation, Marshal.ReleaseComObject
- Word.Document, Range, Selection, Content Controls
- MSBuild VSTO, ClickOnce, deployment
