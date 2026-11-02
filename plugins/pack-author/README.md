# pack-author

A capability pack for marketplace authors. `/pack-author` makes, reviews, or changes a skill or an agent.

Claude, Cursor, and Codex run `/pack-author` (Codex: `$pack-author`). Copilot hides a command that repeats the plugin name, so its picker is `/pack-author:author`. The skill is `pack-author-md`.

This pack is not a Squad phase and not a pack-check probe. v1 writes a skill or an agent. A command, hook, or rule stays on the ADD-* docs.

## Flow

Modes (`make`, `review`, `change`) are in the [skill](skills/pack-author-md/SKILL.md).

## Components

| Component | Name | Responsibility |
|---|---|---|
| Skill | `pack-author-md` | User-invoked routing for `make`, `review`, and `change`. Not model-invoked. |
| Agent | `pack-author-reviewer` | Scores one skill or agent. PASS/WARN/FAIL only. `standard`, `readonly` |
| Command | `pack-author` | Runs the mode. Copilot picker: `/pack-author:author` |

## Editing

Authored: `plugin.json`, `skills/`, `agents/`, and `commands/`.

Generated only in validation output or on `marketplace` / `marketplace-beta`: client manifests and `com.*` provider trees.

Test on a feature branch without merging to `main`:
[ADD-SKILL.md: Test a skill locally](https://github.com/svennijhuis/agentPacks/blob/main/docs/ADD-SKILL.md#test-a-skill-locally).
