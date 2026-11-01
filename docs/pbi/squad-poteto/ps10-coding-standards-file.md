# PS10: CODING_STANDARDS.md per repo and .github instructions example
- Status: todo (docs only, build waits for Sven go per PBI)
- Batch: B2
- Depends on: PS07
- Lane: Builder
- door: two-way
- kind: skill
- Source: [skill map](sources/pstack-matt-skill-map.md) (Repo Research 2026-11-01), pstack `ae5650f`, Matt `4ae2317`

## Goal and why

- Sven: "When CODING_STANDARDS.md maybe add this per repo and instruction file for only .github and then instruction example make skills for it also used the other skills for it like chart skills etc".

## Current state on main (`d76fa14`)

- No `CODING_STANDARDS.md` template. Language packs list standards inline.

## Target structure

- `plugins/squad/skills/squad-md/references/templates/CODING_STANDARDS.md` (template for target repos).
- `plugins/squad/skills/squad-md/references/templates/.github/instructions/coding-standards.instructions.md` (example, only under `.github`).
- Review Standards axis (PS08) reads the target repo `CODING_STANDARDS.md`.
- The file itself is indexed as **standard-coding-standards** (PS14).
- Writing it uses **technical-writing** + **unslop**; diagrams via **statechart** where state matters.

## External skills per step

| Flow step | Skill | Link (pinned SHA) | Reuse mode | Depends on |
|---|---|---|---|---|
| docs | technical-writing | [technical-writing](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/technical-writing/SKILL.md) | pinned as-is (`external-skills.json`, 40-hex) | unslop |
| docs | statechart | [statechart](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/statechart/SKILL.md) | pinned as-is (`external-skills.json`, 40-hex) | Bun, TLC, Java |
| review | [code-review](https://github.com/mattpocock/skills/blob/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/engineering/code-review/SKILL.md) | [code-review](https://github.com/mattpocock/skills/blob/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/engineering/code-review/SKILL.md) | weave (MIT line in NOTICE) | CODING_STANDARDS.md |

## Subagents and what they load

- squad-reviewer reads it; squad-planner proposes it when missing.

## Steps for Builder

1. Write both templates.
2. Planner rule: repo without the file gets a plan task to add it.
3. Fixtures.

## DoD

- Fixture repo without the file gets a task; review cites a rule from it.
- `dotnet run --project tools/AgentPacks.Cli -- validate` exits 0 with 0 errors and 0 warnings.
- `/pack-check` run locally reports 0 warnings (same checks as `validate`, PO decision 1).
- `dotnet test tools/AgentPacks.Cli.Tests` green.
- No em dash, no tokens in markdown, FAKE fixture values only.

## Evidence

- Fixture output.

## Risks and out of scope

- Out of scope: per-language content (PS17).
