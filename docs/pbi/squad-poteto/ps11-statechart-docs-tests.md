# PS11: statechart for docs and tests
- Status: todo (docs only, build waits for Sven go per PBI)
- Batch: B2
- Depends on: PS07
- Lane: Builder
- door: two-way
- kind: skill
- Source: [skill map](sources/pstack-matt-skill-map.md) (Repo Research 2026-11-01), pstack `ae5650f`, Matt `4ae2317`

## Goal and why

- Sven: "/statechart docs and unit test. We can use this for getting docs more with charts so it is clear visual how application is build architecture and developers can take look in it also for testing. This can then also be used for unit test integration test, component test and also smoke testing scenarios".

## Current state on main (`d76fa14`)

- [`plugins/squad/skills/scenarios-md/SKILL.md`](https://github.com/svennijhuis/agentPacks/blob/d76fa14/plugins/squad/skills/scenarios-md/SKILL.md) writes smoke scenarios from changed handlers, no charts.

## Target structure

- plan and domain playbooks call **statechart** when logic is stateful (state machine, retries, workflow).
- Output: Mermaid chart into `docs/architecture/<area>.md`, plus shortest paths to each rule-breaking state.
- Paths feed test tasks: unit, integration, component, and `/scenarios` smoke rows.
- scenarios-md reads the paths (PS16).

## External skills per step

| Flow step | Skill | Link (pinned SHA) | Reuse mode | Depends on |
|---|---|---|---|---|
| plan, docs, review | statechart | [statechart](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/statechart/SKILL.md) | pinned as-is (`external-skills.json`, 40-hex) | references/render.md, scripts (Bun/TS), TLC + Java via mise |
| formal | create-formal-verification-skill | [create-formal-verification-skill](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/create-formal-verification-skill/SKILL.md) | pinned as-is (`external-skills.json`, 40-hex) | statechart |

## Subagents and what they load

- squad-planner loads statechart when the plan has a stateful task.

## Steps for Builder

1. Hook in plan.md and domain.md.
2. Docs path rule.
3. Fixture state machine.

## DoD

- Fixture gives a chart in docs and a scenario list that scenarios-md consumes.
- `dotnet run --project tools/AgentPacks.Cli -- validate` exits 0 with 0 errors and 0 warnings.
- `/pack-check` run locally reports 0 warnings (same checks as `validate`, PO decision 1).
- `dotnet test tools/AgentPacks.Cli.Tests` green.
- No em dash, no tokens in markdown, FAKE fixture values only.

## Evidence

- Chart file, scenario list.

## Risks and out of scope

- Needs network on first run and Java: local only, not in gh-aw offline. Out of scope: TLA+ authoring guide.
