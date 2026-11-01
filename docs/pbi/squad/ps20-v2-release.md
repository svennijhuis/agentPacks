# PS20: Squad v2 release: versions, marketplace tag, changelog, v1 to v2 guide
- Status: todo (docs only; build waits for Sven go per PBI)
- Batch: B7
- Depends on: PS01 to PS19, GA01 to GA06
- Absorbs old: none
- Lane: Builder (Cursor CLI for docs and markdown, Cloud Agent if multi-file C#)
- door: two-way
- Release: squad v2 ([README](README.md#squad-v2))
- Docs: [STRUCTURE](STRUCTURE.md) (where), [SHAPES](SHAPES.md) (how it looks), [FLOWS](FLOWS.md) (flow per command), [MIGRATION](MIGRATION.md) (v1 to v2 rows for this PBI), [gh-aw](gh-aw/README.md)
- Source: [skill map](sources/pstack-matt-skill-map.md), pstack `ae5650f`, Matt `4ae2317`

## Goal and why

- Sven: "nu is moment dat we naar v2 gaan dus doe goed".
- Users of v1, local and in gh-aw, need one clear switch point.

## Current state on main (`6c59f49`)

- `plugins/squad/plugin.json` is `0.1.5`; other plugins are on 0.x or 1.0.0; gh-aw pins `@v1.0.0` or a 40-hex SHA.

## Target structure

Version `2.0.0` in `plugin.json` of every plugin whose files moved (squad, pack-author, pack-check, security, dotnet, rust, typescript). Git and other untouched plugins keep theirs.
Generated marketplace commit gets tag `v2.0.0`; gh-aw pins that tag or its 40-hex SHA (never `marketplace-beta`, it is force-pushed).
`CHANGELOG.md` has a `## 2.0.0` block with a **Breaking** list taken from [MIGRATION](MIGRATION.md#what-breaks-from-v1).
Repo README links the v1 to v2 guide.

## External skills per step

| Step | Skill | Reuse mode | Lives in | Reason |
|---|---|---|---|---|
| release notes | [technical-writing](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/technical-writing/SKILL.md) | pin | external-skills.json | Docs and CODING_STANDARDS.md. |
| release notes | [unslop](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/unslop/SKILL.md) | pin | external-skills.json | Every prose output. |

## Example

Good (what the result looks like):

```markdown
## 2.0.0
**Breaking**
- `skills/squad` is now `skills/squad-md`. gh-aw: change the `skills:` path when you bump to `@v2.0.0`.
- `/statechart` is new; room commands go from 4 to 5.
```

Bad (what we do not want):

Bumping to 2.0.0 with no Breaking list, or tagging `marketplace-beta` and telling gh-aw users to pin it.

## Subagents and what they load

- none

## Files to change

| Kind | Now | After | Action |
|---|---|---|---|
| version | `plugin.json 0.x / 1.0.0` | `2.0.0 for moved plugins, tag v2.0.0` | bump |

Full list for all PBIs: [MIGRATION.md](MIGRATION.md#path-table).

## How to do it

1. Branch from main: `git switch -c pbi/ps20-v2-release origin/main`.
2. Read [SHAPES.md](SHAPES.md) for every kind in the table above, and the upstream source linked in **External skills per step**.
3. Do each row of **Files to change** in order. For a move or rename use `git mv`; for a copy keep the `Source:` line and `## What we changed`.
4. Follow **Steps for Builder** below. Nothing outside the table and the steps.
5. Run the checks in **Done check**. All must pass before you open the PR.
6. Open one PR titled `PS20: Squad v2 release: versions, marketplace tag, changelog, v1 to v2 guide`, body: link to this file, the table, the check output.

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

1. Bump versions (PluginVersionContractTests rule).
2. Changelog block.
3. Tag after the generated commit exists; no tag on source main.
4. README link.

## DoD

- Every plugin with a moved file is 2.0.0; untouched plugins unchanged.
- Tag points at a generated commit, not a branch.
- Local: works in Cursor, Claude Code and CLI (Copilot CLI, Codex). Proof offline: generated client manifests and fixture tests (`ClientManifestTests`, `ComponentGenerationTests`).
- gh-aw: the fixture workflow under `tests/fixtures/gh-aw/` that uses this PBI's files passes `gh aw compile --strict` (pinned gh-aw, no run, no secrets, no Actions minutes). See [gh-aw README](gh-aw/README.md).
- `dotnet run --project tools/AgentPacks.Cli -- validate` exits 0, 0 errors, 0 warnings.
- `/pack-check` locally gives the same result (same rules, one source).
- `dotnet test tools/AgentPacks.Cli.Tests` green; main stays green and all 7 commands still load after merge ([MIGRATION](MIGRATION.md)).
- No em dash, no tokens in markdown, FAKE fixture values only.

## Evidence

- Version list, tag SHA.

## Risks and out of scope

- Tag before GA fixtures pass would publish a broken gh-aw path.
- Out: AW repo pin bump (GA05, done in the AW room).
