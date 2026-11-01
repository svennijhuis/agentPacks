# PS09: Architecture review mode (from Matt improve-codebase-architecture)
- Status: todo (docs only, build waits for Sven go per PBI)
- Batch: B2
- Depends on: PS08
- Lane: Builder
- door: two-way
- kind: skill
- Source: [skill map](sources/pstack-matt-skill-map.md) (Repo Research 2026-11-01), pstack `ae5650f`, Matt `4ae2317`

## Goal and why

- Sven: "I like to use this in review only reviewing code that I created ... but way of patter cursor I like more".

## Current state on main (`d76fa14`)

- No architecture review mode. squad-architect designs, does not review existing code.

## Target structure

`plugins/squad/skills/squad-md/references/architecture-review.md`, loaded by the review playbook only for files whose commits are authored by Sven (git author on the personal account). Steps: scan for deepening opportunities with **codebase-design** vocabulary (deep vs shallow, seam, adapter), check **architect** design-red-flags, one finding per module. Drop Matt's `grilling` call and HTML report (squad grills itself, pstack style).

## External skills per step

| Flow step | Skill | Link (pinned SHA) | Reuse mode | Depends on |
|---|---|---|---|---|
| review | [improve-codebase-architecture](https://github.com/mattpocock/skills/blob/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/engineering/improve-codebase-architecture/SKILL.md) | [improve-codebase-architecture](https://github.com/mattpocock/skills/blob/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/engineering/improve-codebase-architecture/SKILL.md) | rewrite as our skill (MIT line in NOTICE) | codebase-design, domain-modeling |
| review | architect design-red-flags | [architect](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/architect/references/design-red-flags.md) | rides with the architect pin | architect |
| review | codebase-design | [codebase-design](https://github.com/mattpocock/skills/blob/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/engineering/codebase-design/SKILL.md) | pinned as-is (`external-skills.json`, 40-hex) | DESIGN-IT-TWICE.md |

## Subagents and what they load

- squad-architect runs it in review mode (readonly).

## Steps for Builder

1. Write the reference with write-skill.
2. Hook from review.md.
3. Fixture with a shallow module.

## DoD

- Fixture review returns a deepening finding with path:line.
- No grilling call.
- `dotnet run --project tools/AgentPacks.Cli -- validate` exits 0 with 0 errors and 0 warnings.
- `/pack-check` run locally reports 0 warnings (same checks as `validate`, PO decision 1).
- `dotnet test tools/AgentPacks.Cli.Tests` green.
- No em dash, no tokens in markdown, FAKE fixture values only.

## Evidence

- Fixture output.

## Risks and out of scope

- Author filter must use git author, never org data. Out of scope: refactor itself.
