# PS18: Security pack restyle
- Status: todo (docs only, build waits for Sven go per PBI)
- Batch: B6
- Depends on: PS17
- Lane: Builder
- door: two-way
- kind: skill
- Source: [skill map](sources/pstack-matt-skill-map.md) (Repo Research 2026-11-01), pstack `ae5650f`, Matt `4ae2317`

## Goal and why

- Same style everywhere (PO).

## Current state on main (`d76fa14`)

- [`plugins/security/skills/security-audit-md/SKILL.md`](https://github.com/svennijhuis/agentPacks/blob/d76fa14/plugins/security/skills/security-audit-md/SKILL.md) (26 lines, 5 inline steps) and agents security-recon, security-hunter, security-validator.

## Target structure

- security-audit-md: steps to a playbook, Non-negotiables, refs to **interrogate** (prove-by-run) and **blast-radius**.
- Agents light restyle with write-subagent: Boundary Discipline, Prove It Works, abs-path line.

## External skills per step

| Flow step | Skill | Link (pinned SHA) | Reuse mode | Depends on |
|---|---|---|---|---|
| review | interrogate | [interrogate](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/interrogate/SKILL.md) | pinned as-is (`external-skills.json`, 40-hex) | how, arena |
| review | blast-radius | [blast-radius](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/blast-radius/SKILL.md) | pinned as-is (`external-skills.json`, 40-hex) | how, why |

## Subagents and what they load

- security-validator loads interrogate pattern; recon loads how.

## Steps for Builder

1. Restyle skill and 3 agents.

## DoD

- validate 0/0, check-subagent PASS for 3 agents.
- `dotnet run --project tools/AgentPacks.Cli -- validate` exits 0 with 0 errors and 0 warnings.
- `/pack-check` run locally reports 0 warnings (same checks as `validate`, PO decision 1).
- `dotnet test tools/AgentPacks.Cli.Tests` green.
- No em dash, no tokens in markdown, FAKE fixture values only.

## Evidence

- Verdict list.

## Risks and out of scope

- Out of scope: new attack classes.
