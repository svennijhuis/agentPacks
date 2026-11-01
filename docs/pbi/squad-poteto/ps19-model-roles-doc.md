# PS19: pstack model roles mapped to our tiers
- Status: todo (docs only; build waits for Sven go per PBI)
- Batch: B6
- Depends on: PS04
- Absorbs old: none
- Lane: Builder (Cursor CLI for docs and markdown, Cloud Agent if multi-file C#)
- door: two-way
- Source: [skill map](sources/pstack-matt-skill-map.md), pstack `ae5650f`, Matt `4ae2317`, flows in [FLOWS.md](FLOWS.md)

## Goal and why

- Sven: check setup-pstack for our plugins.

## Current state on main (`6c59f49`)

- Tiers in [`models.source.json`](https://github.com/svennijhuis/agentPacks/blob/6c59f49/models.source.json); no role map.

## Target structure

`docs/PSTACK-ROLES.md`: each role named by pinned or copied skills (explorer, explainer, candidate, interrogator, reviewer) to our tier per provider. No live calls, no spend, no `~/.cursor` setup on the shared box.

## External skills per step

| Step | Skill | Reuse mode | Lives in | Reason |
|---|---|---|---|---|
| roles | [setup-pstack](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/setup-pstack/SKILL.md) | not used | `docs/PSTACK-ROLES.md` | Writes `~/.cursor/rules` (Cursor only, and no long-term setup on the shared box). We document role to tier instead. |

## Subagents and what they load

- none

## Steps for Builder

1. Write the doc.

## DoD

- Every role named by a used skill is in the table.
- `dotnet run --project tools/AgentPacks.Cli -- validate` exits 0, 0 errors, 0 warnings.
- `/pack-check` locally gives the same result (same rules, one source).
- `dotnet test tools/AgentPacks.Cli.Tests` green.
- No em dash, no tokens in markdown, FAKE fixture values only.

## Evidence

- Doc.

## Risks and out of scope

- Out: running setup-pstack.
