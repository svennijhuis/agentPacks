# PS16: Helpers: scenarios-md, verify-harness, pack-check-md
- Status: todo (docs only; build waits for Sven go per PBI)
- Batch: B4
- Depends on: PS15
- Absorbs old: none
- Lane: Builder (Cursor CLI for docs and markdown, Cloud Agent if multi-file C#)
- door: two-way
- Source: [skill map](sources/pstack-matt-skill-map.md), pstack `ae5650f`, Matt `4ae2317`, flows in [FLOWS.md](FLOWS.md)

## Goal and why

- PO decision 1 and 2: `validate` is the gate and `/pack-check` runs the same checks; rename to `pack-check-md`.

## Current state on main (`6c59f49`)

- [`plugins/squad/skills/scenarios-md/SKILL.md`](https://github.com/svennijhuis/agentPacks/blob/6c59f49/plugins/squad/skills/scenarios-md/SKILL.md) (12 inline steps), `verify-harness` (43 lines), [`plugins/pack-check/skills/pack-check/SKILL.md`](https://github.com/svennijhuis/agentPacks/blob/6c59f49/plugins/pack-check/skills/pack-check/SKILL.md) (name equals command).

## Target structure

scenarios-md steps to `playbooks/write-scenarios.md` and it reads statechart paths; verify-harness restyle; pack-check-md rename plus hygiene mode that runs validate locally.

## External skills per step

| Step | Skill | Reuse mode | Lives in | Reason |
|---|---|---|---|---|
| scenarios | [statechart](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/statechart/SKILL.md) | copy + adapt | `plugins/squad/skills/statechart-md/` | Upstream has `disable-model-invocation: true` and is named like the command. Sven wants `/statechart` plus auto use in the flow. We rename to `statechart-md`, allow model use, add `docs/architecture/` output and the scenario export to `/scenarios`. Scripts stay byte-equal. |
| verify | [create-verification-skill](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/create-verification-skill/SKILL.md) | pin | external-skills.json | Used as is; covers old feature map PBIs. |

## Subagents and what they load

- none new

## Steps for Builder

1. Restyle with write-skill.
2. Rename.
3. Hygiene mode.

## DoD

- `/pack-check` output equals validate on a fixture with all new skills.
- `dotnet run --project tools/AgentPacks.Cli -- validate` exits 0, 0 errors, 0 warnings.
- `/pack-check` locally gives the same result (same rules, one source).
- `dotnet test tools/AgentPacks.Cli.Tests` green.
- No em dash, no tokens in markdown, FAKE fixture values only.

## Evidence

- Side by side output.

## Risks and out of scope

- Out: CI changes.
