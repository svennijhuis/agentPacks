# PS12: Verification and formal verification pairs
- Status: todo (docs only, build waits for Sven go per PBI)
- Batch: B2
- Depends on: PS07
- Lane: Builder
- door: two-way
- kind: skill
- Source: [skill map](sources/pstack-matt-skill-map.md) (Repo Research 2026-11-01), pstack `ae5650f`, Matt `4ae2317`

## Goal and why

- Sven: "/create-verification-skill ... this follow up skill we need then: maintain-verification-skill" and the same for formal verification.

## Current state on main (`d76fa14`)

- [`plugins/squad/skills/verify-harness/SKILL.md`](https://github.com/svennijhuis/agentPacks/blob/d76fa14/plugins/squad/skills/verify-harness/SKILL.md) writes `docs/verify.md`. Old AP52, AP53, AP59 planned a feature map and gardener.

## Target structure

- build playbook: **create-verification-skill** when the target repo has no project-local verification skill.
- retro playbook: **maintain-verification-skill** (one live session, at most one PR of proven fixes).
- risky code (races, retries, state machines, parsers): **create-formal-verification-skill**, then **maintain-formal-verification-skill** in retro.
- Keep our `verify-harness` (docs/verify.md is a different deliverable).

## External skills per step

| Flow step | Skill | Link (pinned SHA) | Reuse mode | Depends on |
|---|---|---|---|---|
| build | create-verification-skill | [create-verification-skill](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/create-verification-skill/SKILL.md) | pinned as-is (`external-skills.json`, 40-hex) | feature-map-example |
| retro | maintain-verification-skill | [maintain-verification-skill](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/maintain-verification-skill/SKILL.md) | pinned as-is (`external-skills.json`, 40-hex) | create pair |
| build | create-formal-verification-skill | [create-formal-verification-skill](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/create-formal-verification-skill/SKILL.md) | pinned as-is (`external-skills.json`, 40-hex) | statechart |
| retro | maintain-formal-verification-skill | [maintain-formal-verification-skill](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/maintain-formal-verification-skill/SKILL.md) | pinned as-is (`external-skills.json`, 40-hex) | create pair, correct |

## Subagents and what they load

- squad-implementer may run create; squad-retro runs maintain.

## Steps for Builder

1. Hooks in feature.md, domain.md, retro.md.
2. Fixture with create then maintain order.

## DoD

- Pointers resolve; fixture order holds.
- `dotnet run --project tools/AgentPacks.Cli -- validate` exits 0 with 0 errors and 0 warnings.
- `/pack-check` run locally reports 0 warnings (same checks as `validate`, PO decision 1).
- `dotnet test tools/AgentPacks.Cli.Tests` green.
- No em dash, no tokens in markdown, FAKE fixture values only.

## Evidence

- Fixture output.

## Risks and out of scope

- Out of scope: changing verify-harness (PS16).
