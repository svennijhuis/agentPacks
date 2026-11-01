# PS08: CODING_STANDARDS.md per repo plus .github instructions example
- Status: todo (docs only; build waits for Sven go per PBI)
- Batch: B2
- Depends on: PS07
- Absorbs old: none
- Lane: Builder (Cursor CLI for docs and markdown, Cloud Agent if multi-file C#)
- door: two-way
- Source: [skill map](sources/pstack-matt-skill-map.md), pstack `ae5650f`, Matt `4ae2317`, flows in [FLOWS.md](FLOWS.md)

## Goal and why

- Sven: "CODING_STANDARDS.md maybe add this per repo and instruction file for only .github ... used the other skills for it like chart skills".

## Current state on main (`6c59f49`)

- No template on main.

## Target structure

Templates in `squad-md/references/templates/`: `CODING_STANDARDS.md` and `.github/instructions/coding-standards.instructions.md`. Planner adds a task when a target repo lacks it. Review Standards axis reads it. Written with technical-writing and unslop; statechart-md diagrams where state matters.

## External skills per step

| Step | Skill | Reuse mode | Lives in | Reason |
|---|---|---|---|---|
| docs | [technical-writing](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/technical-writing/SKILL.md) | pin | external-skills.json | Docs and CODING_STANDARDS.md. |
| docs | [statechart](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/statechart/SKILL.md) | copy + adapt | `plugins/squad/skills/statechart-md/` | Upstream has `disable-model-invocation: true` and is named like the command. Sven wants `/statechart` plus auto use in the flow. We rename to `statechart-md`, allow model use, add `docs/architecture/` output and the scenario export to `/scenarios`. Scripts stay byte-equal. |
| review | [Matt code-review](https://github.com/mattpocock/skills/blob/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/engineering/code-review/SKILL.md) | weave | `squad-reviewer`, `references/review-contract.md` | Two axes (Standards, Spec). Our severity words and CODING_STANDARDS.md stay. |

## Example

Good (what the result looks like):

```markdown
# Coding standards (repo X)
- HTTP collections use a named envelope: `{"users": [...]}`.
- Application layer never calls a storage SDK directly.
Review cites: "CS-02 violated at src/Orders/Api.cs:41".
```

Bad (what we do not want):

A CODING_STANDARDS.md that says "write clean code" with no rule a reviewer can cite by id.

## Subagents and what they load

- squad-planner proposes it; squad-reviewer reads it.

## Steps for Builder

1. Write templates.
2. Planner rule.
3. Fixtures.

## DoD

- Repo without the file gets a task; review cites a rule from it.
- `dotnet run --project tools/AgentPacks.Cli -- validate` exits 0, 0 errors, 0 warnings.
- `/pack-check` locally gives the same result (same rules, one source).
- `dotnet test tools/AgentPacks.Cli.Tests` green.
- No em dash, no tokens in markdown, FAKE fixture values only.

## Evidence

- Fixture output.

## Risks and out of scope

- Out: language content (PS17).
