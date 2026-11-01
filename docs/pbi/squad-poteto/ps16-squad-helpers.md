# PS16: Squad helpers: scenarios-md, verify-harness, pack-check-md
- Status: todo (docs only, build waits for Sven go per PBI)
- Batch: B4
- Depends on: PS15
- Lane: Builder
- door: two-way
- kind: skill
- Source: [skill map](sources/pstack-matt-skill-map.md) (Repo Research 2026-11-01), pstack `ae5650f`, Matt `4ae2317`

## Goal and why

- PO decision 1: `validate` is the CI gate, `/pack-check` runs the same checks locally. PO decision 2: rename `pack-check` to `pack-check-md`.

## Current state on main (`d76fa14`)

- [`plugins/squad/skills/scenarios-md/SKILL.md`](https://github.com/svennijhuis/agentPacks/blob/d76fa14/plugins/squad/skills/scenarios-md/SKILL.md) (45 lines, 12 steps), [`plugins/squad/skills/verify-harness/SKILL.md`](https://github.com/svennijhuis/agentPacks/blob/d76fa14/plugins/squad/skills/verify-harness/SKILL.md) (43), [`plugins/pack-check/skills/pack-check/SKILL.md`](https://github.com/svennijhuis/agentPacks/blob/d76fa14/plugins/pack-check/skills/pack-check/SKILL.md) (60, name clash with command).

## Target structure

- scenarios-md: 12 steps move to `playbooks/write-scenarios.md`; reads statechart paths (PS11).
- verify-harness: restyle, short frontmatter, keep deliverable `docs/verify.md`.
- pack-check-md: rename, add hygiene mode that runs `validate` locally (same rules, one source).

## External skills per step

| Flow step | Skill | Link (pinned SHA) | Reuse mode | Depends on |
|---|---|---|---|---|
| scenarios | statechart | [statechart](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/statechart/SKILL.md) | pinned as-is (`external-skills.json`, 40-hex) | PS11 |
| verify | create-verification-skill | [create-verification-skill](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/create-verification-skill/SKILL.md) | pinned as-is (`external-skills.json`, 40-hex) | PS12 |

## Subagents and what they load

- none new

## Steps for Builder

1. Restyle with write-skill.
2. Rename pack-check skill, command loads `pack-check-md`.
3. Hygiene mode calls validate.

## DoD

- `/pack-check` on a fixture with all new skills gives 0 warnings, same output as validate.
- `dotnet run --project tools/AgentPacks.Cli -- validate` exits 0 with 0 errors and 0 warnings.
- `/pack-check` run locally reports 0 warnings (same checks as `validate`, PO decision 1).
- `dotnet test tools/AgentPacks.Cli.Tests` green.
- No em dash, no tokens in markdown, FAKE fixture values only.

## Evidence

- Side-by-side output.

## Risks and out of scope

- Out of scope: CI changes.
