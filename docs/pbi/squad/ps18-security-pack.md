# PS18: Security pack restyle
- Status: todo (docs only; build waits for Sven go per PBI)
- Batch: B6
- Depends on: PS17
- Absorbs old: none
- Lane: Builder (Cursor CLI for docs and markdown, Cloud Agent if multi-file C#)
- door: two-way
- Release: squad v2 ([README](README.md#squad-v2))
- Docs: [STRUCTURE](STRUCTURE.md) (where), [SHAPES](SHAPES.md) (how it looks), [FLOWS](FLOWS.md) (flow per command), [MIGRATION](MIGRATION.md) (v1 to v2 rows for this PBI), [gh-aw](gh-aw/README.md)
- Source: [skill map](sources/pstack-matt-skill-map.md), pstack `ae5650f`, Matt `4ae2317`

## Goal and why

- Same style everywhere.

## Current state on main (`6c59f49`)

- [`plugins/security/skills/security-audit-md/SKILL.md`](https://github.com/svennijhuis/agentPacks/blob/6c59f49/plugins/security/skills/security-audit-md/SKILL.md) plus security-recon, -hunter, -validator.

## Target structure

Steps to a playbook, Non-negotiables, interrogate and blast-radius refs; agents restyled with write-subagent.

## External skills per step

| Step | Skill | Reuse mode | Lives in | Reason |
|---|---|---|---|---|
| review | [interrogate](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/interrogate/SKILL.md) | pin | external-skills.json | Used as is in review. |
| review | [blast-radius](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/blast-radius/SKILL.md) | pin | external-skills.json | Used as is in plan, task and review. |

## Example

Good (what the result looks like):

```text
squad-security-reviewer: finding high, src/Upload.cs:22,
path from user input reaches File.Open. Proven by interrogate step.
```

Bad (what we do not want):

A security finding with no file:line or no proof, or a full-repo audit started from a PR review.

## Subagents and what they load

- validator: interrogate; recon: how.

## Steps for Builder

1. Restyle 1 skill, 3 agents.

## DoD

- check-subagent PASS for 3.
- Local: works in Cursor, Claude Code and CLI (Copilot CLI, Codex). Proof offline: generated client manifests and fixture tests (`ClientManifestTests`, `ComponentGenerationTests`).
- gh-aw: the fixture workflow under `tests/fixtures/gh-aw/` that uses this PBI's files passes `gh aw compile --strict` (pinned gh-aw, no run, no secrets, no Actions minutes). See [gh-aw README](gh-aw/README.md).
- `dotnet run --project tools/AgentPacks.Cli -- validate` exits 0, 0 errors, 0 warnings.
- `/pack-check` locally gives the same result (same rules, one source).
- `dotnet test tools/AgentPacks.Cli.Tests` green; main stays green and all 7 commands still load after merge ([MIGRATION](MIGRATION.md)).
- No em dash, no tokens in markdown, FAKE fixture values only.

## Evidence

- Verdicts.

## Risks and out of scope

- Out: new attack classes.
