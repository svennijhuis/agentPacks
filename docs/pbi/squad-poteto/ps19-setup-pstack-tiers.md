# PS19: setup-pstack roles mapped to our tiers
- Status: todo (docs only, build waits for Sven go per PBI)
- Batch: B6
- Depends on: PS04
- Lane: Builder (docs)
- door: two-way
- kind: skill
- Source: [skill map](sources/pstack-matt-skill-map.md) (Repo Research 2026-11-01), pstack `ae5650f`, Matt `4ae2317`

## Goal and why

- Sven: "please setup our plugins etc check these again: setup-pstack".

## Current state on main (`d76fa14`)

- setup-pstack writes `~/.cursor/rules/pstack-models.mdc` (Cursor only). Our tiers in [`models.source.json`](https://github.com/svennijhuis/agentPacks/blob/d76fa14/models.source.json).

## Target structure

Doc table: pstack role (explorer, explainer, arena candidate, interrogator, reviewer, swarm worker) to our tier per provider (Claude, Copilot, Cursor/Grok, Codex, gh-aw). Note the Cursor-only path. No live model calls, no spend.

## External skills per step

| Flow step | Skill | Link (pinned SHA) | Reuse mode | Depends on |
|---|---|---|---|---|
| setup | setup-pstack | [setup-pstack](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/setup-pstack/SKILL.md) | pinned as-is (`external-skills.json`, 40-hex) | role names used by poteto-mode, how, why, arena, swarm, architect, interrogate, reflect |

## Subagents and what they load

- none

## Steps for Builder

1. Write `docs/PSTACK-ROLES.md`.

## DoD

- Table covers every role named by pinned skills; validate 0/0.
- `dotnet run --project tools/AgentPacks.Cli -- validate` exits 0 with 0 errors and 0 warnings.
- `/pack-check` run locally reports 0 warnings (same checks as `validate`, PO decision 1).
- `dotnet test tools/AgentPacks.Cli.Tests` green.
- No em dash, no tokens in markdown, FAKE fixture values only.

## Evidence

- Doc.

## Risks and out of scope

- Out of scope: running setup-pstack on the shared box.
