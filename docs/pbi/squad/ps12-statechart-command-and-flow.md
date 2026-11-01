# PS12: /statechart command plus statechart-md in the squad flow
- Status: todo (docs only; build waits for Sven go per PBI)
- Batch: B2
- Depends on: PS09
- Absorbs old: none
- Lane: Builder (Cursor CLI for docs and markdown, Cloud Agent if multi-file C#)
- door: two-way
- Source: [skill map](sources/pstack-matt-skill-map.md), pstack `ae5650f`, Matt `4ae2317`, flows in [FLOWS.md](FLOWS.md)

## Goal and why

- Sven: "statechart wil ik aparte commando voor maar moet ook in flow zijn die we al hebben".
- Sven: charts in docs so architects and developers see how the app is built; also for unit, integration, component and smoke test scenarios.

## Current state on main (`6c59f49`)

- No statechart. `/scenarios` ([`plugins/squad/skills/scenarios-md/SKILL.md`](https://github.com/svennijhuis/agentPacks/blob/6c59f49/plugins/squad/skills/scenarios-md/SKILL.md)) reads changed handlers only.

## Target structure

- Skill `plugins/squad/skills/statechart-md/` (copy + adapt, reason in reuse table). Scripts byte-equal.
- Command `plugins/squad/commands/statechart.md`: thin wrapper, loads `statechart-md`, no logic of its own.
- Output: Mermaid chart in `docs/architecture/<area>.md` plus shortest path to each rule-breaking state.
- In the flow: plan.md and feature.md call statechart-md for stateful work (docs), review.md checks the chart still matches.
- Paths export to a scenario list that `/scenarios` reads (unit, integration, component, smoke rows).
- Slash count: 6 commands today (squad, squad-review, scenarios, pack-check, pack-author, security-audit). After this PBI 7; room user commands go from 4 to 5. Sven agreed to the extra slash (2026-11-01).

## External skills per step

| Step | Skill | Reuse mode | Lives in | Reason |
|---|---|---|---|---|
| command and flow | [statechart](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/statechart/SKILL.md) | copy + adapt | `plugins/squad/skills/statechart-md/` | Upstream has `disable-model-invocation: true` and is named like the command. Sven wants `/statechart` plus auto use in the flow. We rename to `statechart-md`, allow model use, add `docs/architecture/` output and the scenario export to `/scenarios`. Scripts stay byte-equal. |
| formal | [create-formal-verification-skill](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/create-formal-verification-skill/SKILL.md) | pin | external-skills.json | Risky code only. |

## Example

Good (what the result looks like):

```text
/statechart orders
  wrote docs/architecture/orders.md (mermaid, 5 states)
  shortest path to bad state: Paid -> Cancelled without refund
  added 1 row to docs/smoke/orders.md via /scenarios
```

Bad (what we do not want):

A `/statechart` command that copies the whole upstream skill into the command file, or a chart nobody turns into tests.

## Subagents and what they load

- squad-planner and squad-architect load statechart-md for stateful tasks.

## Steps for Builder

1. Copy and adapt statechart.
2. Write the command.
3. Hooks in plan.md, feature.md, review.md.
4. Scenario export read by scenarios-md.
5. Update command count in README files, `docs/PLAN.md` and room docs.

## DoD

- Command and skill have different names.
- Fixture 1: `/statechart` alone writes a chart in `docs/architecture/`.
- Fixture 2: squad feature run calls statechart-md and `/scenarios` reads the paths.
- validate 0 warnings.
- `dotnet run --project tools/AgentPacks.Cli -- validate` exits 0, 0 errors, 0 warnings.
- `/pack-check` locally gives the same result (same rules, one source).
- `dotnet test tools/AgentPacks.Cli.Tests` green.
- No em dash, no tokens in markdown, FAKE fixture values only.

## Evidence

- Both fixture outputs.

## Risks and out of scope

- Needs Bun, TLC and Java via mise, network on first run: local only, not gh-aw offline.
- Out: TLA+ guide.
