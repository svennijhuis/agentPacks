# PS17: Language packs restyle, import-boundary tests, typescript-best-practices
- Status: todo (docs only; build waits for Sven go per PBI)
- Batch: B5
- Depends on: PS16
- Absorbs old: old 55 (import boundaries)
- Lane: Builder (Cursor CLI for docs and markdown, Cloud Agent if multi-file C#)
- door: two-way
- Release: squad v2 ([README](README.md#squad-v2))
- Docs: [STRUCTURE](STRUCTURE.md) (where), [SHAPES](SHAPES.md) (how it looks), [FLOWS](FLOWS.md) (flow per command), [MIGRATION](MIGRATION.md) (v1 to v2 rows for this PBI), [gh-aw](gh-aw/README.md)
- Source: [skill map](sources/pstack-matt-skill-map.md), pstack `ae5650f`, Matt `4ae2317`

## Goal and why

- Sven: typescript-best-practices for the typescript plugin.
- Old 55: boundary rules as tests, still open on main (no boundary test found).

## Current state on main (`6c59f49`)

- dotnet, rust, typescript skills repeat "Internal. Do not run directly" boilerplate; no principle refs; no boundary tests.

## Target structure

Drop boilerplate, add index lines; pin typescript-best-practices in `plugins/typescript/external-skills.json`; one boundary-test template per language; review skills cite Coding Standards File.

## External skills per step

| Step | Skill | Reuse mode | Lives in | Reason |
|---|---|---|---|---|
| typescript | [typescript-best-practices](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/typescript-best-practices/SKILL.md) | pin | `plugins/typescript/external-skills.json` | Sven: add to the typescript plugin. |
| test | [tdd](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/tdd/SKILL.md) | copy + adapt | `plugins/squad/skills/tdd/` | Upstream has `disable-model-invocation: true` and says "use only when the user explicitly asks". Sven wants it on every behavior change ("We can use this really hard"). So we change the trigger and the flag; the loop stays. |

## Example

Good (what the result looks like):

```text
csharp: test fails when Application references Azure.Storage.*
(import-boundary test, rule from standard Layers).
```

Bad (what we do not want):

A language pack restating the shared HTTP rule in its own words instead of the shared leaf.

## Subagents and what they load

- none

## Files to change

| Kind | Now | After | Action |
|---|---|---|---|
| skill | `plugins/{dotnet,rust,typescript}/skills/*` | same | restyle, drop boilerplate |
| pins | `plugins/typescript/external-skills.json` | same | add typescript-best-practices |

Full list for all PBIs: [MIGRATION.md](MIGRATION.md#path-table).

## How to do it

1. Branch from main: `git switch -c pbi/ps17-language-packs origin/main`.
2. Read [SHAPES.md](SHAPES.md) for every kind in the table above, and the upstream source linked in **External skills per step**.
3. Do each row of **Files to change** in order. For a move or rename use `git mv`; for a copy keep the `Source:` line and `## What we changed`.
4. Follow **Steps for Builder** below. Nothing outside the table and the steps.
5. Run the checks in **Done check**. All must pass before you open the PR.
6. Open one PR titled `PS17: Language packs restyle, import-boundary tests, typescript-best-practices`, body: link to this file, the table, the check output.

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

1. One PR per language.

## DoD

- validate 0/0 per pack.
- Local: works in Cursor, Claude Code and CLI (Copilot CLI, Codex). Proof offline: generated client manifests and fixture tests (`ClientManifestTests`, `ComponentGenerationTests`).
- gh-aw: the fixture workflow under `tests/fixtures/gh-aw/` that uses this PBI's files passes `gh aw compile --strict` (pinned gh-aw, no run, no secrets, no Actions minutes). See [gh-aw README](gh-aw/README.md).
- `dotnet run --project tools/AgentPacks.Cli -- validate` exits 0, 0 errors, 0 warnings.
- `/pack-check` locally gives the same result (same rules, one source).
- `dotnet test tools/AgentPacks.Cli.Tests` green; main stays green and all 7 commands still load after merge ([MIGRATION](MIGRATION.md)).
- No em dash, no tokens in markdown, FAKE fixture values only.

## Evidence

- Per-pack output.

## Risks and out of scope

- Out: new standards text.
