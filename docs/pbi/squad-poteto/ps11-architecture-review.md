# PS11: Architecture review mode for code Sven wrote
- Status: todo (docs only; build waits for Sven go per PBI)
- Batch: B2
- Depends on: PS10
- Absorbs old: none
- Lane: Builder (Cursor CLI for docs and markdown, Cloud Agent if multi-file C#)
- door: two-way
- Source: [skill map](sources/pstack-matt-skill-map.md), pstack `ae5650f`, Matt `4ae2317`, flows in [FLOWS.md](FLOWS.md)

## Goal and why

- Sven: "only reviewing code that I created ... but way of pattern cursor I like more".

## Current state on main (`6c59f49`)

- No such mode.

## Target structure

`squad-md/references/architecture-review.md`, used by review.md only for files whose git author is Sven's personal account. codebase-design vocabulary plus architect red flags; no grilling, no HTML.

## External skills per step

| Step | Skill | Reuse mode | Lives in | Reason |
|---|---|---|---|---|
| review | [Matt improve-codebase-architecture](https://github.com/mattpocock/skills/blob/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/engineering/improve-codebase-architecture/SKILL.md) | rewrite | `references/architecture-review.md` | Only for code Sven wrote; drop grilling and HTML report. |
| review | [architect (+ design-red-flags)](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/architect/SKILL.md) | pin | external-skills.json | Client neutral, used as is. |
| review | [Matt codebase-design](https://github.com/mattpocock/skills/blob/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/engineering/codebase-design/SKILL.md) | pin | external-skills.json | Sven likes design it twice and deep vs shallow; use as is. |

## Subagents and what they load

- squad-architect, readonly.

## Steps for Builder

1. Write it.
2. Hook from review.md.
3. Fixture with a shallow module.

## DoD

- Fixture returns a deepening finding with path:line.
- `dotnet run --project tools/AgentPacks.Cli -- validate` exits 0, 0 errors, 0 warnings.
- `/pack-check` locally gives the same result (same rules, one source).
- `dotnet test tools/AgentPacks.Cli.Tests` green.
- No em dash, no tokens in markdown, FAKE fixture values only.

## Evidence

- Fixture output.

## Risks and out of scope

- Git author only, never org data.
