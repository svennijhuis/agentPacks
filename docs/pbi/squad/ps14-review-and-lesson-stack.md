# PS14: Review and lesson stack
- Status: todo (docs only; build waits for Sven go per PBI)
- Batch: B2
- Depends on: PS09
- Absorbs old: old 51, 57, 58
- Lane: Builder (Cursor CLI for docs and markdown, Cloud Agent if multi-file C#)
- door: two-way
- Source: [skill map](sources/pstack-matt-skill-map.md), pstack `ae5650f`, Matt `4ae2317`, flows in [FLOWS.md](FLOWS.md)

## Goal and why

- Sven: arena "different models are in arena and so in end we have best of all"; interrogate "try to break it"; blast-radius in plan, task, work items, review; correct "not making the same mistakes"; teach and how in planning.

## Current state on main (`6c59f49`)

- One reviewer pass; no repeated-mistake check.

## Target structure

| Slot | Skill | When |
|---|---|---|
| plan | blast-radius, teach, how, why | every plan with code |
| review | interrogate, blast-radius | every review |
| review | arena (our copy, max 3) | high risk only: security, data loss, concurrency |
| retro | correct | every retro; repeated mistakes become checks (old 57, 58, 51) |
| any | figure-it-out | no playbook fits |

## External skills per step

| Step | Skill | Reuse mode | Lives in | Reason |
|---|---|---|---|---|
| review | [interrogate](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/interrogate/SKILL.md) | pin | external-skills.json | Used as is in review. |
| review | [arena](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/arena/SKILL.md) | copy + adapt | `plugins/squad/skills/arena/` | Picks models by setup-pstack roles (Cursor only). We map to our tiers, cap at 3 candidates and high-risk work only (token budget). |
| plan, review | [blast-radius](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/blast-radius/SKILL.md) | pin | external-skills.json | Used as is in plan, task and review. |
| retro | [correct](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/correct/SKILL.md) | pin | external-skills.json | Used as is in retro; covers old lint-first and guard ladder PBIs. |
| plan | [teach](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/teach/SKILL.md) | pin | external-skills.json | Used as is in plan. |
| plan | [how](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/how/SKILL.md) | pin | external-skills.json | Used as is. |
| plan | [why](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/why/SKILL.md) | pin | external-skills.json | Used as is. |
| fallback | [figure-it-out](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/figure-it-out/SKILL.md) | pin | external-skills.json | Fallback when no playbook fits. Resolves our arena copy by name. |

## Example

Good (what the result looks like):

```text
Retro: same missing-null-check found in 2 PRs ->
correct turns it into validator rule or standard leaf (one PR).
```

Bad (what we do not want):

A retro that only lists lessons in prose and never changes a check, test or leaf.

## Subagents and what they load

- reviewer, orchestrator (arena), planner, retro.

## Steps for Builder

1. Hooks in review.md, plan.md, retro.md.
2. Risk flag decides arena.
3. Fixtures.

## DoD

- Arena only in the high-risk fixture.
- `dotnet run --project tools/AgentPacks.Cli -- validate` exits 0, 0 errors, 0 warnings.
- `/pack-check` locally gives the same result (same rules, one source).
- `dotnet test tools/AgentPacks.Cli.Tests` green.
- No em dash, no tokens in markdown, FAKE fixture values only.

## Evidence

- Fixture output.

## Risks and out of scope

- Token cost: arena high risk only.
