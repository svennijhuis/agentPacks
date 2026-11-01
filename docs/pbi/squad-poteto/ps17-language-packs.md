# PS17: Language packs restyle and typescript-best-practices pin
- Status: todo (docs only, build waits for Sven go per PBI)
- Batch: B5
- Depends on: PS16
- Lane: Builder
- door: two-way
- kind: skill
- Source: [skill map](sources/pstack-matt-skill-map.md) (Repo Research 2026-11-01), pstack `ae5650f`, Matt `4ae2317`

## Goal and why

- Sven: "/typescript-best-practices ... adding for typescript plugin".
- Map: language skills carry repeated boilerplate and no principle refs.

## Current state on main (`d76fa14`)

- dotnet, rust, typescript build/review/test skills, 23 to 32 lines each, "Internal. Do not run directly" and "Loop-only" boilerplate twice.

## Target structure

- Drop boilerplate, add principle and standard refs.
- `plugins/typescript/external-skills.json`: `typescript-best-practices` pinned (auto-applies on `**/*.ts`, `**/*.tsx`); typescript-review routes to it.
- Review skills cite **standard-coding-standards** and Matt two-axis review.

## External skills per step

| Flow step | Skill | Link (pinned SHA) | Reuse mode | Depends on |
|---|---|---|---|---|
| build, review | typescript-best-practices | [typescript-best-practices](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/typescript-best-practices/SKILL.md) | pinned as-is (`external-skills.json`, 40-hex) | references/patterns.md, principle-type-system-discipline |
| test | tdd | [tdd](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/tdd/SKILL.md) | pinned as-is (`external-skills.json`, 40-hex) | correct, why |

## Subagents and what they load

- none

## Steps for Builder

1. One PR per language or one bundle if small.
2. Restyle with write-skill.

## DoD

- validate 0/0 per pack.
- `dotnet run --project tools/AgentPacks.Cli -- validate` exits 0 with 0 errors and 0 warnings.
- `/pack-check` run locally reports 0 warnings (same checks as `validate`, PO decision 1).
- `dotnet test tools/AgentPacks.Cli.Tests` green.
- No em dash, no tokens in markdown, FAKE fixture values only.

## Evidence

- Per-pack validate output.

## Risks and out of scope

- Out of scope: new standards content.
