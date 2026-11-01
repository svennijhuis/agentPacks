# PS11: Architecture review mode for code Sven wrote
- Status: todo (docs only; build waits for Sven go per PBI)
- Batch: B2
- Depends on: PS10
- Absorbs old: none
- Lane: Builder (Cursor CLI for docs and markdown, Cloud Agent if multi-file C#)
- door: two-way
- Release: squad v2 ([README](README.md#squad-v2))
- Docs: [STRUCTURE](STRUCTURE.md) (where), [SHAPES](SHAPES.md) (how it looks), [FLOWS](FLOWS.md) (flow per command), [MIGRATION](MIGRATION.md) (v1 to v2 rows for this PBI), [gh-aw](gh-aw/README.md)
- Source: [skill map](sources/pstack-matt-skill-map.md), pstack `ae5650f`, Matt `4ae2317`

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

## Example

Good (what the result looks like):

```text
Finding (architecture, medium): OrderService is shallow (6 public
methods, each 1 line to the repo). Deepen: one PlaceOrder entry.
Only for code Sven wrote; no grilling, no HTML report.
```

Bad (what we do not want):

Running the architecture rewrite on vendored or generated code, or asking Sven 10 grilling questions first.

## Subagents and what they load

- squad-architect, readonly.

## Files to change

Only the files named in **Target structure** and **Steps for Builder**. No moves or renames.

## How to do it

1. Branch from main: `git switch -c pbi/ps11-architecture-review origin/main`.
2. Read [SHAPES.md](SHAPES.md) for every kind in the table above, and the upstream source linked in **External skills per step**.
3. Do each row of **Files to change** in order. For a move or rename use `git mv`; for a copy keep the `Source:` line and `## What we changed`.
4. Follow **Steps for Builder** below. Nothing outside the table and the steps.
5. Run the checks in **Done check**. All must pass before you open the PR.
6. Open one PR titled `PS11: Architecture review mode for code Sven wrote`, body: link to this file, the table, the check output.

## Done check

```bash
dotnet run --project tools/AgentPacks.Cli -- validate
dotnet test tools/AgentPacks.Cli.Tests
dotnet run --project tools/AgentPacks.Cli -- validate-all --out /tmp/agentpacks-marketplace
for f in tests/fixtures/gh-aw/*.md; do gh aw compile --strict "$f"; done   # from GA04 on; before GA04 write "n/a, no fixture yet"
grep -rnP "[\x{2013}\x{2014}]" plugins docs || echo "no dashes"
```

Local: install `/tmp/agentpacks-marketplace` in Cursor or Claude Code (README "Install the generated tree") and run the commands this PBI touches once.

## Steps for Builder

1. Write it.
2. Hook from review.md.
3. Fixture with a shallow module.

## DoD

- Fixture returns a deepening finding with path:line.
- Local: works in Cursor, Claude Code and CLI (Copilot CLI, Codex). Proof offline: generated client manifests and fixture tests (`ClientManifestTests`, `ComponentGenerationTests`).
- gh-aw: the fixture workflow under `tests/fixtures/gh-aw/` that uses this PBI's files passes `gh aw compile --strict` (pinned gh-aw, no run, no secrets, no Actions minutes). See [gh-aw README](gh-aw/README.md).
- `dotnet run --project tools/AgentPacks.Cli -- validate` exits 0, 0 errors, 0 warnings.
- `/pack-check` locally gives the same result (same rules, one source).
- `dotnet test tools/AgentPacks.Cli.Tests` green; main stays green and all 7 commands still load after merge ([MIGRATION](MIGRATION.md)).
- No em dash, no tokens in markdown, FAKE fixture values only.

## Evidence

- Fixture output.

## Risks and out of scope

- Git author only, never org data.
