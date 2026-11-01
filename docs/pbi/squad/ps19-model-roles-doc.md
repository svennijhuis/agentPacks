# PS19: pstack model roles mapped to our tiers
- Status: todo (docs only; build waits for Sven go per PBI)
- Batch: B6
- Depends on: PS04
- Absorbs old: none
- Lane: Builder (Cursor CLI for docs and markdown, Cloud Agent if multi-file C#)
- door: two-way
- Release: squad v2 ([README](README.md#squad-v2))
- Docs: [STRUCTURE](STRUCTURE.md) (where), [SHAPES](SHAPES.md) (how it looks), [FLOWS](FLOWS.md) (flow per command), [MIGRATION](MIGRATION.md) (v1 to v2 rows for this PBI), [gh-aw](gh-aw/README.md)
- Source: [skill map](sources/pstack-matt-skill-map.md), pstack `ae5650f`, Matt `4ae2317`

## Goal and why

- Sven: check setup-pstack for our plugins.

## Current state on main (`6c59f49`)

- Tiers in [`models.source.json`](https://github.com/svennijhuis/agentPacks/blob/6c59f49/models.source.json); no role map.

## Target structure

`docs/PSTACK-ROLES.md`: each role named by pinned or copied skills (explorer, explainer, candidate, interrogator, reviewer) to our tier per provider. No live calls, no spend, no `~/.cursor` setup on the shared box.

## External skills per step

| Step | Skill | Reuse mode | Lives in | Reason |
|---|---|---|---|---|
| roles | [setup-pstack](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/setup-pstack/SKILL.md) | not used | `docs/PSTACK-ROLES.md` | Writes `~/.cursor/rules` (Cursor only, and no long-term setup on the shared box). We document role to tier instead. |

## Example

Good (what the result looks like):

```markdown
| pstack role | Used by | Claude | Copilot | Cursor | Codex |
|---|---|---|---|---|---|
| candidate | arena (our copy) | tier-high | tier-high | tier-high | tier-high |
```

Bad (what we do not want):

Running setup-pstack on the shared box, which writes `~/.cursor/rules`.

## Subagents and what they load

- none

## Files to change

Only the files named in **Target structure** and **Steps for Builder**. No moves or renames.

## How to do it

1. Branch from main: `git switch -c pbi/ps19-model-roles-doc origin/main`.
2. Read [SHAPES.md](SHAPES.md) for every kind in the table above, and the upstream source linked in **External skills per step**.
3. Do each row of **Files to change** in order. For a move or rename use `git mv`; for a copy keep the `Source:` line and `## What we changed`.
4. Follow **Steps for Builder** below. Nothing outside the table and the steps.
5. Run the checks in **Done check**. All must pass before you open the PR.
6. Open one PR titled `PS19: pstack model roles mapped to our tiers`, body: link to this file, the table, the check output.

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

1. Write the doc.

## DoD

- Every role named by a used skill is in the table.
- Local: works in Cursor, Claude Code and CLI (Copilot CLI, Codex). Proof offline: generated client manifests and fixture tests (`ClientManifestTests`, `ComponentGenerationTests`).
- gh-aw: the fixture workflow under `tests/fixtures/gh-aw/` that uses this PBI's files passes `gh aw compile --strict` (pinned gh-aw, no run, no secrets, no Actions minutes). See [gh-aw README](gh-aw/README.md).
- `dotnet run --project tools/AgentPacks.Cli -- validate` exits 0, 0 errors, 0 warnings.
- `/pack-check` locally gives the same result (same rules, one source).
- `dotnet test tools/AgentPacks.Cli.Tests` green; main stays green and all 7 commands still load after merge ([MIGRATION](MIGRATION.md)).
- No em dash, no tokens in markdown, FAKE fixture values only.

## Evidence

- Doc.

## Risks and out of scope

- Out: running setup-pstack.
