# GA07: Delete v1 pointers and trim engineering.md
- Status: todo (docs only; build waits for Sven go per PBI)
- Depends on: GA05
- Release: squad v2
- Docs: [STRUCTURE](../STRUCTURE.md), [SHAPES](../SHAPES.md), [FLOWS](../FLOWS.md), [MIGRATION](../MIGRATION.md), [set README](../README.md), [gh-aw README](README.md)
- AW input: Agentic Workflows room, 2026-11-01

## Goal and why

- Migrate callers, then delete. Only after the AW catalog runs on v2.

## Target

Remove `plugins/squad/skills/squad/` and `plugins/pack-check/skills/pack-check/` pointer dirs. Trim the principle paraphrase in `shared/standards/engineering.md` to links. Update `StandardsGhAwImportContractTests` expectations.

## Example

Good:

Old dirs gone, all links resolve, gh-aw fixture still compiles.

Bad:

Deleting the old dir in PS06, before the AW pin moved.

## How to do it

1. Branch from main: `git switch -c pbi/ga07-delete-v1-paths origin/main`.
2. Read [the gh-aw README](README.md) and [SHAPES.md](../SHAPES.md); the AW repo facts are in its table "Per file".
3. Do the **Target** exactly, then the steps below. Nothing else.
4. Run **Done check**, then open one PR titled `GA07: Delete v1 pointers and trim engineering.md`.

## Steps for Builder

1. Check no generated manifest, test or doc still names the old paths.
2. One PR.

## DoD

- Local: Cursor, Claude Code and CLI still load the same files (generated manifests and fixture tests, offline).
- gh-aw: every fixture under `tests/fixtures/gh-aw/` passes `gh aw compile --strict` with the pinned gh-aw version. No workflow run, no secrets, no Actions minutes.
- `validate` 0 errors, 0 warnings; `/pack-check` the same; tests green; main green after merge.
- Personal svennijhuis only. No em dash, no tokens in markdown, FAKE fixture values only.

## Done check

```bash
dotnet run --project tools/AgentPacks.Cli -- validate
dotnet test tools/AgentPacks.Cli.Tests
dotnet run --project tools/AgentPacks.Cli -- validate-all --out /tmp/agentpacks-marketplace
for f in tests/fixtures/gh-aw/*.md; do gh aw compile --strict "$f"; done   # GA04 and later
```

## Evidence

- Compile output of the fixtures, test names, closure diff.
