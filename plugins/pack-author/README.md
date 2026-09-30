# pack-author

A capability pack for marketplace authors. `/pack-author` makes, reviews, or changes a skill or an agent.

Plugin `pack-author`, command `author`, and skill `pack-author-md` are three names. Copilot hides a command that repeats the plugin name, so the picker is `/pack-author:author`. Claude Code and Cursor run `/author`. Codex runs `$author`.

This pack is not a Squad phase and not a pack-check probe. v1 writes a skill or an agent. A command, hook, or rule stays on the ADD-* docs.

## Flow

```text
/pack-author
1. make — PLAN.md owning pack, Matt-tiny skill or agent stub, ADD-SKILL or ADD-AGENT, validate hints
2. review — pack-author-reviewer returns PASS/FAIL
3. change — at most two human-approved rounds on FAIL items
```

## Components

| Component | Name | Responsibility |
|---|---|---|
| Skill | `pack-author-md` | User-invoked routing for `make`, `review`, and `change`. Not model-invoked. |
| Agent | `pack-author-reviewer` | Scores one skill or agent. PASS/FAIL only. `fast`, `readonly` |
| Command | `author` | Runs the mode. Copilot picker: `/pack-author:author` |

## Editing

Authored: `plugin.json`, `skills/`, `agents/`, and `commands/`.

Generated only in validation output or on `marketplace` / `marketplace-beta`: client manifests and `com.*` provider trees.

Test on a feature branch without merging to `main`:
[ADD-SKILL.md — Test a skill locally](../../docs/ADD-SKILL.md#test-a-skill-locally).
