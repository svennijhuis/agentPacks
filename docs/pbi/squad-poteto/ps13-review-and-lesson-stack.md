# PS13: Review and lesson stack: arena, interrogate, blast-radius, correct, how, why, teach
- Status: todo (docs only, build waits for Sven go per PBI)
- Batch: B2
- Depends on: PS07
- Lane: Builder
- door: two-way
- kind: skill
- Source: [skill map](sources/pstack-matt-skill-map.md) (Repo Research 2026-11-01), pstack `ae5650f`, Matt `4ae2317`

## Goal and why

- Sven on arena: "interesting when review code or review plan task so different models are in arena and so in end we have best of all".
- Interrogate: "use this is review flow so try to break it". Blast-radius: "in creating plan, task, creating work items or reviewing code". Correct: "agents not making the same mistakes all the time". How: "better understanding ... also maybe domain logic". Teach: "planning, work items, tasks". Figure-it-out: "in code flow".

## Current state on main (`d76fa14`)

- Review uses one reviewer pass. No multi-model step. No repeated-mistake check.

## Target structure

| Slot | Skill | When |
|---|---|---|
| plan | blast-radius, teach, how, why | every plan with a code change |
| task | blast-radius | each task with a shared file |
| review | interrogate | every review; only proven findings survive |
| review | arena | plan review and code review on high-risk work, models from our tiers |
| review | blast-radius | every review |
| retro | correct | each retro; repeated mistakes become checks |
| any | figure-it-out | no playbook fits |

Budget: arena only on high risk (security, data loss, concurrency) to save tokens.

## External skills per step

| Flow step | Skill | Link (pinned SHA) | Reuse mode | Depends on |
|---|---|---|---|---|
| review | interrogate | [interrogate](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/interrogate/SKILL.md) | pinned as-is (`external-skills.json`, 40-hex) | how, arena, in-your-own-words |
| review, plan | arena | [arena](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/arena/SKILL.md) | pinned as-is (`external-skills.json`, 40-hex) | setup-pstack roles |
| plan, task, review | blast-radius | [blast-radius](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/blast-radius/SKILL.md) | pinned as-is (`external-skills.json`, 40-hex) | how, why, arena, unslop |
| retro | correct | [correct](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/correct/SKILL.md) | pinned as-is (`external-skills.json`, 40-hex) | why |
| plan | teach | [teach](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/teach/SKILL.md) | pinned as-is (`external-skills.json`, 40-hex) | how, why, unslop |
| plan, docs | how | [how](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/how/SKILL.md) | pinned as-is (`external-skills.json`, 40-hex) | explorer and explainer prompts |
| plan, review | why | [why](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/why/SKILL.md) | pinned as-is (`external-skills.json`, 40-hex) | how |
| fallback | figure-it-out | [figure-it-out](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/figure-it-out/SKILL.md) | pinned as-is (`external-skills.json`, 40-hex) | architect, arena, show-me-your-work |

## Subagents and what they load

- squad-reviewer: interrogate, blast-radius. squad-orchestrator: arena when high risk. squad-planner: teach, blast-radius. squad-retro: correct.

## Steps for Builder

1. Hooks in review.md, plan.md, retro.md.
2. Risk flag in plan decides arena.
3. Fixtures.

## DoD

- Review fixture shows each skill at its slot; arena only on high-risk fixture.
- `dotnet run --project tools/AgentPacks.Cli -- validate` exits 0 with 0 errors and 0 warnings.
- `/pack-check` run locally reports 0 warnings (same checks as `validate`, PO decision 1).
- `dotnet test tools/AgentPacks.Cli.Tests` green.
- No em dash, no tokens in markdown, FAKE fixture values only.

## Evidence

- Fixture output.

## Risks and out of scope

- Token cost of arena: high-risk only. Out of scope: model selection code (PS19).
