# GA06: Marketplace in gh-aw and local: install guide and examples
- Status: todo (docs only; build waits for Sven go per PBI)
- Depends on: GA04
- Release: squad v2
- Docs: [STRUCTURE](../STRUCTURE.md), [SHAPES](../SHAPES.md), [FLOWS](../FLOWS.md), [MIGRATION](../MIGRATION.md), [set README](../README.md), [gh-aw README](README.md)
- AW input: Agentic Workflows room, 2026-11-01

## Goal and why

- Sven: it must work local and in GitHub Agentic Workflows, marketplace and imports.

## Target

`docs/how-to/install-v2.md` in agentPacks:

| Where | How |
|---|---|
| Cursor | marketplace add, pick `v2.0.0`, reload |
| Claude Code | `/plugin marketplace add`, update, reload |
| Copilot CLI | marketplace add, same pin |
| Codex | generated TOML, same pin |
| gh-aw | copy the fixture workflow, pin `@v2.0.0`, list `aw-closure.json` in `skills:`, `gh aw compile --strict` |

Local only (never in gh-aw): yes or no asks (verify harness, Session split confirm), `/pack-check` install and reload, `/statechart` checker scripts, arena with several models, `docs/squad-plan.md` run state, retro writes.

## Example

Good:

Every row names the exact pin and the reload step.

Bad:

"Install the plugin and it works everywhere."

## How to do it

1. Branch from main: `git switch -c pbi/ga06-install-local-and-gh-aw origin/main`.
2. Read [the gh-aw README](README.md) and [SHAPES.md](../SHAPES.md); the AW repo facts are in its table "Per file".
3. Do the **Target** exactly, then the steps below. Nothing else.
4. Run **Done check**, then open one PR titled `GA06: Marketplace in gh-aw and local: install guide and examples`.

## Steps for Builder

1. Write the guide with technical-writing and unslop.
2. One local example (repo with `docs/squad-plan.md`) and one gh-aw example (the fixture).

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
